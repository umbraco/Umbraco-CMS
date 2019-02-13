using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Web.Install;
using Umbraco.Web.Install.InstallSteps;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Composing.Composers
{
    public static class InstallerComposer
    {
        public static Composition ComposeInstaller(this Composition composition)
        {
            // register the installer steps

            composition.Register<NewInstallStep>(Lifetime.Scope);
            composition.Register<UpgradeStep>(Lifetime.Scope);
            composition.Register<FilePermissionsStep>(Lifetime.Scope);
            composition.Register<ConfigureMachineKey>(Lifetime.Scope);
            composition.Register<DatabaseConfigureStep>(Lifetime.Scope);
            composition.Register<DatabaseInstallStep>(Lifetime.Scope);
            composition.Register<DatabaseUpgradeStep>(Lifetime.Scope);

            composition.Register<StarterKitDownloadStep>(Lifetime.Scope);
            composition.Register<StarterKitInstallStep>(Lifetime.Scope);
            composition.Register<StarterKitCleanupStep>(Lifetime.Scope);

            composition.Register<SetUmbracoVersionStep>(Lifetime.Scope);

            composition.Register<InstallStepCollection>();
            composition.Register<InstallHelper>();

            return composition;
        }
    }
}
