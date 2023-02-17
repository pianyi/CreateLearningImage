using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CreateLearningImage.Core.Utils
{
    /// <summary>
    /// ファイル、フォルダ選択処理用クラス
    /// </summary>
    public static class FileOpenProcess
    {
        #region Win32API
        /// <summary>
        /// Win32APIクラス(使う箇所が増えたらちゃんと別クラスに整理する事
        /// </summary>
        private static class NativeMethods
        {
            /// <summary>
            /// ウィンドウの検索
            /// </summary>
            /// <param name="parentWnd">親ウィンドウハンドル</param>
            /// <param name="previousWnd">前のウィンドウハンドル</param>
            /// <param name="className">検索クラス名</param>
            /// <param name="windowText">検索ウィンドウテキスト</param>
            /// <returns>ウィンドウハンドル</returns>
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr FindWindowEx(IntPtr parentWnd, IntPtr previousWnd, string className, string windowText);

            /// <summary>
            /// ウィンドウタイトルを取得
            /// </summary>
            /// <param name="hWnd">ウィンドウハンドル</param>
            /// <param name="lpString">取得文字列</param>
            /// <param name="nMaxCount">取得最大文字列</param>
            /// <returns>ウィンドウタイトル</returns>
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int GetWindowText(IntPtr hWnd, string lpString, long nMaxCount);

            /// <summary>
            /// ウィンドウタイトル文字数
            /// </summary>
            /// <param name="hwnd">ウィンドウハンドル</param>
            /// <returns>ウィンドタイトル文字数</returns>
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int GetWindowTextLength(IntPtr hwnd);

            /// <summary>
            /// ウィンドウにメッセージ送信
            /// </summary>
            /// <param name="hWnd">ウィンドウハンドル</param>
            /// <param name="Msg">送信メッセージID</param>
            /// <param name="wParam">パラメータ1</param>
            /// <param name="lParam">パラメータ2</param>
            /// <returns>結果</returns>
            [DllImport("User32.dll", EntryPoint = "SendMessage")]
            public static extern int SendMessage(IntPtr hWnd, long Msg, long wParam, long lParam);
        }
        #endregion

        /// <summary>
        /// 表示した時のタイトル名称
        /// </summary>
        private static string OpenedTitle = string.Empty;

        /// <summary>
        /// ReCall実行フラグ
        /// </summary>
        public static bool IsRecall { get; private set; } = false;

        /// <summary>
        /// 表示中のタイトルをクローズします
        /// </summary>
        /// <param name="ownerHWnd">親ウィンドウハンドル</param>
        /// <returns>true:検索終了、false：検索継続</returns>
        public static bool Close(IntPtr ownerHWnd = default)
        {
            bool result = false;

            try
            {
                if (string.IsNullOrEmpty(OpenedTitle))
                {
                    return result;
                }

                if (ownerHWnd == default)
                {
                    ownerHWnd = IntPtr.Zero;
                }

                IntPtr hWnd = IntPtr.Zero;
                while (IntPtr.Zero != (hWnd = NativeMethods.FindWindowEx(ownerHWnd, hWnd, null, null)))
                {
                    // タイトルを取得
                    int length = NativeMethods.GetWindowTextLength(hWnd);
                    string title = new('\0', length + 1);
                    _ = NativeMethods.GetWindowText(hWnd, title, title.Length);

                    if (title.Trim('\0') == OpenedTitle)
                    {
                        // タイトルが一致したらクローズする
                        // WM_SYSCOMMAND=0x0112 , SC_CLOSE=0xF060L
                        NativeMethods.SendMessage(hWnd, 0x0112, 0xF060L, 0);
                        result = true;
                        break;
                    }
                    else
                    {
                        if (Close(hWnd))
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return result;
        }

        /// <summary>
        /// 出力フォルダ選択
        /// </summary>
        /// <param name="DirectoryPath">初期参照先</param>
        /// <param name="FileName">出力ファイル</param>
        /// <returns>選択フォルダパス(キャンセルボタン押下時はnull)</returns>
        public static string GetDirectoryPath(string DirectoryPath, string FileName = "")
        {
            return GetDirectoryPath(null,
                                    DirectoryPath,
                                    FileName);
        }

        /// <summary>
        /// 出力フォルダ選択
        /// </summary>
        /// <param name="ownerWnd">親ウィンドウ</param>
        /// <param name="DirectoryPath">初期参照先</param>
        /// <param name="FileName">出力ファイル</param>
        /// <returns>選択フォルダパス(キャンセルボタン押下時はnull)</returns>
        public static string GetDirectoryPath(Window ownerWnd, string DirectoryPath, string FileName = "")
        {
            if (ownerWnd == null)
            {
                ownerWnd = Application.Current.MainWindow;
            }

            using var ofdlg = new CommonOpenFileDialog();

            // 指定先がフォルダの場合
            if (!string.IsNullOrEmpty(DirectoryPath) && Directory.Exists(DirectoryPath) && File.GetAttributes(DirectoryPath).HasFlag(FileAttributes.Directory))
            {
                ofdlg.InitialDirectory = DirectoryPath;
            }
            // 指定先がファイルの場合
            else if (File.Exists(DirectoryPath))
            {
                ofdlg.InitialDirectory = Path.GetDirectoryName(DirectoryPath);
            }

            ofdlg.Title = $"{FileName} {Application.Current.Resources["COM-00-S008-00"]}";
            OpenedTitle = ofdlg.Title;

            ofdlg.IsFolderPicker = true;

            if (ofdlg.ShowDialog(new WindowInteropHelper(ownerWnd).Handle) == CommonFileDialogResult.Ok)
            {
                return ofdlg.FileName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ファイル選択
        /// </summary>
        /// <param name="FilePath">初期参照先</param>
        /// <param name="FileName">選択ファイル</param>
        /// <returns>選択ファイルパス(キャンセルボタン押下時はnull)</returns>
        public static string GetFilePath(string FilePath, string FileName = "")
        {
            return GetFilePath(Application.Current.MainWindow,
                               FilePath,
                               FileName);
        }

        /// <summary>
        /// ファイル選択
        /// </summary>
        /// <param name="ownerWnd">親ウィンドウ</param>
        /// <param name="FilePath">初期参照先</param>
        /// <param name="FileName">選択ファイル</param>
        /// <returns>選択ファイルパス(キャンセルボタン押下時はnull)</returns>
        public static string GetFilePath(Window ownerWnd, string FilePath, string FileName = "")
        {
            var dialog = new OpenFileDialog();

            //指定先がフォルダの場合
            if (!string.IsNullOrEmpty(FilePath) && File.Exists(FilePath) && File.GetAttributes(FilePath).HasFlag(FileAttributes.Directory))
            {
                dialog.InitialDirectory = FilePath;
            }
            //指定先がファイルの場合
            else
            {
                dialog.InitialDirectory = Path.GetDirectoryName(FilePath);
            }

            dialog.Title = $"{FileName} 読み込み";
            OpenedTitle = dialog.Title;

            dialog.Filter = $"すべてのファイル(*.*)|*.*";

            if (dialog.ShowDialog(ownerWnd) == true)
            {
                IsRecall = true;

                return dialog.FileName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ファイル保存
        /// </summary>
        /// <param name="FilePath">初期参照先</param>
        /// <param name="FileName">選択ファイル</param>
        /// <returns>選択ファイルパス(キャンセルボタン押下時はnull)</returns>
        public static string GetSaveFilePath(string FilePath, string FileName = "")
        {
            return GetSaveFilePath(Application.Current.MainWindow,
                                   FilePath,
                                   FileName);
        }
        /// <summary>
        /// ファイル保存
        /// </summary>
        /// <param name="ownerWnd">親ウィンドウ</param>
        /// <param name="FilePath">初期参照先</param>
        /// <param name="FileName">選択ファイル</param>
        /// <returns>選択ファイルパス(キャンセルボタン押下時はnull)</returns>
        public static string GetSaveFilePath(Window ownerWnd, string FilePath, string FileName = "")
        {
            var dialog = new SaveFileDialog();

            //指定先がフォルダの場合
            if (!string.IsNullOrEmpty(FilePath) && File.GetAttributes(FilePath).HasFlag(FileAttributes.Directory))
            {
                dialog.InitialDirectory = FilePath;
            }
            //指定先がファイルの場合
            else
            {
                dialog.InitialDirectory = Path.GetDirectoryName(FilePath);
            }

            dialog.Title = $"保存先";
            OpenedTitle = dialog.Title;

            dialog.Filter = $"保存ファイル名(*.CSV)|*.CSV";

            dialog.FileName = FileName;

            if (dialog.ShowDialog(ownerWnd) == true)
            {
                return dialog.FileName;
            }
            else
            {
                return null;
            }
        }
    }
}
