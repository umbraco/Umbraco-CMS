using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.ImportExport;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class ImportExistingMemberTypeController : MemberTypeControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMemberTypeImportService _memberTypeImportService;

    public ImportExistingMemberTypeController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberTypeImportService memberTypeImportService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _memberTypeImportService = memberTypeImportService;
    }

    [HttpPut("{id:guid}/import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import(
        CancellationToken cancellationToken,
        Guid id,
        ImportMemberTypeRequestModel model)
    {
        Attempt<IMemberType?, MemberTypeImportOperationStatus> importAttempt = await _memberTypeImportService.Import(model.File.Id, CurrentUserKey(_backOfficeSecurityAccessor));

        return importAttempt.Success is false
            ? MemberTypeImportOperationStatusResult(importAttempt.Status)
            : Ok();
    }
}
