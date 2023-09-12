﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.DeliveryApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[ApiVersion("1.0")]
public class QueryMediaApiController : MediaApiControllerBase
{
    private readonly IApiMediaQueryService _apiMediaQueryService;

    public QueryMediaApiController(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IApiMediaWithCropsResponseBuilder apiMediaWithCropsResponseBuilder,
        IApiMediaQueryService apiMediaQueryService)
        : base(publishedSnapshotAccessor, apiMediaWithCropsResponseBuilder)
        => _apiMediaQueryService = apiMediaQueryService;

    /// <summary>
    ///     Gets a paginated list of media item(s) from query.
    /// </summary>
    /// <param name="fetch">Optional fetch query parameter value.</param>
    /// <param name="filter">Optional filter query parameters values.</param>
    /// <param name="sort">Optional sort query parameters values.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>The paged result of the media item(s).</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ApiMediaWithCropsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Query(
        string? fetch,
        [FromQuery] string[] filter,
        [FromQuery] string[] sort,
        int skip = 0,
        int take = 10)
    {
        Attempt<PagedModel<Guid>, ApiMediaQueryOperationStatus> queryAttempt = _apiMediaQueryService.ExecuteQuery(fetch, filter, sort, skip, take);

        if (queryAttempt.Success is false)
        {
            return ApiMediaQueryOperationStatusResult(queryAttempt.Status);
        }

        PagedModel<Guid> pagedResult = queryAttempt.Result;
        IPublishedContent[] mediaItems = pagedResult.Items.Select(PublishedMediaCache.GetById).WhereNotNull().ToArray();

        var model = new PagedViewModel<ApiMediaWithCropsResponse>
        {
            Total = pagedResult.Total,
            Items = mediaItems.Select(BuildApiMediaWithCrops)
        };

        return await Task.FromResult(Ok(model));
    }
}
