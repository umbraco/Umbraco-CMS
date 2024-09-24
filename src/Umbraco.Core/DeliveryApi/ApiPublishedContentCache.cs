using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiPublishedContentCache : IApiPublishedContentCache
{
    private readonly IRequestPreviewService _requestPreviewService;
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IPublishedContentCache _publishedContentCache;
    private DeliveryApiSettings _deliveryApiSettings;

    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 16.")]
    public ApiPublishedContentCache(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IRequestPreviewService requestPreviewService,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
    :this(
        requestPreviewService,
        deliveryApiSettings,
        StaticServiceProvider.Instance.GetRequiredService<IDocumentUrlService>(),
        StaticServiceProvider.Instance.GetRequiredService<IPublishedContentCache>()
        )
    {

    }

    public ApiPublishedContentCache(
        IRequestPreviewService requestPreviewService,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings,
        IDocumentUrlService documentUrlService,
        IPublishedContentCache publishedContentCache)
    {
        _requestPreviewService = requestPreviewService;
        _documentUrlService = documentUrlService;
        _publishedContentCache = publishedContentCache;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
    }


    public IPublishedContent? GetByRoute(string route)
    {
        var isPreviewMode = _requestPreviewService.IsPreview();

        Guid? documentKey = _documentUrlService.GetDocumentKeyByRoute(
            route,
            null,
            null,
            _requestPreviewService.IsPreview()
        );
        IPublishedContent? content = documentKey.HasValue
            ? _publishedContentCache.GetById(isPreviewMode, documentKey.Value)
            : null;

        return ContentOrNullIfDisallowed(content);
    }

    public IPublishedContent? GetById(Guid contentId)
    {
        IPublishedContent? content = _publishedContentCache.GetById(_requestPreviewService.IsPreview(), contentId);
        return ContentOrNullIfDisallowed(content);
    }

    public IEnumerable<IPublishedContent> GetByIds(IEnumerable<Guid> contentIds)
    {
        return contentIds
            .Select(contentId => _publishedContentCache.GetById(_requestPreviewService.IsPreview(), contentId))
            .WhereNotNull()
            .Where(IsAllowedContentType)
            .ToArray();
    }

    private IPublishedContent? ContentOrNullIfDisallowed(IPublishedContent? content)
        => content != null && IsAllowedContentType(content)
            ? content
            : null;

    private bool IsAllowedContentType(IPublishedContent content)
        => _deliveryApiSettings.IsAllowedContentType(content);
}
