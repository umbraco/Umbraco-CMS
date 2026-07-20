using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Tags cached pages for delivery API output caching with their content type alias, enabling eviction by content type.
/// </summary>
internal sealed class DeliveryApiContentTypeOutputCacheTagProvider : IDeliveryApiOutputCacheTagProvider
{
    /// <inheritdoc />
    public IEnumerable<string> GetTags(IPublishedContent content)
    {
        yield return Constants.DeliveryApi.OutputCache.ContentTypeTagPrefix + content.ContentType.Alias;
    }
}
