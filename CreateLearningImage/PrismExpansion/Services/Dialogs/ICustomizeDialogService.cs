using Prism.Services.Dialogs;
using System.Windows;

namespace PrismExpansion.Services.Dialogs
{
    /// <summary>
    /// Interface to show modal and non-modal dialogs.
    /// </summary>
    public interface ICustomizeDialogService : IDialogService
    {
        /// <summary>
        /// 指定された名称のダイアログウィンドウインスタンスを取得します
        /// </summary>
        /// <param name="windowName">名称</param>
        /// <returns>ウィンドウのインスタンス(存在しない場合はnull)</returns>
        public Window? GetDialogWindow(string windowName);

        /// <summary>
        /// 指定された名称のダイアログウィンドウを閉じます
        /// </summary>
        /// <param name="windowName">名称</param>
        public void CloseDialogWindow(string windowName);

        /// <summary>
        /// 表示されているすべてのウィンドウをクローズします
        /// </summary>
        public void CloseAll();
    }
}
