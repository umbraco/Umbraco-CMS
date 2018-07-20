using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web.Install;
using Umbraco.Web.Install.InstallSteps;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Composing.Composers
{
    public static class InstallerComposer
    {
        public static IContainer ComposeInstaller(this IContainer container)
        {
            //register the installer steps in order
            container.RegisterOrdered(typeof(InstallSetupStep),
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
                }, Lifetime.Scope);

            container.Register<InstallStepCollection>();
            container.Register<InstallHelper>();

            return container;
        }
    }
}
