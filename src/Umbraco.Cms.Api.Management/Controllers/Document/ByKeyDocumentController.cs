using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class ByKeyDocumentController : DocumentControllerBase
{
    private readonly IContentService _contentService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByKeyDocumentController(IContentService contentService, IUmbracoMapper umbracoMapper)
    {
        _contentService = contentService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentViewModel>> ByKey(Guid key)
    {
        IContent? content = _contentService.GetById(key);
        if (content == null)
        {
            return NotFound();
        }

        DocumentViewModel model = _umbracoMapper.Map<DocumentViewModel>(content)!;
        return await Task.FromResult(Ok(model));
    }
}
