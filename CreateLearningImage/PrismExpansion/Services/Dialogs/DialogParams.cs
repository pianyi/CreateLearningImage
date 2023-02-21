namespace PrismExpansion.Services.Dialogs
{
    /// <summary>
    /// ダイアログ画面へのパラメータ
    /// </summary>
    public static class DialogParams
    {
        /// <summary>
        /// ダイアログを指定するキー
        /// </summary>
        public const string Key = "Key";
        /// <summary>
        /// タイトルメッセージ
        /// </summary>
        public const string Title = "Title";
        /// <summary>
        /// メインメッセージ
        /// </summary>
        public const string Message = "Message";
        /// <summary>
        /// 画像ファイルパス
        /// </summary>
        public const string ImageFilePath = "ImageFilePath";
        /// <summary>
        /// <para>初期ウィンドウの左の位置を指定します</para>
        /// <para>prism:Dialog.WindowStartupLocationがManual指定時に動作します</para>
        /// </summary>
        public const string Left = "Left";
        /// <summary>
        /// <para>初期ウィンドウの上の位置を指定します</para>
        /// <para>prism:Dialog.WindowStartupLocationがManual指定時に動作します</para>
        /// </summary>
        public const string Top = "Top";

        /// <summary>
        /// 表示するウィンドウのオーナーウィンドウの指定を拒否します
        /// </summary>
        public const string UnsetOwner = "UnsetOwner";

        /// <summary>
        /// フォルダパス
        /// </summary>
        public const string FolderPath = "FolderPath";
        /// <summary>
        /// ファイル名
        /// </summary>
        public const string FileName = "FileName";
        /// <summary>
        /// ファイル名(複数)
        /// </summary>
        public const string FileNameList = "FileNameList";

        /// <summary>
        /// チェックするかどうかのフラグ
        /// </summary>
        public const string IsCheck = "IsCheck";

        /// <summary>
        /// はいボタン表示
        /// </summary>
        public const string IsShowYes = "IsShowYes";
        /// <summary>
        /// いいえボタン表示
        /// </summary>
        public const string IsShowNo = "IsShowNo";
        /// <summary>
        /// OKボタン表示
        /// </summary>
        public const string IsShowOk = "IsShowOk";
        /// <summary>
        /// キャンセルボタン表示
        /// </summary>
        public const string IsShowCancel = "IsShowCancel";
    }
}
