using CreateLearningImage.Datas.Common;
using CreateLearningImage.Datas.Controls;
using CreateLearningImage.Modules.DistributionDialog;
using CreateLearningImage.Modules.DistributionDialog.Views;
using CreateLearningImage.Modules.Main;
using CreateLearningImage.Services.ViewMains;
using CreateLearningImage.Views;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Prism.Unity;
using PrismExpansion.Services.Dialogs;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace CreateLearningImage
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        /// <summary>
        /// ログ出力クラス
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public App() : base()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// 未処理の例外処理を処理します。
        /// </summary>
        /// <param name="sender">イベントオブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void ApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Error(e.Exception, "処理されないシステム例外発生のため、強制終了されました。");

            MessageBox.Show("Error",
                            "A system error has occurred. Exit the application.",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error,
                            MessageBoxResult.OK,
                            MessageBoxOptions.DefaultDesktopOnly);

            Shutdown();
        }

        /// <summary>
        /// アプリケーション起動時の初期ウィンドウ
        /// </summary>
        /// <returns>初期ウィンドウ</returns>
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        /// <summary>
        /// インスタンスのDI登録(インスタンスの生成を任せる)
        /// </summary>
        /// <param name="containerRegistry">インスタンス登録クラス</param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // アプリケーション情報
            containerRegistry.RegisterSingleton<ApplicationSettings>();
            containerRegistry.RegisterSingleton<SessionDatas>();

            SettingFileIO io = new();
            ApplicationSettings app = new();
            // アプリケーション設定情報を読み込み
            app.CopyProperty(io.ReadApplicationSettings());
            containerRegistry.RegisterInstance(app);

            // ダイアログ表示クラスをカスタマイズ(基本は元のコピー)
            containerRegistry.RegisterDialogWindow<MetroDialogWindow>();

            containerRegistry.RegisterSingleton<IDialogService, CustomizeDialogService>();
            containerRegistry.RegisterSingleton<IDialogCoordinator, DialogCoordinator>();
            containerRegistry.RegisterSingleton<ViewMainControlService>();

            containerRegistry.RegisterDialog<DistributionDialog>();
        }

        /// <summary>
        /// モジュールクラスを初期化します
        /// </summary>
        /// <param name="moduleCatalog">モジュール制御クラス</param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ModuleMainModule>();
            moduleCatalog.AddModule<DistributionDialogModule>();
        }

        /// <summary>
        /// ViewとViewModelの関連付けを行います
        /// </summary>
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            // 紐づけのカスタマイズ
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(GetViewModelName);

            // /Views/XXView と /ViewModels/XXViewModel は勝手に紐づく
            // ViewとViewModel の関連付け
            // ルールから外れる場合はここで強制的に紐づける
            //ViewModelLocationProvider.Register<MainWindow, MainWindowViewModel>();
        }

        /// <summary>
        /// ルートディレクトリ以外でも、ViewsとViewModels の自動紐づけを行うようにカスタマイズする
        /// </summary>
        /// <param name="viewType">View側のデータ</param>
        /// <returns>ViewModelのタイプ</returns>
        private Type GetViewModelName(Type viewType)
        {
            // Viewに対応するViewModelの名前を生成
            // viewType.FullNameで取得されるのは名前空間も含めた完全名.
            // その名前空間の｢Views｣の部分を｢ViewModels｣に置換している.
            var viewModelName = $"{viewType.FullName?.Replace("Views", "ViewModels")}ViewModel";
            var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
            return Type.GetType($"{viewModelName}, {viewAssemblyName}");
        }
    }
}
