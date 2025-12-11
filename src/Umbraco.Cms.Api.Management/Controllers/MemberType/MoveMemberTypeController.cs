using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class MoveMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeService _memberTypeService;

    public MoveMemberTypeController(IMemberTypeService memberTypeService)
        => _memberTypeService = memberTypeService;

    [HttpPut("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Move(
        CancellationToken cancellationToken,
        Guid id,
        MoveMemberTypeRequestModel moveMemberTypeRequestModel)
    {
        Attempt<IMemberType?, ContentTypeStructureOperationStatus> result = await _memberTypeService.MoveAsync(id, moveMemberTypeRequestModel.Target?.Id);

        return result.Success
            ? Ok()
            : StructureOperationStatusResult(result.Status);
    }
}
