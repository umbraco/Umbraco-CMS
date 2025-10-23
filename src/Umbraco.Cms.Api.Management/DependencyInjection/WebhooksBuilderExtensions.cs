using Microsoft.Extensions.DependencyInjection;
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

        // deliveryApi will overwrite these more basic ones.
        builder.Services.AddScoped<IOutputExpansionStrategy, ElementOnlyOutputExpansionStrategy>();
        builder.Services.AddSingleton<IOutputExpansionStrategyAccessor, RequestContextOutputExpansionStrategyAccessor>();

        return builder;
    }
}
