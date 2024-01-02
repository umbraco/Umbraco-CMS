using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Snippets;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Snippet;

[ApiVersion("1.0")]
public class ByFileNameController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IPartialViewSnippetPresentationFactory _partialViewSnippetPresentationFactory;

    public ByFileNameController(IPartialViewService partialViewService, IPartialViewSnippetPresentationFactory partialViewSnippetPresentationFactory)
    {
        _partialViewService = partialViewService;
        _partialViewSnippetPresentationFactory = partialViewSnippetPresentationFactory;
    }

    [HttpGet("snippet/{fileName}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PartialViewSnippetResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByFileName(string fileName)
    {
        PartialViewSnippet? snippet = await _partialViewService.GetSnippetByNameAsync(fileName);
        return snippet is not null
            ? Ok(_partialViewSnippetPresentationFactory.CreateSnippetResponseModel(snippet))
            : PartialViewNotFound();
    }
}
