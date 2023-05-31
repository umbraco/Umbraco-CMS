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
}
