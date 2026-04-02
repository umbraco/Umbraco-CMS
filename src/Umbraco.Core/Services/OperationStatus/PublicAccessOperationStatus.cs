namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a public access operation.
/// </summary>
public enum PublicAccessOperationStatus
{
    /// <summary>
    /// The public access operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The content node to be protected was not found.
    /// </summary>
    ContentNotFound,

    /// <summary>
    /// The login page content node was not found.
    /// </summary>
    LoginNodeNotFound,

    /// <summary>
    /// The error page content node was not found.
    /// </summary>
    ErrorNodeNotFound,

    /// <summary>
    /// No allowed member groups or members were specified for the access rule.
    /// </summary>
    NoAllowedEntities,

    /// <summary>
    /// The access rule is ambiguous due to conflicting member and member group restrictions.
    /// </summary>
    AmbiguousRule,

    /// <summary>
    /// The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    /// The specified public access entry was not found.
    /// </summary>
    EntryNotFound,
}
