using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Default implementation of <see cref="IWebsiteOutputCacheTagProvider"/> that tags cached pages
///     with their content type alias.
/// </summary>
internal sealed class DefaultWebsiteOutputCacheTagProvider : IWebsiteOutputCacheTagProvider
{
    /// <inheritdoc />
    public IEnumerable<string> GetTags(IPublishedContent content)
    {
        yield return Constants.Website.OutputCache.ContentTypeTagPrefix + content.ContentType.Alias;
    }
}
