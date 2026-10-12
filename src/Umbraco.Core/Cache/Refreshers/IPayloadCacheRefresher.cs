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

    /// <summary>
    /// Refreshes internal (isolated) caches by a payload.
    /// </summary>
    /// <param name="payloads">The payload.</param>
    /// <remarks>
    /// Subject to the same contract as <see cref="IJsonCacheRefresher.RefreshInternal(string)"/>: in-memory clears only,
    /// with no distributed locks, database access or published-cache work.
    /// </remarks>
    void RefreshInternal(TPayload[] payloads) => Refresh(payloads);
}
