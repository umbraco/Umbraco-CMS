using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup;

/// <summary>
/// Controller for deleting member groups.
/// </summary>
[ApiVersion("1.0")]
public class DeleteMemberGroupController : MemberGroupControllerBase
{
    private readonly IMemberGroupService _memberGroupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMemberGroupController"/> class, responsible for handling member group deletion operations.
    /// </summary>
    /// <param name="memberGroupService">The service used to manage member group operations.</param>
    public DeleteMemberGroupController(IMemberGroupService memberGroupService) => _memberGroupService = memberGroupService;

    /// <summary>
    /// Deletes the member group with the specified unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the member group to delete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Deletes a member group.")]
    [EndpointDescription("Deletes a member group identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IMemberGroup?, MemberGroupOperationStatus> result = await _memberGroupService.DeleteAsync(id);
        return result.Success
            ? Ok()
            : MemberGroupOperationStatusResult(result.Status);
    }
}
