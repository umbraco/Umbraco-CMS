using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Content.Controllers;

public class QueryContentApiController : ContentApiControllerBase
{
    private readonly IApiQueryService _apiQueryService;

    public QueryContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilderBuilder,
        IApiQueryService apiQueryService)
        : base(apiPublishedContentCache, apiContentResponseBuilderBuilder)
        => _apiQueryService = apiQueryService;

    /// <summary>
    ///     Gets content item(s) from query.
    /// </summary>
    /// <param name="fetch">Optional fetch query parameter value.</param>
    /// <param name="filter">Optional filter query parameters values.</param>
    /// <param name="sort">Optional sort query parameters values.</param>
    /// <returns>The content item(s) or empty collection.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<IApiContentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Query(string? fetch, [FromQuery] string[] filter, [FromQuery] string[] sort)
    {
        IEnumerable<Guid> ids = _apiQueryService.ExecuteQuery(fetch, filter, sort);
        IEnumerable<IPublishedContent> contentItems = ApiPublishedContentCache.GetByIds(ids);
        IEnumerable<IApiContentResponse> results = contentItems.Select(ApiContentResponseBuilder.Build);

        return await Task.FromResult(Ok(results));
    }
}
