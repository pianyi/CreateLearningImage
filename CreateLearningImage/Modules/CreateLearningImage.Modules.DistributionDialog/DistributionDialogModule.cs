using Prism.Ioc;
using Prism.Modularity;

namespace CreateLearningImage.Modules.DistributionDialog
{
    public class DistributionDialogModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Views.DistributionDialog>();
        }
    }
}