using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using UserProfile = Umbraco.Web.Models.ContentEditing.UserProfile;

namespace Umbraco.Web.Models.Mapping
{
    internal class UserModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            
            config.CreateMap<UserGroupSave, IUserGroup>()
                .ConstructUsing((UserGroupSave save) => new UserGroup() { CreateDate = DateTime.UtcNow })
                .IgnoreDeletableEntityCommonProperties()
                .ForMember(dest => dest.Id, map => map.Condition(source => GetIntId(source.Id) > 0))
                .ForMember(dest => dest.Id, map => map.MapFrom(source => GetIntId(source.Id)))
                //TODO: This is insane - but with our current version of AutoMapper when mapping from an existing object to another existing object, it will map the private fields which means the public setter is not used! wtf. So zpqrtbnk will laugh and say how crappy AutoMapper is... well he'll win this battle this time, so we need to do this in AfterMap to make sure the public setter is used so the property is dirty
                //.ForMember(dest => dest.Permissions, map => map.MapFrom(source => source.DefaultPermissions))
                .ForMember(dest => dest.Permissions, map => map.Ignore())
                .AfterMap((save, userGroup) =>
                {
                    //TODO: See above comment
                    userGroup.Permissions = save.DefaultPermissions;

                    userGroup.ClearAllowedSections();
                    foreach (var section in save.Sections)
                    {
                        userGroup.AddAllowedSection(section);
                    }
                });

            //Used for merging existing UserSave to an existing IUser instance - this will not create an IUser instance!
            config.CreateMap<UserSave, IUser>()
                .IgnoreDeletableEntityCommonProperties()
                .ForMember(dest => dest.Id, map => map.Condition(source => GetIntId(source.Id) > 0))
                .ForMember(detail => detail.TourData, opt => opt.Ignore())
                .ForMember(detail => detail.SessionTimeout, opt => opt.Ignore())
                .ForMember(detail => detail.EmailConfirmedDate, opt => opt.Ignore())
                .ForMember(detail => detail.UserType, opt => opt.Ignore())
                .ForMember(detail => detail.StartContentId, opt => opt.Ignore())
                .ForMember(detail => detail.StartMediaId, opt => opt.Ignore())
                .ForMember(detail => detail.InvitedDate, opt => opt.Ignore())                
                .ForMember(detail => detail.SecurityStamp, opt => opt.Ignore())
                .ForMember(detail => detail.Avatar, opt => opt.Ignore())
                .ForMember(detail => detail.ProviderUserKey, opt => opt.Ignore())
                .ForMember(detail => detail.RawPasswordValue, opt => opt.Ignore())
                .ForMember(detail => detail.RawPasswordAnswerValue, opt => opt.Ignore())
                .ForMember(detail => detail.PasswordQuestion, opt => opt.Ignore())
                .ForMember(detail => detail.Comments, opt => opt.Ignore())
                .ForMember(detail => detail.IsApproved, opt => opt.Ignore())
                .ForMember(detail => detail.IsLockedOut, opt => opt.Ignore())
                .ForMember(detail => detail.LastLoginDate, opt => opt.Ignore())
                .ForMember(detail => detail.LastPasswordChangeDate, opt => opt.Ignore())
                .ForMember(detail => detail.LastLockoutDate, opt => opt.Ignore())
                .ForMember(detail => detail.FailedPasswordAttempts, opt => opt.Ignore())                
                .ForMember(user => user.Language, expression => expression.MapFrom(save => save.Culture))                
                .AfterMap((save, user) =>
                {
                    user.ClearGroups();
                    var foundGroups = applicationContext.Services.UserService.GetUserGroupsByAlias(save.UserGroups.ToArray());
                    foreach (var group in foundGroups)
                    {
                        user.AddGroup(group.ToReadOnlyGroup());
                    }
                });

