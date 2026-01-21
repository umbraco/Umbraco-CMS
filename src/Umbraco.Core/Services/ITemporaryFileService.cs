using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides functionality for managing temporary files used during uploads and other operations.
/// </summary>
/// <remarks>
/// Temporary files are stored for a limited time and are automatically cleaned up.
/// </remarks>
public interface ITemporaryFileService
{
    /// <summary>
    /// Creates a new temporary file.
    /// </summary>
    /// <param name="createModel">The model containing the file data and metadata.</param>
    /// <returns>An attempt containing the created temporary file model if successful, or an operation status indicating failure.</returns>
    Task<Attempt<TemporaryFileModel?, TemporaryFileOperationStatus>> CreateAsync(CreateTemporaryFileModel createModel);

    /// <summary>
    /// Deletes a temporary file by its key.
    /// </summary>
    /// <param name="key">The unique identifier of the temporary file to delete.</param>
    /// <returns>An attempt containing the deleted temporary file model if successful, or an operation status indicating failure.</returns>
    Task<Attempt<TemporaryFileModel?, TemporaryFileOperationStatus>> DeleteAsync(Guid key);

    /// <summary>
    /// Gets a temporary file by its key.
    /// </summary>
    /// <param name="key">The unique identifier of the temporary file.</param>
    /// <returns>The temporary file model if found; otherwise, null.</returns>
    Task<TemporaryFileModel?> GetAsync(Guid key);

    /// <summary>
    /// Cleans up expired temporary files.
    /// </summary>
    /// <returns>A collection of keys for the temporary files that were cleaned up.</returns>
    Task<IEnumerable<Guid>> CleanUpOldTempFiles();
}
