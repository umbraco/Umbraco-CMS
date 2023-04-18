using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Snippets;

public class GetAllController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IUmbracoMapper _umbracoMapper;

    public GetAllController(IPartialViewService partialViewService, IUmbracoMapper umbracoMapper)
    {
        _partialViewService = partialViewService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("snippets")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<PartialViewSnippetsViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(int skip = 0, int take = 100)
    {
        PagedModel<PartialViewSnippet> snippets = await _partialViewService.GetPartialViewSnippetsAsync(skip, take);

        IEnumerable<PartialViewSnippetsViewModel> viewModels = snippets.Items.Select(snippet => _umbracoMapper.Map<PartialViewSnippetsViewModel>(snippet))!;
        var pageViewModel = new PagedViewModel<PartialViewSnippetsViewModel>
        {
            Total = snippets.Total,
            Items = viewModels,
        };

        return Ok(pageViewModel);
    }
}
