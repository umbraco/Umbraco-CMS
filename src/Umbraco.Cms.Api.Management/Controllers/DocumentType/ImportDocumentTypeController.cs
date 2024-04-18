using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.ImportExport;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
public class ImportDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentTypeImportService _contentTypeImportService;

    public ImportDocumentTypeController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeImportService contentTypeImportService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentTypeImportService = contentTypeImportService;
    }

    [HttpPost("import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import(
        CancellationToken cancellationToken,
        ImportDocumentTypeRequestModel model)
    {
        IUser? user = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        if (user is null)
        {
            return Unauthorized();
        }

        Attempt<IContentType?, ContentTypeImportOperationStatus> importAttempt = await _contentTypeImportService.Import(model.File.Id, user.Id, model.OverWriteExisting);

        return importAttempt.Success is false
            ? ContentTypeImportOperationStatusResult(importAttempt.Status)
            : importAttempt.Status == ContentTypeImportOperationStatus.SuccessCreated
                ? CreatedAtId<ByKeyDocumentTypeController>(controller => nameof(controller.ByKey), importAttempt.Result!.Key)
                : AvailableAtId<ByKeyDocumentTypeController>(controller => nameof(controller.ByKey), importAttempt.Result!.Key);
    }
}
