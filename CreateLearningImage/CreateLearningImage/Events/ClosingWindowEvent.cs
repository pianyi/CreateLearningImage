using Prism.Events;

namespace CreateLearningImage.Events
{
    /// <summary>
    /// 画面が閉じて良いかどうかのイベント
    /// </summary>
    /// <typeparam name="CancelEventArgs"></typeparam>
    class ClosingWindowEvent<CancelEventArgs> : PubSubEvent<CancelEventArgs>
    {
    }
}
