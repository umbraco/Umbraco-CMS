using LightInject;
using Umbraco.Web.Install;
using Umbraco.Web.Install.InstallSteps;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Composing.Composers
{
    public static class InstallerComposer
    {
        public static IServiceRegistry ComposeInstaller(this IServiceRegistry registry)
        {
            //register the installer steps in order
            registry.RegisterOrdered(typeof(InstallSetupStep),
                new[]
                {
                    typeof(NewInstallStep),
                    typeof(UpgradeStep),
                    typeof(FilePermissionsStep),
                    typeof(ConfigureMachineKey),
                    typeof(DatabaseConfigureStep),
                    typeof(DatabaseInstallStep),
                    typeof(DatabaseUpgradeStep),

                    //TODO: Add these back once we have a compatible starter kit
                    //typeof(StarterKitDownloadStep),
                    //typeof(StarterKitInstallStep),
                    //typeof(StarterKitCleanupStep),

                    typeof(SetUmbracoVersionStep)
                }, type => new PerScopeLifetime());

            registry.Register<InstallStepCollection>();
            registry.Register<InstallHelper>();

            return registry;
        }
    }
}
