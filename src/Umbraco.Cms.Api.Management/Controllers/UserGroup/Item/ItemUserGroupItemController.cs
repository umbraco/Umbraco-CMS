using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup.Item;

/// <summary>
/// Controller responsible for managing operations on individual user group items in the Umbraco CMS Management API.
/// </summary>
[ApiVersion("1.0")]
public class ItemUserGroupItemController : UserGroupItemControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemUserGroupItemController"/> class with the specified user group service and mapper.
    /// </summary>
    /// <param name="userGroupService">Service used to manage user groups.</param>
    /// <param name="mapper">The Umbraco object mapper used for mapping between models.</param>
    public ItemUserGroupItemController(IUserGroupService userGroupService, IUmbracoMapper mapper)
    {
        _userGroupService = userGroupService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves user group items for the specified set of IDs.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of user group item IDs to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IActionResult"/> with the collection of user group items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<UserGroupItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of user group items.")]
    [EndpointDescription("Gets a collection of user group items identified by the provided Ids.")]
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
