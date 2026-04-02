using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.User.Item;

/// <summary>
/// API controller responsible for managing user-related items within the Umbraco CMS management interface.
/// </summary>
[ApiVersion("1.0")]
public class ItemUserItemController : UserItemControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemUserItemController"/> class.
    /// </summary>
    /// <param name="userService">The service used for user management operations.</param>
    /// <param name="userPresentationFactory">The factory responsible for creating user presentation models.</param>
    public ItemUserItemController(IUserService userService, IUserPresentationFactory userPresentationFactory)
    {
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
    }

    /// <summary>
    /// Retrieves a collection of user items for the specified user IDs.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="ids">The set of user IDs to retrieve items for.</param>
    /// <returns>A task representing the asynchronous operation, containing an <see cref="IActionResult"/> with the collection of user items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<UserItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of user items.")]
    [EndpointDescription("Gets a collection of user items identified by the provided Ids.")]
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
