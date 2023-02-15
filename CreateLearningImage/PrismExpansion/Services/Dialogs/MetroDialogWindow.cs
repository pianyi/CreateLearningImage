using MahApps.Metro.Controls;
using Prism.Services.Dialogs;

namespace PrismExpansion.Services.Dialogs
{
    /// <summary>
    /// ダイアログをMetroにするためのクラス
    /// </summary>
    public partial class MetroDialogWindow : MetroWindow, IDialogWindow
    {
        /// <summary>
        /// ダイアログの戻り値
        /// </summary>
        public IDialogResult? Result { get; set; }
    }
}
