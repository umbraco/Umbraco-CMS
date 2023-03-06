using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Api.Content.Services;
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
    private readonly IRequestRoutingService _requestRoutingService;

    public QueryContentApiController(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IApiContentBuilder apiContentBuilder,
        IHttpContextAccessor httpContextAccessor,
        IApiQueryService apiQueryService,
        IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, apiContentBuilder)
    {
        _httpContextAccessor = httpContextAccessor;
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
    public async Task<IActionResult> Query()
    {
        HttpContext? context = _httpContextAccessor.HttpContext;
        if (context is null || !context.Request.Query.TryGetValue("fetch", out StringValues queryValue))
        {
            return BadRequest("Missing 'fetch' query parameter. Alternatives are not implemented yet.");
        }

        var queryOption = queryValue.ToString();

        ApiQueryType queryType = _apiQueryService.GetQueryType(queryOption);
        if (queryType == ApiQueryType.Unknown)
        {
            return BadRequest("Invalid value for 'fetch' query parameter.");
        }

        IPublishedContentCache? contentCache = GetContentCache();
        if (contentCache is null)
        {
            return BadRequest(ContentCacheNotFoundProblemDetails());
        }

        //Guid? id = _apiQueryService.GetGuidFromFetch(queryOption); // Remove
        Guid? id = GetGuidFromQuery(queryOption, contentCache);
        if (id is null)
        {
            return BadRequest("Invalid query value format.");
        }

        IEnumerable<Guid> ids = _apiQueryService.GetGuidsFromQuery((Guid)id, queryType);

        IEnumerable<IPublishedContent> contentItems = ids.Select(contentCache.GetById)
                                                         .WhereNotNull()
                                                         .OrderBy(x => x.Path)
                                                         .ThenBy(c => c.SortOrder);

        IEnumerable<IApiContent> results = contentItems.Select(ApiContentBuilder.Build);

        return Ok(results);
    }

    private Guid? GetGuidFromQuery(string fetchQuery, IPublishedContentCache contentCache)
    {
        var queryStringValue = fetchQuery.Substring(fetchQuery.IndexOf(':', StringComparison.Ordinal) + 1);

        if (Guid.TryParse(queryStringValue, out Guid id))
        {
            return id;
        }

        // Check if the passed value is a path of a content item
        var contentRoute = _requestRoutingService.GetContentRoute(queryStringValue);

        IPublishedContent? contentItem = contentCache.GetByRoute(contentRoute);

        if (contentItem is not null)
        {
            return contentItem.Key;
        }

        return null;
    }
}
