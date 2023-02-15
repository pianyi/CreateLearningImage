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
        /// データIndexj
        /// </summary>
        public long Index { get; set; }
    }
}
