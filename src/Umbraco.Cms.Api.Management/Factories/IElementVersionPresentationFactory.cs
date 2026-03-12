using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models for element versions.
/// </summary>
public interface IElementVersionPresentationFactory
{
    /// <summary>
    /// Creates an <see cref="ElementVersionItemResponseModel"/> asynchronously from the given <see cref="ContentVersionMeta"/>.
    /// </summary>
    /// <param name="contentVersion">The content version metadata to create the element version from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="ElementVersionItemResponseModel"/>.</returns>
    Task<ElementVersionItemResponseModel> CreateAsync(ContentVersionMeta contentVersion);

    /// <summary>
    /// Creates multiple element version presentation models asynchronously from the provided content versions.
    /// </summary>
    /// <param name="contentVersions">The collection of content version metadata to create presentation models for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of element version item response models.</returns>
    Task<IEnumerable<ElementVersionItemResponseModel>> CreateMultipleAsync(
        IEnumerable<ContentVersionMeta> contentVersions);
}
