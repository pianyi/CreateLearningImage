using System.Windows.Media.Imaging;

namespace CreateLearningImage.Services.Datas
{
    /// <summary>
    /// 画像解析されたデータ
    /// </summary>
    public class ImageData
    {
        /// <summary>
        /// 操作対象の画像
        /// </summary>
        public BitmapSource Image { get; set; }

        /// <summary>
        /// 保存先フォルダ名
        /// </summary>
        public string FolderName { get; set; }

        /// <summary>
        /// 保存ファイル名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 読み込みデータかどうか(true:読み込みデータ、false:画像解析データ)
        /// </summary>
        public bool IsReadData { get; set; } = false;
    }
}
