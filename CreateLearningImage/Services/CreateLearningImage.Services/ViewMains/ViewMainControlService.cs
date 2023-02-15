using CreateLearningImage.Core;
using CreateLearningImage.Core.Events;
using CreateLearningImage.Datas.Common;
using CreateLearningImage.Services.Datas;
using NLog;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Events;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Rect = OpenCvSharp.Rect;

namespace CreateLearningImage.Services.ViewMains
{
    /// <summary>
    /// メインページの操作を行う
    /// </summary>
    public class ViewMainControlService
    {
        /// <summary>
        /// ログ出力クラス
        /// </summary>
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 再生アイコン名称
        /// </summary>
        private const string IconNamePlay = "Play";
        /// <summary>
        /// 一時停止アイコン名称
        /// </summary>
        private const string IconNamePause = "Pause";

        /// <summary>
        /// イベント登録クラス
        /// </summary>
        [Unity.Dependency]
        internal IEventAggregator EventAggregator;

        /// <summary>
        /// 動画ファイルのパス
        /// </summary>
        public ReactivePropertySlim<string> ImageFilePath { get; set; }

        /// <summary>
        /// 顔認識用定義ファイルパス
        /// </summary>
        public ReactivePropertySlim<string> Lbpcascade { get; set; }

        /// <summary>
        /// データ出力フォルダパス
        /// </summary>
        public ReactivePropertySlim<string> Output { get; set; }

        /// <summary>
        /// 再生・一時停止ボタンの表示名称
        /// </summary>
        public ReactivePropertySlim<string> StartStopButtonName { get; set; }

        /// <summary>
        /// 再生・一時停止の状態
        /// </summary>
        public ReactivePropertySlim<bool> IsStart { get; set; }

        /// <summary>
        /// 画面表示イメージ
        /// </summary>
        public ReactivePropertySlim<BitmapSource> Images { get; set; }

        /// <summary>
        /// 画像解析結果情報
        /// </summary>
        public ReactiveCollection<ImageData> Faces { get; set; }

        /// <summary>
        /// OpenCVのビデオキャプチャクラス
        /// </summary>
        private VideoCapture _videoCapture;

        /// <summary>
        /// 顔検出の定義情報
        /// </summary>
        private CascadeClassifier _cascadeClassifier;

        /// <summary>
        /// 動画描画タイマー
        /// </summary>
        private readonly DispatcherTimer _timer;

        /// <summary>
        /// キャプチャタイミング計測ストップウォッチ
        /// </summary>
        private readonly Stopwatch _stopwatch = new();

        /// <summary>
        /// 動画再生開始時間
        /// </summary>
        private string _executeDateTime = string.Empty;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="settings">アプリケーション設定</param>
        public ViewMainControlService(ApplicationSettings settings)
        {
            ImageFilePath = settings.ToReactivePropertySlimAsSynchronized(x => x.MovieFilePath);
            Lbpcascade = settings.ToReactivePropertySlimAsSynchronized(x => x.LbpcascadeFilePath);
            Output = settings.ToReactivePropertySlimAsSynchronized(x => x.OutputDirectoryPath);
            StartStopButtonName = new ReactivePropertySlim<string>(IconNamePlay);
            IsStart = new ReactivePropertySlim<bool>(false);
            Images = new ReactivePropertySlim<BitmapSource>();

            Faces = new ReactiveCollection<ImageData>();

            Lbpcascade.Subscribe(CreateCascadeClassifier);

            // 優先順位を指定してタイマのインスタンスを生成
            _timer = new DispatcherTimer(DispatcherPriority.Background);
            // 時間が来たら描画を行う
            _timer.Tick += (e, s) => { ReadFrame(); };
        }

        /// <summary>
        /// 顔認識の定義情報を作成します
        /// </summary>
        /// <param name="filePath">XMLファイルパス</param>
        public void CreateCascadeClassifier(string filePath)
        {
            if (File.Exists(filePath))
            {
                _cascadeClassifier = new CascadeClassifier(filePath);
            }
            else
            {
                _cascadeClassifier = null;
            }
        }

