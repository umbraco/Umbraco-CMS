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
    /// Provides API endpoints for managing redirect URLs identified by their unique key.
    /// </summary>
[ApiVersion("1.0")]
public class ByKeyRedirectUrlManagementController : RedirectUrlManagementControllerBase
{
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IRedirectUrlPresentationFactory _redirectUrlPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyRedirectUrlManagementController"/> class.
    /// </summary>
    /// <param name="redirectUrlService">Service used to manage redirect URLs.</param>
    /// <param name="redirectUrlPresentationFactory">Factory used to create redirect URL presentation models.</param>
    public ByKeyRedirectUrlManagementController(
        IRedirectUrlService redirectUrlService,
        IRedirectUrlPresentationFactory redirectUrlPresentationFactory)
    {
        _redirectUrlService = redirectUrlService;
        _redirectUrlPresentationFactory = redirectUrlPresentationFactory;
    }

    /// <summary>
    /// Gets a paged list of redirect URLs associated with the content item identified by the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the content item for which to retrieve redirect URLs.</param>
    /// <param name="skip">The number of items to skip in the result set (for paging).</param>
    /// <param name="take">The number of items to take in the result set (for paging).</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a <see cref="PagedViewModel{T}"/> of <see cref="RedirectUrlResponseModel"/>.
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PagedViewModel<RedirectUrlResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a redirect URL.")]
    [EndpointDescription("Gets a redirect URL identified by the provided Id.")]
    public Task<ActionResult<PagedViewModel<RedirectUrlResponseModel>>> ByKey(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 100)
    {
        IRedirectUrl[] redirects = _redirectUrlService.GetContentRedirectUrls(id).ToArray();

        IEnumerable<RedirectUrlResponseModel> viewModels = _redirectUrlPresentationFactory.CreateMany(redirects);

        return Task.FromResult<ActionResult<PagedViewModel<RedirectUrlResponseModel>>>(new PagedViewModel<RedirectUrlResponseModel>
        {
            Items = viewModels.Skip(skip).Take(take),
            Total = redirects.Length,
        });
    }
}
