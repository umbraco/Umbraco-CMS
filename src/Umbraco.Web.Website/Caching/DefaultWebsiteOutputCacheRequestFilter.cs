using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Default implementation of <see cref="IWebsiteOutputCacheRequestFilter"/> that prevents caching
///     for preview mode and authenticated member requests.
/// </summary>
public class DefaultWebsiteOutputCacheRequestFilter : IWebsiteOutputCacheRequestFilter
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultWebsiteOutputCacheRequestFilter"/> class.
    /// </summary>
    /// <param name="umbracoContextAccessor">The Umbraco context accessor.</param>
    public DefaultWebsiteOutputCacheRequestFilter(IUmbracoContextAccessor umbracoContextAccessor)
        => _umbracoContextAccessor = umbracoContextAccessor;

    /// <inheritdoc />
    public virtual bool IsCacheable(HttpContext context, IPublishedContent content)
        => ShouldExcludePreview() is false && ShouldExcludeAuthenticated(context) is false;

    /// <summary>
    ///     Gets a value indicating whether preview mode requests should be excluded from caching.
    ///     By default returns <c>true</c> when the current request is in Umbraco preview mode.
    /// </summary>
    protected virtual bool ShouldExcludePreview()
        => _umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext)
           && umbracoContext.InPreviewMode;

    /// <summary>
    ///     Gets a value indicating whether authenticated requests should be excluded from caching.
    ///     By default returns <c>true</c> when the current request is from an authenticated user.
    /// </summary>
    protected virtual bool ShouldExcludeAuthenticated(HttpContext context)
        => context.User.Identity?.IsAuthenticated == true;
}
