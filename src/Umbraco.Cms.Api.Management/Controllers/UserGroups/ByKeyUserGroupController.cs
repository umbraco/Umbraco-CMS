using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;


public class ByKeyUserGroupController : UserGroupsControllerBase
{
    private readonly IUserService _userService;
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;

    public ByKeyUserGroupController(
        IUserService userService,
        IContentService contentService,
        IMediaService mediaService)
    {
        _userService = userService;
        _contentService = contentService;
        _mediaService = mediaService;
    }

    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserGroupViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserGroupViewModel>> ByKey(Guid key)
    {
        IUserGroup? userGroup = _userService.GetUserGroupByKey(key);

        if (userGroup is null)
        {
            return NotFound();
        }

        IContent? contentStartNode = userGroup.StartContentId is not null
            ? _contentService.GetById(userGroup.StartContentId.Value)
            : null;

        IMedia? mediaStartNode = userGroup.StartMediaId is not null
            ? _mediaService.GetById(userGroup.StartMediaId.Value)
            : null;

        var viewModel = new UserGroupViewModel
        {
            Name = userGroup.Name,
            Key = userGroup.Key,
            ContentStartNodeKey = contentStartNode?.Key,
            MediaStartNodeKey = mediaStartNode?.Key,
            Icon = userGroup.Icon,
            Languages = userGroup.AllowedLanguages,
            HasAccessToAllLanguages = userGroup.HasAccessToAllLanguages,
            Permissions = new[] {"ad", "hoc", "strings", "from", "save"},
            Sections = userGroup.AllowedSections
        };

        return viewModel;
    }
}
