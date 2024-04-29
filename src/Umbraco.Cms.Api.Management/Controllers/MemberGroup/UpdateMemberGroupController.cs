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

    [HttpPut($"{{{nameof(id)}:guid}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateMemberGroupRequestModel model)
    {
        IMemberGroup? current = await _memberGroupService.GetAsync(id);
        if (current is null)
        {
            return MemberGroupNotFound();
        }

        IMemberGroup updated = _mapper.Map(model, current);

        Attempt<IMemberGroup?, MemberGroupOperationStatus> result = await _memberGroupService.UpdateAsync(updated);
        return result.Success
            ? Ok()
            : MemberGroupOperationStatusResult(result.Status);
    }
}
