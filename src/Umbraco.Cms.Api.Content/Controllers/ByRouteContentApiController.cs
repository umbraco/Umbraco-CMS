using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Controllers;

public class ByRouteContentApiController : ContentApiControllerBase
{
    public ByRouteContentApiController(IPublishedSnapshotAccessor publishedSnapshotAccessor, IApiContentBuilder apiContentBuilder)
        : base(publishedSnapshotAccessor, apiContentBuilder)
    {
    }

    /// <summary>
    ///     Gets a content item by route.
    /// </summary>
    /// <param name="url">The path to the content item.</param>
    /// <returns>The content item or not found result.</returns>
    [HttpGet("{url}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiContent), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByRoute(string url)
    {
        IPublishedContentCache? contentCache = GetContentCache();

        if (contentCache is null)
        {
            return BadRequest(ContentCacheNotFoundProblemDetails());
        }

        var decodedPath = $"/{WebUtility.UrlDecode(url).TrimStart(Constants.CharArrays.ForwardSlash)}";

        IPublishedContent? contentItem = contentCache.GetByRoute(decodedPath);

        if (contentItem is null)
        {
            return NotFound();
        }

        return await Task.FromResult(Ok(ApiContentBuilder.Build(contentItem)));
    }
}
