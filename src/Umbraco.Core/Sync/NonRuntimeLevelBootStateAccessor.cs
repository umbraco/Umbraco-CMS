namespace Umbraco.Cms.Core.Sync;

/// <summary>
///     Boot state implementation for when umbraco is not in the run state
/// </summary>
public sealed class NonRuntimeLevelBootStateAccessor : ISyncBootStateAccessor
{
    public SyncBootState GetSyncBootState() => SyncBootState.Unknown;
}
