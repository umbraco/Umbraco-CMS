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
/// Provides API endpoints for importing existing member types into the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class ImportExistingMemberTypeController : MemberTypeControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMemberTypeImportService _memberTypeImportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportExistingMemberTypeController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">An accessor that provides the current back office security context.</param>
    /// <param name="memberTypeImportService">The service responsible for importing existing member types.</param>
    public ImportExistingMemberTypeController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberTypeImportService memberTypeImportService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _memberTypeImportService = memberTypeImportService;
    }

    /// <summary>
    /// Imports an existing member type using data from a provided file upload.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the member type to import into.</param>
    /// <param name="model">The request model containing the file information for the import operation.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the import operation:
    /// returns <c>200 OK</c> if successful, <c>400 Bad Request</c> if the import fails due to invalid data, or <c>404 Not Found</c> if the member type does not exist.
    /// </returns>
    [HttpPut("{id:guid}/import")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Imports a member type.")]
    [EndpointDescription("Imports a member type from the provided file upload.")]
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
