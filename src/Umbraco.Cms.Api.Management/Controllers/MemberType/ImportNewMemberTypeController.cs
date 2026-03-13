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

/// <summary>
/// Provides API endpoints for importing new member types into the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class ImportNewMemberTypeController : MemberTypeControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMemberTypeImportService _memberTypeImportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportNewMemberTypeController"/> class, used to handle import operations for member types.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features.</param>
    /// <param name="memberTypeImportService">Service responsible for importing member types.</param>
    public ImportNewMemberTypeController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberTypeImportService memberTypeImportService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _memberTypeImportService = memberTypeImportService;
    }

    /// <summary>
    /// Imports a new member type from a file provided in the request model.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="model">The <see cref="ImportMemberTypeRequestModel"/> containing the file information for the member type to import.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that returns <c>201 Created</c> with the imported member type's key on success, or a <c>400 Bad Request</c> or <c>404 Not Found</c> with problem details if the import fails.
    /// </returns>
    [HttpPost("import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Imports a member type.")]
    [EndpointDescription("Imports a member type from the provided file upload.")]
    public async Task<IActionResult> Import(
        CancellationToken cancellationToken,
        ImportMemberTypeRequestModel model)
    {
        Attempt<IMemberType?, MemberTypeImportOperationStatus> importAttempt = await _memberTypeImportService.Import(model.File.Id, CurrentUserKey(_backOfficeSecurityAccessor));

        return importAttempt.Success is false
            ? MemberTypeImportOperationStatusResult(importAttempt.Status)
            : CreatedAtId<ByKeyMemberTypeController>(controller => nameof(controller.ByKey), importAttempt.Result!.Key);
    }
}
