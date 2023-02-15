using Prism.Regions;
using System;

namespace CreateLearningImage.Core.Mvvm
{
    /// <summary>
    /// ページ切替用ViewModel
    /// </summary>
    public class RegionViewModelBase : DisposableBindableViewModelBase, INavigationAware, IConfirmNavigationRequest
    {
        /// <summary>
        /// マネージャ
        /// </summary>
        protected IRegionManager RegionManager { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="regionManager"></param>
        public RegionViewModelBase(IRegionManager regionManager)
        {
            RegionManager = regionManager;
        }

        /// <summary>
        /// ナビゲーションビュー登録処理
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <param name="continuationCallback"></param>
        public virtual void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            continuationCallback(true);
        }

        /// <summary>
        /// ナビゲーションターゲットかどうか
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns></returns>
        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// 前のナビゲーション
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        /// <summary>
        /// 次のナビゲーション
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
    }
}
