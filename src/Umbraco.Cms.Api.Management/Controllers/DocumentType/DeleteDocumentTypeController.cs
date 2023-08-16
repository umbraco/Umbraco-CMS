using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
public class DeleteDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    public DeleteDocumentTypeController(IContentTypeService contentTypeService)
        => _contentTypeService = contentTypeService;

    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentTypeResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        // FIXME: create and use an async get method here.
        IContentType? contentType = _contentTypeService.Get(id);
        if (contentType == null)
        {
            return DocumentTypeNotFound();
        }

        // FIXME: create overload that accepts user key
        _contentTypeService.Delete(contentType, Constants.Security.SuperUserId);
        return await Task.FromResult(Ok());
    }
}
