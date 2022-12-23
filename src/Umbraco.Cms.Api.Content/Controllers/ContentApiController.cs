using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Content.Filters;
using Umbraco.Cms.Api.Content.Routing;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Content.Controllers;

[ContentApiJsonConfiguration]
[VersionedContentApiRoute("query")]
[ApiVersion("1.0")]
public class ContentApiController : Controller
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IApiContentBuilder _apiContentBuilder;

    public ContentApiController(IPublishedSnapshotAccessor publishedSnapshotAccessor, IApiContentBuilder apiContentBuilder)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _apiContentBuilder = apiContentBuilder;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IApiContent>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    // TODO: actually implement the content API, this is just to test the content API output rendering
    public async Task<IActionResult> Get(int skip = 0, int take = 100)
    {
        IPublishedContentCache? contentCache = _publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot)
            ? publishedSnapshot?.Content
            : null;
        if (contentCache == null)
        {
            // TODO: refactor - move ProblemDetailsBuilder to Common and reuse it here
            return BadRequest(
                new ProblemDetailsBuilder()
                    .WithTitle("Content cache is not available")
                    .WithDetail("Could not retrieve the content cache. Umbraco may be in an error state.")
                    .Build());
        }

        IApiContent[] result = contentCache
            .GetAtRoot()
            .Select(content => _apiContentBuilder.Build(content))
            .ToArray();

        return await Task.FromResult(
            Ok(
                new PagedViewModel<IApiContent>
                {
                    Items = result.Skip(skip).Take(take).ToArray(),
                    Total = result.Length
                }));
    }
}
