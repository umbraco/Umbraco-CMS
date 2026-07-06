using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Search.DeliveryApi.Services;

namespace Umbraco.Cms.Search.DeliveryApi.DependencyInjection;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddDeliveryApiSearch(this IUmbracoBuilder builder)
    {
        // swap out the core query provider with a custom one based on the search abstractions
        builder.Services.AddSingleton<IApiContentQueryProvider, DeliveryApiContentQueryProvider>();

        // add a content indexer for Delivery API selectors, filters, sorters etc.
        builder.Services.AddTransient<IContentIndexer, DeliveryApiContentIndexer>();

        return builder;
    }
}
