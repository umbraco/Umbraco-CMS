using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Install;
using Umbraco.Cms.Api.Management.Mapping.Installer;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Installer.Steps;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Installer;
using Umbraco.Cms.Infrastructure.Factories.Installer;
using Umbraco.Cms.Infrastructure.Installer.Steps;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class InstallerBuilderExtensions
{
    internal static IUmbracoBuilder AddInstaller(this IUmbracoBuilder builder)
    {
        IServiceCollection services = builder.Services;

        services.AddTransient<IUserSettingsFactory, UserSettingsFactory>();
        services.AddTransient<IInstallSettingsFactory, InstallSettingsFactory>();
        services.AddTransient<IDatabaseSettingsFactory, DatabaseSettingsFactory>();

        builder.AddInstallSteps();
        services.AddTransient<IInstallService, InstallService>();
        builder.AddNotificationAsyncHandler<UnattendedInstallNotification, CreateUnattendedUserNotificationHandler>();
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<InstallerViewModelsMapDefinition>();

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
            .Append<RestartRuntimeStep>();

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
}
