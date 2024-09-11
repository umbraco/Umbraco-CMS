using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup.Item;

[ApiVersion("1.0")]
public class ItemUserGroupItemController : UserGroupItemControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUmbracoMapper _mapper;

    public ItemUserGroupItemController(IUserGroupService userGroupService, IUmbracoMapper mapper)
    {
        _userGroupService = userGroupService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<UserGroupItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<UserGroupItemResponseModel>());
        }

        IEnumerable<IUserGroup> userGroups = await _userGroupService.GetAsync(ids);
        List<UserGroupItemResponseModel> responseModels = _mapper.MapEnumerable<IUserGroup, UserGroupItemResponseModel>(userGroups);
        return Ok(responseModels);
    }
}
