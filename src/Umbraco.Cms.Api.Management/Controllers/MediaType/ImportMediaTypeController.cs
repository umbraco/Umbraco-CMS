using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.ImportExport;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiVersion("1.0")]
public class ImportMediaTypeController : MediaTypeControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMediaTypeImportService _mediaTypeImportService;

    public ImportMediaTypeController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaTypeImportService mediaTypeImportService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mediaTypeImportService = mediaTypeImportService;
    }

    [HttpPost("import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import(
        CancellationToken cancellationToken,
        ImportMediaTypeRequestModel model)
    {
        IUser? user = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        if (user is null)
        {
            return Unauthorized();
        }

        Attempt<IMediaType?, MediaTypeImportOperationStatus> importAttempt = await _mediaTypeImportService.Import(model.File.Id, user.Id, model.OverWriteExisting);

        return importAttempt.Success is false
            ? MediaTypeImportOperationStatusResult(importAttempt.Status)
            : importAttempt.Status == MediaTypeImportOperationStatus.SuccessCreated
                ? CreatedAtId<ByKeyMediaTypeController>(controller => nameof(controller.ByKey), importAttempt.Result!.Key)
                : AvailableAtId<ByKeyMediaTypeController>(controller => nameof(controller.ByKey), importAttempt.Result!.Key);
    }
}
