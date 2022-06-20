using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

public static class DomainCacheExtensions
{
    public static bool GetAssignedWithCulture(this IDomainCache domainCache, string? culture, int documentId,
        bool includeWildcards = false)
    {
        IEnumerable<Domain> assigned = domainCache.GetAssigned(documentId, includeWildcards);

        // It's super important that we always compare cultures with ignore case, since we can't be sure of the casing!
        // Comparing with string.IsNullOrEmpty since both empty string and null signifies invariant.
        return string.IsNullOrEmpty(culture)
            ? assigned.Any()
            : assigned.Any(x => x.Culture?.Equals(culture, StringComparison.InvariantCultureIgnoreCase) ?? false);
    }
}
