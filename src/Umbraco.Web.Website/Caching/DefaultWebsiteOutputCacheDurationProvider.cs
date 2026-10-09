using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Default implementation of <see cref="IWebsiteOutputCacheDurationProvider"/> that always returns <c>null</c>,
///     deferring to the configured <c>ContentDuration</c> for all content.
/// </summary>
internal sealed class DefaultWebsiteOutputCacheDurationProvider : IWebsiteOutputCacheDurationProvider
{
    /// <inheritdoc />
    public TimeSpan? GetDuration(IPublishedContent content) => null;
}
