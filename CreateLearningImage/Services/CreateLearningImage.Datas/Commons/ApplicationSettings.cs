using CreateLearningImage.Core.Datas;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace CreateLearningImage.Datas.Common
{
    /// <summary>
    /// <para>アプリケーション全体で共通の設定情報を保持します。</para>
    /// <para>DIコンテナにてシングルトン化されています</para>
    /// </summary>
    [DataContract]
    public class ApplicationSettings : AbstractCopyBase, INotifyPropertyChanged
    {
        /// <summary>
        /// アプリケーションバージョンを取得・設定します
        /// </summary>
        [JsonProperty("AapplicationVersion")]
        public string AapplicationVersion { get; set; } = "0.0.0.0";

        /// <summary>
        /// キャプチャタイミング
        /// </summary>
        private int _captureTiming = 5000;
        /// <summary>
        /// キャプチャタイミング
        /// </summary>
        [JsonProperty("CaptureTiming")]
        public int CaptureTiming
        {
            get => _captureTiming;
            set => SetProperty(ref _captureTiming, value);
        }

        /// <summary>
        /// 動画ファイルパス
        /// </summary>
        private string _movieFilePath = string.Empty;
        /// <summary>
        /// 動画ファイルパス
        /// </summary>
        [JsonProperty("MovieFilePath")]
        public string MovieFilePath
        {
            get => _movieFilePath;
            set => SetProperty(ref _movieFilePath, value);
        }

        /// <summary>
        /// 画像認識定義ファイルパス
        /// </summary>
        private string _lbpcascadeFilePath = string.Empty;
        /// <summary>
        /// 画像認識定義ファイルパス
        /// </summary>
        [JsonProperty("LbpcascadeFilePath")]
        public string LbpcascadeFilePath
        {
            get => _lbpcascadeFilePath;
            set => SetProperty(ref _lbpcascadeFilePath, value);
        }

        /// <summary>
        /// 出力先ディレクトリパス
        /// </summary>
        private string _outputDirectoryPath = string.Empty;
        /// <summary>
        /// 出力先ディレクトリパス
        /// </summary>
        [JsonProperty("OutputDirectoryPath")]
        public string OutputDirectoryPath
        {
            get => _outputDirectoryPath;
            set => SetProperty(ref _outputDirectoryPath, value);
        }

        /// <summary>
        /// 画像保存時の幅
        /// </summary>
        private int _resizeWidth = 100;
        /// <summary>
        /// 画像保存時の幅
        /// </summary>
        [JsonProperty("ResizeWidth")]
        public int ResizeWidth
        {
            get => _resizeWidth;
            set => SetProperty(ref _resizeWidth, value);
        }

        /// <summary>
        /// 画像保存時の高さ
        /// </summary>
        private int _resizeHeight = 100;
        /// <summary>
        /// 画像保存時の高さ
        /// </summary>
        [JsonProperty("ResizeHeight")]
        public int ResizeHeight
        {
            get => _resizeHeight;
            set => SetProperty(ref _resizeHeight, value);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ApplicationSettings()
        {
        }
    }
}
