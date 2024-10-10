using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Item;

[ApiVersion("1.0")]
public class ItemMemberTypeItemController : MemberTypeItemControllerBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly IMemberTypeService _memberTypeService;

    public ItemMemberTypeItemController(IUmbracoMapper mapper, IMemberTypeService memberTypeService)
    {
        _mapper = mapper;
        _memberTypeService = memberTypeService;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberTypeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<MemberTypeItemResponseModel>());
        }

        IEnumerable<IMemberType> memberTypes = _memberTypeService.GetMany(ids);
        List<MemberTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IMemberType, MemberTypeItemResponseModel>(memberTypes);
        return Ok(responseModels);
    }
}
