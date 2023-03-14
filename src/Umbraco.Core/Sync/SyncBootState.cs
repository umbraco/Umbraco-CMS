namespace Umbraco.Cms.Core.Sync;

public enum SyncBootState
{
    /// <summary>
    ///     Unknown state. Treat as WarmBoot
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     Cold boot. No Sync state
    /// </summary>
    ColdBoot = 1,

    /// <summary>
    ///     Warm boot. Sync state present
    /// </summary>
    WarmBoot = 2,
}
