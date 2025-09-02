using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PartialViewCacheInvalidators;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Cache.PartialViewCacheInvalidators;

/// <summary>
/// Implementation of <see cref="IMemberPartialViewCacheInvalidator"/> that only remove cached partial views
/// that were cached for the specified member(s).
/// </summary>
public class MemberPartialViewCacheInvalidator : IMemberPartialViewCacheInvalidator
{
    private readonly AppCaches _appCaches;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberPartialViewCacheInvalidator"/> class.
    /// </summary>
    public MemberPartialViewCacheInvalidator(AppCaches appCaches) => _appCaches = appCaches;

    /// <inheritdoc/>
    /// <remarks>
    /// Partial view cache keys follow the following format:
    ///   [] is optional or only added if the information is available
    ///   {} is a parameter
    ///   "Umbraco.Web.PartialViewCacheKey{partialViewName}-[{currentThreadCultureName}-][m{memberId}-][c{contextualKey}-]"
    /// See <see cref="HtmlHelperRenderExtensions.CachedPartialAsync"/> for more information.
    /// </remarks>
    public void ClearPartialViewCacheItems(IEnumerable<int> memberIds)
    {
        foreach (var memberId in memberIds)
        {
            _appCaches.RuntimeCache.ClearByRegex($"{CoreCacheHelperExtensions.PartialViewCacheKey}.*-m{memberId}-*");
        }

        // Since it is possible to add a cache item linked to members without a member logged in, we should always clear these items.
        _appCaches.RuntimeCache.ClearByRegex($"{CoreCacheHelperExtensions.PartialViewCacheKey}.*-m-*");
    }
}
