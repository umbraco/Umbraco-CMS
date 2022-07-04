using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.BackOfficeApi.Mapping.Installer;
using Umbraco.Cms.BackOfficeApi.Services.Installer;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.New.Cms.Core.Factories;
using Umbraco.New.Cms.Core.Installer;
using Umbraco.New.Cms.Core.Installer.Steps;
using Umbraco.New.Cms.Core.Services.Installer;
using Umbraco.New.Cms.Infrastructure.Factories.Installer;
using Umbraco.New.Cms.Infrastructure.Installer.Steps;

namespace Umbraco.Cms.BackOfficeApi.DependencyInjection;

public static class InstallerBuilderExtensions
{
    internal static IUmbracoBuilder AddNewInstaller(this IUmbracoBuilder builder)
    {
        IServiceCollection services = builder.Services;

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<InstallMapDefinition>()
            .Add<InstallSettingsMapDefinition>();

        services.AddTransient<IUserSettingsFactory, UserSettingsFactory>();
        services.AddTransient<IInstallSettingsFactory, InstallSettingsFactory>();
        services.AddTransient<IDatabaseSettingsFactory, DatabaseSettingsFactory>();

        builder.AddInstallSteps();
        services.AddScoped<IInstallService, InstallService>();

        return builder;
    }

    internal static IUmbracoBuilder AddInstallSteps(this IUmbracoBuilder builder)
    {
        builder.InstallSteps()
            .Append<FilePermissionsStep>()
            .Append<TelemetryIdentifierStep>()
            .Append<DatabaseConfigureStep>()
            .Append<DatabaseInstallStep>()
            .Append<DatabaseUpgradeStep>()
            .Append<InstallStep>();

        return builder;
    }

    public static NewInstallStepCollectionBuilder InstallSteps(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<NewInstallStepCollectionBuilder>();
}
