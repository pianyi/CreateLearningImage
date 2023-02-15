using Prism.Services.Dialogs;

namespace PrismExpansion.Services.Dialogs
{
    /// <summary>
    /// ダイアログがクローズした時の戻り値を指定するインターフェース
    /// </summary>
    public interface ICustomizeDialogResult
    {
        /// <summary>
        /// ダイアログがクローズした時のレスポンスを取得します
        /// </summary>
        /// <remarks>戻り値が null だった場合の戻り値を指定(基本は×ボタンが押された時の戻り値)</remarks>
        IDialogResult GetClosedResult { get; }
    }
}
