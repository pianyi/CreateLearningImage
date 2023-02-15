using NLog;
using Unity;

namespace CreateLearningImage.Datas.Common
{
    /// <summary>
    /// <para>アプリケーション起動中のシステム共通設定</para>
    /// <para>保存されません。アプリケーション終了時に破棄されます</para>
    /// <para>DIコンテナにてシングルトン化されています</para>
    /// </summary>
    public class SessionDatas
    {
        /// <summary>
        /// ログクラス
        /// </summary>
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// アプリケーションデータ
        /// </summary>
        [Dependency]
        internal ApplicationSettings ApplicationSetting { private get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SessionDatas()
        {
        }
    }
}