            config.CreateMap<UserInvite, IUser>()
                .IgnoreDeletableEntityCommonProperties()
                .ForMember(detail => detail.Id, opt => opt.Ignore())
                .ForMember(detail => detail.TourData, opt => opt.Ignore())
                .ForMember(detail => detail.StartContentIds, opt => opt.Ignore())
                .ForMember(detail => detail.StartMediaIds, opt => opt.Ignore())
                .ForMember(detail => detail.UserType, opt => opt.Ignore())
                .ForMember(detail => detail.StartContentId, opt => opt.Ignore())
                .ForMember(detail => detail.StartMediaId, opt => opt.Ignore())
                .ForMember(detail => detail.Language, opt => opt.Ignore())
                .ForMember(detail => detail.Username, opt => opt.Ignore())
                .ForMember(detail => detail.PasswordQuestion, opt => opt.Ignore())
                .ForMember(detail => detail.SessionTimeout, opt => opt.Ignore())
                .ForMember(detail => detail.EmailConfirmedDate, opt => opt.Ignore())
                .ForMember(detail => detail.InvitedDate, opt => opt.Ignore())
                .ForMember(detail => detail.SecurityStamp, opt => opt.Ignore())
                .ForMember(detail => detail.Avatar, opt => opt.Ignore())
                .ForMember(detail => detail.ProviderUserKey, opt => opt.Ignore())
                .ForMember(detail => detail.RawPasswordValue, opt => opt.Ignore())
                .ForMember(detail => detail.RawPasswordAnswerValue, opt => opt.Ignore())
                .ForMember(detail => detail.Comments, opt => opt.Ignore())
                .ForMember(detail => detail.IsApproved, opt => opt.Ignore())
                .ForMember(detail => detail.IsLockedOut, opt => opt.Ignore())
                .ForMember(detail => detail.LastLoginDate, opt => opt.Ignore())
                .ForMember(detail => detail.LastPasswordChangeDate, opt => opt.Ignore())
                .ForMember(detail => detail.LastLockoutDate, opt => opt.Ignore())
                .ForMember(detail => detail.FailedPasswordAttempts, opt => opt.Ignore())                
                //all invited users will not be approved, completing the invite will approve the user
                .ForMember(user => user.IsApproved, expression => expression.UseValue(false))                
                .AfterMap((invite, user) =>
                {
                    user.ClearGroups();
                    var foundGroups = applicationContext.Services.UserService.GetUserGroupsByAlias(invite.UserGroups.ToArray());
                    foreach (var group in foundGroups)
                    {
                        user.AddGroup(group.ToReadOnlyGroup());
                    }
                });

