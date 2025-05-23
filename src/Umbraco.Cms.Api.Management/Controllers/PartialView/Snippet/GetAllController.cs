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

[ApiVersion("1.0")]
public class GetAllController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IUmbracoMapper _umbracoMapper;

    public GetAllController(IPartialViewService partialViewService, IUmbracoMapper umbracoMapper)
    {
        _partialViewService = partialViewService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("snippet")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<PartialViewSnippetItemResponseModel>), StatusCodes.Status200OK)]
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
