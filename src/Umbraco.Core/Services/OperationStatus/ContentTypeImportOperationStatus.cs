namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a content type import operation.
/// </summary>
public enum ContentTypeImportOperationStatus
{
    /// <summary>
    ///     The import operation completed successfully and created a new content type.
    /// </summary>
    SuccessCreated,

    /// <summary>
    ///     The import operation completed successfully and updated an existing content type.
    /// </summary>
    SuccessUpdated,

    /// <summary>
    ///     The temporary file for import was not found.
    /// </summary>
    TemporaryFileNotFound,

    /// <summary>
    ///     The temporary file could not be converted to the expected format.
    /// </summary>
    TemporaryFileConversionFailure,

    /// <summary>
    ///     A document type with the same alias already exists.
    /// </summary>
    DocumentTypeExists,

    /// <summary>
    ///     The ID in the import file does not match the target content type ID.
    /// </summary>
    IdMismatch,

    /// <summary>
    ///     The type in the import file does not match the expected content type.
    /// </summary>
    TypeMismatch,
}
