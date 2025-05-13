using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PartialViewCacheInvalidators;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Cache.PartialViewCacheInvalidators;

public class MemberPartialViewCacheInvalidator : IMemberPartialViewCacheInvalidator
{
    private readonly AppCaches _appCaches;

    public MemberPartialViewCacheInvalidator(
        AppCaches appCaches)
    {
        _appCaches = appCaches;
    }

    /// by default, we only remove cached partial views that were cached for the specified member.
    /// partial view cache keys follow the following format
    /// [] is optional or only added if the information is available
    /// {} is a parameter
    /// "Umbraco.Web.PartialViewCacheKey{partialViewName}-[{currentThreadCultureName}-][m{memberId}-][c{contextualKey}-]"
    /// see <see cref="HtmlHelperRenderExtensions.CachedPartialAsync"/> for more information
    public void ClearPartialViewCacheItems(IEnumerable<int> updatedMemberIds)
    {
        foreach (var memberId in updatedMemberIds)
        {
            _appCaches.RuntimeCache.ClearByRegex($"{CoreCacheHelperExtensions.PartialViewCacheKey}.*-m{memberId}-*");
        }
    }
}
