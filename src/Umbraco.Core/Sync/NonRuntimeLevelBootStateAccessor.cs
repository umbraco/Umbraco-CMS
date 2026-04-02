namespace Umbraco.Cms.Core.Sync;

/// <summary>
/// Boot state implementation for when Umbraco is not in the run state.
/// </summary>
/// <remarks>
/// This accessor always returns <see cref="SyncBootState.Unknown"/> because
/// synchronization is not applicable when the application is not fully running.
/// </remarks>
public sealed class NonRuntimeLevelBootStateAccessor : ISyncBootStateAccessor
{
    /// <inheritdoc />
    public SyncBootState GetSyncBootState() => SyncBootState.Unknown;
}
