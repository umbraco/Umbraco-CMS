namespace Umbraco.Cms.Core.Sync;

/// <summary>
///     Retrieve the <see cref="SyncBootState" /> for the application during startup
/// </summary>
public interface ISyncBootStateAccessor
{
    /// <summary>
    ///     Get the <see cref="SyncBootState" />
    /// </summary>
    /// <returns></returns>
    SyncBootState GetSyncBootState();
}
