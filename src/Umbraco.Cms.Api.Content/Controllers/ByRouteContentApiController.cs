using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Controllers;

public class ByRouteContentApiController : ContentApiControllerBase
{
    private readonly GlobalSettings _globalSettings;
    private readonly IStartNodeService _startNodeService;

    public ByRouteContentApiController(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IApiContentBuilder apiContentBuilder,
        IOptions<GlobalSettings> globalSettings,
        IStartNodeService startNodeService)
        : base(publishedSnapshotAccessor, apiContentBuilder)
    {
        _globalSettings = globalSettings.Value;
        _startNodeService = startNodeService;
    }

    /// <summary>
    ///     Gets a content item by route.
    /// </summary>
    /// <param name="path">The path to the content item.</param>
    /// <remarks>
    ///     Optional path for the start node of the content item
    ///     can be added through a "start-node" header.
    /// </remarks>
    /// <returns>The content item or not found result.</returns>
    [HttpGet("{path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiContent), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByRoute(string path)
    {
        IPublishedContentCache? contentCache = GetContentCache();

        if (contentCache is null)
        {
            return BadRequest(ContentCacheNotFoundProblemDetails());
        }

        var decodedPath = ConstructRoute(path);

        IPublishedContent? contentItem = contentCache.GetByRoute(decodedPath);

        if (contentItem is null)
        {
            return NotFound();
        }

        return await Task.FromResult(Ok(ApiContentBuilder.Build(contentItem)));
    }

    // Decode the node path and check "start-node" header if the top level node is not hidden
    private string ConstructRoute(string path)
    {
        var decodedPath = $"/{WebUtility.UrlDecode(path).TrimStart(Constants.CharArrays.ForwardSlash)}";

        if (_globalSettings.HideTopLevelNodeFromPath == false)
        {
            // Construct the path, using the value from "start-node" header
            string? startNodePath = _startNodeService.GetStartNode();

            if (startNodePath is not null)
            {
                var combinedPath = $"/{startNodePath}{decodedPath}";
                return combinedPath;
            }
        }

        return decodedPath;
    }
}
