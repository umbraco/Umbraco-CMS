using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web.Install;
using Umbraco.Web.Install.InstallSteps;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Composing.CompositionExtensions
{
    public static class Installer
    {
        public static Composition ComposeInstaller(this Composition composition)
        {
            // register the installer steps

            composition.Register<InstallSetupStep,NewInstallStep>(Lifetime.Scope);
            composition.Register<InstallSetupStep,UpgradeStep>(Lifetime.Scope);
            composition.Register<InstallSetupStep,FilePermissionsStep>(Lifetime.Scope);
            composition.Register<InstallSetupStep,DatabaseConfigureStep>(Lifetime.Scope);
            composition.Register<InstallSetupStep,DatabaseInstallStep>(Lifetime.Scope);
            composition.Register<InstallSetupStep,DatabaseUpgradeStep>(Lifetime.Scope);

            // TODO: Add these back once we have a compatible Starter kit
            // composition.Register<InstallSetupStep,StarterKitDownloadStep>(Lifetime.Scope);
            // composition.Register<InstallSetupStep,StarterKitInstallStep>(Lifetime.Scope);
            // composition.Register<InstallSetupStep,StarterKitCleanupStep>(Lifetime.Scope);

            composition.Register<InstallSetupStep,CompleteInstallStep>(Lifetime.Scope);

            composition.Register<InstallStepCollection>();
            composition.RegisterUnique<InstallHelper>();

            return composition;
        }
    }
}
