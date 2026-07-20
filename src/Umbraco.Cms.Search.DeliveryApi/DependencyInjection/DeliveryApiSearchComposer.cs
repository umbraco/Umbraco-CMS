using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.DependencyInjection;

namespace Umbraco.Cms.Search.DeliveryApi.DependencyInjection;

public sealed class DeliveryApiSearchComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Only add Delivery API querying when the Delivery API is actually composed (AddDeliveryApi() registers
        // IApiContentQueryService) - otherwise the DeliveryApiContentIndexer would needlessly add Delivery API
        // fields to the published content index.
        if (builder.Services.Any(s => s.ServiceType == typeof(IApiContentQueryService)) is false)
        {
            return;
        }

        builder
            .AddSearchCore()
            .AddDeliveryApiSearch();
    }
}
