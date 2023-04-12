using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups.Item;
using Umbraco.Cms.Api.Management.ViewModels.Users.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups.Item;

public class ItemUserGroupItemController : UserGroupItemControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUmbracoMapper _mapper;

    public ItemUserGroupItemController(IUserGroupService userGroupService, IUmbracoMapper mapper)
    {
        _userGroupService = userGroupService;
        _mapper = mapper;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<UserGroupItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "path")] SortedSet<Guid> ids)
    {
        IEnumerable<IUserGroup> userGroups = await _userGroupService.GetAsync(ids);
        List<UserGroupItemResponseModel> responseModels = _mapper.MapEnumerable<IUserGroup, UserGroupItemResponseModel>(userGroups);
        return Ok(responseModels);
    }
}
