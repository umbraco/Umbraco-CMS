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
    private readonly IMediaUrlFactory _mediaUrlFactory;
    private readonly IIdKeyMap _idKeyMap;

    public MediaPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IMediaUrlFactory mediaUrlFactory,
        IIdKeyMap idKeyMap)
    {
        _umbracoMapper = umbracoMapper;
        _mediaUrlFactory = mediaUrlFactory;
        _idKeyMap = idKeyMap;
    }

    public MediaResponseModel CreateResponseModel(IMedia media)
    {
        MediaResponseModel responseModel = _umbracoMapper.Map<MediaResponseModel>(media)!;

        responseModel.Urls = _mediaUrlFactory.CreateUrls(media);

        return responseModel;
    }

    public MediaItemResponseModel CreateItemResponseModel(IMediaEntitySlim entity)
    {
        Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(entity.ParentId, UmbracoObjectTypes.Media);

        var responseModel = new MediaItemResponseModel
        {
            Id = entity.Key,
            IsTrashed = entity.Trashed,
            Parent = parentKeyAttempt.Success ? new ReferenceByIdModel { Id = parentKeyAttempt.Result } : null,
            HasChildren = entity.HasChildren,
        };

        responseModel.MediaType = _umbracoMapper.Map<MediaTypeReferenceResponseModel>(entity)!;

        responseModel.Variants = CreateVariantsItemResponseModels(entity);

        return responseModel;
    }

    public IEnumerable<VariantItemResponseModel> CreateVariantsItemResponseModels(IMediaEntitySlim entity)
        => new[]
        {
            new VariantItemResponseModel
            {
                Name = entity.Name ?? string.Empty,
                Culture = null
            }
        };

    public MediaTypeReferenceResponseModel CreateMediaTypeReferenceResponseModel(IMediaEntitySlim entity)
        => _umbracoMapper.Map<MediaTypeReferenceResponseModel>(entity)!;
}
