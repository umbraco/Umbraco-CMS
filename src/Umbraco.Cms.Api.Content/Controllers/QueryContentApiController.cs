using System.Web;
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
    public async Task<IActionResult> Query(string? fetch, string? filter, string? sort)
    {
        HttpContext? context = _httpContextAccessor.HttpContext;
        if (context is null || (fetch is null && filter is null && sort is null))
        {
            return BadRequest("Not implemented yet.");
        }

        IPublishedContentCache? contentCache = GetContentCache();
        if (contentCache is null)
        {
            return BadRequest(ContentCacheNotFoundProblemDetails());
        }

        Guid? id = GetGuidFromQuery(fetch!, contentCache);
        if (id is null)
        {
            return BadRequest("Invalid query value format.");
        }

        IEnumerable<Guid> ids = Enumerable.Empty<Guid>();

        string query = HttpUtility.UrlDecode(context.Request.QueryString.Value!.Substring(1));
        var queryParams = query.Split('&')
            .Select(p => p.Split('='))
            .ToDictionary(p => p[0], p => p[1]);
        ids = _apiQueryService.ExecuteQuery(queryParams, ((Guid)id).ToString());

        IEnumerable<IPublishedContent> contentItems = ids.Select(contentCache.GetById)
                                                         .WhereNotNull()
                                                         .OrderBy(x => x.Path)
                                                         .ThenBy(c => c.SortOrder);

        // Currently sorting is not supported through the ContentAPI index
        // So we need to add the name to it
        if (sort is not null && sort.StartsWith("name"))
        {
            string sortValue = sort.Substring(sort.IndexOf(':', StringComparison.Ordinal) + 1);
            if (sortValue.StartsWith("asc"))
            {
                contentItems = contentItems.OrderBy(x => x.Name);
            }
            else
            {
                contentItems = contentItems.OrderByDescending(x => x.Name);
            }
        }

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
