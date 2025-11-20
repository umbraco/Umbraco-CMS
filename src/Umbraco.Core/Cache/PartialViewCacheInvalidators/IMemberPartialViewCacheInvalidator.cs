namespace Umbraco.Cms.Core.Cache.PartialViewCacheInvalidators;

/// <summary>
/// Defines behaviours for clearing of cached partials views that are configured to be cached individually by member.
/// </summary>
public interface IMemberPartialViewCacheInvalidator
{
    /// <summary>
    /// Clears the partial view cache items for the specified member ids.
    /// </summary>
    /// <param name="memberIds">The member Ids to clear the cache for.</param>
    /// <remarks>
    /// Called from the <see cref="MemberCacheRefresher"/> when a member is saved or deleted.
    /// </remarks>
    void ClearPartialViewCacheItems(IEnumerable<int> memberIds);
}
