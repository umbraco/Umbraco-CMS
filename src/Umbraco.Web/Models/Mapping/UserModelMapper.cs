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

namespace Umbraco.Web.Models.Mapping
{
    internal class UserModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            //Used for merging existing UserSave to an existing IUser instance - this will not create an IUser instance!
            config.CreateMap<UserSave, IUser>()
                .ForMember(user => user.Language, expression => expression.MapFrom(save => save.Culture))                
                .ForMember(user => user.Avatar, expression => expression.Ignore())
                .ForMember(user => user.SessionTimeout, expression => expression.Ignore())
                .ForMember(user => user.SecurityStamp, expression => expression.Ignore())
                .ForMember(user => user.ProviderUserKey, expression => expression.Ignore())
                .ForMember(user => user.RawPasswordValue, expression => expression.Ignore())
                .ForMember(user => user.PasswordQuestion, expression => expression.Ignore())
                .ForMember(user => user.RawPasswordAnswerValue, expression => expression.Ignore())
                .ForMember(user => user.Comments, expression => expression.Ignore())
                .ForMember(user => user.IsApproved, expression => expression.Ignore())
                .ForMember(user => user.IsLockedOut, expression => expression.Ignore())
                .ForMember(user => user.LastLoginDate, expression => expression.Ignore())
                .ForMember(user => user.LastPasswordChangeDate, expression => expression.Ignore())
                .ForMember(user => user.LastLockoutDate, expression => expression.Ignore())
                .ForMember(user => user.FailedPasswordAttempts, expression => expression.Ignore())
                .ForMember(user => user.DeletedDate, expression => expression.Ignore())
                .ForMember(user => user.CreateDate, expression => expression.Ignore())
                .ForMember(user => user.UpdateDate, expression => expression.Ignore())
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
                .ConstructUsing(invite => new User(invite.Name, invite.Email, invite.Email, Guid.NewGuid().ToString("N")))
                //generate a token for the invite
                .ForMember(user => user.SecurityStamp, expression => expression.MapFrom(x => (DateTime.Now + x.Email).ToSHA1()))
                //all invited users will not be approved, completing the invite will approve the user
                .ForMember(user => user.IsApproved, expression => expression.UseValue(false))
                .ForMember(user => user.Id, expression => expression.Ignore())
                .ForMember(user => user.Avatar, expression => expression.Ignore())
                .ForMember(user => user.SessionTimeout, expression => expression.Ignore())
                .ForMember(user => user.StartContentIds, expression => expression.Ignore())
                .ForMember(user => user.StartMediaIds, expression => expression.Ignore())
                .ForMember(user => user.Language, expression => expression.Ignore())                
                .ForMember(user => user.ProviderUserKey, expression => expression.Ignore())
                .ForMember(user => user.Username, expression => expression.Ignore())
                .ForMember(user => user.RawPasswordValue, expression => expression.Ignore())
                .ForMember(user => user.PasswordQuestion, expression => expression.Ignore())
                .ForMember(user => user.RawPasswordAnswerValue, expression => expression.Ignore())
                .ForMember(user => user.Comments, expression => expression.Ignore())
                .ForMember(user => user.IsApproved, expression => expression.Ignore())
                .ForMember(user => user.IsLockedOut, expression => expression.Ignore())
                .ForMember(user => user.LastLoginDate, expression => expression.Ignore())
                .ForMember(user => user.LastPasswordChangeDate, expression => expression.Ignore())
                .ForMember(user => user.LastLockoutDate, expression => expression.Ignore())
                .ForMember(user => user.FailedPasswordAttempts, expression => expression.Ignore())
                .ForMember(user => user.DeletedDate, expression => expression.Ignore())
                .ForMember(user => user.CreateDate, expression => expression.Ignore())
                .ForMember(user => user.UpdateDate, expression => expression.Ignore())
                .AfterMap((invite, user) =>
                {
                    user.ClearGroups();
                    var foundGroups = applicationContext.Services.UserService.GetUserGroupsByAlias(invite.UserGroups.ToArray());
                    foreach (var group in foundGroups)
                    {
                        user.AddGroup(group.ToReadOnlyGroup());
                    }
                });

