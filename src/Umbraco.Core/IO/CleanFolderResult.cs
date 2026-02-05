namespace Umbraco.Cms.Core.IO;

/// <summary>
/// Represents the result of a folder cleaning operation.
/// </summary>
public class CleanFolderResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CleanFolderResult"/> class.
    /// </summary>
    private CleanFolderResult()
    {
    }

    /// <summary>
    /// Gets the status of the clean folder operation.
    /// </summary>
    public CleanFolderResultStatus Status { get; private set; }

    /// <summary>
    /// Gets the collection of errors that occurred during the clean folder operation.
    /// </summary>
    public IReadOnlyCollection<Error>? Errors { get; private set; }

    /// <summary>
    /// Creates a successful clean folder result.
    /// </summary>
    /// <returns>A <see cref="CleanFolderResult"/> indicating success.</returns>
    public static CleanFolderResult Success() => new CleanFolderResult { Status = CleanFolderResultStatus.Success };

    /// <summary>
    /// Creates a failed clean folder result indicating the folder does not exist.
    /// </summary>
    /// <returns>A <see cref="CleanFolderResult"/> indicating the folder does not exist.</returns>
    public static CleanFolderResult FailedAsDoesNotExist() =>
        new CleanFolderResult { Status = CleanFolderResultStatus.FailedAsDoesNotExist };

    /// <summary>
    /// Creates a failed clean folder result with the specified errors.
    /// </summary>
    /// <param name="errors">The list of errors that occurred during the operation.</param>
    /// <returns>A <see cref="CleanFolderResult"/> containing the errors.</returns>
    public static CleanFolderResult FailedWithErrors(List<Error> errors) =>
        new CleanFolderResult { Status = CleanFolderResultStatus.FailedWithException, Errors = errors.AsReadOnly() };

    /// <summary>
    /// Represents an error that occurred while cleaning a folder.
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="erroringFile">The file that caused the error.</param>
        public Error(Exception exception, FileInfo erroringFile)
        {
            Exception = exception;
            ErroringFile = erroringFile;
        }

        /// <summary>
        /// Gets or sets the exception that occurred.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the file that caused the error.
        /// </summary>
        public FileInfo ErroringFile { get; set; }
    }
}
