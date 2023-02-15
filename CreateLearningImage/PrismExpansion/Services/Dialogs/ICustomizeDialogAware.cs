using Prism.Services.Dialogs;

namespace PrismExpansion.Services.Dialogs
{
    /// <summary>
    /// ダイアログに対するパラメータ設定を追加
    /// </summary>
    public interface ICustomizeDialogAware : IDialogAware
    {
        /// <summary>
        /// オーナーウィンドウの指定を拒否します
        /// </summary>
        bool UnsetOwner { get; }

        /// <summary>
        /// ダイアログの左初期位置
        /// </summary>
        double Left { get; }

        /// <summary>
        /// ダイアログの上初期位置
        /// </summary>
        double Top { get; }
    }
}
