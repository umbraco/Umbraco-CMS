using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Default tag provider that tags cached Delivery API responses with the content type alias.
/// </summary>
internal sealed class ContentTypeDeliveryApiOutputCacheTagProvider : IDeliveryApiOutputCacheTagProvider
{
    /// <inheritdoc />
    public IEnumerable<string> GetTags(IPublishedContent content)
    {
        yield return Constants.DeliveryApi.OutputCache.ContentTypeTagPrefix + content.ContentType.Alias;
    }
}
