using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class MediaPresentationFactory
    : ContentPresentationFactoryBase<IMediaType, IMediaTypeService>, IMediaPresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMediaUrlFactory _mediaUrlFactory;

    public MediaPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IMediaTypeService mediaTypeService,
        IMediaUrlFactory mediaUrlFactory)
        : base(mediaTypeService, umbracoMapper)
    {
        _umbracoMapper = umbracoMapper;
        _mediaTypeService = mediaTypeService;
        _mediaUrlFactory = mediaUrlFactory;
    }

    public MediaResponseModel CreateResponseModel(IMedia media)
    {
        MediaResponseModel responseModel = _umbracoMapper.Map<MediaResponseModel>(media)!;

        responseModel.Urls = _mediaUrlFactory.CreateUrls(media);

        return responseModel;
    }

    public MediaItemResponseModel CreateItemResponseModel(IMediaEntitySlim entity)
    {
        var responseModel = new MediaItemResponseModel
        {
            Id = entity.Key,
            IsTrashed = entity.Trashed
        };

        IMediaType? mediaType = _mediaTypeService.Get(entity.ContentTypeAlias);
        if (mediaType is not null)
        {
            responseModel.MediaType = _umbracoMapper.Map<MediaTypeReferenceResponseModel>(mediaType)!;
        }

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
        => CreateContentTypeReferenceResponseModel<MediaTypeReferenceResponseModel>(entity);
}
