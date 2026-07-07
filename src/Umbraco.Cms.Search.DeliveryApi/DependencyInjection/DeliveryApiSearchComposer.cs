using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.DependencyInjection;

namespace Umbraco.Cms.Search.DeliveryApi.DependencyInjection;

public sealed class DeliveryApiSearchComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Only flip Delivery API querying when the Delivery API is actually composed (AddDeliveryApi() registers
        // the core IApiContentQueryProvider) - otherwise the DeliveryApiContentIndexer would needlessly add
        // Delivery API fields to the published content index.
        if (builder.Services.Any(s => s.ServiceType == typeof(IApiContentQueryProvider)) is false)
        {
            return;
        }

        // Temporary escape hatch: revert Delivery API querying to the legacy Examine based implementation.
        SearchSettings? searchSettings = builder.Config
            .GetSection(Umbraco.Cms.Core.Constants.Configuration.ConfigSearch)
            .Get<SearchSettings>();
        if (searchSettings?.UseLegacySearchServices is true)
        {
            return;
        }

        builder
            .AddSearchCore()
            .AddDeliveryApiSearch();
    }
}
