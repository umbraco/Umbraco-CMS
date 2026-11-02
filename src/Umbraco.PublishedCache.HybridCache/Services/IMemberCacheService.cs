using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
/// Defines a service for mapping member entities to published members.
/// </summary>
public interface IMemberCacheService
{
    /// <summary>
    /// Gets the published member for the given member entity.
    /// </summary>
    /// <param name="member">The member entity.</param>
    /// <returns>The published member, or <c>null</c> if not found.</returns>
    Task<IPublishedMember?> Get(IMember member);

    /// <summary>
    /// Does nothing. Members are mapped from the member entity when read, rather than served from the
    /// published cache database table, so there is nothing to rebuild.
    /// </summary>
    /// <param name="contentTypeIds">The member type ids. Ignored.</param>
    [Obsolete("Members are not stored in the published cache database table, so this does nothing. Scheduled for removal in Umbraco 19.")]
    void Rebuild(IReadOnlyCollection<int> contentTypeIds)
    {
    }
}
