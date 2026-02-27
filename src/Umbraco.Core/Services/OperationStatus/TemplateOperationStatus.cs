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
    ///     The operation failed because the layout template could not be found.
    /// </summary>
    LayoutTemplateNotFound,

    /// <summary>
    ///     The operation failed because it would create a circular reference in the layout template hierarchy.
    /// </summary>
    CircularLayoutTemplateReference,

    /// <summary>
    ///     The operation failed because a layout template cannot be deleted while it has child templates.
    /// </summary>
    LayoutTemplateCannotBeDeleted,

    /// <inheritdoc cref="LayoutTemplateNotFound" />
    [Obsolete("Use LayoutTemplateNotFound instead. Scheduled for removal in Umbraco 20.")]
    MasterTemplateNotFound = LayoutTemplateNotFound,

    /// <inheritdoc cref="CircularLayoutTemplateReference" />
    [Obsolete("Use CircularLayoutTemplateReference instead. Scheduled for removal in Umbraco 20.")]
    CircularMasterTemplateReference = CircularLayoutTemplateReference,

    /// <inheritdoc cref="LayoutTemplateCannotBeDeleted" />
    [Obsolete("Use LayoutTemplateCannotBeDeleted instead. Scheduled for removal in Umbraco 20.")]
    MasterTemplateCannotBeDeleted = LayoutTemplateCannotBeDeleted,
}
