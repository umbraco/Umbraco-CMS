using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Search.DeliveryApi.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.DeliveryApi.DependencyInjection;

public static class UmbracoBuilderExtensions
{
    /// <summary>
    /// Enables Umbraco Search as the querying engine for the Delivery API.
    /// </summary>
    /// <remarks>
    /// This method is idempotent - calling it multiple times has no effect after the first call.
    /// </remarks>
    public static IUmbracoBuilder AddDeliveryApiSearch(this IUmbracoBuilder builder)
    {
        // Idempotency check - safe to call multiple times.
        if (builder.Services.Any(s => s.ServiceType == typeof(AddDeliveryApiSearchMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<AddDeliveryApiSearchMarker>();

        // swap out the core query provider with a custom one based on the search abstractions
        builder.Services.AddUnique<IApiContentQueryProvider, DeliveryApiContentQueryProvider>();

        // add a content indexer for Delivery API selectors, filters, sorters etc.
        builder.Services.AddTransient<IContentIndexer, DeliveryApiContentIndexer>();

        return builder;
    }

    private sealed class AddDeliveryApiSearchMarker
    {
    }
}
