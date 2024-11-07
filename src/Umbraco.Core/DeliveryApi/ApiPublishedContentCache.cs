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
    private readonly IRequestCultureService _requestCultureService;
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IPublishedContentCache _publishedContentCache;
    private DeliveryApiSettings _deliveryApiSettings;

    public ApiPublishedContentCache(
        IRequestPreviewService requestPreviewService,
        IRequestCultureService requestCultureService,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings,
        IDocumentUrlService documentUrlService,
        IPublishedContentCache publishedContentCache)
    {
        _requestPreviewService = requestPreviewService;
        _requestCultureService = requestCultureService;
        _documentUrlService = documentUrlService;
        _publishedContentCache = publishedContentCache;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
    }

    public async Task<IPublishedContent?> GetByRouteAsync(string route)
    {
        var isPreviewMode = _requestPreviewService.IsPreview();

        // Handle the nasty logic with domain document ids in front of paths.
        int? documentStartNodeId = null;
        if (route.StartsWith("/") is false)
        {
            var index = route.IndexOf('/');

            if (index > -1 && int.TryParse(route.Substring(0, index), out var nodeId))
            {
                documentStartNodeId = nodeId;
                route = route.Substring(index);
            }
        }

        Guid? documentKey = _documentUrlService.GetDocumentKeyByRoute(
            route,
            _requestCultureService.GetRequestedCulture(),
            documentStartNodeId,
            _requestPreviewService.IsPreview()
        );
        IPublishedContent? content = documentKey.HasValue
            ? await _publishedContentCache.GetByIdAsync(documentKey.Value, isPreviewMode)
            : null;

        return ContentOrNullIfDisallowed(content);
    }

    public IPublishedContent? GetByRoute(string route)
    {
        var isPreviewMode = _requestPreviewService.IsPreview();


        // Handle the nasty logic with domain document ids in front of paths.
        int? documentStartNodeId = null;
        if (route.StartsWith("/") is false)
        {
            var index = route.IndexOf('/');

            if (index > -1 && int.TryParse(route.Substring(0, index), out var nodeId))
            {
                documentStartNodeId = nodeId;
                route = route.Substring(index);
            }
        }

        Guid? documentKey = _documentUrlService.GetDocumentKeyByRoute(
            route,
            _requestCultureService.GetRequestedCulture(),
            documentStartNodeId,
            _requestPreviewService.IsPreview()
        );
        IPublishedContent? content = documentKey.HasValue
            ? _publishedContentCache.GetById(isPreviewMode, documentKey.Value)
            : null;

        return ContentOrNullIfDisallowed(content);
    }

    public async Task<IPublishedContent?> GetByIdAsync(Guid contentId)
    {
        IPublishedContent? content = await _publishedContentCache.GetByIdAsync(contentId, _requestPreviewService.IsPreview()).ConfigureAwait(false);
        return ContentOrNullIfDisallowed(content);
    }

    public IPublishedContent? GetById(Guid contentId)
    {
        IPublishedContent? content = _publishedContentCache.GetById(_requestPreviewService.IsPreview(), contentId);
        return ContentOrNullIfDisallowed(content);
    }
    public async Task<IEnumerable<IPublishedContent>> GetByIdsAsync(IEnumerable<Guid> contentIds)
    {
        var isPreviewMode = _requestPreviewService.IsPreview();

        IEnumerable<Task<IPublishedContent?>> tasks = contentIds
            .Select(contentId => _publishedContentCache.GetByIdAsync(contentId, isPreviewMode));

        IPublishedContent?[] allContent = await Task.WhenAll(tasks);

        return allContent
            .WhereNotNull()
            .Where(IsAllowedContentType)
            .ToArray();
    }

    public IEnumerable<IPublishedContent> GetByIds(IEnumerable<Guid> contentIds)
    {
        var isPreviewMode = _requestPreviewService.IsPreview();
        return contentIds
            .Select(contentId => _publishedContentCache.GetById(isPreviewMode, contentId))
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
