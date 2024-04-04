﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.User.Item;

[ApiVersion("1.0")]
public class ItemUserItemController : UserItemControllerBase
{
    private readonly IUserService _userService;
    private readonly IUmbracoMapper _mapper;

    public ItemUserItemController(IUserService userService, IUmbracoMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<UserItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item(CancellationToken cancellationToken, [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IEnumerable<IUser> users = await _userService.GetAsync(ids.ToArray());
        List<UserItemResponseModel> responseModels = _mapper.MapEnumerable<IUser, UserItemResponseModel>(users);
        return Ok(responseModels);
    }
}
