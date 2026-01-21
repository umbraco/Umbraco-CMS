namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a content editing operation.
/// </summary>
public enum ContentEditingOperationStatus
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
    ///     The specified content type was not found.
    /// </summary>
    ContentTypeNotFound,

    /// <summary>
    ///     The content's culture variance does not match the content type's culture variance setting.
    /// </summary>
    ContentTypeCultureVarianceMismatch,

    /// <summary>
    ///     The content's segment variance does not match the content type's segment variance setting.
    /// </summary>
    ContentTypeSegmentVarianceMismatch,

    /// <summary>
    ///     The specified content item was not found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The specified parent content item was not found.
    /// </summary>
    ParentNotFound,

    /// <summary>
    ///     The specified parent is invalid for this operation.
    /// </summary>
    ParentInvalid,

    /// <summary>
    ///     The operation is not allowed due to permission or structural constraints.
    /// </summary>
    NotAllowed,

    /// <summary>
    ///     The specified template was not found.
    /// </summary>
    TemplateNotFound,

    /// <summary>
    ///     The specified template is not allowed for this content type.
    /// </summary>
    TemplateNotAllowed,

    /// <summary>
    ///     The specified property type was not found on the content type.
    /// </summary>
    PropertyTypeNotFound,

    /// <summary>
    ///     The content item is in the recycle bin and cannot be edited.
    /// </summary>
    InTrash,

    /// <summary>
    ///     The content item is not in the recycle bin.
    /// </summary>
    NotInTrash,

    /// <summary>
    ///     The sorting operation contains invalid data.
    /// </summary>
    SortingInvalid,

    /// <summary>
    ///     One or more property values failed validation.
    /// </summary>
    PropertyValidationError,

    /// <summary>
    ///     The specified culture is invalid or not configured.
    /// </summary>
    InvalidCulture,

    /// <summary>
    ///     A content item with the same key already exists.
    /// </summary>
    DuplicateKey,

    /// <summary>
    ///     A content item with the same name already exists at this level.
    /// </summary>
    DuplicateName,

    /// <summary>
    ///     An unknown error occurred during the operation.
    /// </summary>
    Unknown,

    /// <summary>
    ///     The content item cannot be deleted because it is referenced by other items.
    /// </summary>
    CannotDeleteWhenReferenced,

    /// <summary>
    ///     The content item cannot be moved to the recycle bin because it is referenced by other items.
    /// </summary>
    CannotMoveToRecycleBinWhenReferenced,
}
