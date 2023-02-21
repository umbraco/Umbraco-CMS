using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class ByKeyDocumentController : DocumentControllerBase
{
    private readonly IContentEditingService _contentEditingService;
    private readonly IDocumentViewModelFactory _documentViewModelFactory;

    public ByKeyDocumentController(IContentEditingService contentEditingService, IDocumentViewModelFactory documentViewModelFactory)
    {
        _contentEditingService = contentEditingService;
        _documentViewModelFactory = documentViewModelFactory;
    }

    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(Guid key)
    {
        IContent? content = await _contentEditingService.GetAsync(key);
        if (content == null)
        {
            return NotFound();
        }

        DocumentViewModel model = await _documentViewModelFactory.CreateViewModelAsync(content);
        return Ok(model);
    }
}
