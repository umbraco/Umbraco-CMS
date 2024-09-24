using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiPublishedContentCache : IApiPublishedContentCache
{
    private readonly IPublishedContentCache _contentCache;
    private readonly IRequestPreviewService _requestPreviewService;
    private DeliveryApiSettings _deliveryApiSettings;

    public ApiPublishedContentCache(IPublishedContentCache contentCache, IRequestPreviewService requestPreviewService, IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
    {
        _contentCache = contentCache;
        _requestPreviewService = requestPreviewService;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
    }

    public IPublishedContent? GetByRoute(string route)
    {
        IPublishedContentCache contentCache = _contentCache;

        IPublishedContent? content = contentCache.GetByRoute(_requestPreviewService.IsPreview(), route);
        return ContentOrNullIfDisallowed(content);
    }

    public IPublishedContent? GetById(Guid contentId)
    {
        IPublishedContentCache contentCache = _contentCache;

        IPublishedContent? content = contentCache.GetById(_requestPreviewService.IsPreview(), contentId);
        return ContentOrNullIfDisallowed(content);
    }

    public IEnumerable<IPublishedContent> GetByIds(IEnumerable<Guid> contentIds)
    {
        IPublishedContentCache contentCache = _contentCache;

        return contentIds
            .Select(contentId => contentCache.GetById(_requestPreviewService.IsPreview(), contentId))
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
