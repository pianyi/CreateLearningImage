using System.IO;

namespace CreateLearningImage.Core
{
    /// <summary>
    /// 画面切り替え箇所の名称定義
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// メイン
        /// </summary>
        public const string ContentRegion = "ContentRegion";

        /// <summary>
        /// 学習データ用フォルダ
        /// </summary>
        public const string DirectoryNameLearning = "Learning";
        /// <summary>
        /// テストデータ用フォルダ
        /// </summary>
        public const string DirectoryNameTest = "Test";

        /// <summary>
        /// その他の画像を入れるフォルダパス
        /// </summary>
        public const string DirectoryNameOthers = "Others";

        /// <summary>
        /// JSONファイルの拡張子
        /// </summary>
        public const string FileExtensionJSON = ".json";

        /// <summary>
        /// アプリケーション設定ファイル名称
        /// </summary>
        public const string ApplicationSettingFileName = "ApplicationSettings" + FileExtensionJSON;

        /// <summary>
        /// 実行ファイルと同じフォルダパスを取得します
        /// </summary>
        public static string ExecuteFolder
        {
            get
            {
                return Path.GetFullPath("./");
            }
        }

        /// <summary>
        /// アプリケーション設定ファイルパスを取得します
        /// </summary>
        public static string ApplicationSettingFilePath
        {
            get
            {
                return Path.Join(ExecuteFolder, ApplicationSettingFileName);
            }
        }
    }
}
