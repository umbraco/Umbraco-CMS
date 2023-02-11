using System.Globalization;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Sections;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using UserProfile = Umbraco.Cms.Core.Models.ContentEditing.UserProfile;

namespace Umbraco.Cms.Core.Models.Mapping;

public class UserMapDefinition : IMapDefinition
{
    private readonly ActionCollection _actions;
    private readonly AppCaches _appCaches;
    private readonly IEntityService _entityService;
    private readonly GlobalSettings _globalSettings;
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly MediaFileManager _mediaFileManager;
    private readonly ISectionService _sectionService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ILocalizedTextService _textService;
    private readonly IUserService _userService;
    private readonly ILocalizationService _localizationService;

    public UserMapDefinition(
        ILocalizedTextService textService,
        IUserService userService,
        IEntityService entityService,
        ISectionService sectionService,
        AppCaches appCaches,
        ActionCollection actions,
        IOptions<GlobalSettings> globalSettings,
        MediaFileManager mediaFileManager,
        IShortStringHelper shortStringHelper,
        IImageUrlGenerator imageUrlGenerator,
        ILocalizationService localizationService)
    {
        _sectionService = sectionService;
        _entityService = entityService;
        _userService = userService;
        _textService = textService;
        _actions = actions;
        _appCaches = appCaches;
        _globalSettings = globalSettings.Value;
        _mediaFileManager = mediaFileManager;
        _shortStringHelper = shortStringHelper;
        _imageUrlGenerator = imageUrlGenerator;
        _localizationService = localizationService;
    }

    [Obsolete("Please use constructor that takes an ILocalizationService instead")]
    public UserMapDefinition(
        ILocalizedTextService textService,
        IUserService userService,
        IEntityService entityService,
        ISectionService sectionService,
        AppCaches appCaches,
        ActionCollection actions,
        IOptions<GlobalSettings> globalSettings,
        MediaFileManager mediaFileManager,
        IShortStringHelper shortStringHelper,
        IImageUrlGenerator imageUrlGenerator)
    : this(
        textService,
        userService,
        entityService,
        sectionService,
        appCaches,
        actions,
        globalSettings,
        mediaFileManager,
        shortStringHelper,
        imageUrlGenerator,
        StaticServiceProvider.Instance.GetRequiredService<ILocalizationService>())
    {
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<UserGroupSave, IUserGroup>(
            (source, context) => new UserGroup(_shortStringHelper) { CreateDate = DateTime.UtcNow }, Map);
        mapper.Define<UserInvite, IUser>(Map);
        mapper.Define<IProfile, UserProfile>((source, context) => new UserProfile(), Map);
        mapper.Define<IReadOnlyUserGroup, UserGroupBasic>((source, context) => new UserGroupBasic(), Map);
        mapper.Define<IUserGroup, UserGroupBasic>((source, context) => new UserGroupBasic(), Map);
        mapper.Define<IUserGroup, AssignedUserGroupPermissions>(
            (source, context) => new AssignedUserGroupPermissions(),
            Map);
        mapper.Define<EntitySlim, AssignedContentPermissions>(
            (source, context) => new AssignedContentPermissions(),
            Map);
        mapper.Define<IUserGroup, UserGroupDisplay>((source, context) => new UserGroupDisplay(), Map);
        mapper.Define<IUser, UserBasic>((source, context) => new UserBasic(), Map);
        mapper.Define<IUser, UserDetail>((source, context) => new UserDetail(), Map);

        // used for merging existing UserSave to an existing IUser instance - this will not create an IUser instance!
        mapper.Define<UserSave, IUser>(Map);

        // important! Currently we are never mapping to multiple UserDisplay objects but if we start doing that
        // this will cause an N+1 and we'll need to change how this works.
        mapper.Define<IUser, UserDisplay>((source, context) => new UserDisplay(), Map);
    }

