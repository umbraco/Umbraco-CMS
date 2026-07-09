namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a temporary file operation.
/// </summary>
public enum TemporaryFileOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success = 0,

    /// <summary>
    ///     The operation failed because the file extension is not allowed.
    /// </summary>
    FileExtensionNotAllowed = 1,

    /// <summary>
    ///     The operation failed because the specified key is already in use by another temporary file.
    /// </summary>
    KeyAlreadyUsed = 2,

    /// <summary>
    ///     The operation failed because the temporary file could not be found.
    /// </summary>
    NotFound = 3,

    /// <summary>
    ///     The operation failed because file uploads are currently blocked.
    /// </summary>
    UploadBlocked = 4,

    /// <summary>
    ///     The operation failed because the file name is invalid.
    /// </summary>
    InvalidFileName = 5,
}
