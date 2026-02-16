using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Factory for creating media presentation (view) models from domain models.
/// </summary>
public interface IMediaPresentationFactory
{
    /// <summary>
    /// Creates a media response model from the specified media.
    /// </summary>
    /// <param name="media">The media to create the response model from.</param>
    /// <returns>The media response model.</returns>
    MediaResponseModel CreateResponseModel(IMedia media);

    /// <summary>
    /// Creates a media item response model from the specified entity.
    /// </summary>
    /// <param name="entity">The media entity.</param>
    /// <returns>The media item response model.</returns>
    MediaItemResponseModel CreateItemResponseModel(IMediaEntitySlim entity);

    /// <summary>
    /// Creates variant item response models for the specified media entity.
    /// </summary>
    /// <param name="entity">The media entity.</param>
    /// <returns>The variant item response models.</returns>
    IEnumerable<VariantItemResponseModel> CreateVariantsItemResponseModels(IMediaEntitySlim entity);

    /// <summary>
    /// Creates a media type reference response model from the specified entity.
    /// </summary>
    /// <param name="entity">The media entity.</param>
    /// <returns>The media type reference response model.</returns>
    MediaTypeReferenceResponseModel CreateMediaTypeReferenceResponseModel(IMediaEntitySlim entity);

    /// <summary>
    /// Creates a search item response model for a media entity, including ancestor breadcrumbs.
    /// </summary>
    // TODO (V18): Remove default implementation.
    SearchMediaItemResponseModel CreateSearchItemResponseModel(IMediaEntitySlim entity, IEnumerable<SearchResultAncestorModel> ancestors)
    {
        MediaItemResponseModel baseModel = CreateItemResponseModel(entity);
        return new SearchMediaItemResponseModel
        {
            Id = baseModel.Id,
            IsTrashed = baseModel.IsTrashed,
            Parent = baseModel.Parent,
            HasChildren = baseModel.HasChildren,
            MediaType = baseModel.MediaType,
            Variants = baseModel.Variants,
            Flags = baseModel.Flags,
            Ancestors = ancestors,
        };
    }
}
