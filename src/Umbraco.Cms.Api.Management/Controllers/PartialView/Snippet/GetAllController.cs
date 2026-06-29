using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Snippets;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Snippet;

/// <summary>
/// Controller for retrieving all partial view snippets.
/// </summary>
[ApiVersion("1.0")]
public class GetAllController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllController"/> class, responsible for handling requests to retrieve all partial view snippets.
    /// </summary>
    /// <param name="partialViewService">Service used to manage partial views.</param>
    /// <param name="umbracoMapper">The mapper used for converting between Umbraco models and API models.</param>
    public GetAllController(IPartialViewService partialViewService, IUmbracoMapper umbracoMapper)
    {
        _partialViewService = partialViewService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a paginated collection of available partial view code snippets that can be used when creating new partial views.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{PartialViewSnippetItemResponseModel}"/> representing the paginated collection of partial view snippets.
    /// </returns>
    [HttpGet("snippet")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<PartialViewSnippetItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of partial view snippets.")]
    [EndpointDescription("Gets a paginated collection of available partial view code snippets that can be used when creating new partial views.")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        PagedModel<PartialViewSnippetSlim> snippets = await _partialViewService.GetSnippetsAsync(skip, take);

        var pageViewModel = new PagedViewModel<PartialViewSnippetItemResponseModel>
        {
            Total = snippets.Total,
            Items = _umbracoMapper.MapEnumerable<PartialViewSnippetSlim, PartialViewSnippetItemResponseModel>(snippets.Items)
        };

        return Ok(pageViewModel);
    }
}
