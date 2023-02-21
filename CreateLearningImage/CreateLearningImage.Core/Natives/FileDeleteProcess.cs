using System;
using System.Runtime.InteropServices;

namespace CreateLearningImage.Core.Natives
{
    /// <summary>
    /// ファイル操作ネイティブメソッド
    /// </summary>
    public class FileDeleteProcess
    {
        #region Win32API
        /// <summary>
        /// Win32APIクラス(使う箇所が増えたらちゃんと別クラスに整理する事
        /// </summary>
        private static class NativeMethods
        {
            /// <summary>
            /// ファイルをコピー・移動・削除・名前変更します。
            /// </summary>
            /// <param name="lpFileOp"></param>
            /// <returns>正常時0。異常時の値の意味は https://docs.microsoft.com/ja-jp/windows/win32/api/shellapi/nf-shellapi-shfileoperationa を参照。</returns>
            [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);
        }
        #endregion

        /// <summary>
        /// ファイル操作を開始します。
        /// </summary>
        /// <param name="fs">ファイル操作情報クラス</param>
        /// <returns>処理結果</returns>
        public static int StartFileOperation(ref FileDeleteProcess.SHFILEOPSTRUCT fs)
        {
            return NativeMethods.SHFileOperation(ref fs);
        }

        /// <summary>
        /// ファイル操作指示用構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)] public FileFuncFlags wFunc;
            [MarshalAs(UnmanagedType.LPWStr)] public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)] public string pTo;
            [MarshalAs(UnmanagedType.U2)] public FILEOP_FLAGS fFlags;
            [MarshalAs(UnmanagedType.Bool)] public bool ffAnyOperationsAborted;
            public IntPtr hNameMappings; //FOF_WANTMAPPINGHANDLEフラグとともに使用します。
            [MarshalAs(UnmanagedType.LPWStr)] public string lplpszProgressTitle; //FOF_SIMPLEPROGRESSフラグとともに使用します。
        }

        /// <summary>
        /// ファイル操作処理指定
        /// </summary>
        public enum FileFuncFlags
        {
            /// <summary>pFrom から pTo にファイルを移動します。</summary>
            FO_MOVE = 0x1,
            /// <summary>pFrom から pTo にファイルをコピーします。</summary>
            FO_COPY = 0x2,
            /// <summary>pFrom からファイルを削除します。</summary>
            FO_DELETE = 0x3,
            /// <summary>pFrom のファイルの名前を変更します。複数ファイルを対象とする場合は FO_MOVE を使用します。</summary>
            FO_RENAME = 0x4
        }

        /// <summary>
        /// ファイル操作動作
        /// </summary>
        [Flags]
        public enum FILEOP_FLAGS : short
        {
            /// <summary>pToにはpFromに１対１で対応する複数のコピー先を指定します。</summary>
            FOF_MULTIDESTFILES = 0x1,
            /// <summary>このフラグは使用しません。</summary>
            FOF_CONFIRMMOUSE = 0x2,
            /// <summary>進捗状況のダイアログを表示しません。</summary>
            FOF_SILENT = 0x4,
            /// <summary>同名のファイルが既に存在する場合、新しい名前を付けます。</summary>
            FOF_RENAMEONCOLLISION = 0x8,
            /// <summary>確認ダイアログを表示せず、すべて「はい」を選択したものとします。</summary>
            FOF_NOCONFIRMATION = 0x10,
            /// <summary>FOF_RENAMEONCOLLISIONフラグによるファイル名の衝突回避が発生した場合、SHFILEOPSTRUCT.hNameMappingsに新旧ファイル名の情報を格納します。この情報はSHFreeNameMappingsを使って開放する必要があります。</summary>
            FOF_WANTMAPPINGHANDLE = 0x20,
            /// <summary>可能であれば、操作を元に戻せるようにします。</summary>
            FOF_ALLOWUNDO = 0x40,
            /// <summary>ワイルドカードが使用された場合、ファイルのみを対象とします。</summary>
            FOF_FILESONLY = 0x80,
            /// <summary>進捗状況のダイアログを表示しますが、個々のファイル名は表示しません。</summary>
            FOF_SIMPLEPROGRESS = 0x100,
            /// <summary>新しいフォルダーの作成する前にユーザーに確認しません。</summary>
            FOF_NOCONFIRMMKDIR = 0x200,
            /// <summary>エラーが発生してもダイアログを表示しません。</summary>
            FOF_NOERRORUI = 0x400,
            /// <summary>ファイルのセキュリティ属性はコピーしません。コピー後のファイルはコピー先のフォルダーのセキュリティ属性を引き継ぎます。</summary>
            FOF_NOCOPYSECURITYATTRIBS = 0x800,
            /// <summary>サブディレクトリーを再帰的に処理しません。これは既定の動作です。</summary>
            FOF_NORECURSION = 0x1000,
            /// <summary>グループとして連結しているファイルは移動しません。指定されたファイルだけを移動します。</summary>
            FOF_NO_CONNECTED_ELEMENTS = 0x2000,
            /// <summary>ファイルが恒久的に削除される場合、警告を表示します。このフラグはFOF_NOCONFIRMATIONより優先されます。 </summary>
            FOF_WANTNUKEWARNING = 0x4000,
            /// <summary>UIを表示しません。</summary>
            FOF_NO_UI = FOF_SILENT | FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_NOCONFIRMMKDIR
        }
    }
}
