using Prism.Common;
using Prism.Ioc;
using Prism.Services.Dialogs;
using PrismExpansion.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace PrismExpansion.Services.Dialogs
{
    /// <summary>
    /// ダイアログウィンドウ制御クラスをPrismをベースにカスタイマイズする
    /// </summary>
    public class CustomizeDialogService : ICustomizeDialogService
    {
        /// <summary>
        /// DIコンテナ
        /// </summary>
        private readonly IContainerExtension _containerExtension;

        /// <summary>
        /// 表示されているウィンドウの制御
        /// </summary>
        private readonly Dictionary<string, IDialogWindow> _dialogList = new();
        /// <summary>
        /// 未設定のウィンドウ名称
        /// </summary>
        private int windowNameIndex = 1;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="containerExtension"></param>
        public CustomizeDialogService(IContainerExtension containerExtension)
        {
            _containerExtension = containerExtension;
        }

        /// <summary>
        /// 指定された名称のダイアログウィンドウインスタンスを取得します
        /// </summary>
        /// <param name="windowName">名称</param>
        /// <returns>ウィンドウのインスタンス(存在しない場合はnull)</returns>
        public Window? GetDialogWindow(string windowName)
        {
            if (_dialogList.TryGetValue(windowName, out var dialogWindow))
            {
                if (dialogWindow is Window result)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 指定された名称のダイアログウィンドウインスタンスが存在するかどうかを判断します
        /// </summary>
        /// <param name="windowName"></param>
        /// <returns></returns>
        public bool HasDialogWindow(string windowName)
        {
            return GetDialogWindow(windowName) != null;
        }

        /// <summary>
        /// 指定された名称のダイアログウィンドウを閉じます
        /// </summary>
        /// <param name="windowName">名称</param>
        public void CloseDialogWindow(string windowName)
        {
            var window = GetDialogWindow(windowName);
            if (window != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    window.Close();
                }));
            }
        }

        /// <summary>
        /// 表示されているすべてのウィンドウをクローズします
        /// </summary>
        public void CloseAll()
        {
            foreach (IDialogWindow win in _dialogList.Values)
            {
                win.Close();
            }

            _dialogList.Clear();
        }

        /// <summary>
        /// モーダレスモードで表示
        /// </summary>
        /// <param name="name">表示クラス名(UserControl名)</param>
        /// <param name="parameters">渡すパラメータ</param>
        /// <param name="callback">戻り値</param>
        public void Show(string name, IDialogParameters? parameters = null, Action<IDialogResult>? callback = null)
        {
            ShowDialogInternal(name, parameters, callback, false);
        }

        /// <summary>
        /// モーダレスモードで表示
        /// </summary>
        /// <param name="name">表示クラス名(UserControl名)</param>
        /// <param name="parameters">渡すパラメータ</param>
        /// <param name="callback">戻り値</param>
        /// <param name="windowName">The name of the hosting window registered with the IContainerRegistry.</param>
        public void Show(string name, IDialogParameters? parameters = null, Action<IDialogResult>? callback = null, string? windowName = null)
        {
            ShowDialogInternal(name, parameters, callback, false, windowName);
        }

        /// <summary>
        /// モーダルモードで表示
        /// </summary>
        /// <param name="name">表示クラス名(UserControl名)</param>
        /// <param name="parameters">渡すパラメータ</param>
        /// <param name="callback">戻り値</param>
        public void ShowDialog(string name, IDialogParameters? parameters = null, Action<IDialogResult>? callback = null)
        {
            ShowDialogInternal(name, parameters, callback, true);
        }

        /// <summary>
        /// モーダルモードで表示
        /// </summary>
        /// <param name="name">表示クラス名(UserControl名)</param>
        /// <param name="parameters">渡すパラメータ</param>
        /// <param name="callback">戻り値</param>
        /// <param name="windowName">The name of the hosting window registered with the IContainerRegistry.</param>
        public void ShowDialog(string name, IDialogParameters? parameters, Action<IDialogResult>? callback, string? windowName = null)
        {
            ShowDialogInternal(name, parameters, callback, true, windowName);
        }

        /// <summary>
        /// ダイアログを表示します
        /// </summary>
        /// <param name="name">表示クラス名(UserControl名)</param>
        /// <param name="parameters">渡すパラメータ</param>
        /// <param name="callback">戻り値</param>
        /// <param name="isModal">true:モーダルダイアログ、false:モーダレスダイアログ</param>
        /// <param name="windowName">The name of the hosting window registered with the IContainerRegistry.</param>
        private void ShowDialogInternal(string name, IDialogParameters? parameters, Action<IDialogResult>? callback, bool isModal, string? windowName = null)
        {
            if (parameters == null)
            {
                parameters = new DialogParameters();
            }
            if (windowName == null)
            {
                windowName = string.Empty;
            }

            if (_dialogList.TryGetValue(windowName, out var dialogWindow))
            {
                // 存在するダイアログは既存を取得し、アクティブ(前面)に移動
                if (dialogWindow is Window hostWindow)
                {
                    hostWindow.Activate();
                }
            }
            else
            {
                // 存在しないダイアログは新規作成
                IDialogWindow newWindow = CreateDialogWindow(windowName);
                ConfigureDialogWindowEvents(newWindow, callback);
                ConfigureDialogWindowContent(name, newWindow, parameters);

                ShowDialogWindow(newWindow, isModal);
            }
        }

        /// <summary>
        /// Shows the dialog window.
        /// </summary>
        /// <param name="dialogWindow">The dialog window to show.</param>
        /// <param name="isModal">If true; dialog is shown as a modal</param>
        protected virtual void ShowDialogWindow(IDialogWindow dialogWindow, bool isModal)
        {
            if (isModal)
            {
                dialogWindow.ShowDialog();
            }
            else
            {
                dialogWindow.Show();
            }

            // 起動時に設定された情報を画面に即時反映する
            UIThreadMessageHelper.DoEvents();
        }

        /// <summary>
        /// Create a new <see cref="IDialogWindow"/>.
        /// </summary>
        /// <param name="windowName">The name of the hosting window registered with the IContainerRegistry.</param>
        /// <returns>The created <see cref="IDialogWindow"/>.</returns>
        protected virtual IDialogWindow CreateDialogWindow(string windowName)
        {
            IDialogWindow newWindow = _containerExtension.Resolve<IDialogWindow>();

            // 名前を付けて保存する
            string windowNameTmp = windowName;
            if (string.IsNullOrWhiteSpace(windowNameTmp))
            {
                windowNameTmp = windowNameIndex.ToString();
                windowNameIndex++;
            }
            _dialogList.Add(windowNameTmp, newWindow);

            return newWindow;
        }

        /// <summary>
        /// ウィンドウの設定を行います
        /// </summary>
        /// <param name="dialogName"></param>
        /// <param name="window"></param>
        /// <param name="parameters"></param>
        private void ConfigureDialogWindowContent(string dialogName, IDialogWindow window, IDialogParameters parameters)
        {
            var content = _containerExtension.Resolve<object>(dialogName);
            if (content is not FrameworkElement dialogContent)
            {
                throw new NullReferenceException("A dialog's content must be a FrameworkElement");
            }

            if (dialogContent.DataContext is not IDialogAware viewModel)
            {
                throw new NullReferenceException("A dialog's ViewModel must implement the IDialogAware interface");
            }

            MvvmHelpers.ViewAndViewModelAction<IDialogAware>(viewModel, d => d.OnDialogOpened(parameters));

            ConfigureDialogWindowProperties(window, dialogContent, viewModel);
        }

        /// <summary>
        /// ダイアログのイベント制御を追加します
        /// </summary>
        /// <param name="dialogWindow"></param>
        /// <param name="callback"></param>
        private void ConfigureDialogWindowEvents(IDialogWindow dialogWindow, Action<IDialogResult>? callback)
        {
            #region ローカル関数
            void requestCloseHandler(IDialogResult o)
            {
                dialogWindow.Result = o;
                dialogWindow.Close();
            }

            void loadedHandler(object o, RoutedEventArgs e)
            {
                dialogWindow.Loaded -= loadedHandler;
                ((IDialogAware)dialogWindow.DataContext).RequestClose += requestCloseHandler;
            }

            void closingHandler(object? o, CancelEventArgs e)
            {
                if (!((IDialogAware)dialogWindow.DataContext).CanCloseDialog())
                {
                    e.Cancel = true;
                }
            }

            void closedHandler(object? o, EventArgs e)
            {
                dialogWindow.Closed -= closedHandler;
                dialogWindow.Closing -= closingHandler;
                var vm = (IDialogAware)dialogWindow.DataContext;
                vm.RequestClose -= requestCloseHandler;

                vm.OnDialogClosed();

                if (dialogWindow.Result == null)
                {
                    if (vm is ICustomizeDialogResult rs)
                    {
                        dialogWindow.Result = rs.GetClosedResult;
                    }

                    if (dialogWindow.Result == null)
                    {
                        dialogWindow.Result = new DialogResult();
                    }
                }

                callback?.Invoke(dialogWindow.Result);

                dialogWindow.DataContext = null;
                dialogWindow.Content = null;

                // 独自登録したインスタンスを破棄
                var key = _dialogList.FirstOrDefault(c => c.Value == dialogWindow).Key;
                if (key != null)
                {
                    _dialogList.Remove(key);
                }
            }
            #endregion ローカル関数

            dialogWindow.Loaded += loadedHandler;
            dialogWindow.Closing += closingHandler;
            dialogWindow.Closed += closedHandler;
        }

        /// <summary>
        /// ダイアログのコンフィグの設定を行います
        /// </summary>
        /// <param name="window"></param>
        /// <param name="dialogContent"></param>
        /// <param name="viewModel"></param>
        private void ConfigureDialogWindowProperties(IDialogWindow window, FrameworkElement dialogContent, IDialogAware viewModel)
        {
            bool unsetOwner = false;

            if (window is Window hostWindow && viewModel is ICustomizeDialogAware styleableVM)
            {
                if (!double.IsNaN(styleableVM.Left) && !double.IsNaN(styleableVM.Top))
                {
                    // 位置指定がある場合は、マニュアル固定にする
                    hostWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    hostWindow.Left = styleableVM.Left;
                    hostWindow.Top = styleableVM.Top;
                }

                unsetOwner = styleableVM.UnsetOwner;
            }

            var windowStyle = Dialog.GetWindowStyle(dialogContent);
            if (windowStyle != null)
            {
                window.Style = windowStyle;
            }

            window.Content = dialogContent;
            window.DataContext = viewModel; //we want the host window and the dialog to share the same data contex

            if (!unsetOwner && window.Owner == null)
            {
                Window? owner = null;

                // TODO 解析画面を表示中に測定ダイアログを前面に出したい場合はこれに似た対応をする必要がある
                //if (owner == null)
                //{
                //    // アクティブウィンドウが無ければ、最後に開いたデータが最前面のはず
                //    var tmp = Application.Current.Windows.OfType<Window>().ToList();
                //    if (2 < tmp.Count)
                //    {
                //        // 最後のデータは自分自身なので、その1つ前が呼び出し前のウィンドウのはず
                //        owner = tmp[tmp.Count - 2];
                //    }
                //}

                // アクティブなウィンドウをオーナーにする
                owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(e => e.IsActive);

                if (owner == null)
                {
                    // アクティブウィンドウが無い場合は、メイン画面をオーナーにする
                    owner = Application.Current.MainWindow;
                }

                try
                {
                    window.Owner = owner;
                }
                catch
                {
                    // 一度も表示していないウィンドウの場合例外が発生するので無視する
                    // その代わり画面中央とする
                    if (window is Window tmp)
                    {
                        tmp.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    }
                }
            }
        }
    }
}
