using CreateLearningImage.Core.Mvvm;
using CreateLearningImage.Datas.Common;
using CreateLearningImage.Datas.Controls;
using CreateLearningImage.Events;
using CreateLearningImage.Services.ViewMains;
using MahApps.Metro.Controls.Dialogs;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using PrismExpansion.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.ComponentModel;
using Unity;

namespace CreateLearningImage.ViewModels
{
    /// <summary>
    /// メインウィンドウの制御
    /// </summary>
    public class MainWindowViewModel : DisposableBindableViewModelBase
    {
        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        public ReactiveProperty<string> Title { get; set; }

        /// <summary>
        /// DI用マネージャ
        /// </summary>
        [Dependency]
        internal IRegionManager RegionManager = null;

        /// <summary>
        /// イベント登録クラス
        /// </summary>
        [Dependency]
        internal IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// ダイアログ制御サービス
        /// </summary>
        [Dependency]
        internal IDialogService DialogService { get; set; }

        /// <summary>
        /// Metro用ダイアログ制御クラス
        /// </summary>
        [Dependency]
        internal IDialogCoordinator DialogCoordinator { get; set; }

        /// <summary>
        /// メインコントロールサービス
        /// </summary>
        [Dependency]
        internal ApplicationSettings ApplicationSetting { get; set; }

        /// <summary>
        /// メイン画面操作サービス
        /// </summary>
        [Dependency]
        internal ViewMainControlService MainControlService { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;

            Title = new ReactiveProperty<string>("Create Learning Images").AddTo(Disposables);

            // ウィンドウが閉じる前の動作
            EventAggregator.GetEvent<ClosingWindowEvent<CancelEventArgs>>()
                           .Subscribe(MainWindowClosing)
                           .AddTo(Disposables);

            // ウィンドウが閉じた後の動作
            EventAggregator.GetEvent<ClosedWindowEvent>()
                           .Subscribe(MainWindowClosed)
                           .AddTo(Disposables);
        }

        /// <summary>
        /// 画面を閉じる前に動作します
        /// </summary>
        /// <param name="e">メインウィンドウを閉じるかどうかのイベント</param>
        private void MainWindowClosing(CancelEventArgs e)
        {
            var result = DialogCoordinator.ShowModalMessageExternal(this, "確認", "画面を閉じて良いですか？", MessageDialogStyle.AffirmativeAndNegative);

            if (result != MessageDialogResult.Affirmative)
            {
                e.Cancel = true;
            }
            else
            {
                if (DialogService is CustomizeDialogService dialog)
                {
                    // Closedで行うとメイン画面が先に消えてしまい、見た目が良くないのでここで行う
                    dialog.CloseAll();
                }
            }
        }

        /// <summary>
        /// ウィンドウが閉じる時の処理を行います
        /// </summary>
        private void MainWindowClosed()
        {
            try
            {
                MainControlService.Stop();
            }
            catch
            {
                // 終了処理なので例外は無視
            }

            try
            {
                SettingFileIO io = new();
                io.WriteApplicationSettings(ApplicationSetting);
            }
            catch
            {
                // 終了なので無視する
            }

            Destroy();
        }

        #region IDisposable Support
        /// <summary>
        /// オブジェクトの破棄処理
        /// </summary>
        /// <param name="disposing">処理フラグ</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Disposables.Dispose();

                    foreach (var region in RegionManager.Regions)
                    {
                        region.RemoveAll();
                    }
                }

                disposedValue = true;
            }
        }
        #endregion
    }
}
