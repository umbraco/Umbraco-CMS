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
            // composition.Services.AddScoped<InstallSetupStep,StarterKitDownloadStep>();
            // composition.Services.AddScoped<InstallSetupStep,StarterKitInstallStep>();
            // composition.Services.AddScoped<InstallSetupStep,StarterKitCleanupStep>();

            composition.Services.AddScoped<InstallSetupStep,CompleteInstallStep>();

            composition.Services.AddTransient<InstallStepCollection>();
            composition.RegisterUnique<InstallHelper>();

            return composition;
        }
    }
}
