using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

/// <summary>
///     Service interface for creating and managing document types (content types) in the Umbraco backoffice.
/// </summary>
/// <remarks>
///     This service provides operations for creating, updating, and querying document types,
///     including support for compositions and inheritance hierarchies.
/// </remarks>
public interface IContentTypeEditingService
{
    /// <summary>
    ///     Creates a new document type based on the provided model.
    /// </summary>
    /// <param name="model">The model containing the document type definition including properties, compositions, and templates.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     An <see cref="Attempt{TResult,TStatus}"/> containing the created <see cref="IContentType"/> on success,
    ///     or a <see cref="ContentTypeOperationStatus"/> indicating the reason for failure.
    /// </returns>
    Task<Attempt<IContentType?, ContentTypeOperationStatus>> CreateAsync(ContentTypeCreateModel model, Guid userKey);

    /// <summary>
    ///     Updates an existing document type with the provided model data.
    /// </summary>
    /// <param name="contentType">The existing document type to update.</param>
    /// <param name="model">The model containing the updated document type definition.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     An <see cref="Attempt{TResult,TStatus}"/> containing the updated <see cref="IContentType"/> on success,
    ///     or a <see cref="ContentTypeOperationStatus"/> indicating the reason for failure.
    /// </returns>
    Task<Attempt<IContentType?, ContentTypeOperationStatus>> UpdateAsync(IContentType contentType, ContentTypeUpdateModel model, Guid userKey);

    /// <summary>
    ///     Gets the available compositions for a document type.
    /// </summary>
    /// <param name="key">The unique identifier of the document type, or <c>null</c> for a new document type.</param>
    /// <param name="currentCompositeKeys">The keys of currently selected compositions.</param>
    /// <param name="currentPropertyAliases">The aliases of properties currently defined on the document type.</param>
    /// <param name="isElement">Whether the document type is configured as an element type.</param>
    /// <returns>
    ///     A collection of <see cref="ContentTypeAvailableCompositionsResult"/> indicating which compositions
    ///     are available and which are not allowed due to conflicts.
    /// </returns>
    Task<IEnumerable<ContentTypeAvailableCompositionsResult>> GetAvailableCompositionsAsync(
        Guid? key,
        IEnumerable<Guid> currentCompositeKeys,
        IEnumerable<string> currentPropertyAliases,
        bool isElement);
}
