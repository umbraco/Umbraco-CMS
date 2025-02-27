using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup;

[ApiVersion("1.0")]
public class DeleteMemberGroupController : MemberGroupControllerBase
{
    private readonly IMemberGroupService _memberGroupService;

    public DeleteMemberGroupController(IMemberGroupService memberGroupService) => _memberGroupService = memberGroupService;

    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IMemberGroup?, MemberGroupOperationStatus> result = await _memberGroupService.DeleteAsync(id);
        return result.Success
            ? Ok()
            : MemberGroupOperationStatusResult(result.Status);
    }
}
