namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a content type operation.
/// </summary>
public enum ContentTypeOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     A content type with the same alias already exists.
    /// </summary>
    DuplicateAlias,

    /// <summary>
    ///     The content type alias is invalid.
    /// </summary>
    InvalidAlias,

    /// <summary>
    ///     The property type alias is invalid.
    /// </summary>
    InvalidPropertyTypeAlias,

    /// <summary>
    ///     A property type with the same alias already exists on this content type.
    /// </summary>
    DuplicatePropertyTypeAlias,

    /// <summary>
    ///     The specified data type was not found.
    /// </summary>
    DataTypeNotFound,

    /// <summary>
    ///     The inheritance configuration is invalid.
    /// </summary>
    InvalidInheritance,

    /// <summary>
    ///     The composition configuration is invalid.
    /// </summary>
    InvalidComposition,

    /// <summary>
    ///     The specified parent content type is invalid.
    /// </summary>
    InvalidParent,

    /// <summary>
    ///     The container name is invalid.
    /// </summary>
    InvalidContainerName,

    /// <summary>
    ///     The container type is invalid for this operation.
    /// </summary>
    InvalidContainerType,

    /// <summary>
    ///     The specified container is missing.
    /// </summary>
    MissingContainer,

    /// <summary>
    ///     A container with the same name already exists at this level.
    /// </summary>
    DuplicateContainer,

    /// <summary>
    ///     The specified content type was not found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The operation is not allowed due to permission or structural constraints.
    /// </summary>
    NotAllowed,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     The property type alias cannot be the same as the content type alias.
    /// </summary>
    PropertyTypeAliasCannotEqualContentTypeAlias,

    /// <summary>
    ///     The content type name cannot be empty.
    /// </summary>
    NameCannotBeEmpty,

    /// <summary>
    ///     The content type name exceeds the maximum allowed length.
    /// </summary>
    NameTooLong,

    /// <summary>
    ///     Cannot change the element flag because the document type has existing content.
    /// </summary>
    InvalidElementFlagDocumentHasContent,

    /// <summary>
    ///     Cannot change the element flag because the element type is used in property editor configuration.
    /// </summary>
    InvalidElementFlagElementIsUsedInPropertyEditorConfiguration,

    /// <summary>
    ///     The element flag setting is incompatible with the parent content type.
    /// </summary>
    InvalidElementFlagComparedToParent,

    /// <summary>
    ///     An unknown error occurred during the operation.
    /// </summary>
    Unknown,

    /// <summary>
    ///     The specified template alias is invalid.
    /// </summary>
    InvalidTemplateAlias,
}
