using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Web.Install;
using Umbraco.Web.Install.InstallSteps;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Composing.CompositionExtensions
{
    public static class Installer
    {
        public static IUmbracoBuilder ComposeInstaller(this IUmbracoBuilder builder)
        {
            // register the installer steps

            builder.Services.AddScoped<InstallSetupStep,NewInstallStep>();
            builder.Services.AddScoped<InstallSetupStep,UpgradeStep>();
            builder.Services.AddScoped<InstallSetupStep,FilePermissionsStep>();
            builder.Services.AddScoped<InstallSetupStep,DatabaseConfigureStep>();
            builder.Services.AddScoped<InstallSetupStep,DatabaseInstallStep>();
            builder.Services.AddScoped<InstallSetupStep,DatabaseUpgradeStep>();

            // TODO: Add these back once we have a compatible Starter kit
            // composition.Services.AddScoped<InstallSetupStep,StarterKitDownloadStep>();
            // composition.Services.AddScoped<InstallSetupStep,StarterKitInstallStep>();
            // composition.Services.AddScoped<InstallSetupStep,StarterKitCleanupStep>();

            builder.Services.AddScoped<InstallSetupStep,CompleteInstallStep>();

            builder.Services.AddTransient<InstallStepCollection>();
            builder.Services.AddUnique<InstallHelper>();

            return builder;
        }
    }
}
