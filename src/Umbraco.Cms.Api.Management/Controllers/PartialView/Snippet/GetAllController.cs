using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Snippet;

[ApiVersion("1.0")]
public class GetAllController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IPartialViewSnippetPresentationFactory _partialViewSnippetPresentationFactory;

    public GetAllController(IPartialViewService partialViewService, IPartialViewSnippetPresentationFactory partialViewSnippetPresentationFactory)
    {
        _partialViewService = partialViewService;
        _partialViewSnippetPresentationFactory = partialViewSnippetPresentationFactory;
    }

    [HttpGet("snippet")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<PartialViewSnippetItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(int skip = 0, int take = 100)
    {
        PagedModel<string> snippets = await _partialViewService.GetSnippetNamesAsync(skip, take);

        var pageViewModel = new PagedViewModel<PartialViewSnippetItemResponseModel>
        {
            Total = snippets.Total,
            Items = snippets.Items.Select(_partialViewSnippetPresentationFactory.CreateSnippetItemResponseModel).ToArray(),
        };

        return Ok(pageViewModel);
    }
}
