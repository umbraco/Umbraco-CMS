using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Api.Management.ViewModels.User.Item;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly IUserService _userService;
    private readonly IContentService _contentService;

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 17.")]
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
        : this(
              entityService,
            appCaches,
            mediaFileManager,
            imageUrlGenerator,
            userGroupPresentationFactory,
            absoluteUrlBuilder,
            emailSender,
            passwordConfigurationPresentationFactory,
            securitySettings,
            externalLoginProviders,
            StaticServiceProvider.Instance.GetRequiredService<IUserService>(),
            StaticServiceProvider.Instance.GetRequiredService<IContentService>())
    {
    }

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
        IBackOfficeExternalLoginProviders externalLoginProviders,
        IUserService userService,
        IContentService contentService)
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
        _userService = userService;
        _contentService = contentService;
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
            UserGroupIds = new HashSet<ReferenceByIdModel>(user.Groups.Select(x => new ReferenceByIdModel(x.Key))),
            DocumentStartNodeIds = GetKeysFromIds(user.StartContentIds, UmbracoObjectTypes.Document),
            HasDocumentRootAccess = HasRootAccess(user.StartContentIds),
            MediaStartNodeIds = GetKeysFromIds(user.StartMediaIds, UmbracoObjectTypes.Media),
            HasMediaRootAccess = HasRootAccess(user.StartMediaIds),
            FailedLoginAttempts = user.FailedPasswordAttempts,
            LastLoginDate = user.LastLoginDate,
            LastLockoutDate = user.LastLockoutDate,
            LastPasswordChangeDate = user.LastPasswordChangeDate,
            IsAdmin = user.IsAdmin(),
            Kind = user.Kind
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
            Kind = user.Kind
        };

    public async Task<UserCreateModel> CreateCreationModelAsync(CreateUserRequestModel requestModel)
    {
        var createModel = new UserCreateModel
        {
            Id = requestModel.Id,
            Email = requestModel.Email,
            Name = requestModel.Name,
            UserName = requestModel.UserName,
            UserGroupKeys = requestModel.UserGroupIds.Select(x => x.Id).ToHashSet(),
            Kind = requestModel.Kind
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
            UserGroupKeys = requestModel.UserGroupIds.Select(x => x.Id).ToHashSet(),
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

            // You should not be able to change any password or set 2fa if any providers has deny local login set.
            AllowChangePassword = _externalLoginProviders.HasDenyLocalLogin() is false,
            AllowTwoFactor = _externalLoginProviders.HasDenyLocalLogin() is false,
        };

        return await Task.FromResult(model);
    }

    public Task<UserConfigurationResponseModel> CreateUserConfigurationModelAsync() =>
        Task.FromResult(new UserConfigurationResponseModel
        {
            // You should not be able to invite users if any providers has deny local login set.
            CanInviteUsers = _emailSender.CanSendRequiredEmail() && _externalLoginProviders.HasDenyLocalLogin() is false,
            UsernameIsEmail = _securitySettings.UsernameIsEmail,
            PasswordConfiguration = _passwordConfigurationPresentationFactory.CreatePasswordConfigurationResponseModel(),

            // You should not be able to change any password or set 2fa if any providers has deny local login set.
            AllowChangePassword = _externalLoginProviders.HasDenyLocalLogin() is false,
            AllowTwoFactor = _externalLoginProviders.HasDenyLocalLogin() is false,
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
            ContentStartNodeKeys = updateModel.DocumentStartNodeIds.Select(x => x.Id).ToHashSet(),
            HasContentRootAccess = updateModel.HasDocumentRootAccess,
            MediaStartNodeKeys = updateModel.MediaStartNodeIds.Select(x => x.Id).ToHashSet(),
            HasMediaRootAccess = updateModel.HasMediaRootAccess,
        };

        model.UserGroupKeys = updateModel.UserGroupIds.Select(x => x.Id).ToHashSet();

        return await Task.FromResult(model);
    }

    public async Task<CurrentUserResponseModel> CreateCurrentUserResponseModelAsync(IUser user)
    {
        var presentationUser = CreateResponseModel(user);
        var presentationGroups = await _userGroupPresentationFactory.CreateMultipleAsync(user.Groups);
        var languages = presentationGroups.SelectMany(x => x.Languages).Distinct().ToArray();
        var mediaStartNodeIds = user.CalculateMediaStartNodeIds(_entityService, _appCaches);
        var mediaStartNodeKeys = GetKeysFromIds(mediaStartNodeIds, UmbracoObjectTypes.Media);
        var contentStartNodeIds = user.CalculateContentStartNodeIds(_entityService, _appCaches);
        var documentStartNodeKeys = GetKeysFromIds(contentStartNodeIds, UmbracoObjectTypes.Document);

        var permissions = GetAggregatedGranularPermissions(user, presentationGroups);
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
            HasMediaRootAccess = HasRootAccess(mediaStartNodeIds),
            DocumentStartNodeIds = documentStartNodeKeys,
            HasDocumentRootAccess = HasRootAccess(contentStartNodeIds),
            Permissions = permissions,
            FallbackPermissions = fallbackPermissions,
            HasAccessToAllLanguages = hasAccessToAllLanguages,
            HasAccessToSensitiveData = user.HasAccessToSensitiveData(),
            AllowedSections = allowedSections,
            IsAdmin = user.IsAdmin(),
            UserGroupIds = presentationUser.UserGroupIds,
        });
    }

    private HashSet<IPermissionPresentationModel> GetAggregatedGranularPermissions(IUser user, IEnumerable<UserGroupResponseModel> presentationGroups)
    {
        var permissions = presentationGroups.SelectMany(x => x.Permissions).ToHashSet();

        // The raw permission data consists of several permissions for each document.  We want to aggregate this server-side so
        // we return one set of aggregate permissions per document that the client will use.

        // Get the unique document keys that have granular permissions.
        IEnumerable<Guid> documentKeysWithGranularPermissions = permissions
            .Where(x => x is DocumentPermissionPresentationModel)
            .Cast<DocumentPermissionPresentationModel>()
            .Select(x => x.Document.Id)
            .Distinct();

        var aggregatedPermissions = new HashSet<IPermissionPresentationModel>();
        foreach (Guid documentKey in documentKeysWithGranularPermissions)
        {
            // Retrieve the path of the document.
            var path = _contentService.GetById(documentKey)?.Path;
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            // With the path we can call the same logic as used server-side for authorizing access to resources.
            EntityPermissionSet permissionsForPath = _userService.GetPermissionsForPath(user, path);
            aggregatedPermissions.Add(new DocumentPermissionPresentationModel
            {
                Document = new ReferenceByIdModel(documentKey),
                Verbs = permissionsForPath.GetAllPermissions()
            });
        }

        return aggregatedPermissions;
    }

    public async Task<CalculatedUserStartNodesResponseModel> CreateCalculatedUserStartNodesResponseModelAsync(IUser user)
    {
        var mediaStartNodeIds = user.CalculateMediaStartNodeIds(_entityService, _appCaches);
        ISet<ReferenceByIdModel> mediaStartNodeKeys = GetKeysFromIds(mediaStartNodeIds, UmbracoObjectTypes.Media);
        var contentStartNodeIds = user.CalculateContentStartNodeIds(_entityService, _appCaches);
        ISet<ReferenceByIdModel> documentStartNodeKeys = GetKeysFromIds(contentStartNodeIds, UmbracoObjectTypes.Document);

        return await Task.FromResult(new CalculatedUserStartNodesResponseModel()
        {
            Id = user.Key,
            MediaStartNodeIds = mediaStartNodeKeys,
            HasMediaRootAccess = HasRootAccess(mediaStartNodeIds),
            DocumentStartNodeIds = documentStartNodeKeys,
            HasDocumentRootAccess = HasRootAccess(contentStartNodeIds),
        });
    }

    private ISet<ReferenceByIdModel> GetKeysFromIds(IEnumerable<int>? ids, UmbracoObjectTypes type)
    {
        IEnumerable<ReferenceByIdModel>? models = ids?
            .Select(x => _entityService.GetKey(x, type))
            .Where(x => x.Success)
            .Select(x => x.Result)
            .Select(x => new ReferenceByIdModel(x));

        return models is null
            ? new HashSet<ReferenceByIdModel>()
            : new HashSet<ReferenceByIdModel>(models);
    }

    private bool HasRootAccess(IEnumerable<int>? startNodeIds)
        => startNodeIds?.Contains(Constants.System.Root) is true;
}
