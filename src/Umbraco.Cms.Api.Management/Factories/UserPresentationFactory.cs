using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class UserPresentationFactory : IUserPresentationFactory
{
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly IUserGroupService _userGroupService;

    public UserPresentationFactory(
        IEntityService entityService,
        AppCaches appCaches,
        MediaFileManager mediaFileManager,
        IImageUrlGenerator imageUrlGenerator,
        IUserGroupService userGroupService)
    {
        _entityService = entityService;
        _appCaches = appCaches;
        _mediaFileManager = mediaFileManager;
        _imageUrlGenerator = imageUrlGenerator;
        _userGroupService = userGroupService;
    }

    public UserResponseModel CreateResponseModel(IUser user)
    {
        var responseModel = new UserResponseModel
        {
            Id = user.Key,
            Email = user.Email,
            Name = user.Name ?? string.Empty,
            AvatarUrls = user.GetUserAvatarUrls(_appCaches.RuntimeCache, _mediaFileManager, _imageUrlGenerator),
            UserName = user.Username,
            LanguageIsoCode = user.Language,
            CreateDate = user.CreateDate,
            UpdateDate = user.UpdateDate,
            State = user.UserState,
            UserGroupIds = new SortedSet<Guid>(user.Groups.Select(x => x.Key)),
            ContentStartNodeIds = GetKeysFromIds(user.StartContentIds, UmbracoObjectTypes.Document),
            MediaStartNodeIds = GetKeysFromIds(user.StartMediaIds, UmbracoObjectTypes.Media),
            FailedLoginAttempts = user.FailedPasswordAttempts,
            LastLoginDate = user.LastLoginDate,
            LastlockoutDate = user.LastLockoutDate,
            LastPasswordChangeDate = user.LastPasswordChangeDate,
        };

        return responseModel;
    }

    public async Task<UserCreateModel> CreateCreationModelAsync(CreateUserRequestModel requestModel)
    {
        IEnumerable<IUserGroup> groups = await _userGroupService.GetAsync(requestModel.UserGroupIds);

        var createModel = new UserCreateModel
        {
            Email = requestModel.Email,
            Name = requestModel.Name,
            UserName = requestModel.UserName,
            UserGroups = new HashSet<IUserGroup>(groups),
        };

        return createModel;
    }

    public async Task<UserInviteModel> CreateInviteModelAsync(InviteUserRequestModel requestModel)
    {
        IEnumerable<IUserGroup> groups = await _userGroupService.GetAsync(requestModel.UserGroupIds);

        var inviteModel = new UserInviteModel
        {
            Email = requestModel.Email,
            Name = requestModel.Name,
            UserName = requestModel.UserName,
            UserGroups = new HashSet<IUserGroup>(groups),
            Message = requestModel.Message,
        };

        return inviteModel;
    }

    public async Task<UserUpdateModel> CreateUpdateModelAsync(IUser existingUser, UpdateUserRequestModel updateModel)
    {
        var model = new UserUpdateModel
        {
            ExistingUser = existingUser,
            Email = updateModel.Email,
            Name = updateModel.Name,
            UserName = updateModel.UserName,
            Language = updateModel.LanguageIsoCode,
            ContentStartNodeKeys = updateModel.ContentStartNodeIds,
            MediaStartNodeKeys = updateModel.MediaStartNodeIds,
        };

        IEnumerable<IUserGroup> userGroups = await _userGroupService.GetAsync(updateModel.UserGroupIds);
        model.UserGroups = userGroups;

        return model;
    }

    public CreateUserResponseModel CreateCreationResponseModel(UserCreationResult creationResult)
        => new()
        {
            UserId = creationResult.CreatedUser?.Key ?? Guid.Empty,
            InitialPassword = creationResult.InitialPassword,
        };

    private SortedSet<Guid> GetKeysFromIds(IEnumerable<int>? ids, UmbracoObjectTypes type)
    {
        IEnumerable<Guid>? keys = ids?
            .Select(x => _entityService.GetKey(x, type))
            .Where(x => x.Success)
            .Select(x => x.Result);

        return keys is null
            ? new SortedSet<Guid>()
            : new SortedSet<Guid>(keys);
    }
}
