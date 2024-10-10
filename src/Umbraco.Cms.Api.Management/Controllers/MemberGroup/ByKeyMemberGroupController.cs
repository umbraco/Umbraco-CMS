using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup;

[ApiVersion("1.0")]
public class ByKeyMemberGroupController : MemberGroupControllerBase
{
    private readonly IMemberGroupService _memberGroupService;
    private readonly IUmbracoMapper _mapper;

    public ByKeyMemberGroupController(IMemberGroupService memberGroupService, IUmbracoMapper mapper)
    {
        _memberGroupService = memberGroupService;
        _mapper = mapper;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MemberGroupResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IMemberGroup? memberGroup = await _memberGroupService.GetAsync(id);
        if (memberGroup is null)
        {
            return MemberGroupNotFound();
        }

        MemberGroupResponseModel responseModel = _mapper.Map<MemberGroupResponseModel>(memberGroup)!;

        return Ok(responseModel);
    }
}
