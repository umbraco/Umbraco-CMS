using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory interface for creating instances of media presentation models within the management API.
/// </summary>
public interface IMediaPresentationFactory
{
    /// <summary>
    /// Creates a response model from the given media.
    /// </summary>
    /// <param name="media">The media item to create the response model from.</param>
    /// <returns>A MediaResponseModel representing the media.</returns>
    MediaResponseModel CreateResponseModel(IMedia media);

    /// <summary>
    /// Creates a response model for the given media entity.
    /// </summary>
    /// <param name="entity">The media entity to create the response model from.</param>
    /// <returns>A <see cref="MediaItemResponseModel"/> representing the media entity.</returns>
    MediaItemResponseModel CreateItemResponseModel(IMediaEntitySlim entity);

    /// <summary>
    /// Creates variant item response models for the specified media entity.
    /// </summary>
    /// <param name="entity">The media entity for which to create variant item response models.</param>
    /// <returns>An enumerable collection of <see cref="VariantItemResponseModel"/> representing the media entity's variants.</returns>
    IEnumerable<VariantItemResponseModel> CreateVariantsItemResponseModels(IMediaEntitySlim entity);

    /// <summary>
    /// Creates a MediaTypeReferenceResponseModel from the given media entity.
    /// </summary>
    /// <param name="entity">The media entity to create the reference model from.</param>
    /// <returns>A MediaTypeReferenceResponseModel representing the media type reference.</returns>
    MediaTypeReferenceResponseModel CreateMediaTypeReferenceResponseModel(IMediaEntitySlim entity);
}
