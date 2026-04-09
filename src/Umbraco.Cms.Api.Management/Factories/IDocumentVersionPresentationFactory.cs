using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models for document versions.
/// </summary>
public interface IDocumentVersionPresentationFactory
{
    /// <summary>
    /// Creates a <see cref="DocumentVersionItemResponseModel"/> asynchronously from the given <see cref="ContentVersionMeta"/>.
    /// </summary>
    /// <param name="contentVersion">The content version metadata to create the document version from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="DocumentVersionItemResponseModel"/>.</returns>
    Task<DocumentVersionItemResponseModel> CreateAsync(ContentVersionMeta contentVersion);

    /// <summary>
    /// Creates multiple document version presentation models asynchronously from the provided content versions.
    /// </summary>
    /// <param name="contentVersions">The collection of content version metadata to create presentation models for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of document version item response models.</returns>
    Task<IEnumerable<DocumentVersionItemResponseModel>> CreateMultipleAsync(
        IEnumerable<ContentVersionMeta> contentVersions);
}
