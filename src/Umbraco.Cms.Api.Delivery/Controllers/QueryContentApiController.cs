using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[ApiVersion("1.0")]
public class QueryContentApiController : ContentApiControllerBase
{
    private readonly IApiContentQueryService _apiContentQueryService;

    public QueryContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilderBuilder,
        IApiContentQueryService apiContentQueryService)
        : base(apiPublishedContentCache, apiContentResponseBuilderBuilder)
        => _apiContentQueryService = apiContentQueryService;

    /// <summary>
    ///     Gets a paginated list of content item(s) from query.
    /// </summary>
    /// <param name="fetch">Optional fetch query parameter value.</param>
    /// <param name="filter">Optional filter query parameters values.</param>
    /// <param name="sort">Optional sort query parameters values.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>The paged result of the content item(s).</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IApiContentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Query(
        string? fetch,
        [FromQuery] string[] filter,
        [FromQuery] string[] sort,
        int skip = 0,
        int take = 10)
    {
        Attempt<PagedModel<Guid>, ApiContentQueryOperationStatus> queryAttempt = _apiContentQueryService.ExecuteQuery(fetch, filter, sort, skip, take);

        if (queryAttempt.Success is false)
        {
            return ApiContentQueryOperationStatusResult(queryAttempt.Status);
        }

        PagedModel<Guid> pagedResult = queryAttempt.Result;
        IEnumerable<IPublishedContent> contentItems = ApiPublishedContentCache.GetByIds(pagedResult.Items);
        IApiContentResponse[] apiContentItems = contentItems.Select(ApiContentResponseBuilder.Build).WhereNotNull().ToArray();

        var model = new PagedViewModel<IApiContentResponse>
        {
            Total = pagedResult.Total,
            Items = apiContentItems
        };

        return await Task.FromResult(Ok(model));
    }
}
