using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Install.InstallSteps;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Install.InstallSteps;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds the services for the Umbraco installer
    /// </summary>
    internal static IUmbracoBuilder AddInstaller(this IUmbracoBuilder builder)
    {
        // register the installer steps
        builder.Services.AddScoped<InstallSetupStep, NewInstallStep>();
        builder.Services.AddScoped<InstallSetupStep, UpgradeStep>();
        builder.Services.AddScoped<InstallSetupStep, FilePermissionsStep>();
        builder.Services.AddScoped<InstallSetupStep, TelemetryIdentifierStep>(provider =>
        {
            return new TelemetryIdentifierStep(
                provider.GetRequiredService<IOptions<GlobalSettings>>(),
                provider.GetRequiredService<ISiteIdentifierService>());
        });
        builder.Services.AddScoped<InstallSetupStep, DatabaseConfigureStep>();
        builder.Services.AddScoped<InstallSetupStep, DatabaseInstallStep>();
        builder.Services.AddScoped<InstallSetupStep, DatabaseUpgradeStep>();

        builder.Services.AddScoped<InstallSetupStep, CompleteInstallStep>();

        builder.Services.AddTransient<InstallStepCollection>();
        builder.Services.AddSingleton<InstallHelper>();

        builder.Services.AddTransient<PackageMigrationRunner>();

        return builder;
    }
}
