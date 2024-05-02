using Umbraco.Cms.Api.Management.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Api.Management.ViewModels.User.Item;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class UserPresentationFactory : IUserPresentationFactory
{

    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly IUserGroupPresentationFactory _userGroupPresentationFactory;
    private readonly IAbsoluteUrlBuilder _absoluteUrlBuilder;
    private readonly IEmailSender _emailSender;
    private readonly IPasswordConfigurationPresentationFactory _passwordConfigurationPresentationFactory;
    private readonly IBackOfficeExternalLoginProviders _externalLoginProviders;
    private readonly SecuritySettings _securitySettings;

    public UserPresentationFactory(
        IEntityService entityService,
        AppCaches appCaches,
        MediaFileManager mediaFileManager,
        IImageUrlGenerator imageUrlGenerator,
        IUserGroupPresentationFactory userGroupPresentationFactory,
        IAbsoluteUrlBuilder absoluteUrlBuilder,
        IEmailSender emailSender,
        IPasswordConfigurationPresentationFactory passwordConfigurationPresentationFactory,
        IOptionsSnapshot<SecuritySettings> securitySettings,
        IBackOfficeExternalLoginProviders externalLoginProviders)
    {
        _entityService = entityService;
        _appCaches = appCaches;
        _mediaFileManager = mediaFileManager;
        _imageUrlGenerator = imageUrlGenerator;
        _userGroupPresentationFactory = userGroupPresentationFactory;
        _emailSender = emailSender;
        _passwordConfigurationPresentationFactory = passwordConfigurationPresentationFactory;
        _externalLoginProviders = externalLoginProviders;
        _securitySettings = securitySettings.Value;
        _absoluteUrlBuilder = absoluteUrlBuilder;
    }

    public UserResponseModel CreateResponseModel(IUser user)
    {
        var responseModel = new UserResponseModel
        {
            Id = user.Key,
            Email = user.Email,
            Name = user.Name ?? string.Empty,
            AvatarUrls = user.GetUserAvatarUrls(_appCaches.RuntimeCache, _mediaFileManager, _imageUrlGenerator)
                .Select(url => _absoluteUrlBuilder.ToAbsoluteUrl(url).ToString()),
            UserName = user.Username,
            LanguageIsoCode = user.Language,
            CreateDate = user.CreateDate,
            UpdateDate = user.UpdateDate,
            State = user.UserState,
            UserGroupIds = new HashSet<Guid>(user.Groups.Select(x => x.Key)),
            DocumentStartNodeIds = GetKeysFromIds(user.StartContentIds, UmbracoObjectTypes.Document),
            MediaStartNodeIds = GetKeysFromIds(user.StartMediaIds, UmbracoObjectTypes.Media),
            FailedLoginAttempts = user.FailedPasswordAttempts,
            LastLoginDate = user.LastLoginDate,
            LastLockoutDate = user.LastLockoutDate,
            LastPasswordChangeDate = user.LastPasswordChangeDate,
            IsAdmin = user.IsAdmin(),
        };

        return responseModel;
    }

    public UserItemResponseModel CreateItemResponseModel(IUser user) =>
        new()
        {
            Id = user.Key,
            Name = user.Name ?? user.Username,
            AvatarUrls = user.GetUserAvatarUrls(_appCaches.RuntimeCache, _mediaFileManager, _imageUrlGenerator)
                .Select(url => _absoluteUrlBuilder.ToAbsoluteUrl(url).ToString()),
        };

    public async Task<UserCreateModel> CreateCreationModelAsync(CreateUserRequestModel requestModel)
    {
        var createModel = new UserCreateModel
        {
            Id = requestModel.Id,
            Email = requestModel.Email,
            Name = requestModel.Name,
            UserName = requestModel.UserName,
            UserGroupKeys = requestModel.UserGroupIds,
        };

        return await Task.FromResult(createModel);
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

        return await Task.FromResult(inviteModel);
    }

    public async Task<UserResendInviteModel> CreateResendInviteModelAsync(ResendInviteUserRequestModel requestModel)
    {
        var inviteModel = new UserResendInviteModel
        {
            InvitedUserKey = requestModel.User.Id,
            Message = requestModel.Message,
        };

        return await Task.FromResult(inviteModel);
    }

    public async Task<CurrenUserConfigurationResponseModel> CreateCurrentUserConfigurationModelAsync()
    {
        var model = new CurrenUserConfigurationResponseModel
        {
            KeepUserLoggedIn = _securitySettings.KeepUserLoggedIn,
            UsernameIsEmail = _securitySettings.UsernameIsEmail,
            PasswordConfiguration = _passwordConfigurationPresentationFactory.CreatePasswordConfigurationResponseModel(),
        };

        return await Task.FromResult(model);
    }

    public Task<UserConfigurationResponseModel> CreateUserConfigurationModelAsync() =>
        Task.FromResult(new UserConfigurationResponseModel
        {
            // You should not be able to invite users if any providers has deny local login set.
            CanInviteUsers = _emailSender.CanSendRequiredEmail() && _externalLoginProviders.HasDenyLocalLogin() is false,
            PasswordConfiguration = _passwordConfigurationPresentationFactory.CreatePasswordConfigurationResponseModel(),
        });

    public async Task<UserUpdateModel> CreateUpdateModelAsync(Guid existingUserKey, UpdateUserRequestModel updateModel)
    {
        var model = new UserUpdateModel
        {
            ExistingUserKey = existingUserKey,
            Email = updateModel.Email,
            Name = updateModel.Name,
            UserName = updateModel.UserName,
            LanguageIsoCode = updateModel.LanguageIsoCode,
            ContentStartNodeKeys = updateModel.DocumentStartNodeIds,
            MediaStartNodeKeys = updateModel.MediaStartNodeIds,
        };

        model.UserGroupKeys = updateModel.UserGroupIds;

        return await Task.FromResult(model);
    }

    public async Task<CurrentUserResponseModel> CreateCurrentUserResponseModelAsync(IUser user)
    {
        var presentationUser = CreateResponseModel(user);
        var presentationGroups = await _userGroupPresentationFactory.CreateMultipleAsync(user.Groups);
        var languages = presentationGroups.SelectMany(x => x.Languages).Distinct().ToArray();
        var mediaStartNodeKeys = GetKeysFromIds(user.CalculateMediaStartNodeIds(_entityService, _appCaches), UmbracoObjectTypes.Media);
        var documentStartNodeKeys = GetKeysFromIds(user.CalculateContentStartNodeIds(_entityService, _appCaches), UmbracoObjectTypes.Document);

        var permissions = presentationGroups.SelectMany(x => x.Permissions).ToHashSet();
        var fallbackPermissions = presentationGroups.SelectMany(x => x.FallbackPermissions).ToHashSet();

        var hasAccessToAllLanguages = presentationGroups.Any(x => x.HasAccessToAllLanguages);

        var allowedSections = presentationGroups.SelectMany(x => x.Sections).ToHashSet();

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
            DocumentStartNodeIds = documentStartNodeKeys,
            Permissions = permissions,
            FallbackPermissions = fallbackPermissions,
            HasAccessToAllLanguages = hasAccessToAllLanguages,
            HasAccessToSensitiveData = user.HasAccessToSensitiveData(),
            AllowedSections = allowedSections,
            IsAdmin = user.IsAdmin()
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
