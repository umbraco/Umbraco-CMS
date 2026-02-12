using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Options;
using Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;
using Umbraco.Cms.Web.Common.ModelsBuilder;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> for the common Umbraco functionality
/// </summary>
public static class UmbracoBuilderDependencyInjectionExtensions
{
    /// <summary>
    ///     Adds umbraco's embedded model builder support
    /// </summary>
    public static IUmbracoBuilder AddModelsBuilder(this IUmbracoBuilder builder)
    {
        var umbServices = new UniqueServiceDescriptor(typeof(UmbracoServices), typeof(UmbracoServices), ServiceLifetime.Singleton);
        if (builder.Services.Contains(umbServices))
        {
            // if this ext method is called more than once just exit
            return builder;
        }

        builder.Services.Add(umbServices);

        if (builder.Config.GetRuntimeMode() != RuntimeMode.Production)
        {
            // Configure service to allow models generation
            builder.AddNotificationHandler<ServerVariablesParsingNotification, ModelsBuilderNotificationHandler>();
            builder.AddNotificationHandler<TemplateSavingNotification, ModelsBuilderNotificationHandler>();

            builder.AddNotificationHandler<UmbracoApplicationStartingNotification, AutoModelsNotificationHandler>();
            builder.AddNotificationHandler<UmbracoRequestEndNotification, AutoModelsNotificationHandler>();
            builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, AutoModelsNotificationHandler>();
            builder.AddNotificationHandler<DataTypeCacheRefresherNotification, AutoModelsNotificationHandler>();

            builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, OutOfDateModelsStatus>();
            builder.AddNotificationHandler<DataTypeCacheRefresherNotification, OutOfDateModelsStatus>();
        }

        builder.Services.TryAddSingleton<IModelsBuilderDashboardProvider, NoopModelsBuilderDashboardProvider>();

        // Register required services for ModelsBuilderDashboardController
        builder.Services.AddSingleton<IModelsGenerator, ModelsGenerator>();

        // TODO: Remove in v13 - this is only here in case someone is already using this generator directly
        builder.Services.AddSingleton<ModelsGenerator>();
        builder.Services.AddSingleton<OutOfDateModelsStatus>();
        builder.Services.AddSingleton<ModelsGenerationError>();

        builder.Services.ConfigureOptions<ConfigurePropertySettingsOptions>();

        builder.AddNotificationHandler<UmbracoApplicationStartedNotification, RazorRuntimeCompilationValidator>();

        return builder;
    }


}
