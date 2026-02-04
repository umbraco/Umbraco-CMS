using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Mapping.Permissions;
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

/// <summary>
/// Factory for creating user presentation models, implementing <see cref="IUserPresentationFactory"/>.
/// </summary>
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
    private readonly Dictionary<Type, IPermissionPresentationMapper> _permissionPresentationMappersByType;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPresentationFactory"/> class.
    /// </summary>
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
        IEnumerable<IPermissionPresentationMapper> permissionPresentationMappers)
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
        _permissionPresentationMappersByType = permissionPresentationMappers.ToDictionary(x => x.PresentationModelToHandle);
    }

    /// <inheritdoc/>
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
            ElementStartNodeIds = GetKeysFromIds(user.StartElementIds, UmbracoObjectTypes.ElementContainer),
            HasElementRootAccess = HasRootAccess(user.StartElementIds),
            FailedLoginAttempts = user.FailedPasswordAttempts,
            LastLoginDate = user.LastLoginDate,
            LastLockoutDate = user.LastLockoutDate,
            LastPasswordChangeDate = user.LastPasswordChangeDate,
            IsAdmin = user.IsAdmin(),
            Kind = user.Kind
        };

        return responseModel;
    }

    /// <inheritdoc/>
    public UserItemResponseModel CreateItemResponseModel(IUser user) =>
        new()
        {
            Id = user.Key,
            Name = user.Name ?? user.Username,
            AvatarUrls = user.GetUserAvatarUrls(_appCaches.RuntimeCache, _mediaFileManager, _imageUrlGenerator)
                .Select(url => _absoluteUrlBuilder.ToAbsoluteUrl(url).ToString()),
            Kind = user.Kind,
        };

    /// <inheritdoc/>
    public Task<UserCreateModel> CreateCreationModelAsync(CreateUserRequestModel requestModel)
    {
        var createModel = new UserCreateModel
        {
            Id = requestModel.Id,
            Email = requestModel.Email,
            Name = requestModel.Name,
            UserName = requestModel.UserName,
            UserGroupKeys = requestModel.UserGroupIds.Select(x => x.Id).ToHashSet(),
            Kind = requestModel.Kind,
        };

        return Task.FromResult(createModel);
    }

    /// <inheritdoc/>
    public Task<UserInviteModel> CreateInviteModelAsync(InviteUserRequestModel requestModel)
    {
        var inviteModel = new UserInviteModel
        {
            Email = requestModel.Email,
            Name = requestModel.Name,
            UserName = requestModel.UserName,
            UserGroupKeys = requestModel.UserGroupIds.Select(x => x.Id).ToHashSet(),
            Message = requestModel.Message,
        };

        return Task.FromResult(inviteModel);
    }

    /// <inheritdoc/>
    public Task<UserResendInviteModel> CreateResendInviteModelAsync(ResendInviteUserRequestModel requestModel)
    {
        var inviteModel = new UserResendInviteModel
        {
            InvitedUserKey = requestModel.User.Id,
            Message = requestModel.Message,
        };

        return Task.FromResult(inviteModel);
    }

    /// <inheritdoc/>
    public Task<CurrentUserConfigurationResponseModel> CreateCurrentUserConfigurationModelAsync()
    {
        var model = new CurrentUserConfigurationResponseModel
        {
            KeepUserLoggedIn = _securitySettings.KeepUserLoggedIn,
            PasswordConfiguration = _passwordConfigurationPresentationFactory.CreatePasswordConfigurationResponseModel(),

            // You should not be able to change any password or set 2fa if any providers has deny local login set.
            AllowChangePassword = _externalLoginProviders.HasDenyLocalLogin() is false,
            AllowTwoFactor = _externalLoginProviders.HasDenyLocalLogin() is false,
        };

        return Task.FromResult(model);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public Task<UserUpdateModel> CreateUpdateModelAsync(Guid existingUserKey, UpdateUserRequestModel updateModel)
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
            ElementStartNodeKeys = updateModel.ElementStartNodeIds.Select(x => x.Id).ToHashSet(),
            HasElementRootAccess = updateModel.HasElementRootAccess,
            UserGroupKeys = updateModel.UserGroupIds.Select(x => x.Id).ToHashSet()
        };

        return Task.FromResult(model);
    }

    /// <inheritdoc/>
    public async Task<CurrentUserResponseModel> CreateCurrentUserResponseModelAsync(IUser user)
    {
        UserResponseModel presentationUser = CreateResponseModel(user);
        IEnumerable<UserGroupResponseModel> presentationGroups = await _userGroupPresentationFactory.CreateMultipleAsync(user.Groups);
        var languages = presentationGroups.SelectMany(x => x.Languages).Distinct().ToArray();
        var mediaStartNodeIds = user.CalculateMediaStartNodeIds(_entityService, _appCaches);
        ISet<ReferenceByIdModel> mediaStartNodeKeys = GetKeysFromIds(mediaStartNodeIds, UmbracoObjectTypes.Media);
        var contentStartNodeIds = user.CalculateContentStartNodeIds(_entityService, _appCaches);
        ISet<ReferenceByIdModel> documentStartNodeKeys = GetKeysFromIds(contentStartNodeIds, UmbracoObjectTypes.Document);
        var elementStartNodeIds = user.CalculateElementStartNodeIds(_entityService, _appCaches);
        ISet<ReferenceByIdModel> elementStartNodeKeys = GetKeysFromIds(elementStartNodeIds, UmbracoObjectTypes.ElementContainer);

        HashSet<IPermissionPresentationModel> permissions = GetAggregatedGranularPermissions(user, presentationGroups);
        var fallbackPermissions = presentationGroups.SelectMany(x => x.FallbackPermissions).ToHashSet();

        var hasAccessToAllLanguages = presentationGroups.Any(x => x.HasAccessToAllLanguages);

        var allowedSections = presentationGroups.SelectMany(x => x.Sections).ToHashSet();

        return new CurrentUserResponseModel()
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
            ElementStartNodeIds = elementStartNodeKeys,
            HasElementRootAccess = HasRootAccess(elementStartNodeIds),
            Permissions = permissions,
            FallbackPermissions = fallbackPermissions,
            HasAccessToAllLanguages = hasAccessToAllLanguages,
            HasAccessToSensitiveData = user.HasAccessToSensitiveData(),
            AllowedSections = allowedSections,
            IsAdmin = user.IsAdmin(),
            UserGroupIds = presentationUser.UserGroupIds,
        };
    }

    private HashSet<IPermissionPresentationModel> GetAggregatedGranularPermissions(IUser user, IEnumerable<UserGroupResponseModel> presentationGroups)
    {
        var permissions = presentationGroups.SelectMany(x => x.Permissions).ToHashSet();
        return GetAggregatedGranularPermissions(user, permissions);
    }

    private HashSet<IPermissionPresentationModel> GetAggregatedGranularPermissions(IUser user, HashSet<IPermissionPresentationModel> permissions)
    {
        // The raw permission data consists of several permissions for each entity (e.g. document), as permissions are assigned to user groups
        // and a user may be part of multiple groups.  We want to aggregate this server-side so we return one set of aggregate permissions per
        // entity that the client will use.
        // We need to handle here not just permissions known to core (e.g. document and document property value permissions), but also custom
        // permissions defined by packages or implemetors.
        IEnumerable<(Type, IEnumerable<IPermissionPresentationModel>)> permissionModelsByType = permissions
            .GroupBy(x => x.GetType())
            .Select(x => (x.Key, x.Select(y => y)));

        var aggregatedPermissions = new HashSet<IPermissionPresentationModel>();
        foreach ((Type Type, IEnumerable<IPermissionPresentationModel> Models) permissionModelByType in permissionModelsByType)
        {
            if (_permissionPresentationMappersByType.TryGetValue(permissionModelByType.Type, out IPermissionPresentationMapper? mapper))
            {

                IEnumerable<IPermissionPresentationModel> aggregatedModels = mapper.AggregatePresentationModels(user, permissionModelByType.Models);
                foreach (IPermissionPresentationModel aggregatedModel in aggregatedModels)
                {
                    aggregatedPermissions.Add(aggregatedModel);
                }
            }
            else
            {
                IEnumerable<(string Context, ISet<string> Verbs)> groupedModels = permissionModelByType.Models
                    .Where(x => x is UnknownTypePermissionPresentationModel)
                    .Cast<UnknownTypePermissionPresentationModel>()
                    .GroupBy(x => x.Context)
                    .Select(x => (x.Key, (ISet<string>)x.SelectMany(y => y.Verbs).Distinct().ToHashSet()));

                foreach ((string context, ISet<string> verbs) in groupedModels)
                {
                    aggregatedPermissions.Add(new UnknownTypePermissionPresentationModel
                    {
                        Context = context,
                        Verbs = verbs
                    });
                }
            }
        }

        return aggregatedPermissions;
    }

    /// <inheritdoc/>
    public Task<CalculatedUserStartNodesResponseModel> CreateCalculatedUserStartNodesResponseModelAsync(IUser user)
    {
        var mediaStartNodeIds = user.CalculateMediaStartNodeIds(_entityService, _appCaches);
        ISet<ReferenceByIdModel> mediaStartNodeKeys = GetKeysFromIds(mediaStartNodeIds, UmbracoObjectTypes.Media);
        var contentStartNodeIds = user.CalculateContentStartNodeIds(_entityService, _appCaches);
        ISet<ReferenceByIdModel> documentStartNodeKeys = GetKeysFromIds(contentStartNodeIds, UmbracoObjectTypes.Document);
        var elementStartNodeIds = user.CalculateElementStartNodeIds(_entityService, _appCaches);
        ISet<ReferenceByIdModel> elementStartNodeKeys = GetKeysFromIds(elementStartNodeIds, UmbracoObjectTypes.ElementContainer);

        return Task.FromResult<CalculatedUserStartNodesResponseModel>(new CalculatedUserStartNodesResponseModel()
        {
            Id = user.Key,
            MediaStartNodeIds = mediaStartNodeKeys,
            HasMediaRootAccess = HasRootAccess(mediaStartNodeIds),
            DocumentStartNodeIds = documentStartNodeKeys,
            HasDocumentRootAccess = HasRootAccess(contentStartNodeIds),
            ElementStartNodeIds = elementStartNodeKeys,
            HasElementRootAccess = HasRootAccess(elementStartNodeIds),
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

    private static bool HasRootAccess(IEnumerable<int>? startNodeIds)
        => startNodeIds?.Contains(Constants.System.Root) is true;
}
