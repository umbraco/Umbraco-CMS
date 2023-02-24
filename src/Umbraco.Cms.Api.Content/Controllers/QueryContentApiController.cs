using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.PublishedCache;

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
        const string childrenSpecifier = "children:";

        if (context is null || !context.Request.Query.TryGetValue("fetch", out StringValues queryValue))
        {
            return BadRequest("Missing 'fetch' query parameter.");
        }

        var queryOption = queryValue.ToString();
        if (!queryOption.StartsWith(childrenSpecifier, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Invalid value for 'fetch' query parameter.");
        }

        var guidString = queryOption.Substring(childrenSpecifier.Length);
        if (!Guid.TryParse(guidString, out Guid id))
        {
            return BadRequest("Invalid GUID format.");
        }

        IEnumerable<Guid> results = _apiQueryService.GetChildren(id);
        return Ok(results);
    }
}