    // mappers
    private static void Map(UserGroupSave source, IUserGroup target, MapperContext context)
    {
        if (!(target is UserGroup ttarget))
        {
            throw new NotSupportedException($"{nameof(target)} must be a UserGroup.");
        }

        Map(source, ttarget);
    }

    // Umbraco.Code.MapAll -CreateDate -UpdateDate -DeleteDate
    private static void Map(UserGroupSave source, UserGroup target)
    {
        target.StartMediaId = source.StartMediaId;
        target.StartContentId = source.StartContentId;
        target.Icon = source.Icon;
        target.Alias = source.Alias;
        target.Name = source.Name;
        target.Permissions = source.DefaultPermissions;
        target.Key = source.Key;
        target.HasAccessToAllLanguages = source.HasAccessToAllLanguages;

        var id = GetIntId(source.Id);
        if (id > 0)
        {
            target.Id = id;
        }

        target.ClearAllowedSections();
        if (source.Sections is not null)
        {
            foreach (var section in source.Sections)
            {
                target.AddAllowedSection(section);
            }
        }

        target.ClearAllowedLanguages();
        if (source.AllowedLanguages is not null)
        {
            foreach (var language in source.AllowedLanguages)
            {
                target.AddAllowedLanguage(language);
            }
        }
    }

    // Umbraco.Code.MapAll
    private static void Map(IProfile source, UserProfile target, MapperContext context)
    {
        target.Name = source.Name;
        target.UserId = source.Id;
    }

    // Umbraco.Code.MapAll -Trashed -Alias -AssignedPermissions
    private static void Map(EntitySlim source, AssignedContentPermissions target, MapperContext context)
    {
        target.Icon = MapContentTypeIcon(source);
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = source.ParentId;
        target.Path = source.Path;
        target.Udi = Udi.Create(ObjectTypes.GetUdiType(source.NodeObjectType), source.Key);

        if (source.NodeObjectType == Constants.ObjectTypes.Member && target.Icon.IsNullOrWhiteSpace())
        {
            target.Icon = Constants.Icons.Member;
        }
    }

    private static string? MapContentTypeIcon(IEntitySlim entity)
        => entity is IContentEntitySlim contentEntity ? contentEntity.ContentTypeIcon : null;

    private static int GetIntId(object? id)
    {
        if (id is string strId &&
            int.TryParse(strId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asInt))
        {
            return asInt;
        }

        Attempt<int> result = id.TryConvertTo<int>();
        if (result.Success == false)
        {
            throw new InvalidOperationException(
                "Cannot convert the profile to a " + typeof(UserDetail).Name +
                " object since the id is not an integer");
        }

        return result.Result;
    }

    // Umbraco.Code.MapAll -CreateDate -UpdateDate -DeleteDate
    // Umbraco.Code.MapAll -Id -TourData -StartContentIds -StartMediaIds -Language -Username
    // Umbraco.Code.MapAll -PasswordQuestion -SessionTimeout -EmailConfirmedDate -InvitedDate
    // Umbraco.Code.MapAll -SecurityStamp -Avatar -ProviderUserKey -RawPasswordValue
    // Umbraco.Code.MapAll -RawPasswordAnswerValue -Comments -IsApproved -IsLockedOut -LastLoginDate
    // Umbraco.Code.MapAll -LastPasswordChangeDate -LastLockoutDate -FailedPasswordAttempts
    // Umbraco.Code.MapAll -PasswordConfiguration
    private void Map(UserInvite source, IUser target, MapperContext context)
    {
        target.Email = source.Email;
        target.Key = source.Key;
        target.Name = source.Name;
        target.IsApproved = false;

        target.ClearGroups();
        IEnumerable<IUserGroup> groups = _userService.GetUserGroupsByAlias(source.UserGroups.ToArray());
        foreach (IUserGroup group in groups)
        {
            target.AddGroup(group.ToReadOnlyGroup());
        }
    }

