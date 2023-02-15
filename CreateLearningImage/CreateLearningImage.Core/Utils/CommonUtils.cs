using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CreateLearningImage.Core.Utils
{
    /// <summary>
    /// システム共通メソッド
    /// </summary>
    public static class CommonUtils
    {
        /// <summary>
        /// SHA512 ハッシュコード生成プロバイダ
        /// </summary>
        private static readonly SHA512 hashProvider = SHA512.Create();

        /// <summary>
        /// SHA512 ハッシュコードを生成します
        /// </summary>
        /// <param name="value">ハッシュ元</param>
        /// <returns>ハッシュコード</returns>
        public static string GetSHAHashed(string value)
        {
            return string.Join("", hashProvider.ComputeHash(Encoding.UTF8.GetBytes($"任意の文字列で{value}ハッシュを分かりにくくする")).Select(x => $"{x:X2}"));
        }

        /// <summary>
        /// 引数の文字列に、ファイル名に利用できない文字列があるかを確認します
        /// </summary>
        /// <param name="value">チェック文字列</param>
        /// <returns>true:エラー文字あり、false:エラー文字無し</returns>
        public static bool IsErrorFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            bool isError = false;

            if (0 <= value.IndexOfAny(invalid))
            {
                isError = true;
            }

            return isError;
        }

        /// <summary>
        /// 「-0」を「0」に変換して取得します
        /// </summary>
        /// <param name="value">変換元データ</param>
        /// <returns>変換後のデータ</returns>
        public static string GetAbsZero(string value)
        {
            string result = value;

            if (result.StartsWith("-"))
            {
                decimal tmp = Convert.ToDecimal(value);
                if (tmp == decimal.Zero)
                {
                    result = result.Substring(1, result.Length - 1);
                }
            }

            return result;
        }

        /// <summary>
        /// 強制GCを実行します
        /// </summary>
        /// <remarks>
        /// 第2世代までのGCを強制的に行います。
        /// 本機能を呼び出すときは画面及び処理がすべて停止します。
        /// むやみに利用しない事。
        /// </remarks>
        public static void ForceGC()
        {
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
