namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a media type import operation.
/// </summary>
public enum MediaTypeImportOperationStatus
{
    /// <summary>
    /// The media type was successfully created from the import.
    /// </summary>
    SuccessCreated,

    /// <summary>
    /// The media type was successfully updated from the import.
    /// </summary>
    SuccessUpdated,

    /// <summary>
    /// The temporary file containing the import data was not found.
    /// </summary>
    TemporaryFileNotFound,

    /// <summary>
    /// The temporary file could not be converted to a valid import format.
    /// </summary>
    TemporaryFileConversionFailure,

    /// <summary>
    /// A media type with the same identifier already exists.
    /// </summary>
    MediaTypeExists,

    /// <summary>
    /// The import data contains a type that does not match the expected media type.
    /// </summary>
    TypeMismatch,

    /// <summary>
    /// The identifier in the import data does not match the expected identifier.
    /// </summary>
    IdMismatch
}
