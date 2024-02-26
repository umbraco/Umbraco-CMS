using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Web.BackOffice.Mapping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class WebhookBuilderExtensions
{
    internal static IUmbracoBuilder AddWebhooks(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<WebhookMapDefinition>();
        builder.Services.AddUnique<IWebhookPresentationFactory, WebhookPresentationFactory>();

        return builder;
    }
}
