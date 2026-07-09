using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides methods for importing media files into Umbraco.
/// </summary>
public interface IMediaImportService
{
    /// <summary>
    ///     Imports a media file asynchronously.
    /// </summary>
    /// <param name="fileName">The name of the file being imported.</param>
    /// <param name="fileStream">The stream containing the file data.</param>
    /// <param name="parentId">The parent folder identifier, or null to import at the root.</param>
    /// <param name="mediaTypeAlias">The media type alias, or null to auto-detect.</param>
    /// <param name="userKey">The key of the user performing the import.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the imported <see cref="IMedia"/>.</returns>
    public Task<IMedia> ImportAsync(string fileName, Stream fileStream, Guid? parentId, string? mediaTypeAlias, Guid userKey);
}
