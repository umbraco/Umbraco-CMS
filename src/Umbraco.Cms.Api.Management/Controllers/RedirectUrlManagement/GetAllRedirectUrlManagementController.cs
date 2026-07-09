using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

/// <summary>
/// Controller for retrieving all redirect URL management entries.
/// </summary>
[ApiVersion("1.0")]
public class GetAllRedirectUrlManagementController : RedirectUrlManagementControllerBase
{
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IRedirectUrlPresentationFactory _redirectUrlPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllRedirectUrlManagementController"/> class, which manages retrieval of all redirect URLs.
    /// </summary>
    /// <param name="redirectUrlService">Service used to manage and retrieve redirect URLs.</param>
    /// <param name="redirectUrlPresentationFactory">Factory for creating presentation models for redirect URLs.</param>
    public GetAllRedirectUrlManagementController(
        IRedirectUrlService redirectUrlService,
        IRedirectUrlPresentationFactory redirectUrlPresentationFactory)
    {
        _redirectUrlService = redirectUrlService;
        _redirectUrlPresentationFactory = redirectUrlPresentationFactory;
    }

    /// <summary>
    /// Retrieves a paginated list of redirect URLs, optionally filtered by a search string.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="filter">An optional string to filter redirect URLs by matching criteria; if null, all redirect URLs are returned.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return in the result set (used for pagination).</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a <see cref="PagedViewModel{RedirectUrlResponseModel}"/> holding the filtered and paginated redirect URLs.
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedViewModel<RedirectUrlResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of redirect URLs.")]
    [EndpointDescription("Gets a paginated collection of redirect URLs with support for filtering and sorting.")]
    public Task<ActionResult<PagedViewModel<RedirectUrlResponseModel>>> GetAll(
        CancellationToken cancellationToken,
        string? filter,
        int skip = 0,
        int take = 100)
    {
        long total;
        IEnumerable<IRedirectUrl> redirects = filter is null
            ? _redirectUrlService.GetAllRedirectUrls(skip, take, out total)
            : _redirectUrlService.SearchRedirectUrls(filter, skip, take, out total);

        IEnumerable<RedirectUrlResponseModel> redirectViewModels = _redirectUrlPresentationFactory.CreateMany(redirects);
        return Task.FromResult<ActionResult<PagedViewModel<RedirectUrlResponseModel>>>(new PagedViewModel<RedirectUrlResponseModel> { Items = redirectViewModels, Total = total });
    }
}
