using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Snippet;

[ApiVersion("1.0")]
public class GetAllController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;

    public GetAllController(IPartialViewService partialViewService) => _partialViewService = partialViewService;

    [HttpGet("snippet")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SnippetItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(int skip = 0, int take = 100)
    {
        PagedModel<string> snippets = await _partialViewService.GetSnippetNamesAsync(skip, take);

        var pageViewModel = new PagedViewModel<SnippetItemResponseModel>
        {
            Total = snippets.Total,
            Items = snippets.Items.Select(x => new SnippetItemResponseModel(x)),
        };

        return Ok(pageViewModel);
    }
}
