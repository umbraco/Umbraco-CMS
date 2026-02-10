namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a template operation.
/// </summary>
public enum TemplateOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     The operation failed because the template alias is invalid.
    /// </summary>
    InvalidAlias,

    /// <summary>
    ///     The operation failed because a template with the same alias already exists.
    /// </summary>
    DuplicateAlias,

    /// <summary>
    ///     The operation failed because the template could not be found.
    /// </summary>
    TemplateNotFound,

    /// <summary>
    ///     The operation failed because the master template could not be found.
    /// </summary>
    MasterTemplateNotFound,

    /// <summary>
    ///     The operation failed because it would create a circular reference in the master template hierarchy.
    /// </summary>
    CircularMasterTemplateReference,

    /// <summary>
    ///     The operation failed because the master template cannot be deleted while it has child templates.
    /// </summary>
    MasterTemplateCannotBeDeleted,
}
