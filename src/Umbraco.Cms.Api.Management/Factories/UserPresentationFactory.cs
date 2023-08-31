using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core;
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
    private readonly IUserGroupPresentationFactory _userGroupPresentationFactory;

    public UserPresentationFactory(
        IEntityService entityService,
        AppCaches appCaches,
        MediaFileManager mediaFileManager,
        IImageUrlGenerator imageUrlGenerator,
        IUserGroupPresentationFactory userGroupPresentationFactory)
    {
        _entityService = entityService;
        _appCaches = appCaches;
        _mediaFileManager = mediaFileManager;
        _imageUrlGenerator = imageUrlGenerator;
        _userGroupPresentationFactory = userGroupPresentationFactory;
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
            UserGroupIds = new HashSet<Guid>(user.Groups.Select(x => x.Key)),
            ContentStartNodeIds = GetKeysFromIds(user.StartContentIds, UmbracoObjectTypes.Document),
            MediaStartNodeIds = GetKeysFromIds(user.StartMediaIds, UmbracoObjectTypes.Media),
            FailedLoginAttempts = user.FailedPasswordAttempts,
            LastLoginDate = user.LastLoginDate,
            LastLockoutDate = user.LastLockoutDate,
            LastPasswordChangeDate = user.LastPasswordChangeDate,
        };

        return responseModel;
    }

    public async Task<UserCreateModel> CreateCreationModelAsync(CreateUserRequestModel requestModel)
    {
        var createModel = new UserCreateModel
        {
            Email = requestModel.Email,
            Name = requestModel.Name,
            UserName = requestModel.UserName,
            UserGroupKeys = requestModel.UserGroupIds,
            Key = requestModel.Id,
        };

        return createModel;
    }

    public async Task<UserInviteModel> CreateInviteModelAsync(InviteUserRequestModel requestModel)
    {
        var inviteModel = new UserInviteModel
        {
            Email = requestModel.Email,
            Name = requestModel.Name,
            UserName = requestModel.UserName,
            UserGroupKeys = requestModel.UserGroupIds,
            Message = requestModel.Message,
        };

        return inviteModel;
    }

    public async Task<UserUpdateModel> CreateUpdateModelAsync(Guid existingUserKey, UpdateUserRequestModel updateModel)
    {
        var model = new UserUpdateModel
        {
            ExistingUserKey = existingUserKey,
            Email = updateModel.Email,
            Name = updateModel.Name,
            UserName = updateModel.UserName,
            LanguageIsoCode = updateModel.LanguageIsoCode,
            ContentStartNodeKeys = updateModel.ContentStartNodeIds,
            MediaStartNodeKeys = updateModel.MediaStartNodeIds,
        };

        model.UserGroupKeys = updateModel.UserGroupIds;

        return model;
    }

    public async Task<CurrentUserResponseModel> CreateCurrentUserResponseModelAsync(IUser user)
    {
        var presentationUser = CreateResponseModel(user);
        var presentationGroups = await _userGroupPresentationFactory.CreateMultipleAsync(user.Groups);
        var languages = presentationGroups.SelectMany(x => x.Languages).Distinct().ToArray();
        var mediaStartNodeKeys = GetKeysFromIds(user.CalculateMediaStartNodeIds(_entityService, _appCaches), UmbracoObjectTypes.Media);
        var documentStartNodeKeys = GetKeysFromIds(user.CalculateContentStartNodeIds(_entityService, _appCaches), UmbracoObjectTypes.Document);

        var permissions = presentationGroups.SelectMany(x => x.Permissions).Distinct().ToHashSet();
        var hasAccessToAllLanguages = presentationGroups.Any(x => x.HasAccessToAllLanguages);

        return await Task.FromResult(new CurrentUserResponseModel()
        {
            Id = presentationUser.Id,
            Email = presentationUser.Email,
            Name = presentationUser.Name,
            UserName = presentationUser.UserName,
            Languages = languages,
            AvatarUrls = presentationUser.AvatarUrls,
            LanguageIsoCode = presentationUser.LanguageIsoCode,
            MediaStartNodeIds = mediaStartNodeKeys,
            ContentStartNodeIds = documentStartNodeKeys,
            Permissions = permissions,
            HasAccessToAllLanguages = hasAccessToAllLanguages
        });
    }

    private ISet<Guid> GetKeysFromIds(IEnumerable<int>? ids, UmbracoObjectTypes type)
    {
        IEnumerable<Guid>? keys = ids?
            .Select(x => _entityService.GetKey(x, type))
            .Where(x => x.Success)
            .Select(x => x.Result);

        return keys is null
            ? new HashSet<Guid>()
            : new HashSet<Guid>(keys);
    }


}
