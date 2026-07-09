namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a member type import operation.
/// </summary>
public enum MemberTypeImportOperationStatus
{
    /// <summary>
    /// The member type was successfully created from the import.
    /// </summary>
    SuccessCreated,

    /// <summary>
    /// The member type was successfully updated from the import.
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
    /// A member type with the same identifier already exists.
    /// </summary>
    MemberTypeExists,

    /// <summary>
    /// The import data contains a type that does not match the expected member type.
    /// </summary>
    TypeMismatch,

    /// <summary>
    /// The identifier in the import data does not match the expected identifier.
    /// </summary>
    IdMismatch
}
