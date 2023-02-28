using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Content.Controllers;

public class QueryContentApiController : ContentApiControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApiQueryService _apiQueryService;

    public QueryContentApiController(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IApiContentBuilder apiContentBuilder,
        IHttpContextAccessor httpContextAccessor,
        IApiQueryService apiQueryService)
        : base(publishedSnapshotAccessor, apiContentBuilder)
    {
        _httpContextAccessor = httpContextAccessor;
        _apiQueryService = apiQueryService;
    }

    /// <summary>
    ///     Gets content item(s) from query.
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiContent), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Query()
    {
        HttpContext? context = _httpContextAccessor.HttpContext;
        if (context is null || !context.Request.Query.TryGetValue("fetch", out StringValues queryValue))
        {
            return BadRequest("Missing 'fetch' query parameter.");
        }

        var queryOption = queryValue.ToString();

        ApiQueryType queryType = _apiQueryService.GetQueryType(queryOption);
        if (queryType == ApiQueryType.Unknown)
        {
            return BadRequest("Invalid value for 'fetch' query parameter.");
        }

        Guid? id = _apiQueryService.GetGuidFromFetch(queryOption);
        if (id is null)
        {
            return BadRequest("Invalid GUID format.");
        }

        IEnumerable<Guid> ids = _apiQueryService.GetGuidsFromQuery((Guid)id, queryType);

        IPublishedContentCache? contentCache = GetContentCache();

        if (contentCache is null)
        {
            return BadRequest(ContentCacheNotFoundProblemDetails());
        }

        IEnumerable<IPublishedContent> contentItems = ids.Select(contentCache.GetById).WhereNotNull();

        IEnumerable<IApiContent> results = contentItems.Select(ApiContentBuilder.Build);

        return Ok(results);
    }
}
