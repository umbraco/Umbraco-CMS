using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Tags cached pages with their content type alias, enabling eviction by content type.
/// </summary>
internal sealed class ContentTypeOutputCacheTagProvider : IWebsiteOutputCacheTagProvider
{
    /// <inheritdoc />
    public IEnumerable<string> GetTags(IPublishedContent content)
    {
        yield return Constants.Website.OutputCache.ContentTypeTagPrefix + content.ContentType.Alias;
    }
}