    // Umbraco.Code.MapAll -CreateDate -UpdateDate -DeleteDate
    // Umbraco.Code.MapAll -TourData -SessionTimeout -EmailConfirmedDate -InvitedDate -SecurityStamp -Avatar
    // Umbraco.Code.MapAll -ProviderUserKey -RawPasswordValue -RawPasswordAnswerValue -PasswordQuestion -Comments
    // Umbraco.Code.MapAll -IsApproved -IsLockedOut -LastLoginDate -LastPasswordChangeDate -LastLockoutDate
    // Umbraco.Code.MapAll -FailedPasswordAttempts
    // Umbraco.Code.MapAll -PasswordConfiguration
    private void Map(UserSave source, IUser target, MapperContext context)
    {
        target.Name = source.Name;
        target.StartContentIds = source.StartContentIds ?? Array.Empty<int>();
        target.StartMediaIds = source.StartMediaIds ?? Array.Empty<int>();
        target.Language = source.Culture;
        target.Email = source.Email;
        target.Key = source.Key;
        target.Username = source.Username;
        target.Id = source.Id;

        target.ClearGroups();
        IEnumerable<IUserGroup> groups = _userService.GetUserGroupsByAlias(source.UserGroups.ToArray());
        foreach (IUserGroup group in groups)
        {
            target.AddGroup(group.ToReadOnlyGroup());
        }
    }

        // Umbraco.Code.MapAll -ContentStartNode -UserCount -MediaStartNode -Key -Languages -Sections
    // Umbraco.Code.MapAll -Notifications -Udi -Trashed -AdditionalData -IsSystemUserGroup
    private void Map(IReadOnlyUserGroup source, UserGroupBasic target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.Icon = source.Icon;
        target.Id = source.Id;
        target.Name = source.Name;
        target.ParentId = -1;
        target.Path = "-1," + source.Id;
        target.IsSystemUserGroup = source.IsSystemUserGroup();
        target.HasAccessToAllLanguages = source.HasAccessToAllLanguages;

        MapUserGroupBasic(target, source.AllowedLanguages, source.AllowedSections, source.StartContentId, source.StartMediaId, context);
    }

    // Umbraco.Code.MapAll -ContentStartNode -MediaStartNode -Languages -Sections -Notifications
    // Umbraco.Code.MapAll -Udi -Trashed -AdditionalData -IsSystemUserGroup
    private void Map(IUserGroup source, UserGroupBasic target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.Icon = source.Icon;
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = -1;
        target.Path = "-1," + source.Id;
        target.UserCount = source.UserCount;
        target.IsSystemUserGroup = source.IsSystemUserGroup();
        target.HasAccessToAllLanguages = source.HasAccessToAllLanguages;

        MapUserGroupBasic(target, source.AllowedLanguages, source.AllowedSections, source.StartContentId, source.StartMediaId, context);
    }

    // Umbraco.Code.MapAll -Udi -Trashed -AdditionalData -AssignedPermissions
    private void Map(IUserGroup source, AssignedUserGroupPermissions target, MapperContext context)
    {
        target.Id = source.Id;
        target.Alias = source.Alias;
        target.Icon = source.Icon;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = -1;
        target.Path = "-1," + source.Id;

        target.DefaultPermissions = MapUserGroupDefaultPermissions(source);

        if (target.Icon.IsNullOrWhiteSpace())
        {
            target.Icon = Constants.Icons.UserGroup;
        }
    }

