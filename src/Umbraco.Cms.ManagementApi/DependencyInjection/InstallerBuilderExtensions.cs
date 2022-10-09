using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.Mapping.Dictionary;
using Umbraco.Cms.ManagementApi.Mapping.Installer;
using Umbraco.Cms.ManagementApi.Services.Entities;
using Umbraco.Cms.ManagementApi.Services.Paging;
using Umbraco.New.Cms.Core.Factories;
using Umbraco.New.Cms.Core.Installer;
using Umbraco.New.Cms.Core.Installer.Steps;
using Umbraco.New.Cms.Core.Services.Installer;
using Umbraco.New.Cms.Infrastructure.Factories.Installer;
using Umbraco.New.Cms.Infrastructure.Installer.Steps;
using Umbraco.New.Cms.Web.Common.Installer;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class InstallerBuilderExtensions
{
    internal static IUmbracoBuilder AddNewInstaller(this IUmbracoBuilder builder)
    {
        IServiceCollection services = builder.Services;

        services.AddTransient<IUserSettingsFactory, UserSettingsFactory>();
        services.AddTransient<IInstallSettingsFactory, InstallSettingsFactory>();
        services.AddTransient<IDatabaseSettingsFactory, DatabaseSettingsFactory>();

        builder.AddInstallSteps();
        services.AddTransient<IInstallService, InstallService>();

        return builder;
    }

    internal static IUmbracoBuilder AddUpgrader(this IUmbracoBuilder builder)
    {
        IServiceCollection services = builder.Services;

        services.AddTransient<IUpgradeSettingsFactory, UpgradeSettingsFactory>();
        builder.AddUpgradeSteps();
        services.AddTransient<IUpgradeService, UpgradeService>();

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
            .Append<CreateUserStep>()
            .Append<RegisterInstallCompleteStep>()
            .Append<RestartRuntimeStep>()
            .Append<SignInUserStep>();

        return builder;
    }

    public static NewInstallStepCollectionBuilder InstallSteps(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<NewInstallStepCollectionBuilder>();

    internal static IUmbracoBuilder AddUpgradeSteps(this IUmbracoBuilder builder)
    {
        builder.UpgradeSteps()
            .Append<FilePermissionsStep>()
            .Append<TelemetryIdentifierStep>()
            .Append<DatabaseInstallStep>()
            .Append<DatabaseUpgradeStep>()
            .Append<RegisterInstallCompleteStep>()
            .Append<RestartRuntimeStep>();

        return builder;
    }

    public static UpgradeStepCollectionBuilder UpgradeSteps(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<UpgradeStepCollectionBuilder>();

    internal static IUmbracoBuilder AddTrees(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IUserStartNodeEntitiesService, UserStartNodeEntitiesService>();
        return builder;
    }
}