            config.CreateMap<IReadOnlyUserGroup, UserGroupBasic>()
                .ForMember(detail => detail.ContentStartNode, opt => opt.Ignore())
                .ForMember(detail => detail.UserCount, opt => opt.Ignore())
                .ForMember(detail => detail.MediaStartNode, opt => opt.Ignore())
                .ForMember(detail => detail.Key, opt => opt.Ignore())
                .ForMember(detail => detail.Sections, opt => opt.Ignore())
                .ForMember(detail => detail.Notifications, opt => opt.Ignore())
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore())
                .AfterMap((group, display) =>
                {
                    MapUserGroupBasic(applicationContext.Services, group, display);
                });

            config.CreateMap<IUserGroup, UserGroupBasic>()
                .ForMember(detail => detail.ContentStartNode, opt => opt.Ignore())
                .ForMember(detail => detail.MediaStartNode, opt => opt.Ignore())
                .ForMember(detail => detail.Sections, opt => opt.Ignore())
                .ForMember(detail => detail.Notifications, opt => opt.Ignore())
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore())
                .AfterMap((group, display) =>
                {
                    MapUserGroupBasic(applicationContext.Services, group, display);
                });

            //create a map to assign a user group's default permissions to the AssignedUserGroupPermissions instance
            config.CreateMap<IUserGroup, AssignedUserGroupPermissions>()
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())                
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore())
                .ForMember(detail => detail.Id, opt => opt.MapFrom(group => group.Id))
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(detail => detail.DefaultPermissions, expression => expression.ResolveUsing(new UserGroupDefaultPermissionsResolver(applicationContext.Services.TextService)))
                //these will be manually mapped and by default they are null
                .ForMember(detail => detail.AssignedPermissions, opt => opt.Ignore())
                .AfterMap((group, display) =>
                {
                    if (display.Icon.IsNullOrWhiteSpace())
                    {
                        display.Icon = "icon-users";
                    }
                });

            config.CreateMap<UmbracoEntity, AssignedContentPermissions>()
                .ForMember(x => x.Udi, expression => expression.MapFrom(x => Udi.Create(UmbracoObjectTypesExtensions.GetUdiType(x.NodeObjectTypeId), x.Key)))
                .ForMember(basic => basic.Icon, expression => expression.MapFrom(entity => entity.ContentTypeIcon))
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(x => x.Alias, expression => expression.Ignore())
                .ForMember(x => x.AssignedPermissions, expression => expression.Ignore())
                .AfterMap((entity, basic) =>
                {
                    if (entity.NodeObjectTypeId == Constants.ObjectTypes.MemberGuid && basic.Icon.IsNullOrWhiteSpace())
                    {
                        basic.Icon = "icon-user";
                    }
                });

            config.CreateMap<IUserGroup, UserGroupDisplay>()
                .ForMember(detail => detail.ContentStartNode, opt => opt.Ignore())
                .ForMember(detail => detail.MediaStartNode, opt => opt.Ignore())
                .ForMember(detail => detail.Sections, opt => opt.Ignore())
                .ForMember(detail => detail.Notifications, opt => opt.Ignore())
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore())
                .ForMember(detail => detail.Users, opt => opt.Ignore())
                .ForMember(detail => detail.DefaultPermissions, expression => expression.ResolveUsing(new UserGroupDefaultPermissionsResolver(applicationContext.Services.TextService)))
                .ForMember(detail => detail.AssignedPermissions, opt => opt.Ignore())
                .AfterMap((group, display) =>
                {
                    MapUserGroupBasic(applicationContext.Services, group, display);

                    //Important! Currently we are never mapping to multiple UserGroupDisplay objects but if we start doing that
                    // this will cause an N+1 and we'll need to change how this works.
                    var users = applicationContext.Services.UserService.GetAllInGroup(group.Id);
                    display.Users = Mapper.Map<IEnumerable<UserBasic>>(users);

                    //Deal with assigned permissions:

                    var allContentPermissions = applicationContext.Services.UserService.GetPermissions(@group, true)
                        .ToDictionary(x => x.EntityId, x => x);

                    IEnumerable<IUmbracoEntity> contentEntities;
                    if (allContentPermissions.Keys.Count == 0)
                    {
                        contentEntities = new IUmbracoEntity[0];
                    }
                    else
                    {
                        // a group can end up with way more than 2000 assigned permissions,
                        // so we need to break them into groups in order to avoid breaking
                        // the entity service due to too many Sql parameters.

                        var list = new List<IUmbracoEntity>();
                        contentEntities = list;
                        var entityService = applicationContext.Services.EntityService;
                        foreach (var idGroup in allContentPermissions.Keys.InGroupsOf(2000))
                            list.AddRange(entityService.GetAll(UmbracoObjectTypes.Document, idGroup.ToArray()));
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
            
            config.CreateMap<IUser, UserDisplay>()
                .ForMember(detail => detail.Avatars, opt => opt.MapFrom(user => user.GetUserAvatarUrls(applicationContext.ApplicationCache.RuntimeCache)))
                .ForMember(detail => detail.Username, opt => opt.MapFrom(user => user.Username))
                .ForMember(detail => detail.LastLoginDate, opt => opt.MapFrom(user => user.LastLoginDate == default(DateTime) ? null : (DateTime?) user.LastLoginDate))
                .ForMember(detail => detail.UserGroups, opt => opt.MapFrom(user => user.Groups))
                .ForMember(detail => detail.Navigation, opt => opt.MapFrom(user => CreateUserEditorNavigation(applicationContext.Services.TextService)))
                .ForMember(
                    detail => detail.CalculatedStartContentIds,
                    opt => opt.MapFrom(user => GetStartNodeValues(
                        user.CalculateContentStartNodeIds(applicationContext.Services.EntityService),
                        applicationContext.Services.TextService,
                        applicationContext.Services.EntityService,
                        UmbracoObjectTypes.Document,
                        "content/contentRoot")))
                .ForMember(
                    detail => detail.CalculatedStartMediaIds,
                    opt => opt.MapFrom(user => GetStartNodeValues(
                        user.CalculateMediaStartNodeIds(applicationContext.Services.EntityService),
                        applicationContext.Services.TextService,
                        applicationContext.Services.EntityService,
                        UmbracoObjectTypes.Media,
                        "media/mediaRoot")))
                .ForMember(
                    detail => detail.StartContentIds,
                    opt => opt.MapFrom(user => GetStartNodeValues(
                        user.StartContentIds.ToArray(),
                        applicationContext.Services.TextService,
                        applicationContext.Services.EntityService,
                        UmbracoObjectTypes.Document,
                        "content/contentRoot")))
                .ForMember(
                    detail => detail.StartMediaIds,
                    opt => opt.MapFrom(user => GetStartNodeValues(
                        user.StartMediaIds.ToArray(),
                        applicationContext.Services.TextService,
                        applicationContext.Services.EntityService,
                        UmbracoObjectTypes.Media,
                        "media/mediaRoot")))
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))                
                .ForMember(
                    detail => detail.AvailableCultures,
                    opt => opt.MapFrom(user => applicationContext.Services.TextService.GetSupportedCultures().ToDictionary(x => x.Name, x => x.DisplayName)))
                .ForMember(
                    detail => detail.EmailHash,
                    opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().GenerateHash()))
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(user => "-1," + user.Id))
                .ForMember(detail => detail.Notifications, opt => opt.Ignore())
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Icon, opt => opt.Ignore())
                .ForMember(detail => detail.IsCurrentUser, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.ResetPasswordValue, opt => opt.Ignore())
                .ForMember(detail => detail.Alias, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore());

            config.CreateMap<IUser, UserBasic>()
                //Loading in the user avatar's requires an external request if they don't have a local file avatar, this means that initial load of paging may incur a cost
                //Alternatively, if this is annoying the back office UI would need to be updated to request the avatars for the list of users separately so it doesn't look
                //like the load time is waiting.
                .ForMember(detail => 
                    detail.Avatars, 
                    opt => opt.MapFrom(user => user.GetUserAvatarUrls(applicationContext.ApplicationCache.RuntimeCache)))
                .ForMember(detail => detail.Username, opt => opt.MapFrom(user => user.Username))
                .ForMember(detail => detail.UserGroups, opt => opt.MapFrom(user => user.Groups))
                .ForMember(detail => detail.LastLoginDate, opt => opt.MapFrom(user => user.LastLoginDate == default(DateTime) ? null : (DateTime?) user.LastLoginDate))
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))
                .ForMember(
                    detail => detail.EmailHash,
                    opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().ToMd5()))
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(user => "-1," + user.Id))
                .ForMember(detail => detail.Notifications, opt => opt.Ignore())
                .ForMember(detail => detail.IsCurrentUser, opt => opt.Ignore())
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Icon, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.Alias, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore());

            config.CreateMap<IUser, UserDetail>()
                .ForMember(detail => detail.Avatars, opt => opt.MapFrom(user => user.GetUserAvatarUrls(applicationContext.ApplicationCache.RuntimeCache)))
                .ForMember(detail => detail.UserId, opt => opt.MapFrom(user => GetIntId(user.Id)))
                .ForMember(detail => detail.StartContentIds, opt => opt.MapFrom(user => user.CalculateContentStartNodeIds(applicationContext.Services.EntityService)))
                .ForMember(detail => detail.StartMediaIds, opt => opt.MapFrom(user => user.CalculateMediaStartNodeIds(applicationContext.Services.EntityService)))
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))
                .ForMember(
                    detail => detail.EmailHash,
                    opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().GenerateHash()))
                .ForMember(detail => detail.SecondsUntilTimeout, opt => opt.Ignore())
                .ForMember(detail => detail.UserGroups, opt => opt.Ignore())
                .AfterMap((user, detail) =>
                {
                    //we need to map the legacy UserType
                    //the best we can do here is to return the user's first user group as a IUserType object
                    //but we should attempt to return any group that is the built in ones first
                    var groups = user.Groups.ToArray();
                    detail.UserGroups = user.Groups.Select(x => x.Alias).ToArray();

                    if (groups.Length == 0)
                    {
                        //In backwards compatibility land, a user type cannot be null! so we need to return a fake one. 
                        detail.UserType = "temp";
                    }
                    else
                    {
                        var builtIns = new[] { Constants.Security.AdminGroupAlias, "writer", "editor", Constants.Security.TranslatorGroupAlias };
                        var foundBuiltIn = groups.FirstOrDefault(x => builtIns.Contains(x.Alias));
                        if (foundBuiltIn != null)
                        {
                            detail.UserType = foundBuiltIn.Alias;
                        }
                        else
                        {
                            //otherwise return the first
                            detail.UserType = groups[0].Alias;
                        }
                    }
                    
                });

            config.CreateMap<IProfile, UserProfile>()
                  .ForMember(detail => detail.UserId, opt => opt.MapFrom(profile => GetIntId(profile.Id)));
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
            if (startNodeIds.Length > 0)
            {
                var startNodes = new List<EntityBasic>();
                if (startNodeIds.Contains(-1))
                {
                    startNodes.Add(RootNode(textService.Localize(localizedKey)));
                }
                var mediaItems = entityService.GetAll(objectType, startNodeIds);
                startNodes.AddRange(Mapper.Map<IEnumerable<IUmbracoEntity>, IEnumerable<EntityBasic>>(mediaItems));
                return startNodes;
            }
            return Enumerable.Empty<EntityBasic>();
        }

        private void MapUserGroupBasic(ServiceContext services, dynamic group, UserGroupBasic display)
        {
            var allSections = services.SectionService.GetSections();
            display.Sections = allSections.Where(x => Enumerable.Contains(group.AllowedSections, x.Alias)).Select(Mapper.Map<ContentEditing.Section>);

            if (group.StartMediaId > 0)
            {
                display.MediaStartNode = Mapper.Map<EntityBasic>(
                    services.EntityService.Get(group.StartMediaId, UmbracoObjectTypes.Media));
            }
            else if (group.StartMediaId == -1)
            {
                //create the root node
                display.MediaStartNode = RootNode(services.TextService.Localize("media/mediaRoot"));
            }

            if (group.StartContentId > 0)
            {
                display.ContentStartNode = Mapper.Map<EntityBasic>(
                    services.EntityService.Get(group.StartContentId, UmbracoObjectTypes.Document));
            }
            else if (group.StartContentId == -1)
            {
                //create the root node
                display.ContentStartNode = RootNode(services.TextService.Localize("content/contentRoot"));
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
