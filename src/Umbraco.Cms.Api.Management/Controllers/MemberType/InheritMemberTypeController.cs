using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
public class InheritMemberTypeController : MemberTypeControllerBase
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public InheritMemberTypeController(IMemberTypeService memberTypeService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _memberTypeService = memberTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}/inherit")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inherit(
        CancellationToken cancellationToken,
        Guid id,
        InheritMemberTypeRequestModel inheritMemberTypeRequestModel)
    {
        Attempt<ContentTypeOperationStatus> result =
            await _memberTypeService.InheritAsync(
                id,
                inheritMemberTypeRequestModel.Target?.Id,
                CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Result);
    }
}