            config.CreateMap<IReadOnlyUserGroup, UserGroupDisplay>()
                .ForMember(detail => detail.StartContentId, opt => opt.Ignore())
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
                    var allSections = applicationContext.Services.SectionService.GetSections();
                    display.Sections = allSections.Where(x => group.Alias == x.Alias).Select(Mapper.Map<ContentEditing.Section>);

                    //applicationContext.Services.EntityService.Get<IContent>(x.StartContentId, false))
                    //applicationContext.Services.EntityService.Get<IMedia>(x.StartMediaId, false))

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
                .AfterMap((group, display) =>
                {
                    var allSections = applicationContext.Services.SectionService.GetSections();
                    display.Sections = allSections.Where(x => group.Alias == x.Alias).Select(Mapper.Map<ContentEditing.Section>);

                    //applicationContext.Services.EntityService.Get<IContent>(x.StartContentId, false))
                    //applicationContext.Services.EntityService.Get<IMedia>(x.StartMediaId, false))

                });

            config.CreateMap<IUser, UserDisplay>()
                .ForMember(detail => detail.Avatars, opt => opt.MapFrom(user => user.GetCurrentUserAvatarUrls(applicationContext.Services.UserService, applicationContext.ApplicationCache.RuntimeCache)))
                .ForMember(detail => detail.Username, opt => opt.MapFrom(user => user.Username))
                .ForMember(detail => detail.LastLoginDate, opt => opt.MapFrom(user => user.LastLoginDate == default(DateTime) ? null : (DateTime?) user.LastLoginDate))
                .ForMember(detail => detail.UserGroups, opt => opt.Ignore())
                .ForMember(detail => detail.StartContentIds, opt => opt.Ignore())
                .ForMember(detail => detail.StartMediaIds, opt => opt.Ignore())
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))                
                .ForMember(
                    detail => detail.AvailableCultures,
                    opt => opt.MapFrom(user => applicationContext.Services.TextService.GetSupportedCultures().ToDictionary(x => x.Name, x => x.DisplayName)))
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
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore())
                .AfterMap((user, display) =>
                {
                    var contentItems = applicationContext.Services.EntityService.GetAll(UmbracoObjectTypes.Document, user.StartContentIds.ToArray());
                    var mediaItems = applicationContext.Services.EntityService.GetAll(UmbracoObjectTypes.Document, user.StartContentIds.ToArray());
                    display.StartContentIds = Mapper.Map<IEnumerable<IUmbracoEntity>, IEnumerable<EntityBasic>>(contentItems);
                    display.StartMediaIds = Mapper.Map<IEnumerable<IUmbracoEntity>, IEnumerable<EntityBasic>>(mediaItems);

                    display.UserGroups = Mapper.Map<IEnumerable<IReadOnlyUserGroup>, IEnumerable<UserGroupDisplay>>(user.Groups);
                    
                });

            config.CreateMap<IUser, UserDetail>()
                .ForMember(detail => detail.Avatars, opt => opt.MapFrom(user => user.GetCurrentUserAvatarUrls(applicationContext.Services.UserService, applicationContext.ApplicationCache.RuntimeCache)))
                .ForMember(detail => detail.UserId, opt => opt.MapFrom(user => GetIntId(user.Id)))
                .ForMember(detail => detail.StartContentIds, opt => opt.MapFrom(user => user.StartContentIds))
                .ForMember(detail => detail.StartMediaIds, opt => opt.MapFrom(user => user.StartMediaIds))
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))
                .ForMember(
                    detail => detail.EmailHash,
                    opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().ToMd5()))
                .ForMember(detail => detail.SecondsUntilTimeout, opt => opt.Ignore());            

            config.CreateMap<IProfile, UserBasic>()
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