    // Umbraco.Code.MapAll -ContentStartNode -MediaStartNode -Languages -Sections -Notifications -Udi
    // Umbraco.Code.MapAll -Trashed -AdditionalData -Users -AssignedPermissions
    private void Map(IUserGroup source, UserGroupDisplay target, MapperContext context)
    {
        target.Alias = source.Alias;
        target.DefaultPermissions = MapUserGroupDefaultPermissions(source);
        target.Icon = source.Icon;
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.Name;
        target.ParentId = -1;
        target.Path = "-1," + source.Id;
        target.UserCount = source.UserCount;
        target.IsSystemUserGroup = source.IsSystemUserGroup();
        target.HasAccessToAllLanguages = source.HasAccessToAllLanguages;

        MapUserGroupBasic(target, source.AllowedLanguages, source.AllowedSections, source.StartContentId, source.StartMediaId, context);

        // Important! Currently we are never mapping to multiple UserGroupDisplay objects but if we start doing that
        // this will cause an N+1 and we'll need to change how this works.
        IEnumerable<IUser> users = _userService.GetAllInGroup(source.Id);
        target.Users = context.MapEnumerable<IUser, UserBasic>(users).WhereNotNull();

        // Deal with assigned permissions:
        var allContentPermissions = _userService.GetPermissions(source, true)
            .ToDictionary(x => x.EntityId, x => x);

        IEntitySlim[] contentEntities;
        if (allContentPermissions.Keys.Count == 0)
        {
            contentEntities = Array.Empty<IEntitySlim>();
        }
        else
        {
            // a group can end up with way more than 2000 assigned permissions,
            // so we need to break them into groups in order to avoid breaking
            // the entity service due to too many Sql parameters.
            var list = new List<IEntitySlim>();
            foreach (IEnumerable<int> idGroup in allContentPermissions.Keys.InGroupsOf(Constants.Sql.MaxParameterCount))
            {
                list.AddRange(_entityService.GetAll(UmbracoObjectTypes.Document, idGroup.ToArray()));
            }

            contentEntities = list.ToArray();
        }

        var allAssignedPermissions = new List<AssignedContentPermissions>();
        foreach (IEntitySlim entity in contentEntities)
        {
            EntityPermission contentPermissions = allContentPermissions[entity.Id];

            AssignedContentPermissions? assignedContentPermissions = context.Map<AssignedContentPermissions>(entity);
            if (assignedContentPermissions is null)
            {
                continue;
            }

            assignedContentPermissions.AssignedPermissions =
                AssignedUserGroupPermissions.ClonePermissions(target.DefaultPermissions);

            // since there is custom permissions assigned to this node for this group, we need to clear all of the default permissions
            // and we'll re-check it if it's one of the explicitly assigned ones
            foreach (Permission permission in assignedContentPermissions.AssignedPermissions.SelectMany(x => x.Value))
            {
                permission.Checked = false;
                permission.Checked =
                    contentPermissions.AssignedPermissions.Contains(
                        permission.PermissionCode,
                        StringComparer.InvariantCulture);
            }

            allAssignedPermissions.Add(assignedContentPermissions);
        }

        target.AssignedPermissions = allAssignedPermissions;
    }

    // Umbraco.Code.MapAll -Notifications -Udi -Icon -IsCurrentUser -Trashed -ResetPasswordValue
    // Umbraco.Code.MapAll -Alias -AdditionalData
    private void Map(IUser source, UserDisplay target, MapperContext context)
    {
        target.AvailableCultures = _textService.GetSupportedCultures().ToDictionary(x => x.Name, x => x.DisplayName);
        target.Avatars = source.GetUserAvatarUrls(_appCaches.RuntimeCache, _mediaFileManager, _imageUrlGenerator);
        target.CalculatedStartContentIds =
            GetStartNodes(source.CalculateContentStartNodeIds(_entityService, _appCaches), UmbracoObjectTypes.Document, "content", "contentRoot", context);
        target.CalculatedStartMediaIds = GetStartNodes(source.CalculateMediaStartNodeIds(_entityService, _appCaches), UmbracoObjectTypes.Media, "media", "mediaRoot", context);
        target.CreateDate = source.CreateDate;
        target.Culture = source.GetUserCulture(_textService, _globalSettings).ToString();
        target.Email = source.Email;
        target.EmailHash = source.Email?.ToLowerInvariant().Trim().GenerateHash();
        target.FailedPasswordAttempts = source.FailedPasswordAttempts;
        target.Id = source.Id;
        target.Key = source.Key;
        target.LastLockoutDate = source.LastLockoutDate;
        target.LastLoginDate = source.LastLoginDate == default(DateTime) ? null : source.LastLoginDate;
        target.LastPasswordChangeDate = source.LastPasswordChangeDate;
        target.Name = source.Name;
        target.Navigation = CreateUserEditorNavigation();
        target.ParentId = -1;
        target.Path = "-1," + source.Id;
        target.StartContentIds = GetStartNodes(source.StartContentIds?.ToArray(), UmbracoObjectTypes.Document, "content", "contentRoot", context);
        target.StartMediaIds = GetStartNodes(source.StartMediaIds?.ToArray(), UmbracoObjectTypes.Media, "media", "mediaRoot", context);
        target.UpdateDate = source.UpdateDate;
        target.UserGroups = context.MapEnumerable<IReadOnlyUserGroup, UserGroupBasic>(source.Groups).WhereNotNull();
        target.Username = source.Username;
        target.UserState = source.UserState;
    }

