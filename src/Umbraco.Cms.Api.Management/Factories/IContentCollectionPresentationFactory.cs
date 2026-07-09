using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Defines a factory interface for creating presentation models for collections of content items.
/// </summary>
/// <typeparam name="TContent">The type representing the content items.</typeparam>
/// <typeparam name="TCollectionResponseModel">The type representing the collection response model.</typeparam>
/// <typeparam name="TValueResponseModelBase">The base type for value response models within the collection.</typeparam>
/// <typeparam name="TVariantResponseModel">The type representing the variant response model for content items.</typeparam>
public interface IContentCollectionPresentationFactory<TContent, TCollectionResponseModel, TValueResponseModelBase, TVariantResponseModel>
    where TContent : class, IContentBase
    where TCollectionResponseModel : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueResponseModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    /// <summary>
    /// Asynchronously creates a collection response model from the provided paged content items.
    /// </summary>
    /// <param name="content">The paged model containing the content items to be transformed.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of collection response models.</returns>
    Task<List<TCollectionResponseModel>> CreateCollectionModelAsync(ListViewPagedModel<TContent> content);
}
