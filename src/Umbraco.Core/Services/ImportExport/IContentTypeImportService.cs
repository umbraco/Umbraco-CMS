using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ImportExport;

/// <summary>
///     Service for importing content types (document types) from XML files.
/// </summary>
public interface IContentTypeImportService
{
    /// <summary>
    ///     Imports a content type from a temporary file containing XML data.
    /// </summary>
    /// <param name="temporaryFileId">The unique identifier of the temporary file containing the content type XML definition.</param>
    /// <param name="userKey">The unique key of the user performing the import operation.</param>
    /// <param name="contentTypeId">
    ///     Optional. The unique identifier of an existing content type to update.
    ///     When <c>null</c>, a new content type will be created.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult, TStatus}"/>
    ///     with the imported <see cref="IContentType"/> on success, or <c>null</c> with an appropriate
    ///     <see cref="ContentTypeImportOperationStatus"/> on failure.
    /// </returns>
    Task<Attempt<IContentType?, ContentTypeImportOperationStatus>> Import(Guid temporaryFileId, Guid userKey, Guid? contentTypeId = null);
}
