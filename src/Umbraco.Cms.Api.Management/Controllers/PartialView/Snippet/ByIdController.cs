using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Snippets;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Snippet;

[ApiVersion("1.0")]
public class ByIdController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByIdController(IPartialViewService partialViewService, IUmbracoMapper umbracoMapper)
    {
        _partialViewService = partialViewService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("snippet/{id}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PartialViewSnippetResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        CancellationToken cancellationToken,
        string id)
    {
        PartialViewSnippet? snippet = await _partialViewService.GetSnippetAsync(id);
        return snippet is not null
            ? Ok(_umbracoMapper.Map<PartialViewSnippetResponseModel>(snippet)!)
            : PartialViewNotFound();
    }
}
