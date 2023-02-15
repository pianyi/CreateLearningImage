using CreateLearningImage.Core;
using CreateLearningImage.Datas.Common;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace CreateLearningImage.Datas.Controls
{
    /// <summary>
    /// 設定ファイルのIOを制御します
    /// </summary>
    public class SettingFileIO
    {
        /// <summary>
        /// ロガー
        /// </summary>
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// アプリケーション設定ファイルを読み込みします
        /// </summary>
        public ApplicationSettings ReadApplicationSettings()
        {
            var applicationSetting = ReadSettings<ApplicationSettings>(Constants.ApplicationSettingFilePath);

            if (applicationSetting == null)
            {
                applicationSetting = new ApplicationSettings();

                //アプリケーション設定ファイルが無い場合は作成する
                WriteApplicationSettings(applicationSetting);
            }

            var applicationVersion = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString();
            if (applicationVersion != applicationSetting.AapplicationVersion)
            {
                _logger.Debug("バージョンが違う設定ファイルが読み込まれました。ファイル：{0}、アプリ：{1}",
                             applicationVersion,
                             applicationSetting.AapplicationVersion);

                if (string.IsNullOrWhiteSpace(applicationVersion))
                {
                    applicationVersion = "0.0.0.0";
                }
                applicationSetting.AapplicationVersion = applicationVersion;
            }

            return applicationSetting;
        }

        /// <summary>
        /// 指定ファイルを読み込みます
        /// </summary>
        /// <typeparam name="T">ファイルに対応するデータクラス</typeparam>
        /// <param name="filePath">読込ファイル名称</param>
        /// <returns>読み込んだデータインスタンス</returns>
        public T ReadSettings<T>(string filePath)
        {
            _logger.Debug($"読込ファイル：{filePath}、クラス：{typeof(T)}");

            try
            {
                if (File.Exists(filePath))
                {
                    using var sr = new StreamReader(filePath, new UTF8Encoding(false));
                    return JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "アプリケーション設定ファイルの読み込みに失敗しました");
            }

            return default;
        }

        /// <summary>
        /// アプリケーション設定ファイルを出力します
        /// </summary>
        /// <param name="applicationSettings">保存するアプリケーション設定インスタンス</param>
        public void WriteApplicationSettings(ApplicationSettings applicationSettings)
        {
            WriteSettings(Constants.ApplicationSettingFilePath, applicationSettings);
        }

        /// <summary>
        /// 指定オブジェクトをファイルとして出力します
        /// </summary>
        /// <typeparam name="T">出力対象のデータクラス</typeparam>
        /// <param name="filePath">出力先ファイルパス</param>
        /// <param name="instance">出力対象のデータインスタンス</param>
        public void WriteSettings<T>(string filePath, T instance)
        {
            if (instance == null)
            {
                _logger.Warn("書き込みデータインスタンスが存在しません。");
                return;
            }

            _logger.Debug($"書込ファイル：{filePath}、クラス：{typeof(T)}");

            // 保存先のディレクトリを作成
            var directory = Path.GetDirectoryName(filePath);
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(filePath), "ファイルパスからディレクトリ名が取得できませんでした。");
            }
            Directory.CreateDirectory(directory);

            using var sw = new StreamWriter(filePath, false);
            var json = JsonConvert.SerializeObject(instance, Formatting.Indented);
            sw.WriteLine(json);
        }
    }
}