    // Umbraco.Code.MapAll -Notifications -IsCurrentUser -Udi -Icon -Trashed -Alias -AdditionalData
    private void Map(IUser source, UserBasic target, MapperContext context)
    {
        // Loading in the user avatar's requires an external request if they don't have a local file avatar, this means that initial load of paging may incur a cost
        // Alternatively, if this is annoying the back office UI would need to be updated to request the avatars for the list of users separately so it doesn't look
        // like the load time is waiting.
        target.Avatars = source.GetUserAvatarUrls(_appCaches.RuntimeCache, _mediaFileManager, _imageUrlGenerator);
        target.Culture = source.GetUserCulture(_textService, _globalSettings).ToString();
        target.Email = source.Email;
        target.EmailHash = source.Email?.ToLowerInvariant().Trim().GenerateHash();
        target.Id = source.Id;
        target.Key = source.Key;
        target.LastLoginDate = source.LastLoginDate == default ? null : source.LastLoginDate;
        target.Name = source.Name;
        target.ParentId = -1;
        target.Path = "-1," + source.Id;
        target.UserGroups = context.MapEnumerable<IReadOnlyUserGroup, UserGroupBasic>(source.Groups).WhereNotNull();
        target.Username = source.Username;
        target.UserState = source.UserState;
    }

    // Umbraco.Code.MapAll -SecondsUntilTimeout
    private void Map(IUser source, UserDetail target, MapperContext context)
    {
        target.AllowedSections = source.AllowedSections;
        target.AllowedLanguageIds = source.CalculateAllowedLanguageIds(_localizationService);
        target.Avatars = source.GetUserAvatarUrls(_appCaches.RuntimeCache, _mediaFileManager, _imageUrlGenerator);
        target.Culture = source.GetUserCulture(_textService, _globalSettings).ToString();
        target.Email = source.Email;
        target.EmailHash = source.Email?.ToLowerInvariant().Trim().GenerateHash();
        target.Name = source.Name;
        target.StartContentIds = source.CalculateContentStartNodeIds(_entityService, _appCaches);
        target.StartMediaIds = source.CalculateMediaStartNodeIds(_entityService, _appCaches);
        target.UserId = source.Id;

        // we need to map the legacy UserType
        // the best we can do here is to return the user's first user group as a IUserType object
        // but we should attempt to return any group that is the built in ones first
        target.UserGroups = source.Groups.Select(x => x.Alias).ToArray();
    }

