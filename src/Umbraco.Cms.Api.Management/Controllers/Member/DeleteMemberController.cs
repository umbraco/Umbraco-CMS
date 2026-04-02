using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Member;

/// <summary>
/// Provides API endpoints for deleting members in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class DeleteMemberController : MemberControllerBase
{
    private readonly IMemberEditingService _memberEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMemberController"/> class, which handles member deletion operations.
    /// </summary>
    /// <param name="memberEditingService">Service used to perform member editing and deletion operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and authorization.</param>
    public DeleteMemberController(IMemberEditingService memberEditingService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _memberEditingService = memberEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Deletes the member with the specified unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the member to delete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the delete operation.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a member.")]
    [EndpointDescription("Deletes a member identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IMember?, MemberEditingStatus> result = await _memberEditingService.DeleteAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : MemberEditingStatusResult(result.Status);
    }
}
