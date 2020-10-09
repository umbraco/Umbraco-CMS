using Microsoft.Extensions.DependencyInjection;
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

            composition.Services.AddScoped<InstallSetupStep,NewInstallStep>();
            composition.Services.AddScoped<InstallSetupStep,UpgradeStep>();
            composition.Services.AddScoped<InstallSetupStep,FilePermissionsStep>();
            composition.Services.AddScoped<InstallSetupStep,DatabaseConfigureStep>();
            composition.Services.AddScoped<InstallSetupStep,DatabaseInstallStep>();
            composition.Services.AddScoped<InstallSetupStep,DatabaseUpgradeStep>();

            // TODO: Add these back once we have a compatible Starter kit
            // composition.Register<StarterKitDownloadStep>(Lifetime.Scope);
            // composition.Register<StarterKitInstallStep>(Lifetime.Scope);
            // composition.Register<StarterKitCleanupStep>(Lifetime.Scope);

            composition.Services.AddScoped<InstallSetupStep,CompleteInstallStep>();

            composition.Services.AddTransient<InstallStepCollection>();
            composition.Services.AddUnique<InstallHelper>();

            return composition;
        }
    }
}