    // helpers
    private void MapUserGroupBasic(UserGroupBasic target, IEnumerable<int> sourceAllowedLanguages, IEnumerable<string> sourceAllowedSections, int? sourceStartContentId, int? sourceStartMediaId, MapperContext context)
    {
        var allLanguages = _localizationService.GetAllLanguages();
        var applicableLanguages = Enumerable.Empty<ILanguage>();


        if (sourceAllowedLanguages.Any())
        {
            applicableLanguages = allLanguages.Where(x => sourceAllowedLanguages.Contains(x.Id));
        }

        target.Languages = context.MapEnumerable<ILanguage, ContentEditing.Language>(applicableLanguages).WhereNotNull();

        var allSections = _sectionService.GetSections();
        target.Sections = context.MapEnumerable<ISection, Section>(allSections.Where(x => sourceAllowedSections.Contains(x.Alias))).WhereNotNull();

        if (sourceStartMediaId > 0)
        {
            target.MediaStartNode =
                context.Map<EntityBasic>(_entityService.Get(sourceStartMediaId.Value, UmbracoObjectTypes.Media));
        }
        else if (sourceStartMediaId == -1)
        {
            target.MediaStartNode = CreateRootNode(_textService.Localize("media", "mediaRoot"));
        }

        if (sourceStartContentId > 0)
        {
            target.ContentStartNode =
                context.Map<EntityBasic>(_entityService.Get(sourceStartContentId.Value, UmbracoObjectTypes.Document));
        }
        else if (sourceStartContentId == -1)
        {
            target.ContentStartNode = CreateRootNode(_textService.Localize("content", "contentRoot"));
        }

        if (target.Icon.IsNullOrWhiteSpace())
        {
            target.Icon = Constants.Icons.UserGroup;
        }
    }

    private IDictionary<string, IEnumerable<Permission>> MapUserGroupDefaultPermissions(IUserGroup source)
    {
        Permission GetPermission(IAction action)
        {
            return new()
            {
                Category = action.Category.IsNullOrWhiteSpace()
                    ? _textService.Localize(
                        "actionCategories",
                        Constants.Conventions.PermissionCategories.OtherCategory)
                    : _textService.Localize("actionCategories", action.Category),
                Name = _textService.Localize("actions", action.Alias),
                Description = _textService.Localize("actionDescriptions", action.Alias),
                Icon = action.Icon,
                Checked = source.Permissions != null &&
                          source.Permissions.Contains(action.Letter.ToString(CultureInfo.InvariantCulture)),
                PermissionCode = action.Letter.ToString(CultureInfo.InvariantCulture),
            };
        }

        return _actions
            .Where(x => x.CanBePermissionAssigned)
            .Select(GetPermission)
            .GroupBy(x => x.Category)
            .ToDictionary(x => x.Key, x => (IEnumerable<Permission>)x.ToArray());
    }

    private IEnumerable<EntityBasic> GetStartNodes(int[]? startNodeIds, UmbracoObjectTypes objectType, string localizedArea, string localizedAlias, MapperContext context)
    {
        if (startNodeIds is null || startNodeIds.Length <= 0)
        {
            return Enumerable.Empty<EntityBasic>();
        }

        var startNodes = new List<EntityBasic>();
        if (startNodeIds.Contains(-1))
        {
            startNodes.Add(CreateRootNode(_textService.Localize(localizedArea, localizedAlias)));
        }

        IEnumerable<IEntitySlim> mediaItems = _entityService.GetAll(objectType, startNodeIds);
        startNodes.AddRange(context.MapEnumerable<IEntitySlim, EntityBasic>(mediaItems).WhereNotNull());
        return startNodes;
    }

    private IEnumerable<EditorNavigation> CreateUserEditorNavigation() =>
        new[]
        {
            new EditorNavigation
            {
                Active = true,
                Alias = "details",
                Icon = "icon-umb-users",
                Name = _textService.Localize("general", "user"),
                View = "views/users/views/user/details.html",
            },
        };

    private EntityBasic CreateRootNode(string name) =>
        new EntityBasic
        {
            Name = name,
            Path = "-1",
            Icon = "icon-folder",
            Id = -1,
            Trashed = false,
            ParentId = -1,
        };
}
