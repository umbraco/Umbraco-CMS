using Umbraco.Cms.Core.Models.TemporaryFile;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
/// Persists temporary files.
/// </summary>
public interface ITemporaryFileRepository
{
    /// <summary>
    /// Gets a temporary file from its key.
    /// </summary>
    /// <param name="key">The unique key of the temporary file.</param>
    /// <returns>The temporary file model if found on that specified key, otherwise null.</returns>
    Task<TemporaryFileModel?> GetAsync(Guid key);

    /// <summary>
    /// Creates or update a temporary file.
    /// </summary>
    /// <param name="model">The model for the temporary file</param>
    Task SaveAsync(TemporaryFileModel model);

    /// <summary>
    /// Deletes a temporary file using it's unique key.
    /// </summary>
    /// <param name="key">The unique key for the temporary file.</param>
    Task DeleteAsync(Guid key);

    /// <summary>
    /// Removes all temporary files that have its TempFileModel.AvailableUntil lower than a specified time.
    /// </summary>
    /// <returns>The keys of the delete temporary files.</returns>
    Task<IEnumerable<Guid>> CleanUpOldTempFiles(DateTime dateTime);
}
