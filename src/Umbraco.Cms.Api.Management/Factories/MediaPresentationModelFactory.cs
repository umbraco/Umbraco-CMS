using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class MediaPresentationModelFactory : IMediaPresentationModelFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ContentSettings _contentSettings;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;

    public MediaPresentationModelFactory(IUmbracoMapper umbracoMapper, IOptions<ContentSettings> contentSettings, MediaUrlGeneratorCollection mediaUrlGenerators)
    {
        _umbracoMapper = umbracoMapper;
        _contentSettings = contentSettings.Value;
        _mediaUrlGenerators = mediaUrlGenerators;
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
        var responseModel = _umbracoMapper.Map<IMediaEntitySlim, MediaItemResponseModel>(entity)!;
        return responseModel;
    }
}
