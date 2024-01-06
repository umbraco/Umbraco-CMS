using Microsoft.Extensions.Options;
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

public class MediaPresentationModelFactory : IMediaPresentationModelFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ContentSettings _contentSettings;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IMediaTypeService _mediaTypeService;

    public MediaPresentationModelFactory(
        IUmbracoMapper umbracoMapper,
        IOptions<ContentSettings> contentSettings,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IMediaTypeService mediaTypeService)
    {
        _umbracoMapper = umbracoMapper;
        _contentSettings = contentSettings.Value;
        _mediaUrlGenerators = mediaUrlGenerators;
        _mediaTypeService = mediaTypeService;
    }

    public Task<MediaResponseModel> CreateResponseModelAsync(IMedia media)
    {
        MediaResponseModel responseModel = _umbracoMapper.Map<MediaResponseModel>(media)!;

        responseModel.Urls = media
            .GetUrls(_contentSettings, _mediaUrlGenerators)
            .WhereNotNull()
            .Select(mediaUrl => new ContentUrlInfo
            {
                Culture = null,
                Url = mediaUrl
            })
            .ToArray();

        return Task.FromResult(responseModel);
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

        return responseModel;
    }

    public MediaTypeReferenceResponseModel CreateMediaTypeReferenceResponseModel(IMediaEntitySlim entity)
    {
        IMediaType? mediaType = _mediaTypeService.Get(entity.ContentTypeAlias);
        return mediaType is not null
            ? _umbracoMapper.Map<MediaTypeReferenceResponseModel>(mediaType)!
            : new MediaTypeReferenceResponseModel();
    }
}
