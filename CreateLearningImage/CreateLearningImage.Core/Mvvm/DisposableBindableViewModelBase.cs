using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Reactive.Disposables;

namespace CreateLearningImage.Core.Mvvm
{
    /// <summary>
    /// オブジェクトの破棄機能を追加したBindableBaseクラス
    /// </summary>
    public abstract class DisposableBindableViewModelBase : BindableBase, IDestructible, IDisposable
    {
        /// <summary>
        /// オブジェクト破棄時に親が一致するかを判断するための文字列を取得します
        /// </summary>
        /// <returns>nameofを使った画面文字列</returns>
        protected virtual string ParentCloseDialogControl()
        {
            return string.Empty;
        }

        /// <summary>
        /// IDisposableなオブジェクトを一括でDisposeします。
        /// </summary>
        protected CompositeDisposable Disposables = new();

        /// <summary>
        /// 重複する呼び出しを検出する
        /// </summary>
        protected bool disposedValue = false;

        /// <summary>
        /// オブジェクトの破棄処理
        /// </summary>
        /// <param name="disposing">処理フラグ</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Disposables.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// オブジェクトの破棄処理
        /// </summary>
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 対応したオブジェクトの場合に破棄します
        /// </summary>
        /// <param name="type"></param>
        public void Dispose(string type)
        {
            if (string.IsNullOrEmpty(ParentCloseDialogControl()) ||
                ParentCloseDialogControl() == type)
            {
                Dispose();
            }
        }

        /// <summary>
        /// ViewModelを破棄します。
        /// </summary>
        public void Destroy() => this.Dispose();
    }
}
