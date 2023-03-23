using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Api.Content.Filters;
using Umbraco.Cms.Api.Content.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Controllers;

[ApiController]
[VersionedContentApiRoute("content")]
[ApiExplorerSettings(GroupName = "Content")]
[ApiVersion("1.0")]
[ContentApiAccess]
[JsonOptionsName(Constants.JsonOptionsNames.ContentApi)]
[LocalizeFromAcceptLanguageHeader]
public abstract class ContentApiControllerBase : Controller
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

    protected IApiContentBuilder ApiContentBuilder { get; }

    protected ContentApiControllerBase(IPublishedSnapshotAccessor publishedSnapshotAccessor, IApiContentBuilder apiContentBuilder)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;

        ApiContentBuilder = apiContentBuilder;
    }

    protected IPublishedContentCache? GetContentCache() =>
        _publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot)
            ? publishedSnapshot?.Content
            : null;

    protected ProblemDetails ContentCacheNotFoundProblemDetails() =>
        new ProblemDetailsBuilder()
            .WithTitle("Content cache is not available")
            .WithDetail("Could not retrieve the content cache. Umbraco may be in an error state.")
            .Build();
}
