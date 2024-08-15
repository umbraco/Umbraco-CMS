using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

public class PublishedContentCacheAccessor : IPublishedContentCacheAccessor
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
    private readonly ILoggerFactory _loggerFactory;
    private PublishedContentTypeCache? _contentTypeCache;

    public PublishedContentCacheAccessor(IContentTypeService contentTypeService,
        IMemberTypeService memberTypeService,
        IMediaTypeService mediaTypeService,
        IPublishedContentTypeFactory publishedContentTypeFactory,
        ILoggerFactory loggerFactory)
    {
        _contentTypeService = contentTypeService;
        _memberTypeService = memberTypeService;
        _mediaTypeService = mediaTypeService;
        _publishedContentTypeFactory = publishedContentTypeFactory;
        _loggerFactory = loggerFactory;
    }

    public PublishedContentTypeCache Get() =>
        _contentTypeCache ??= new PublishedContentTypeCache(
            _contentTypeService,
            _mediaTypeService,
            _memberTypeService,
            _publishedContentTypeFactory,
            _loggerFactory.CreateLogger<PublishedContentTypeCache>());
}
