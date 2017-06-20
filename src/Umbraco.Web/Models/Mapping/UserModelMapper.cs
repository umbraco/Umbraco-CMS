using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using umbraco;
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
                .ConstructUsing((UserGroupSave save) => new UserGroup() { CreateDate = DateTime.Now })
                .IgnoreAllUnmapped()
                .ForMember(user => user.Alias, expression => expression.MapFrom(save => save.Alias))
                .ForMember(user => user.Name, expression => expression.MapFrom(save => save.Name))
                .ForMember(user => user.StartMediaId, expression => expression.MapFrom(save => save.StartMediaId))
                .ForMember(user => user.StartContentId, expression => expression.MapFrom(save => save.StartContentId))
                .AfterMap((save, userGroup) =>
                {
                    userGroup.ClearAllowedSections();
                    foreach (var section in save.Sections)
                    {
                        userGroup.AddAllowedSection(section);
                    }
                });

            //Used for merging existing UserSave to an existing IUser instance - this will not create an IUser instance!
            config.CreateMap<UserSave, IUser>()
                .IgnoreAllUnmapped()
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
                .IgnoreAllUnmapped()                
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
                .ForMember(detail => detail.StartContentId, opt => opt.Ignore())
                .ForMember(detail => detail.UserCount, opt => opt.Ignore())
                .ForMember(detail => detail.StartMediaId, opt => opt.Ignore())
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
                .ForMember(detail => detail.StartContentId, opt => opt.Ignore())
                .ForMember(detail => detail.StartMediaId, opt => opt.Ignore())
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

            config.CreateMap<IUserGroup, UserGroupDisplay>()
                .ForMember(detail => detail.StartContentId, opt => opt.Ignore())
                .ForMember(detail => detail.StartMediaId, opt => opt.Ignore())
                .ForMember(detail => detail.Sections, opt => opt.Ignore())
                .ForMember(detail => detail.Notifications, opt => opt.Ignore())
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore())
                .ForMember(detail => detail.Users, opt => opt.Ignore())
                .AfterMap((group, display) =>
                {
                    MapUserGroupBasic(applicationContext.Services, group, display);
                    var users = applicationContext.Services.UserService.GetAllInGroup(group.Id);
                    display.Users = Mapper.Map<IEnumerable<UserBasic>>(users);
                });

            config.CreateMap<IUser, UserDisplay>()
                .ForMember(detail => detail.Avatars, opt => opt.MapFrom(user => user.GetCurrentUserAvatarUrls(applicationContext.Services.UserService, applicationContext.ApplicationCache.RuntimeCache)))
                .ForMember(detail => detail.Username, opt => opt.MapFrom(user => user.Username))
                .ForMember(detail => detail.LastLoginDate, opt => opt.MapFrom(user => user.LastLoginDate == default(DateTime) ? null : (DateTime?) user.LastLoginDate))
                .ForMember(detail => detail.UserGroups, opt => opt.Ignore())
                .ForMember(detail => detail.StartContentIds, opt => opt.UseValue(Enumerable.Empty<EntityBasic>()))
                .ForMember(detail => detail.StartMediaIds, opt => opt.UseValue(Enumerable.Empty<EntityBasic>()))
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
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.Alias, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore())
                .AfterMap((user, display) =>
                {
                    var startContentIds = user.StartContentIds.ToArray();
                    if (startContentIds.Length > 0)
                    {
                        var contentItems = applicationContext.Services.EntityService.GetAll(UmbracoObjectTypes.Document, startContentIds);
                        display.StartContentIds = Mapper.Map<IEnumerable<IUmbracoEntity>, IEnumerable<EntityBasic>>(contentItems);
                    }
                    var startMediaIds = user.StartContentIds.ToArray();
                    if (startMediaIds.Length > 0)
                    {
                        var mediaItems = applicationContext.Services.EntityService.GetAll(UmbracoObjectTypes.Document, startMediaIds);
                        display.StartMediaIds = Mapper.Map<IEnumerable<IUmbracoEntity>, IEnumerable<EntityBasic>>(mediaItems);
                    }
                    display.UserGroups = Mapper.Map<IEnumerable<IReadOnlyUserGroup>, IEnumerable<UserGroupBasic>>(user.Groups);
                    
                });

            config.CreateMap<IUser, UserBasic>()
                .ForMember(detail => detail.Avatars, opt => opt.MapFrom(user => user.GetCurrentUserAvatarUrls(applicationContext.Services.UserService, applicationContext.ApplicationCache.RuntimeCache)))
                .ForMember(detail => detail.Username, opt => opt.MapFrom(user => user.Username))
                .ForMember(detail => detail.LastLoginDate, opt => opt.MapFrom(user => user.LastLoginDate == default(DateTime) ? null : (DateTime?) user.LastLoginDate))
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))
                .ForMember(
                    detail => detail.EmailHash,
                    opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().ToMd5()))
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(user => "-1," + user.Id))
                .ForMember(detail => detail.Notifications, opt => opt.Ignore())
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Icon, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.Alias, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore());

            config.CreateMap<IUser, UserDetail>()
                .ForMember(detail => detail.Avatars, opt => opt.MapFrom(user => user.GetCurrentUserAvatarUrls(applicationContext.Services.UserService, applicationContext.ApplicationCache.RuntimeCache)))
                .ForMember(detail => detail.UserId, opt => opt.MapFrom(user => GetIntId(user.Id)))
                .ForMember(detail => detail.StartContentIds, opt => opt.MapFrom(user => user.StartContentIds))
                .ForMember(detail => detail.StartMediaIds, opt => opt.MapFrom(user => user.StartMediaIds))
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))
                .ForMember(
                    detail => detail.EmailHash,
                    opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().GenerateHash()))
                .ForMember(detail => detail.SecondsUntilTimeout, opt => opt.Ignore());            

            config.CreateMap<IProfile, UserProfile>()
                  .ForMember(detail => detail.UserId, opt => opt.MapFrom(profile => GetIntId(profile.Id)));

            config.CreateMap<IUser, UserData>()
                .ConstructUsing((IUser user) => new UserData())
                .ForMember(detail => detail.Id, opt => opt.MapFrom(user => user.Id))
                .ForMember(detail => detail.AllowedApplications, opt => opt.MapFrom(user => user.AllowedSections))
                .ForMember(detail => detail.RealName, opt => opt.MapFrom(user => user.Name))
                .ForMember(detail => detail.Roles, opt => opt.MapFrom(user => user.Groups.ToArray()))
                .ForMember(detail => detail.StartContentNodes, opt => opt.MapFrom(user => user.StartContentIds))
                .ForMember(detail => detail.StartMediaNodes, opt => opt.MapFrom(user => user.StartMediaIds))
                .ForMember(detail => detail.Username, opt => opt.MapFrom(user => user.Username))
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))
                .ForMember(detail => detail.SessionId, opt => opt.MapFrom(user => user.SecurityStamp.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString("N") : user.SecurityStamp));
            
        }

        private void MapUserGroupBasic(ServiceContext services, dynamic group, UserGroupBasic display)
        {
            var allSections = services.SectionService.GetSections();
            display.Sections = allSections.Where(x => Enumerable.Contains(group.AllowedSections, x.Alias)).Select(Mapper.Map<ContentEditing.Section>);
            if (group.StartMediaId > 0)
            {
                display.StartMediaId = Mapper.Map<EntityBasic>(
                    services.EntityService.Get(group.StartMediaId, UmbracoObjectTypes.Media));
            }
            if (group.StartContentId > 0)
            {
                display.StartContentId = Mapper.Map<EntityBasic>(
                    services.EntityService.Get(group.StartContentId, UmbracoObjectTypes.Document));
            }
            if (display.Icon.IsNullOrWhiteSpace())
            {
                display.Icon = "icon-users";
            }
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