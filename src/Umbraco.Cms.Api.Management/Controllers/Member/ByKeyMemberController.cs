using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Member;

[ApiVersion("1.0")]
public class ByKeyMemberController : MemberControllerBase
{
    private readonly IMemberEditingService _memberEditingService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;

    public ByKeyMemberController(IMemberEditingService memberEditingService, IMemberPresentationFactory memberPresentationFactory)
    {
        _memberEditingService = memberEditingService;
        _memberPresentationFactory = memberPresentationFactory;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MemberResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(Guid id)
    {
        IMember? member = await _memberEditingService.GetAsync(id);
        if (member == null)
        {
            return MemberNotFound();
        }

        MemberResponseModel model = await _memberPresentationFactory.CreateResponseModelAsync(member);
        return Ok(model);
    }
}
