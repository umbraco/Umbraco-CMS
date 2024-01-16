using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Controllers.Content;

[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class QueryContentApiController : ContentApiControllerBase
{
    private readonly IRequestMemberAccessService _requestMemberAccessService;
    private readonly IApiContentQueryService _apiContentQueryService;

    [Obsolete($"Please use the constructor that accepts {nameof(IRequestMemberAccessService)}. Will be removed in V14.")]
    public QueryContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilderBuilder,
        IApiContentQueryService apiContentQueryService)
        : this(
            apiPublishedContentCache,
            apiContentResponseBuilderBuilder,
            apiContentQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IRequestMemberAccessService>())
    {
    }

    [ActivatorUtilitiesConstructor]
    public QueryContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilderBuilder,
        IApiContentQueryService apiContentQueryService,
        IRequestMemberAccessService requestMemberAccessService)
        : base(apiPublishedContentCache, apiContentResponseBuilderBuilder)
    {
        _apiContentQueryService = apiContentQueryService;
        _requestMemberAccessService = requestMemberAccessService;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IApiContentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Obsolete("Please use version 2 of this API. Will be removed in V15.")]
    public async Task<IActionResult> Query(
        string? fetch,
        [FromQuery] string[] filter,
        [FromQuery] string[] sort,
        int skip = 0,
        int take = 10)
        => await HandleRequest(fetch, filter, sort, skip, take);

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
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(PagedViewModel<IApiContentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QueryV20(
        string? fetch,
        [FromQuery] string[] filter,
        [FromQuery] string[] sort,
        int skip = 0,
        int take = 10)
        => await HandleRequest(fetch, filter, sort, skip, take);

    private async Task<IActionResult> HandleRequest(string? fetch, string[] filter, string[] sort, int skip, int take)
    {
        ProtectedAccess protectedAccess = await _requestMemberAccessService.MemberAccessAsync();
        Attempt<PagedModel<Guid>, ApiContentQueryOperationStatus> queryAttempt = _apiContentQueryService.ExecuteQuery(fetch, filter, sort, protectedAccess, skip, take);

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

        return Ok(model);
    }
}
