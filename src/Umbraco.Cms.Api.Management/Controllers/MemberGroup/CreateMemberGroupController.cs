using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup;

[ApiVersion("1.0")]
public class CreateMemberGroupController : MemberGroupControllerBase
{
    private readonly IMemberGroupService _memberGroupService;
    private readonly IUmbracoMapper _mapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateMemberGroupController(IMemberGroupService memberGroupService, IUmbracoMapper mapper, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _memberGroupService = memberGroupService;
        _mapper = mapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(CreateMemberGroupResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateMemberGroupRequestModel model)
    {
        IMemberGroup? memberGroup = _mapper.Map<IMemberGroup>(model);
        Attempt<IMemberGroup, MemberGroupOperationStatus> result = await _memberGroupService.CreateAsync(memberGroup!, CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok(_mapper.Map<CreateMemberGroupResponseModel>(result.Result))
            : MemberGroupOperationStatusResult(result.Status);
    }
}
