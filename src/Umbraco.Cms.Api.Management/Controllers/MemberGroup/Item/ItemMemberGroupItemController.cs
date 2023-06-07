using Asp.Versioning;
using J2N.Collections.Generic.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup.Item;

[ApiVersion("1.0")]
public class ItemMemberGroupItemController : MemberGroupItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IUmbracoMapper _mapper;

    public ItemMemberGroupItemController(IEntityService entityService, IUmbracoMapper mapper)
    {
        _entityService = entityService;
        _mapper = mapper;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MemberGroupItemReponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IEnumerable<IEntitySlim> memberGroups = _entityService.GetAll(UmbracoObjectTypes.MemberGroup, ids.ToArray());
        List<MemberGroupItemReponseModel> responseModel = _mapper.MapEnumerable<IEntitySlim, MemberGroupItemReponseModel>(memberGroups);
        return Ok(responseModel);
    }
}
