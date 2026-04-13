using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MediaPresentationFactory : IMediaPresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaPresentationFactory"/> class.
    /// </summary>
    /// <param name="umbracoMapper">An instance of <see cref="IUmbracoMapper"/> used for mapping objects within the factory.</param>
    /// <param name="idKeyMap">An instance of <see cref="IIdKeyMap"/> used for mapping IDs to keys.</param>
    public MediaPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IIdKeyMap idKeyMap)
    {
        _umbracoMapper = umbracoMapper;
        _idKeyMap = idKeyMap;
    }

    /// <summary>
    /// Creates a <see cref="MediaResponseModel"/> from the given <see cref="IMedia"/> instance.
    /// </summary>
    /// <param name="media">The media item to create the response model from.</param>
    /// <returns>A <see cref="MediaResponseModel"/> representing the media item.</returns>
    public MediaResponseModel CreateResponseModel(IMedia media) => _umbracoMapper.Map<MediaResponseModel>(media)!;

    /// <summary>
    /// Creates a <see cref="MediaItemResponseModel"/> from the given <see cref="IMediaEntitySlim"/> entity.
    /// </summary>
    /// <param name="entity">The media entity to create the response model from.</param>
    /// <returns>A <see cref="MediaItemResponseModel"/> representing the media entity.</returns>
    public MediaItemResponseModel CreateItemResponseModel(IMediaEntitySlim entity)
    {
        Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(entity.ParentId, UmbracoObjectTypes.Media);

        return new MediaItemResponseModel
        {
            Id = entity.Key,
            IsTrashed = entity.Trashed,
            Parent = parentKeyAttempt.Success ? new ReferenceByIdModel { Id = parentKeyAttempt.Result } : null,
            HasChildren = entity.HasChildren,
            MediaType = _umbracoMapper.Map<MediaTypeReferenceResponseModel>(entity)!,
            Variants = CreateVariantsItemResponseModels(entity)
        };
    }

    /// <summary>
    /// Creates a collection of <see cref="VariantItemResponseModel"/> instances for the specified media entity.
    /// </summary>
    /// <param name="entity">The media entity for which to create variant response models.</param>
    /// <returns>An enumerable containing <see cref="VariantItemResponseModel"/> instances representing the entity's variants.</returns>
    public IEnumerable<VariantItemResponseModel> CreateVariantsItemResponseModels(IMediaEntitySlim entity)
        =>
        [
            new VariantItemResponseModel
            {
                Name = entity.Name ?? string.Empty,
                Culture = null
            }
        ];

    /// <summary>
    /// Maps the specified <see cref="IMediaEntitySlim"/> to a <see cref="MediaTypeReferenceResponseModel"/>.
    /// </summary>
    /// <param name="entity">The media entity to map.</param>
    /// <returns>The mapped <see cref="MediaTypeReferenceResponseModel"/>.</returns>
    public MediaTypeReferenceResponseModel CreateMediaTypeReferenceResponseModel(IMediaEntitySlim entity)
        => _umbracoMapper.Map<MediaTypeReferenceResponseModel>(entity)!;
}
