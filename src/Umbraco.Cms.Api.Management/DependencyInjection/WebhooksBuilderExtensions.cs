using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class WebhooksBuilderExtensions
{
    internal static IUmbracoBuilder AddWebhooks(this IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IWebhookPresentationFactory, WebhookPresentationFactory>();

        return builder;
    }
}
