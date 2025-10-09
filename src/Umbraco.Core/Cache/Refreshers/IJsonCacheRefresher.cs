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
    void RefreshInternal(string json) => Refresh(json);
}
