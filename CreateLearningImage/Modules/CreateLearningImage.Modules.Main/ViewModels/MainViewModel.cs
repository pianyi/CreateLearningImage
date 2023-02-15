using CreateLearningImage.Core.Mvvm;
using CreateLearningImage.Core.Utils;
using CreateLearningImage.Services.ViewMains;
using MahApps.Metro.Controls.Dialogs;
using Prism.Regions;
using Prism.Services.Dialogs;
using PrismExpansion.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Unity;

namespace CreateLearningImage.Modules.Main.ViewModels
{
    /// <summary>
    /// メインページ処理
    /// </summary>
    public class MainViewModel : RegionViewModelBase
    {
        /// <summary>
        /// Metro用ダイアログ制御クラス
        /// </summary>
        [Dependency]
        internal IDialogCoordinator DialogCoordinator { get; set; }

        /// <summary>
        /// ダイアログ表示サービスを表します。
        /// </summary>
        private IDialogService DialogService { get; set; }

        /// <summary>
        /// メイン画面操作サービス
        /// </summary>
        public ViewMainControlService MainControlService { get; set; }

        /// <summary>
        /// 動画ファイルのパス
        /// </summary>
        public ReactiveProperty<string> ImageFilePath { get; set; }

        /// <summary>
        /// 顔認識用定義ファイルパス
        /// </summary>
        public ReactiveProperty<string> Lbpcascade { get; set; }

        /// <summary>
        /// データ出力フォルダパス
        /// </summary>
        public ReactiveProperty<string> Output { get; set; }

        /// <summary>
        /// 画面表示イメージ
        /// </summary>
        public ReactivePropertySlim<BitmapSource> Images { get; set; }

        /// <summary>
        /// 再生・一時停止ボタンの表示名称
        /// </summary>
        public ReactiveProperty<string> StartStopButtonName { get; set; }

        /// <summary>
        /// 再生・一時停止の状態
        /// </summary>
        public ReactiveProperty<bool> IsStart { get; set; }

        /// <summary>
        /// ファイル選択ダイアログ表示
        /// </summary>
        public ReactiveCommand<string> BrowseFileCommand { get; set; }

        /// <summary>
        /// フォルダ選択ダイアログ表示
        /// </summary>
        public ReactiveCommand BrowseDirectoryCommand { get; set; }

        /// <summary>
        /// 再生・一時停止ボタン
        /// </summary>
        public ReactiveCommand StartStopCommand { get; set; }

        /// <summary>
        /// 振り分け用ダイアログ表示
        /// </summary>
        public ReactiveCommand DistributionCommand { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="regionManager"></param>
        /// <param name="dialogService"></param>
        /// <param name="mainControlService"></param>
        public MainViewModel(IRegionManager regionManager,
                             IDialogService dialogService,
                             ViewMainControlService mainControlService) :
            base(regionManager)
        {
            DialogService = dialogService;
            MainControlService = mainControlService;

            ImageFilePath = MainControlService.ImageFilePath
                                              .ToReactivePropertyAsSynchronized(e => e.Value)
                                              .AddTo(Disposables);
            Lbpcascade = MainControlService.Lbpcascade
                                           .ToReactivePropertyAsSynchronized(e => e.Value)
                                           .AddTo(Disposables);
            Output = MainControlService.Output
                                       .ToReactivePropertyAsSynchronized(e => e.Value)
                                       .AddTo(Disposables);
            Images = MainControlService.Images
                                       .ToReactivePropertySlimAsSynchronized(e => e.Value)
                                       .AddTo(Disposables);
            StartStopButtonName = MainControlService.StartStopButtonName
                                                    .ToReactivePropertyAsSynchronized(e => e.Value)
                                                    .AddTo(Disposables);
            IsStart = MainControlService.IsStart
                                        .ToReactivePropertyAsSynchronized(e => e.Value)
                                        .AddTo(Disposables);

            BrowseFileCommand = new ReactiveCommand<string>().WithSubscribe(BrowseFile).AddTo(Disposables);
            BrowseDirectoryCommand = new ReactiveCommand().WithSubscribe(BrowseDirectory).AddTo(Disposables);
            StartStopCommand = new ReactiveCommand().WithSubscribe(DoStartStop).AddTo(Disposables);
            DistributionCommand = new ReactiveCommand().WithSubscribe(ShowDistributionDialog).AddTo(Disposables);
        }

        /// <summary>
        /// ファイルパスを取得します
        /// </summary>
        /// <param name="type">画面(XML)から渡されるパラメータ</param>
        private void BrowseFile(string type)
        {
            var srcFilePath = string.Empty;
            if (type == "0")
            {
                srcFilePath = ImageFilePath.Value;
            }
            else if (type == "1")
            {
                srcFilePath = Lbpcascade.Value;
            }

            var selectedFilePath = FileOpenProcess.GetFilePath(srcFilePath);

            if (File.Exists(selectedFilePath))
            {
                if (type == "0")
                {
                    ImageFilePath.Value = selectedFilePath;
                }
                else if (type == "1")
                {
                    Lbpcascade.Value = selectedFilePath;
                }
            }
        }

        /// <summary>
        /// フォルダファイルパスを取得します
        /// </summary>
        private void BrowseDirectory()
        {
            var selectedDirectoryPath = FileOpenProcess.GetDirectoryPath(Output.Value);
            if (Directory.Exists(selectedDirectoryPath))
            {
                Output.Value = selectedDirectoryPath;

                // TODO フォルダ構成を再取得し、コンボボックスの値を再取得するように変更する
            }
        }

        /// <summary>
        /// 動画の再生・一時停止を制御します
        /// </summary>
        private void DoStartStop()
        {
            if (IsStart.Value)
            {
                if (!File.Exists(ImageFilePath.Value) || !File.Exists(Lbpcascade.Value))
                {
                    DialogCoordinator.ShowModalMessageExternal(this,
                                                               "エラー",
                                                               "画像またはXMLファイルが存在しません。",
                                                               MessageDialogStyle.Affirmative);
                    IsStart.Value = !IsStart.Value;
                    return;
                }

                if (!Directory.Exists(Output.Value))
                {
                    DialogCoordinator.ShowModalMessageExternal(this,
                                                               "エラー",
                                                               "出力フォルダが存在しません。",
                                                               MessageDialogStyle.Affirmative);
                    IsStart.Value = !IsStart.Value;
                    return;
                }

                // 振り分けダイアログの表示
                ShowDistributionDialog();

                Thread.Sleep(1000);

                // 再生開始
                MainControlService.Start();
            }
            else
            {
                MainControlService.Stop();
            }
        }

        /// <summary>
        /// 振り分け画面を表示します
        /// </summary>
        private void ShowDistributionDialog()
        {
            // 画像振り分け画面を表示する
            DialogParameters param = new()
            {
                { DialogParams.Title, "Distribution" },
                { DialogParams.Top, Application.Current.MainWindow.Top },
                { DialogParams.Left, Application.Current.MainWindow.Left + Application.Current.MainWindow.Width },
                { DialogParams.FolderPath, Output.Value }
            };

            DialogService.Show(nameof(DistributionDialog), param, null, nameof(DistributionDialog));
        }
    }
}
