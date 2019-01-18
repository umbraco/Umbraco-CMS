using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Services;


namespace Umbraco.Web.Models.Mapping
{
    internal class UserMapperProfile : Profile
    {
        private static string GetContentTypeIcon(EntitySlim entity)
            => entity is ContentEntitySlim contentEntity ? contentEntity.ContentTypeIcon : null;

        public UserMapperProfile(ILocalizedTextService textService, IUserService userService, IEntityService entityService, ISectionService sectionService,
            IRuntimeCacheProvider runtimeCache, ActionCollection actions, IGlobalSettings globalSettings)
        {
            var userGroupDefaultPermissionsResolver = new UserGroupDefaultPermissionsResolver(textService, actions);

            CreateMap<UserGroupSave, IUserGroup>()
                .ConstructUsing(save => new UserGroup { CreateDate = DateTime.UtcNow })
                .IgnoreEntityCommonProperties()
                .ForMember(dest => dest.Id, opt => opt.Condition(source => GetIntId(source.Id) > 0))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => GetIntId(source.Id)))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(source => source.DefaultPermissions))
                .AfterMap((save, userGroup) =>
                {
                    userGroup.ClearAllowedSections();
                    foreach (var section in save.Sections)
                    {
                        userGroup.AddAllowedSection(section);
                    }
                });

            //Used for merging existing UserSave to an existing IUser instance - this will not create an IUser instance!
            CreateMap<UserSave, IUser>()
                .IgnoreEntityCommonProperties()
                .ForMember(dest => dest.Id, opt => opt.Condition(src => GetIntId(src.Id) > 0))
                .ForMember(detail => detail.TourData, opt => opt.Ignore())
                .ForMember(dest => dest.SessionTimeout, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmedDate, opt => opt.Ignore())
                .ForMember(dest => dest.InvitedDate, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.Avatar, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderUserKey, opt => opt.Ignore())
                .ForMember(dest => dest.RawPasswordValue, opt => opt.Ignore())
                .ForMember(dest => dest.RawPasswordAnswerValue, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordQuestion, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.IsApproved, opt => opt.Ignore())
                .ForMember(dest => dest.IsLockedOut, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastPasswordChangeDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastLockoutDate, opt => opt.Ignore())
                .ForMember(dest => dest.FailedPasswordAttempts, opt => opt.Ignore())
                .ForMember(user => user.Language, opt => opt.MapFrom(save => save.Culture))
                .AfterMap((save, user) =>
                {
                    user.ClearGroups();
                    var foundGroups = userService.GetUserGroupsByAlias(save.UserGroups.ToArray());
                    foreach (var group in foundGroups)
                    {
                        user.AddGroup(group.ToReadOnlyGroup());
                    }
                });

            CreateMap<UserInvite, IUser>()
                .IgnoreEntityCommonProperties()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(detail => detail.TourData, opt => opt.Ignore())
                .ForMember(dest => dest.StartContentIds, opt => opt.Ignore())
                .ForMember(dest => dest.StartMediaIds, opt => opt.Ignore())
                .ForMember(dest => dest.Language, opt => opt.Ignore())
                .ForMember(dest => dest.Username, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordQuestion, opt => opt.Ignore())
                .ForMember(dest => dest.SessionTimeout, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmedDate, opt => opt.Ignore())
                .ForMember(dest => dest.InvitedDate, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.Avatar, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderUserKey, opt => opt.Ignore())
                .ForMember(dest => dest.RawPasswordValue, opt => opt.Ignore())
                .ForMember(dest => dest.RawPasswordAnswerValue, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.IsApproved, opt => opt.Ignore())
                .ForMember(dest => dest.IsLockedOut, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastPasswordChangeDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastLockoutDate, opt => opt.Ignore())
                .ForMember(dest => dest.FailedPasswordAttempts, opt => opt.Ignore())
                //all invited users will not be approved, completing the invite will approve the user
                .ForMember(user => user.IsApproved, opt => opt.UseValue(false))
                .AfterMap((invite, user) =>
                {
                    user.ClearGroups();
                    var foundGroups = userService.GetUserGroupsByAlias(invite.UserGroups.ToArray());
                    foreach (var group in foundGroups)
                    {
                        user.AddGroup(group.ToReadOnlyGroup());
                    }
                });

            CreateMap<IReadOnlyUserGroup, UserGroupBasic>()
                .ForMember(dest => dest.ContentStartNode, opt => opt.Ignore())
                .ForMember(dest => dest.UserCount, opt => opt.Ignore())
                .ForMember(dest => dest.MediaStartNode, opt => opt.Ignore())
                .ForMember(dest => dest.Key, opt => opt.Ignore())
                .ForMember(dest => dest.Sections, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.UseValue(-1))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .AfterMap((group, display) =>
                {
                    MapUserGroupBasic(sectionService, entityService, textService, group, display);
                });

            CreateMap<IUserGroup, UserGroupBasic>()
                .ForMember(dest => dest.ContentStartNode, opt => opt.Ignore())
                .ForMember(dest => dest.MediaStartNode, opt => opt.Ignore())
                .ForMember(dest => dest.Sections, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.UseValue(-1))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .AfterMap((group, display) =>
                {
                    MapUserGroupBasic(sectionService, entityService, textService, group, display);
                });

            //create a map to assign a user group's default permissions to the AssignedUserGroupPermissions instance
            CreateMap<IUserGroup, AssignedUserGroupPermissions>()
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(group => group.Id))
                .ForMember(dest => dest.ParentId, opt => opt.UseValue(-1))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(dest => dest.DefaultPermissions, opt => opt.ResolveUsing(src => userGroupDefaultPermissionsResolver.Resolve(src)))
                //these will be manually mapped and by default they are null
                .ForMember(dest => dest.AssignedPermissions, opt => opt.Ignore())
                .AfterMap((group, display) =>
                {
                    if (display.Icon.IsNullOrWhiteSpace())
                    {
                        display.Icon = "icon-users";
                    }
                });

            CreateMap<EntitySlim, AssignedContentPermissions>()
                .ForMember(x => x.Udi, opt => opt.MapFrom(x => Udi.Create(ObjectTypes.GetUdiType(x.NodeObjectType), x.Key)))
                .ForMember(basic => basic.Icon, opt => opt.MapFrom(entity => GetContentTypeIcon(entity)))
                .ForMember(dto => dto.Trashed, opt => opt.Ignore())
                .ForMember(x => x.Alias, opt => opt.Ignore())
                .ForMember(x => x.AssignedPermissions, opt => opt.Ignore())
                .AfterMap((entity, basic) =>
                {
                    if (entity.NodeObjectType == Constants.ObjectTypes.Member && basic.Icon.IsNullOrWhiteSpace())
                    {
                        basic.Icon = "icon-user";
                    }
                });

            CreateMap<IUserGroup, UserGroupDisplay>()
                .ForMember(dest => dest.ContentStartNode, opt => opt.Ignore())
                .ForMember(dest => dest.MediaStartNode, opt => opt.Ignore())
                .ForMember(dest => dest.Sections, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.UseValue(-1))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.Users, opt => opt.Ignore())
                .ForMember(dest => dest.DefaultPermissions, opt => opt.ResolveUsing(src => userGroupDefaultPermissionsResolver.Resolve(src)))
                .ForMember(dest => dest.AssignedPermissions, opt => opt.Ignore())
                .AfterMap((group, display) =>
                {
                    MapUserGroupBasic(sectionService, entityService, textService, group, display);

                    //Important! Currently we are never mapping to multiple UserGroupDisplay objects but if we start doing that
                    // this will cause an N+1 and we'll need to change how this works.
                    var users = userService.GetAllInGroup(group.Id);
                    display.Users = Mapper.Map<IEnumerable<UserBasic>>(users);

                    //Deal with assigned permissions:

                    var allContentPermissions = userService.GetPermissions(@group, true)
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
                        foreach (var idGroup in allContentPermissions.Keys.InGroupsOf(2000))
                            list.AddRange(entityService.GetAll(UmbracoObjectTypes.Document, idGroup.ToArray()));
                        contentEntities = list.ToArray();
                    }

                    var allAssignedPermissions = new List<AssignedContentPermissions>();
                    foreach (var entity in contentEntities)
                    {
                        var contentPermissions = allContentPermissions[entity.Id];

                        var assignedContentPermissions = Mapper.Map<AssignedContentPermissions>(entity);
                        assignedContentPermissions.AssignedPermissions = AssignedUserGroupPermissions.ClonePermissions(display.DefaultPermissions);

                        //since there is custom permissions assigned to this node for this group, we need to clear all of the default permissions
                        //and we'll re-check it if it's one of the explicitly assigned ones
                        foreach (var permission in assignedContentPermissions.AssignedPermissions.SelectMany(x => x.Value))
                        {
                            permission.Checked = false;
                            permission.Checked = contentPermissions.AssignedPermissions.Contains(permission.PermissionCode, StringComparer.InvariantCulture);
                        }

                        allAssignedPermissions.Add(assignedContentPermissions);
                    }

                    display.AssignedPermissions = allAssignedPermissions;
                });

            //Important! Currently we are never mapping to multiple UserDisplay objects but if we start doing that
            // this will cause an N+1 and we'll need to change how this works.
            CreateMap<IUser, UserDisplay>()
                .ForMember(dest => dest.Avatars, opt => opt.MapFrom(user => user.GetUserAvatarUrls(runtimeCache)))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(user => user.Username))
                .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(user => user.LastLoginDate == default(DateTime) ? null : (DateTime?)user.LastLoginDate))
                .ForMember(dest => dest.UserGroups, opt => opt.MapFrom(user => user.Groups))
                .ForMember(detail => detail.Navigation, opt => opt.MapFrom(user => CreateUserEditorNavigation(textService)))
                .ForMember(
                    dest => dest.CalculatedStartContentIds,
                    opt => opt.MapFrom(src => GetStartNodeValues(
                        src.CalculateContentStartNodeIds(entityService),
                        textService, entityService, UmbracoObjectTypes.Document, "content/contentRoot")))
                .ForMember(
                    dest => dest.CalculatedStartMediaIds,
                    opt => opt.MapFrom(src => GetStartNodeValues(
                        src.CalculateMediaStartNodeIds(entityService),
                        textService, entityService, UmbracoObjectTypes.Media, "media/mediaRoot")))
                .ForMember(
                    dest => dest.StartContentIds,
                    opt => opt.MapFrom(src => GetStartNodeValues(
                        src.StartContentIds.ToArray(),
                        textService, entityService, UmbracoObjectTypes.Document, "content/contentRoot")))
                .ForMember(
                    dest => dest.StartMediaIds,
                    opt => opt.MapFrom(src => GetStartNodeValues(
                        src.StartMediaIds.ToArray(),
                        textService, entityService, UmbracoObjectTypes.Media, "media/mediaRoot")))
                .ForMember(dest => dest.Culture, opt => opt.MapFrom(user => user.GetUserCulture(textService, globalSettings)))
                .ForMember(
                    dest => dest.AvailableCultures,
                    opt => opt.MapFrom(user => textService.GetSupportedCultures().ToDictionary(x => x.Name, x => x.DisplayName)))
                .ForMember(
                    dest => dest.EmailHash,
                    opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().GenerateHash()))
                .ForMember(dest => dest.ParentId, opt => opt.UseValue(-1))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(user => "-1," + user.Id))
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.IsCurrentUser, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.ResetPasswordValue, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<IUser, UserBasic>()
                //Loading in the user avatar's requires an external request if they don't have a local file avatar, this means that initial load of paging may incur a cost
                //Alternatively, if this is annoying the back office UI would need to be updated to request the avatars for the list of users separately so it doesn't look
                //like the load time is waiting.
                .ForMember(detail =>
                    detail.Avatars,
                    opt => opt.MapFrom(user => user.GetUserAvatarUrls(runtimeCache)))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(user => user.Username))
                .ForMember(dest => dest.UserGroups, opt => opt.MapFrom(user => user.Groups))
                .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(user => user.LastLoginDate == default(DateTime) ? null : (DateTime?)user.LastLoginDate))
                .ForMember(dest => dest.Culture, opt => opt.MapFrom(user => user.GetUserCulture(textService, globalSettings)))
                .ForMember(
                    dest => dest.EmailHash,
                    opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().ToMd5()))
                .ForMember(dest => dest.ParentId, opt => opt.UseValue(-1))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(user => "-1," + user.Id))
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.IsCurrentUser, opt => opt.Ignore())
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<IUser, UserDetail>()
                .ForMember(dest => dest.Avatars, opt => opt.MapFrom(user => user.GetUserAvatarUrls(runtimeCache)))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(user => GetIntId(user.Id)))
                .ForMember(dest => dest.StartContentIds, opt => opt.MapFrom(user => user.CalculateContentStartNodeIds(entityService)))
                .ForMember(dest => dest.StartMediaIds, opt => opt.MapFrom(user => user.CalculateMediaStartNodeIds(entityService)))
                .ForMember(dest => dest.Culture, opt => opt.MapFrom(user => user.GetUserCulture(textService, globalSettings)))
                .ForMember(
                    dest => dest.EmailHash,
                    opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().GenerateHash()))
                .ForMember(dest => dest.SecondsUntilTimeout, opt => opt.Ignore())
                .ForMember(dest => dest.UserGroups, opt => opt.Ignore())
                .AfterMap((user, detail) =>
                {
                    //we need to map the legacy UserType
                    //the best we can do here is to return the user's first user group as a IUserType object
                    //but we should attempt to return any group that is the built in ones first
                    var groups = user.Groups.ToArray();
                    detail.UserGroups = user.Groups.Select(x => x.Alias).ToArray();

                });

            CreateMap<IProfile, ContentEditing.UserProfile>()
                  .ForMember(dest => dest.UserId, opt => opt.MapFrom(profile => GetIntId(profile.Id)));
        }

        private IEnumerable<EditorNavigation> CreateUserEditorNavigation(ILocalizedTextService textService)
        {
            return new[]
            {
                new EditorNavigation
                {
                    Active = true,
                    Alias = "details",
                    Icon = "icon-umb-users",
                    Name = textService.Localize("general/user"),
                    View = "views/users/views/user/details.html"
                }
            };
        }

        private IEnumerable<EntityBasic> GetStartNodeValues(int[] startNodeIds,
            ILocalizedTextService textService, IEntityService entityService, UmbracoObjectTypes objectType,
            string localizedKey)
        {
            if (startNodeIds.Length <= 0)
                return Enumerable.Empty<EntityBasic>();

            var startNodes = new List<EntityBasic>();
            if (startNodeIds.Contains(-1))
                startNodes.Add(RootNode(textService.Localize(localizedKey)));

            var mediaItems = entityService.GetAll(objectType, startNodeIds);
            startNodes.AddRange(Mapper.Map<IEnumerable<IUmbracoEntity>, IEnumerable<EntityBasic>>(mediaItems));
            return startNodes;
        }

        private void MapUserGroupBasic(ISectionService sectionService, IEntityService entityService, ILocalizedTextService textService, dynamic group, UserGroupBasic display)
        {
            var allSections = sectionService.GetSections();
            display.Sections = allSections.Where(x => Enumerable.Contains(group.AllowedSections, x.Alias)).Select(Mapper.Map<ContentEditing.Section>);

            if (group.StartMediaId > 0)
            {
                display.MediaStartNode = Mapper.Map<EntityBasic>(
                    entityService.Get(group.StartMediaId, UmbracoObjectTypes.Media));
            }
            else if (group.StartMediaId == -1)
            {
                //create the root node
                display.MediaStartNode = RootNode(textService.Localize("media/mediaRoot"));
            }

            if (group.StartContentId > 0)
            {
                display.ContentStartNode = Mapper.Map<EntityBasic>(
                    entityService.Get(group.StartContentId, UmbracoObjectTypes.Document));
            }
            else if (group.StartContentId == -1)
            {
                //create the root node
                display.ContentStartNode = RootNode(textService.Localize("content/contentRoot"));
            }

            if (display.Icon.IsNullOrWhiteSpace())
            {
                display.Icon = "icon-users";
            }
        }

        private EntityBasic RootNode(string name)
        {
            return new EntityBasic
            {
                Name = name,
                Path = "-1",
                Icon = "icon-folder",
                Id = -1,
                Trashed = false,
                ParentId = -1
            };
        }

        private static int GetIntId(object id)
        {
            var result = id.TryConvertTo<int>();
            if (result.Success == false)
            {
                throw new InvalidOperationException(
                    "Cannot convert the profile to a " + typeof(UserDetail).Name + " object since the id is not an integer");
            }
            return result.Result;
        }
    }
}
