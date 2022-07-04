using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.BackOfficeApi.Factories.Installer;
using Umbraco.Cms.BackOfficeApi.Mapping;
using Umbraco.Cms.BackOfficeApi.Services;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.NewInstallSteps;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Install.NewInstallSteps;

namespace Umbraco.Cms.BackOfficeApi.DependencyInjection;

public static class InstallerExtensions
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
            .Append<NewFilePermissionsStep>()
            .Append<NewTelemetryIdentifierStep>()
            .Append<NewDatabaseConfigureStep>()
            .Append<NewDatabaseInstallStep>()
            .Append<NewDatabaseUpgradeStep>()
            .Append<NewNewInstallStep>();

        return builder;
    }

    public static NewInstallStepCollectionBuilder InstallSteps(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<NewInstallStepCollectionBuilder>();
}
