using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.User.Item;

[ApiVersion("1.0")]
public class ItemUserItemController : UserItemControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;

    public ItemUserItemController(IUserService userService, IUserPresentationFactory userPresentationFactory)
    {
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<UserItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item(CancellationToken cancellationToken, [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<UserItemResponseModel>());
        }

        IEnumerable<IUser> users = await _userService.GetAsync(ids.ToArray());
        var responseModels = users.Select(_userPresentationFactory.CreateItemResponseModel).ToList();
        return Ok(responseModels);
    }
}
