namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a named draft branch operation.
/// </summary>
public enum ContentDraftBranchOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The specified content item was not found.
    /// </summary>
    ContentNotFound,

    /// <summary>
    ///     The specified draft branch was not found.
    /// </summary>
    BranchNotFound,

    /// <summary>
    ///     The content item has no default draft to fork from (this should not normally happen, since every
    ///     content item has a default draft from the moment it is created).
    /// </summary>
    NoDefaultDraft,

    /// <summary>
    ///     The branch's own version row could not be found (should not normally happen once a branch exists).
    /// </summary>
    BranchVersionNotFound,

    /// <summary>
    ///     Saving edits to the branch's content failed. Also produced by a failed promotion, since promoting
    ///     a branch is a plain save onto the node's default draft, never a publish.
    /// </summary>
    SaveFailed,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     An empty culture set was supplied for a promote/schedule operation on culture-variant content. At
    ///     least one culture must be specified.
    /// </summary>
    CultureMissing,

    /// <summary>
    ///     A requested culture does not exist on the branch's own content, or a culture was supplied for an
    ///     operation on invariant content (which accepts none).
    /// </summary>
    InvalidCulture,
}
