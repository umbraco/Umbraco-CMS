namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     A cache refresher that supports refreshing or removing cache based on a custom Json payload
/// </summary>
public interface IJsonCacheRefresher : ICacheRefresher
{
    /// <summary>
    ///     Refreshes, clears, etc... any cache based on the information provided in the json
    /// </summary>
    /// <param name="json"></param>
    void Refresh(string json);

    /// <summary>
    /// Refreshes internal (isolated) caches by a json payload.
    /// </summary>
    /// <param name="json">The json payload.</param>
    /// <remarks>
    /// Runs inline in repository reads, possibly while the caller holds distributed locks. Implementations must only
    /// clear in-memory state: no distributed locks, no database access and no published-cache work. Anything that
    /// needs those belongs in <see cref="Refresh(string)"/>, which the background synchronization runs. The default
    /// implementation delegates to <see cref="Refresh(string)"/>, so a refresher whose <c>Refresh</c> does more than
    /// clear in-memory state should override this member.
    /// </remarks>
    void RefreshInternal(string json) => Refresh(json);
}
