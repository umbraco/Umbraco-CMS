using System;
using System.Linq;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.PublishedCache
{
    public static class DomainCacheExtensions
    {
        public static bool GetAssignedWithCulture(this IDomainCache domainCache, string? culture, int documentId, bool includeWildcards = false)
        {
            var assigned = domainCache.GetAssigned(documentId, includeWildcards);

            // It's super important that we always compare cultures with ignore case, since we can't be sure of the casing!
            return culture is null ? assigned.Any() : assigned.Any(x => x.Culture?.Equals(culture, StringComparison.InvariantCultureIgnoreCase) ?? false);
        }
    }
}
