using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Web.Install;
using Umbraco.Web.Install.InstallSteps;

namespace Umbraco.Web.Composing.Composers
{
    public static class InstallerComposer
    {
        public static Composition ComposeInstaller(this Composition composition)
        {
            var container = composition.Container;

            // register the installer steps

            container.Register<NewInstallStep>(Lifetime.Scope);
            container.Register<UpgradeStep>(Lifetime.Scope);
            container.Register<FilePermissionsStep>(Lifetime.Scope);
            container.Register<ConfigureMachineKey>(Lifetime.Scope);
            container.Register<DatabaseConfigureStep>(Lifetime.Scope);
            container.Register<DatabaseInstallStep>(Lifetime.Scope);
            container.Register<DatabaseUpgradeStep>(Lifetime.Scope);

            //TODO: Add these back once we have a compatible starter kit
            //container.Register<StarterKitDownloadStep>(Lifetime.Scope);
            //container.Register<StarterKitInstallStep>(Lifetime.Scope);
            //container.Register<StarterKitCleanupStep>(Lifetime.Scope);

            container.Register<SetUmbracoVersionStep>(Lifetime.Scope);

            container.Register<InstallStepCollection>();
            container.Register<InstallHelper>();

            return composition;
        }
    }
}
