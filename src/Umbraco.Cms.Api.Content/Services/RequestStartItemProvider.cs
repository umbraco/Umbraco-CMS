using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Content.Services;

internal sealed class RequestStartItemProvider : RequestHeaderHandler, IRequestStartItemProvider
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

    // this provider lifetime is Scope, so we can cache this as a field
    private IPublishedContent? _requestedStartContent;

    public RequestStartItemProvider(
        IHttpContextAccessor httpContextAccessor,
        IPublishedSnapshotAccessor publishedSnapshotAccessor)
        : base(httpContextAccessor) =>
        _publishedSnapshotAccessor = publishedSnapshotAccessor;

    /// <inheritdoc/>
    public IPublishedContent? GetStartItem()
    {
        if (_requestedStartContent != null)
        {
            return _requestedStartContent;
        }

        var headerValue = GetHeaderValue("Start-Item");
        if (headerValue.IsNullOrWhiteSpace())
        {
            return null;
        }

        if (_publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot) == false || publishedSnapshot?.Content == null)
        {
            return null;
        }

        IEnumerable<IPublishedContent> rootContent = publishedSnapshot.Content.GetAtRoot();

        _requestedStartContent = Guid.TryParse(headerValue, out Guid key)
            ? rootContent.FirstOrDefault(c => c.Key == key)
            : rootContent.FirstOrDefault(c => c.UrlSegment == headerValue);

        return _requestedStartContent;
    }
}
