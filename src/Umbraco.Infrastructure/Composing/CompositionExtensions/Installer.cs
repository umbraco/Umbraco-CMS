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

            composition.Services.AddScoped<NewInstallStep>();
            composition.Services.AddScoped<UpgradeStep>();
            composition.Services.AddScoped<FilePermissionsStep>();
            composition.Services.AddScoped<DatabaseConfigureStep>();
            composition.Services.AddScoped<DatabaseInstallStep>();
            composition.Services.AddScoped<DatabaseUpgradeStep>();

            // TODO: Add these back once we have a compatible Starter kit
            // composition.Register<StarterKitDownloadStep>(Lifetime.Scope);
            // composition.Register<StarterKitInstallStep>(Lifetime.Scope);
            // composition.Register<StarterKitCleanupStep>(Lifetime.Scope);

            composition.Services.AddScoped<CompleteInstallStep>();

            composition.Services.AddTransient<InstallStepCollection>();
            composition.Services.AddUnique<InstallHelper>();

            return composition;
        }
    }
}
