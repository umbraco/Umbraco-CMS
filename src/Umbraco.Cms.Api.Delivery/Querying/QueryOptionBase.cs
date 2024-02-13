using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Querying;

public abstract class QueryOptionBase
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IRequestRoutingService _requestRoutingService;

    public QueryOptionBase(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IRequestRoutingService requestRoutingService)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _requestRoutingService = requestRoutingService;
    }

    protected Guid? GetGuidFromQuery(string queryStringValue)
    {
        if (queryStringValue.IsNullOrWhiteSpace())
        {
            return null;
        }

        if (Guid.TryParse(queryStringValue, out Guid id))
        {
            return id;
        }

        IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();

        // Check if the passed value is a path of a content item
        var contentRoute = _requestRoutingService.GetContentRoute(queryStringValue);
        IPublishedContent? contentItem = publishedSnapshot.Content?.GetByRoute(contentRoute);

        return contentItem?.Key;
    }
}
