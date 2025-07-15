namespace Umbraco.Cms.Infrastructure.Models;

/// <summary>
/// Represents the status of an index rebuild trigger.
/// </summary>
public enum IndexRebuildResult
{
    /// <summary>
    /// The rebuild was either successful or enqueued successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The index is already being rebuilt.
    /// </summary>
    AlreadyRebuilding,

    /// <summary>
    /// The index rebuild was not scheduled because it's not allowed to run at this time.
    /// </summary>
    NotAllowedToRun,

    /// <summary>
    /// The index rebuild was not scheduled due to an unknown error.
    /// </summary>
    Unknown,
}
