using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Item;

[ApiVersion("1.0")]
public class ItemMemberItemController : MemberItemControllerBase
{
    private readonly IMemberService _memberService;
    private readonly IUmbracoMapper _mapper;

    public ItemMemberItemController(IMemberService memberService, IUmbracoMapper mapper)
    {
        _memberService = memberService;
        _mapper = mapper;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IEnumerable<IMember> members = await _memberService.GetByKeysAsync(ids.ToArray());
        List<MemberItemResponseModel> responseModels = _mapper.MapEnumerable<IMember, MemberItemResponseModel>(members);
        return Ok(responseModels);
    }
}
