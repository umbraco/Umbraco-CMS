using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.ImportExport;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class ImportExistingDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentTypeImportService _contentTypeImportService;

    public ImportExistingDocumentTypeController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeImportService contentTypeImportService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentTypeImportService = contentTypeImportService;
    }

    [HttpPut("{id:guid}/import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import(
        CancellationToken cancellationToken,
        Guid id,
        ImportDocumentTypeRequestModel model)
    {
        Attempt<IContentType?, ContentTypeImportOperationStatus> importAttempt = await _contentTypeImportService.Import(model.File.Id, CurrentUserKey(_backOfficeSecurityAccessor), id);

        return importAttempt.Success is false
            ? ContentTypeImportOperationStatusResult(importAttempt.Status)
            : Ok();
    }
}
