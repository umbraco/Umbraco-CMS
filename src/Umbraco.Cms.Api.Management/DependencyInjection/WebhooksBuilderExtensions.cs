using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Api.Common.Accessors;
using Umbraco.Cms.Api.Common.Rendering;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Webhook;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class WebhooksBuilderExtensions
{
    internal static IUmbracoBuilder AddWebhooks(this IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IWebhookPresentationFactory, WebhookPresentationFactory>();
        builder.AddMapDefinition<WebhookEventMapDefinition>();

        // We have to use TryAdd here, as if they are registered by the delivery API, we don't want to register them
        // Delivery API will also overwrite these IF it is enabled.
        builder.Services.TryAddScoped<IOutputExpansionStrategy, ElementOnlyOutputExpansionStrategy>();
        builder.Services.TryAddSingleton<IOutputExpansionStrategyAccessor, RequestContextOutputExpansionStrategyAccessor>();

        return builder;
    }
}
