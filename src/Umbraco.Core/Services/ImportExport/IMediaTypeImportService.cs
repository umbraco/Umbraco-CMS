using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

/// <summary>
///     Service for importing media types from XML files.
/// </summary>
public interface IMediaTypeImportService
{
    /// <summary>
    ///     Imports a media type from a temporary file containing XML data.
    /// </summary>
    /// <param name="temporaryFileId">The unique identifier of the temporary file containing the media type XML definition.</param>
    /// <param name="userKey">The unique key of the user performing the import operation.</param>
    /// <param name="mediaTypeId">
    ///     Optional. The unique identifier of an existing media type to update.
    ///     When <c>null</c>, a new media type will be created.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult, TStatus}"/>
    ///     with the imported <see cref="IMediaType"/> on success, or <c>null</c> with an appropriate
    ///     <see cref="MediaTypeImportOperationStatus"/> on failure.
    /// </returns>
    Task<Attempt<IMediaType?, MediaTypeImportOperationStatus>> Import(
        Guid temporaryFileId,
        Guid userKey,
        Guid? mediaTypeId = null);
}
