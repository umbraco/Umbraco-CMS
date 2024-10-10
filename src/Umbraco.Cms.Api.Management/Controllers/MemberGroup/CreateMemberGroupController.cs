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
public class CreateMemberGroupController : MemberGroupControllerBase
{
    private readonly IMemberGroupService _memberGroupService;
    private readonly IUmbracoMapper _mapper;

    public CreateMemberGroupController(IMemberGroupService memberGroupService, IUmbracoMapper mapper)
    {
        _memberGroupService = memberGroupService;
        _mapper = mapper;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateMemberGroupRequestModel model)
    {
        IMemberGroup? memberGroup = _mapper.Map<IMemberGroup>(model);
        Attempt<IMemberGroup?, MemberGroupOperationStatus> result = await _memberGroupService.CreateAsync(memberGroup!);
        return result.Success
            ? CreatedAtId<ByKeyMemberGroupController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : MemberGroupOperationStatusResult(result.Status);
    }
}
