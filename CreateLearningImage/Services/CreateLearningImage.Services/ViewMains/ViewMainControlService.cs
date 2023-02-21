using CreateLearningImage.Core;
using CreateLearningImage.Core.Events;
using CreateLearningImage.Core.Natives;
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
using System.Web;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VideoLibrary;
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
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 設定情報
        /// </summary>
        private readonly ApplicationSettings _applicationSetting;

        /// <summary>
        /// 学習用データ作成
        /// </summary>
        public ReactivePropertySlim<bool> IsLearning { get; set; }

        /// <summary>
        /// テスト用データ作成
        /// </summary>
        public ReactivePropertySlim<bool> IsTest { get; set; }

        /// <summary>
        /// キャプチャタイミング
        /// </summary>
        public ReactivePropertySlim<int> Timing { get; set; }

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
        /// 作成した画像のIndex 
        /// </summary>
        private long _imageIndex = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="settings">アプリケーション設定</param>
        public ViewMainControlService(IEventAggregator eventAggregator,
                                      ApplicationSettings settings)
        {
            _eventAggregator = eventAggregator;
            _applicationSetting = settings;

            IsLearning = new ReactivePropertySlim<bool>(true);
            IsTest = new ReactivePropertySlim<bool>(false);
            Timing = _applicationSetting.ToReactivePropertySlimAsSynchronized(x => x.CaptureTiming);
            ImageFilePath = _applicationSetting.ToReactivePropertySlimAsSynchronized(x => x.MovieFilePath);
            Lbpcascade = _applicationSetting.ToReactivePropertySlimAsSynchronized(x => x.LbpcascadeFilePath);
            Output = _applicationSetting.ToReactivePropertySlimAsSynchronized(x => x.OutputDirectoryPath);
            StartStopButtonName = new ReactivePropertySlim<string>(IconNamePlay);
            IsStart = new ReactivePropertySlim<bool>(false);
            Images = new ReactivePropertySlim<BitmapSource>();

            Faces = new ReactiveCollection<ImageData>();

            Lbpcascade.Subscribe(CreateCascadeClassifier);

            // 優先順位を指定してタイマのインスタンスを生成
            _timer = new DispatcherTimer(DispatcherPriority.Normal);
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
        /// 動画の再生を最初に戻します。
        /// </summary>
        public void StepBack()
        {
            if (_videoCapture != null)
            {
                _videoCapture.PosFrames = 0;
                ReadFrame();
            }
        }

        /// <summary>
        /// 動画再生を開始します
        /// </summary>
        public async Task StartAsync()
        {
            if (string.IsNullOrEmpty(ImageFilePath.Value))
            {
                Stop();
                return;
            }

            string youtubeUri = GetYoutubeURL(ImageFilePath.Value);
            if (youtubeUri == null && File.Exists(ImageFilePath.Value))
            {
                // 再生データが無いので処理しない
                Stop();
                return;
            }

            if (_videoCapture?.FrameCount == _videoCapture?.PosFrames)
            {
                // 現在のフレームと最大フレームば同じ場合は再生完了とし、最初から再生する

                // 前の動画を破棄する
                _videoCapture?.Dispose();

                if (string.IsNullOrEmpty(youtubeUri))
                {
                    // ファイルを再生する
                    _videoCapture = new VideoCapture(ImageFilePath.Value);
                }
                else
                {
                    // youtube をcaptureする
                    YouTubeVideo video = await YouTube.Default.GetVideoAsync(youtubeUri);
                    _videoCapture = new VideoCapture(video.Uri);
                }

                if (!_videoCapture.IsOpened())
                {
                    throw new ArgumentException("指定ファイルの読み込みに失敗しました。");
                }

                // 一番最初から再生する
                _videoCapture.Set(VideoCaptureProperties.BufferSize, _videoCapture.FrameCount);
                StepBack();
                Faces.Clear();
                _imageIndex = 0;

                _eventAggregator.GetEvent<InitializeEvent>().Publish();

                // FPS に合わせた画像更新を行う
                _timer.Interval = TimeSpan.FromMilliseconds((int)(1000 / _videoCapture.Fps));

                _executeDateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            }

            _stopwatch.Restart();
            StartStopButtonName.Value = IconNamePause;
            IsStart.Value = true;

            _timer.Start();
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
            try
            {
                using var mat = new Mat();
                if (_videoCapture.Read(mat))
                {
                    if (mat.IsContinuous())
                    {
                        if (Timing.Value < _stopwatch.ElapsedMilliseconds && !mat.IsDisposed)
                        {
                            // 指定秒数経過した画像を解析に回す
                            Task.Run(() => AnalyseAsync(mat.Clone()));
                            _stopwatch.Restart();
                        }

                        // 画像を表示する
                        Images.Value = mat.ToWriteableBitmap();
                    }
                }

                if (_videoCapture.FrameCount == _videoCapture.PosFrames)
                {
                    // 動画再生終わり
                    Stop();
                }
            }
            catch
            {
                _logger.Warn("ファイルの読み込みで想定外エラー");
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
                    ImageData data = new()
                    {
                        Image = image,
                        FileName = $"{_executeDateTime}_{_imageIndex}.png"
                    };
                    _imageIndex++;

                    if (IsLearning.Value)
                    {
                        // 学習データはユーザ選択用に保持し保存しない
                        Faces.AddOnScheduler(data);
                    }
                    else
                    {
                        // テストデータはそのまま保存
                        SaveImage(data);
                    }
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
        /// YoutubeのURLが正しいかを確認します。
        /// </summary>
        /// <param name="url">チェックURL</param>
        /// <returns>正常なURL</returns>
        private string GetYoutubeURL(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            string videoId;
            string link;

            var uri = new Uri(url);
            switch (uri.Authority)
            {
                case "www.youtube.com":
                    var nv = HttpUtility.ParseQueryString(uri.Query);
                    videoId = nv["v"];
                    if (videoId == null)
                    {
                        return null;
                    }
                    link = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}?v={videoId}";
                    break;

                case "youtu.be":
                    link = uri.AbsoluteUri;
                    break;

                default:
                    return null;
            }

            return link;
        }

        /// <summary>
        /// 指定画像情報を削除します
        /// </summary>
        /// <param name="index"></param>
        public void DeleteAt(int index)
        {
            // データは0始まり、index は1始まり
            int value = index - 1;
            if (0 <= value && value < Faces.Count)
            {
                var data = Faces[value];

                if (!string.IsNullOrEmpty(data.FolderName))
                {
                    string filePath = Path.Combine(GetOutputFolderPath(), data.FolderName);
                    filePath = Path.Combine(filePath, data.FileName);
                    if (File.Exists(filePath))
                    {
                        //File.Delete(filePath);

                        FileDeleteProcess.SHFILEOPSTRUCT sf = new()
                        {
                            wFunc = FileDeleteProcess.FileFuncFlags.FO_DELETE, //削除を指示します。
                            fFlags = FileDeleteProcess.FILEOP_FLAGS.FOF_ALLOWUNDO | FileDeleteProcess.FILEOP_FLAGS.FOF_NO_UI, // 元に戻すを有効。かつUI表示無し
                            pFrom = filePath + "\0"
                        };

                        if (FileDeleteProcess.StartFileOperation(ref sf) != 0)
                        {
                            _logger.Warn("ファイルの削除に失敗しました。");
                        }
                    }
                }

                Faces.Remove(Faces[value]);
            }
        }

        /// <summary>
        /// 画像ファイルを保存します
        /// </summary>
        /// <param name="data">保存データクラス</param>
        /// <param name="fromFolderName">変更前のフォルダ名</param>
        public void SaveImage(ImageData data, string fromFolderName = "")
        {
            if (IsTest.Value)
            {
                // テストデータの出力(フォルダ階層を下げる)
                string newPath = GetOutputFolderPath();
                // テストの場合はIndexが指定されていないのでフォルダ内のデータ数を設定する
                newPath = Path.Combine(newPath, data.FileName);

                using var fileStream = new FileStream(newPath, FileMode.Create);
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(data.Image));
                encoder.Save(fileStream);
            }
            else
            {
                // 学習用データの出力(出力先に分類フォルダをさらに追加)
                string newPath = GetOutputFolderPath();
                newPath = Path.Combine(newPath, data.FolderName);
                newPath = Path.Combine(newPath, data.FileName);

                if (!string.IsNullOrEmpty(data.FolderName) && string.IsNullOrEmpty(fromFolderName))
                {
                    // 保存先が指定されており、元フォルダ名が未指定の場合、新規ファイル作成で保存する

                    _logger.Debug($"出力：{newPath}");

                    using var fileStream = new FileStream(newPath, FileMode.Create);
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(data.Image));
                    encoder.Save(fileStream);
                }
                else if (!string.IsNullOrEmpty(data.FolderName) && !string.IsNullOrEmpty(fromFolderName) &&
                         data.FolderName != fromFolderName)
                {
                    // 保存先が指定されており、元フォルダ名も指定されている場合、ファイルを移動する
                    string fromPath = Path.Combine(GetOutputFolderPath(), fromFolderName);
                    fromPath = Path.Combine(fromPath, data.FileName);

                    _logger.Info($"移動：{fromPath} → {newPath}");

                    if (File.Exists(fromPath))
                    {
                        File.Move(fromPath, newPath);
                    }
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

        /// <summary>
        /// データ出力先フォルダを取得します
        /// </summary>
        /// <returns>出力先フォルダパス</returns>
        public string GetOutputFolderPath()
        {
            string result = Output.Value;

            if (IsLearning.Value)
            {
                result = Path.Combine(result, Constants.DirectoryNameLearning);
            }
            else
            {
                result = Path.Combine(result, Constants.DirectoryNameTest);
            }

            Directory.CreateDirectory(result);

            return result;
        }

        /// <summary>
        /// 現在出力されている画像を読み込みます
        /// </summary>
        /// <param name="findFolderPath">読込先ファイルパス</param>
        public void ReadExistingData(string findFolderPath)
        {
            if (Directory.Exists(findFolderPath))
            {
                Faces.Clear();

                foreach (string filePath in Directory.EnumerateFiles(findFolderPath, "*", SearchOption.AllDirectories))
                {
                    _logger.Debug($"読み込み：{filePath}");
                    var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite);
                    var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);

                    Faces.AddOnScheduler(new ImageData()
                    {
                        IsReadData = true,
                        Image = decoder.Frames[0],
                        FolderName = Path.GetFileName(Path.GetDirectoryName(filePath)),
                        FileName = Path.GetFileName(filePath)
                    });
                }
            }
        }
    }
}
