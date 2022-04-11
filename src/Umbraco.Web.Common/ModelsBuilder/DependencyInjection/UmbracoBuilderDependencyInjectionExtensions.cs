using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Cms.Web.Common.ModelsBuilder;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the common Umbraco functionality
    /// </summary>
    public static class UmbracoBuilderDependencyInjectionExtensions
    {
        /// <summary>
        /// Adds umbraco's embedded model builder support
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

            builder.AddInMemoryModelsRazorEngine();

            // TODO: I feel like we could just do builder.AddNotificationHandler<ModelsBuilderNotificationHandler>() and it
            // would automatically just register for all implemented INotificationHandler{T}?
            builder.AddNotificationHandler<TemplateSavingNotification, ModelsBuilderNotificationHandler>();
            builder.AddNotificationHandler<ServerVariablesParsingNotification, ModelsBuilderNotificationHandler>();
            builder.AddNotificationHandler<ModelBindingErrorNotification, ModelsBuilderNotificationHandler>();
            builder.AddNotificationHandler<UmbracoApplicationStartingNotification, AutoModelsNotificationHandler>();
            builder.AddNotificationHandler<UmbracoRequestEndNotification, AutoModelsNotificationHandler>();
            builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, AutoModelsNotificationHandler>();
            builder.AddNotificationHandler<DataTypeCacheRefresherNotification, AutoModelsNotificationHandler>();

            builder.Services.AddSingleton<ModelsGenerator>();
            builder.Services.AddSingleton<OutOfDateModelsStatus>();
            builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, OutOfDateModelsStatus>();
            builder.AddNotificationHandler<DataTypeCacheRefresherNotification, OutOfDateModelsStatus>();
            builder.Services.AddSingleton<ModelsGenerationError>();

            builder.Services.AddSingleton<InMemoryModelFactory>();

            // This is what the community MB would replace, all of the above services are fine to be registered
            // even if the community MB is in place.
            builder.Services.AddSingleton<IPublishedModelFactory>(factory =>
            {
                ModelsBuilderSettings config = factory.GetRequiredService<IOptions<ModelsBuilderSettings>>().Value;
                if (config.ModelsMode == ModelsMode.InMemoryAuto)
                {
                    return factory.GetRequiredService<InMemoryModelFactory>();
                }
                else
                {
                    return factory.CreateDefaultPublishedModelFactory();
                }
            });


            if (!builder.Services.Any(x => x.ServiceType == typeof(IModelsBuilderDashboardProvider)))
            {
                builder.Services.AddUnique<IModelsBuilderDashboardProvider, NoopModelsBuilderDashboardProvider>();
            }

            return builder;
        }

        /// <remarks>
        /// See notes in <see cref="RefreshingRazorViewEngine"/>
        /// </remarks>
        private static IUmbracoBuilder AddInMemoryModelsRazorEngine(this IUmbracoBuilder builder)
        {
            // Replace the default with our custom engine
            builder.Services.AddUnique<IRazorViewEngine, RefreshingRazorViewEngine>();

            // Inner IRazorViewEngine for RefreshingRazorViewEngine
            builder.Services.AddSingleton<InvalidatableRazorViewEngine>();

            return builder;
        }
    }
}
