using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class ExportDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IUdtFileContentFactory _fileContentFactory;

    public ExportDocumentTypeController(
        IContentTypeService contentTypeService,
        IUdtFileContentFactory fileContentFactory)
    {
        _contentTypeService = contentTypeService;
        _fileContentFactory = fileContentFactory;
    }

    [HttpGet("{id:guid}/export")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Export(
        CancellationToken cancellationToken,
        Guid id)
    {
        IContentType? contentType = _contentTypeService.Get(id);
        if (contentType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        return _fileContentFactory.Create(contentType);
    }
}
