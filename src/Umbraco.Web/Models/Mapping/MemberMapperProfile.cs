using System;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for members.
    /// </summary>
    internal class MemberMapperProfile : Profile
    {
        public MemberMapperProfile(
            MemberTabsAndPropertiesResolver tabsAndPropertiesResolver,
            MemberTreeNodeUrlResolver memberTreeNodeUrlResolver,
            MemberBasicPropertiesResolver memberBasicPropertiesResolver,
            IUserService userService,
            IMemberTypeService memberTypeService,
            IMemberService memberService)
        {
            // create, capture, cache
            var memberOwnerResolver = new OwnerResolver<IMember>(userService);
            var memberProfiderFieldMappingResolver = new MemberProviderFieldResolver();
            var membershipScenarioMappingResolver = new MembershipScenarioResolver(memberTypeService);
            var memberDtoPropertiesResolver = new MemberDtoPropertiesResolver();

            //FROM MembershipUser TO MediaItemDisplay - used when using a non-umbraco membership provider
            CreateMap<MembershipUser, MemberDisplay>().ConvertUsing<MembershipUserTypeConverter>();

            //FROM MembershipUser TO IMember - used when using a non-umbraco membership provider
            CreateMap<MembershipUser, IMember>()
                .ConstructUsing(src => MemberService.CreateGenericMembershipProviderMember(src.UserName, src.Email, src.UserName, ""))
                //we're giving this entity an ID of int.MaxValue - TODO: SD: I can't remember why this mapping is here?
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => int.MaxValue))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comment))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreationDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.LastActivityDate))
                .ForMember(dest => dest.LastPasswordChangeDate, opt => opt.MapFrom(src => src.LastPasswordChangedDate))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.ProviderUserKey.TryConvertTo<Guid>().Result.ToString("N")))
                //This is a special case for password - we don't actually care what the password is but it either needs to be something or nothing
                // so we'll set it to something if the member is actually created, otherwise nothing if it is a new member.
                .ForMember(dest => dest.RawPasswordValue, opt => opt.MapFrom(src => src.CreationDate > DateTime.MinValue ? Guid.NewGuid().ToString("N") : ""))
                .ForMember(dest => dest.Properties, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.Level, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.CultureInfos, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.FailedPasswordAttempts, opt => opt.Ignore())
                .ForMember(dest => dest.DeleteDate, opt => opt.Ignore())
                .ForMember(dest => dest.WriterId, opt => opt.Ignore())
                // TODO: Support these eventually
                .ForMember(dest => dest.PasswordQuestion, opt => opt.Ignore())
                .ForMember(dest => dest.RawPasswordAnswerValue, opt => opt.Ignore());

            //FROM IMember TO MemberDisplay
            CreateMap<IMember, MemberDisplay>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(content => Udi.Create(Constants.UdiEntityType.Member, content.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => memberOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.ContentTypeName, opt => opt.MapFrom(src => src.ContentType.Name))
                .ForMember(dest => dest.Properties, opt => opt.Ignore())
                .ForMember(dest => dest.Tabs, opt => opt.ResolveUsing(tabsAndPropertiesResolver))
                .ForMember(dest => dest.MemberProviderFieldMapping, opt => opt.ResolveUsing(src => memberProfiderFieldMappingResolver.Resolve(src)))
                .ForMember(dest => dest.MembershipScenario, opt => opt.ResolveUsing(src => membershipScenarioMappingResolver.Resolve(src)))
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Errors, opt => opt.Ignore())
                .ForMember(dest => dest.State, opt => opt.UseValue<ContentSavedState?>(null))
                .ForMember(dest => dest.Edited, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.IsChildOfListView, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.IsContainer, opt => opt.Ignore())
                .ForMember(dest => dest.TreeNodeUrl, opt => opt.ResolveUsing(memberTreeNodeUrlResolver))
                .ForMember(dest => dest.VariesByCulture, opt => opt.Ignore());

            //FROM IMember TO MemberBasic
            CreateMap<IMember, MemberBasic>()
                //we're giving this entity an ID of int.MaxValue - this is kind of a hack to force angular to use the Key instead of the Id in list views
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => int.MaxValue))
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(content => Udi.Create(Constants.UdiEntityType.Member, content.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => memberOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.State, opt => opt.UseValue<ContentSavedState?>(null))
                .ForMember(dest => dest.Edited, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dto => dto.Properties, expression => expression.ResolveUsing(memberBasicPropertiesResolver))
                .ForMember(dest => dest.VariesByCulture, opt => opt.Ignore());

            //FROM MembershipUser TO MemberBasic
            CreateMap<MembershipUser, MemberBasic>()
                //we're giving this entity an ID of int.MaxValue - TODO: SD: I can't remember why this mapping is here?
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => int.MaxValue))
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreationDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.LastActivityDate))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.ProviderUserKey.TryConvertTo<Guid>().Result.ToString("N")))
                .ForMember(dest => dest.Owner, opt => opt.UseValue(new UserProfile {Name = "Admin", UserId = -1 }))
                .ForMember(dest => dest.Icon, opt => opt.UseValue("icon-user"))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Properties, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.State, opt => opt.UseValue(ContentSavedState.Draft))
                .ForMember(dest => dest.Edited, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.Ignore())
                .ForMember(dest => dest.VariesByCulture, opt => opt.Ignore());

            //FROM IMember TO ContentItemDto<IMember>
            CreateMap<IMember, ContentPropertyCollectionDto>()
                //.ForMember(dest => dest.Udi, opt => opt.MapFrom(content => Udi.Create(Constants.UdiEntityType.Member, content.Key)))
                //.ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => memberOwnerResolver.Resolve(src)))
                //.ForMember(dest => dest.Published, opt => opt.Ignore())
                //.ForMember(dest => dest.Edited, opt => opt.Ignore())
                //.ForMember(dest => dest.Updater, opt => opt.Ignore())
                //.ForMember(dest => dest.Icon, opt => opt.Ignore())
                //.ForMember(dest => dest.Alias, opt => opt.Ignore())
                //do no map the custom member properties (currently anyways, they were never there in 6.x)
                .ForMember(dest => dest.Properties, opt => opt.ResolveUsing(src => memberDtoPropertiesResolver.Resolve(src)));

            //FROM IMemberGroup TO MemberGroupDisplay
            CreateMap<IMemberGroup, MemberGroupDisplay>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(Constants.UdiEntityType.MemberGroup, src.Key)))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(group => "-1," + group.Id))
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore());
        }
    }
}
