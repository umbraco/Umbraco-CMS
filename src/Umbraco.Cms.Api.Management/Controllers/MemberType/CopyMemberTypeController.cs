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
public class CopyMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeService _memberTypeService;

    public CopyMemberTypeController(IMemberTypeService memberTypeService)
        => _memberTypeService = memberTypeService;

    [Obsolete("Please use the overload that includes all parameters. Scheduled for removal in Umbraco 19.")]
    [NonAction]
    public async Task<IActionResult> Copy(
        CancellationToken cancellationToken,
        Guid id) => await Copy(cancellationToken, id, null);

    [HttpPost("{id:guid}/copy")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Copy(
        CancellationToken cancellationToken,
        Guid id,
        CopyMemberTypeRequestModel? copyMemberTypeRequestModel)
    {
        Attempt<IMemberType?, ContentTypeStructureOperationStatus> result = await _memberTypeService.CopyAsync(id, copyMemberTypeRequestModel?.Target?.Id);

        return result.Success
            ? CreatedAtId<ByKeyMemberTypeController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : StructureOperationStatusResult(result.Status);
    }
}
