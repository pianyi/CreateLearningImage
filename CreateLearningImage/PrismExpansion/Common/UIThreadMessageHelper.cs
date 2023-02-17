using System.Windows.Threading;

namespace PrismExpansion.Common
{
    /// <summary>
    /// UIスレッドに関するクラス
    /// </summary>
    public static class UIThreadMessageHelper
    {
        /// <summary>
        /// 現在メッセージ待ち行列の中にある全てのUIメッセージを処理します。
        /// </summary>
        /// <remarks>VBのDoEventsと同じ。本来は別スレッド化で対応するため、可能な限り使わない方が良い。</remarks>
        public static void DoEvents()
        {
            DispatcherFrame frame = new();
            var callback = new DispatcherOperationCallback(obj =>
            {
                ((DispatcherFrame)obj).Continue = false;
                return null;
            });
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);
        }
    }
}
