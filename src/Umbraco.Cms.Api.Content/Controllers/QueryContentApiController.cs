using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Content.Controllers;

public class QueryContentApiController : ContentApiControllerBase
{
    private readonly IApiQueryService _apiQueryService;
    private readonly IRequestRoutingService _requestRoutingService;

    public QueryContentApiController(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IApiContentBuilder apiContentBuilder,
        IApiQueryService apiQueryService,
        IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, apiContentBuilder)
    {
        _apiQueryService = apiQueryService;
        _requestRoutingService = requestRoutingService;
    }

    /// <summary>
    ///     Gets content item(s) from query.
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiContent), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Query(string? fetch, [FromQuery] string[] filter, [FromQuery] string[] sort)
    {
        IPublishedContentCache? contentCache = GetContentCache();
        if (contentCache is null)
        {
            return BadRequest(ContentCacheNotFoundProblemDetails());
        }

        IEnumerable<Guid> ids = Enumerable.Empty<Guid>();
        ids = _apiQueryService.ExecuteQuery(fetch, filter, sort);

        IEnumerable<IPublishedContent> contentItems = ids.Select(contentCache.GetById).WhereNotNull();

        IEnumerable<IApiContent> results = contentItems.Select(ApiContentBuilder.Build);

        return Ok(results);
    }
}
