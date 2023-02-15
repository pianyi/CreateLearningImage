using CreateLearningImage.Core;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace CreateLearningImage.Modules.Main
{
    public class ModuleMainModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public ModuleMainModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate(Constants.ContentRegion, nameof(Views.Main));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Views.Main>();
        }
    }
}