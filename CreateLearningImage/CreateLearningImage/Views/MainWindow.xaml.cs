using CreateLearningImage.Events;
using MahApps.Metro.Controls;
using NLog;
using Prism.Events;
using System.ComponentModel;
using System;
using Prism.Services.Dialogs;
using Unity;

namespace CreateLearningImage.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// 登録済みDIコンテナクラス
        /// ロガー
        /// </summary>
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// イベント制御
        /// </summary>
        [Dependency]
        internal IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Closing += MainWindowClosing;
            Closed += MainWindowClosed;
        }

        /// <summary>
        /// ウィンドウが閉じる前の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            EventAggregator.GetEvent<ClosingWindowEvent<CancelEventArgs>>().Publish(e);
        }

        /// <summary>
        /// ウィンドウが閉じた時の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowClosed(object sender, EventArgs e)
        {
            _logger.Debug("メイン画面を閉じる時のイベントを発行");

            EventAggregator.GetEvent<ClosedWindowEvent>().Publish();

            Closing -= MainWindowClosing;
            Closed -= MainWindowClosed;
        }
    }
}
