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
public class ImportNewDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentTypeImportService _contentTypeImportService;

    public ImportNewDocumentTypeController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeImportService contentTypeImportService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentTypeImportService = contentTypeImportService;
    }

    [HttpPost("import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import(
        CancellationToken cancellationToken,
        ImportDocumentTypeRequestModel model)
    {

        Attempt<IContentType?, ContentTypeImportOperationStatus> importAttempt = await _contentTypeImportService.Import(model.File.Id, CurrentUserKey(_backOfficeSecurityAccessor));

        return importAttempt.Success is false
            ? ContentTypeImportOperationStatusResult(importAttempt.Status)
            : CreatedAtId<ByKeyDocumentTypeController>(
                controller => nameof(controller.ByKey),
                importAttempt.Result!.Key);
    }
}