        /// <summary>
        /// 動画再生を開始します
        /// </summary>
        public void Start()
        {
            try
            {
                if (!File.Exists(ImageFilePath.Value))
                {
                    return;
                }

                if (_videoCapture?.FrameCount == _videoCapture?.PosFrames)
                {
                    // 現在のフレームと最大フレームば同じ場合は再生完了とし、最初から再生する

                    // 前の動画を破棄する
                    _videoCapture?.Dispose();

                    _videoCapture = new VideoCapture(ImageFilePath.Value);
                    if (!_videoCapture.IsOpened())
                    {
                        throw new ArgumentException("指定ファイルの読み込みに失敗しました。");
                    }

                    // 一番最初から再生する
                    _videoCapture.PosFrames = 0;
                    Faces.Clear();

                    EventAggregator.GetEvent<InitializeEvent>().Publish();

                    // FPS に合わせた画像更新を行う
                    _timer.Interval = new TimeSpan((long)(1000 / _videoCapture.Fps));

                    _executeDateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                }

                _stopwatch.Restart();
                StartStopButtonName.Value = IconNamePause;
                IsStart.Value = true;

                _timer.Start();
            }
            catch
            {
                Stop();
                throw;
            }
        }

        /// <summary>
        /// 動画再生を停止します
        /// </summary>
        public void Stop()
        {
            try
            {
                _stopwatch.Stop();
                _timer.Stop();
                StartStopButtonName.Value = IconNamePlay;
                IsStart.Value = false;
            }
            catch
            {
                _stopwatch.Stop();
                _timer.Stop();
                StartStopButtonName.Value = IconNamePlay;
                IsStart.Value = false;
                throw;
            }
        }

        /// <summary>
        /// 動画情報から画面描画用bitmapを読み込みます
        /// </summary>
        private void ReadFrame()
        {
            bool isEnd = false;

            try
            {
                _timer.Stop();

                using var mat = new Mat();
                if (_videoCapture.Read(mat) && mat.IsContinuous())
                {
                    if (500 < _stopwatch.ElapsedMilliseconds)
                    {
                        Task.Run(() => AnalyseAsync(mat.Clone()));
                        _stopwatch.Restart();
                    }

                    var image = mat.ToBitmapSource();
                    if (image.CanFreeze)
                    {
                        image.Freeze();
                    }

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Images.Value = image;
                    }));
                }
                else
                {
                    // 動画再生終わり
                    Stop();
                    isEnd = true;
                }
            }
            catch
            {
                _logger.Warn("コマ落ちしてます");
            }
            finally
            {
                if (!isEnd)
                {
                    _timer.Start();
                }
            }
        }

        /// <summary>
        /// 画像データを元に顔認識処理を実行します
        /// </summary>
        /// <param name="mat">解析元画像データ</param>
        private void AnalyseAsync(Mat mat)
        {
            try
            {
                if (mat == null)
                {
                    return;
                }

                var gray = mat.CvtColor(ColorConversionCodes.BGR2GRAY);
                var rects = _cascadeClassifier.DetectMultiScale(gray);
                foreach (Rect rect in rects)
                {
                    // 認識された顔を格納
                    var image = mat[rect].ToBitmapSource();
                    image.Freeze();
                    Faces.Add(new ImageData()
                    {
                        Image = image,
                        Index = Faces.Count
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                mat?.Dispose();
            }
        }

        /// <summary>
        /// 画像ファイルを保存します
        /// </summary>
        /// <param name="data">保存データクラス</param>
        /// <param name="fromFolderName">変更前のフォルダ名</param>
        public void SaveImage(ImageData data, string fromFolderName = "")
        {
            // 保存先フォルダパスを作成する
            string newPath = Path.Combine(Output.Value, data.FolderName);
            newPath = Path.Combine(newPath, $"{_executeDateTime}_{data.Index}.png");

            if (!string.IsNullOrEmpty(data.FolderName) && string.IsNullOrEmpty(fromFolderName))
            {
                // 保存先が指定されており、元フォルダ名が未指定の場合、新規ファイル作成で保存する

                using var fileStream = new FileStream(newPath, FileMode.Create);
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(data.Image));
                encoder.Save(fileStream);
            }
            else if (!string.IsNullOrEmpty(data.FolderName) && !string.IsNullOrEmpty(fromFolderName) &&
                     data.FolderName != fromFolderName)
            {
                // 保存先が指定されており、元フォルダ名も指定されている場合、ファイルを移動する
                string fromPath = Path.Combine(Output.Value, fromFolderName);
                fromPath = Path.Combine(fromPath, $"{_executeDateTime}_{data.Index}.png");

                if (File.Exists(fromPath))
                {
                    File.Move(fromPath, newPath);
                }
            }
        }

        /// <summary>
        /// 画像データをフォルダに保存します。
        /// </summary>
        public void AllSaveImage()
        {
            foreach (ImageData data in Faces)
            {
                if (string.IsNullOrEmpty(data.FolderName))
                {
                    // 指定されていない場合は、デフォルト値を設定する
                    data.FolderName = Constants.DirectoryNameOthers;
                }

                SaveImage(data);
            }
        }
    }
}
