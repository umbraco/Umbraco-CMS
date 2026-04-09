namespace Umbraco.Cms.Core.Sync;

/// <summary>
/// Represents the synchronization boot state of the application.
/// </summary>
/// <remarks>
/// The boot state indicates whether the application has previous sync state
/// available, which affects how cache synchronization is performed during startup.
/// </remarks>
public enum SyncBootState
{
    /// <summary>
    /// Unknown state. Treat as WarmBoot.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Cold boot. No sync state is present; full cache rebuild may be required.
    /// </summary>
    ColdBoot = 1,

    /// <summary>
    /// Warm boot. Sync state is present; incremental cache updates can be applied.
    /// </summary>
    WarmBoot = 2,
}
