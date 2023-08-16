using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Snippets;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Snippet;

[ApiVersion("1.0")]
public class ByNameController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByNameController(IPartialViewService partialViewService, IUmbracoMapper umbracoMapper)
    {
        _partialViewService = partialViewService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("snippet/{name}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PartialViewSnippetResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name)
    {
        PartialViewSnippet? snippet = await _partialViewService.GetSnippetByNameAsync(name);

        if (snippet is null)
        {
            return PartialViewNotFound();
        }

        PartialViewSnippetResponseModel? viewModel = _umbracoMapper.Map<PartialViewSnippetResponseModel>(snippet);

        return Ok(viewModel);
    }
}
