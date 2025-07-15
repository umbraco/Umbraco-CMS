using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.ImportExport;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class ImportNewMediaTypeController : MediaTypeControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMediaTypeImportService _mediaTypeImportService;

    public ImportNewMediaTypeController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaTypeImportService mediaTypeImportService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mediaTypeImportService = mediaTypeImportService;
    }

    [HttpPost("import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import(
        CancellationToken cancellationToken,
        ImportMediaTypeRequestModel model)
    {
        Attempt<IMediaType?, MediaTypeImportOperationStatus> importAttempt = await _mediaTypeImportService.Import(model.File.Id, CurrentUserKey(_backOfficeSecurityAccessor));

        return importAttempt.Success is false
            ? MediaTypeImportOperationStatusResult(importAttempt.Status)
            : CreatedAtId<ByKeyMediaTypeController>(controller => nameof(controller.ByKey), importAttempt.Result!.Key);
    }
}
