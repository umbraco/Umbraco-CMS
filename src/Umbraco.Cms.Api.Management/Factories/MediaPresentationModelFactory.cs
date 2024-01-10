﻿using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class MediaPresentationModelFactory : IMediaPresentationModelFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ContentSettings _contentSettings;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IAbsoluteUrlBuilder _absoluteUrlBuilder;

    public MediaPresentationModelFactory(
        IUmbracoMapper umbracoMapper,
        IOptions<ContentSettings> contentSettings,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IAbsoluteUrlBuilder absoluteUrlBuilder)
    {
        _umbracoMapper = umbracoMapper;
        _contentSettings = contentSettings.Value;
        _mediaUrlGenerators = mediaUrlGenerators;
        _absoluteUrlBuilder = absoluteUrlBuilder;
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
                Url = _absoluteUrlBuilder.ToAbsoluteUrl(mediaUrl).ToString(),
            })
            .ToArray();

        return Task.FromResult(responseModel);
    }
}
