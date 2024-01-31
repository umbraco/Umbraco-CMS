using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup;

[ApiVersion("1.0")]
public class UpdateMemberGroupController : MemberGroupControllerBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly IMemberGroupService _memberGroupService;

    public UpdateMemberGroupController(IUmbracoMapper mapper, IMemberGroupService memberGroupService)
    {
        _mapper = mapper;
        _memberGroupService = memberGroupService;
    }

    [HttpPut]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MemberGroupResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateMemberGroupRequestModel model)
    {
        IMemberGroup? memberGroup = _mapper.Map<IMemberGroup>(model);
        Attempt<IMemberGroup?, MemberGroupOperationStatus> result = await _memberGroupService.UpdateAsync(memberGroup!);
        return result.Success
            ? Ok(_mapper.Map<MemberGroupResponseModel>(result.Result))
            : MemberGroupOperationStatusResult(result.Status);
    }
}
