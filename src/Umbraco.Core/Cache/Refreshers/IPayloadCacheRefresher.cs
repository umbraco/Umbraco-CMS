namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     A cache refresher that supports refreshing cache based on a custom payload
/// </summary>
public interface IPayloadCacheRefresher<TPayload> : IJsonCacheRefresher
{
    /// <summary>
    ///     Refreshes, clears, etc... any cache based on the information provided in the payload
    /// </summary>
    /// <param name="payloads"></param>
    void Refresh(TPayload[] payloads);
}
