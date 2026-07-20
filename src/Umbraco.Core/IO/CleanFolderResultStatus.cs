namespace Umbraco.Cms.Core.IO;

/// <summary>
/// Represents the status of a folder cleaning operation.
/// </summary>
public enum CleanFolderResultStatus
{
    /// <summary>
    /// The folder was cleaned successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The operation failed because the folder does not exist.
    /// </summary>
    FailedAsDoesNotExist,

    /// <summary>
    /// The operation failed with one or more exceptions.
    /// </summary>
    FailedWithException,
}
