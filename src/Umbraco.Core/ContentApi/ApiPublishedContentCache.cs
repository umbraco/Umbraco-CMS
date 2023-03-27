using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiPublishedContentCache : IApiPublishedContentCache
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IRequestPreviewService _requestPreviewService;
    private ContentApiSettings _contentApiSettings;

    public ApiPublishedContentCache(IPublishedSnapshotAccessor publishedSnapshotAccessor, IRequestPreviewService requestPreviewService, IOptionsMonitor<ContentApiSettings> contentApiSettings)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _requestPreviewService = requestPreviewService;
        _contentApiSettings = contentApiSettings.CurrentValue;
        contentApiSettings.OnChange(settings => _contentApiSettings = settings);
    }

    public IPublishedContent? GetByRoute(string route)
    {
        IPublishedContentCache? contentCache = GetContentCache();
        if (contentCache == null)
        {
            return null;
        }

        IPublishedContent? content = contentCache.GetByRoute(_requestPreviewService.IsPreview(), route);
        return ContentOrNullIfDisallowed(content);
    }

    public IPublishedContent? GetById(Guid contentId)
    {
        IPublishedContentCache? contentCache = GetContentCache();
        if (contentCache == null)
        {
            return null;
        }

        IPublishedContent? content = contentCache.GetById(_requestPreviewService.IsPreview(), contentId);
        return ContentOrNullIfDisallowed(content);
    }

    public IEnumerable<IPublishedContent> GetByIds(IEnumerable<Guid> contentIds)
    {
        IPublishedContentCache? contentCache = GetContentCache();
        if (contentCache == null)
        {
            return Enumerable.Empty<IPublishedContent>();
        }

        return contentIds
            .Select(contentId => contentCache.GetById(_requestPreviewService.IsPreview(), contentId))
            .WhereNotNull()
            .Where(IsAllowedContentType)
            .ToArray();
    }

    private IPublishedContentCache? GetContentCache() =>
        _publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot)
            ? publishedSnapshot?.Content
            : null;

    private IPublishedContent? ContentOrNullIfDisallowed(IPublishedContent? content)
        => content != null && IsAllowedContentType(content)
            ? content
            : null;

    private bool IsAllowedContentType(IPublishedContent content)
        => _contentApiSettings.DisallowedContentTypeAliases.InvariantContains(content.ContentType.Alias) is false;
}
