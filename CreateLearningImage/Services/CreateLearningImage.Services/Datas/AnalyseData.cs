using OpenCvSharp;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace CreateLearningImage.Services.Datas
{
    /// <summary>
    /// 解析結果データ
    /// </summary>
    public class AnalyseData
    {
        /// <summary>
        /// 対象の画像
        /// </summary>
        public BitmapSource Image { get; private set; }

        /// <summary>
        /// 顔認識された座標
        /// </summary>
        public List<Rect> Results { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="image">画像</param>
        /// <param name="results">位置情報</param>
        public AnalyseData(BitmapSource image, List<Rect> results)
        {
            Image = image;
            Results = results;
        }
    }
}
