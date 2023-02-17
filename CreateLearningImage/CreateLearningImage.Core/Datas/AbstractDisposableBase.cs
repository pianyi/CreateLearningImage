using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Reactive.Disposables;

namespace CreateLearningImage.Core.Datas
{
    /// <summary>
    /// データオブジェクトの破棄ベースクラス
    /// </summary>
    public abstract class AbstractDisposableBase : BindableBase, IDestructible, IDisposable
    {
        #region Dispose系
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
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// オブジェクトの破棄処理
        /// </summary>
        public void Destroy()
        {
            Dispose();
        }
        #endregion Dispose系
    }
}
