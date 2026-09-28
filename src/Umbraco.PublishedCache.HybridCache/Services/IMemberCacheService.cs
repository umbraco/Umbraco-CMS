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
}
