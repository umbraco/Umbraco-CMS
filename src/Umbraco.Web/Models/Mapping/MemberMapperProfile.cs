using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using System.Linq;
using Umbraco.Core.Security;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.Composing;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for members.
    /// </summary>
    internal class MemberMapperProfile : Profile
    {
        public MemberMapperProfile(IUserService userService, ILocalizedTextService textService, IMemberTypeService memberTypeService, IMemberService memberService)
        {
            // create, capture, cache
            var memberOwnerResolver = new OwnerResolver<IMember>(userService);
            var tabsAndPropertiesResolver = new MemberTabsAndPropertiesResolver(textService);
            var memberProfiderFieldMappingResolver = new MemberProviderFieldResolver();
            var membershipScenarioMappingResolver = new MembershipScenarioResolver(new Lazy<IMemberTypeService>(() => memberTypeService));
            var memberDtoPropertiesResolver = new MemberDtoPropertiesResolver();

            //FROM MembershipUser TO MediaItemDisplay - used when using a non-umbraco membership provider
            CreateMap<MembershipUser, MemberDisplay>()
                .ConvertUsing(user =>
                {
                    var member = Mapper.Map<MembershipUser, IMember>(user);
                    return Mapper.Map<IMember, MemberDisplay>(member);
                });

            //FROM MembershipUser TO IMember - used when using a non-umbraco membership provider
            CreateMap<MembershipUser, IMember>()
                .ConstructUsing(src => MemberService.CreateGenericMembershipProviderMember(src.UserName, src.Email, src.UserName, ""))
                //we're giving this entity an ID of 0 - we cannot really map it but it needs an id so the system knows it's not a new entity
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
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.FailedPasswordAttempts, opt => opt.Ignore())
                .ForMember(dest => dest.DeleteDate, opt => opt.Ignore())
                .ForMember(dest => dest.WriterId, opt => opt.Ignore())
                //TODO: Support these eventually
                .ForMember(dest => dest.PasswordQuestion, opt => opt.Ignore())
                .ForMember(dest => dest.RawPasswordAnswerValue, opt => opt.Ignore());

            //FROM IMember TO MediaItemDisplay
            CreateMap<IMember, MemberDisplay>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(content => Udi.Create(Constants.UdiEntityType.Member, content.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => memberOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.ContentTypeName, opt => opt.MapFrom(src => src.ContentType.Name))
                .ForMember(dest => dest.Properties, opt => opt.Ignore())
                .ForMember(dest => dest.Tabs, opt => opt.ResolveUsing(src => tabsAndPropertiesResolver.Resolve(src)))
                .ForMember(dest => dest.MemberProviderFieldMapping, opt => opt.ResolveUsing(src => memberProfiderFieldMappingResolver.Resolve(src)))
                .ForMember(dest => dest.MembershipScenario, opt => opt.ResolveUsing(src => membershipScenarioMappingResolver.Resolve(src)))
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Errors, opt => opt.Ignore())
                .ForMember(dest => dest.Published, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.IsChildOfListView, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.IsContainer, opt => opt.Ignore())
                .ForMember(dest => dest.TreeNodeUrl, opt => opt.Ignore())
                .AfterMap((src, dest) => MapGenericCustomProperties(memberService, userService, src, dest, textService));

            //FROM IMember TO MemberBasic
            CreateMap<IMember, MemberBasic>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(content => Udi.Create(Constants.UdiEntityType.Member, content.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => memberOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentType.Icon))
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.MapFrom(src => src.ContentType.Alias))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.Published, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore());

            //FROM MembershipUser TO MemberBasic
            CreateMap<MembershipUser, MemberBasic>()
                //we're giving this entity an ID of 0 - we cannot really map it but it needs an id so the system knows it's not a new entity
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => int.MaxValue))
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreationDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.LastActivityDate))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.ProviderUserKey.TryConvertTo<Guid>().Result.ToString("N")))
                .ForMember(dest => dest.Owner, opt => opt.UseValue(new ContentEditing.UserProfile {Name = "Admin", UserId = 0}))
                .ForMember(dest => dest.Icon, opt => opt.UseValue("icon-user"))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Properties, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.Published, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.ContentTypeAlias, opt => opt.Ignore());

            //FROM IMember TO ContentItemDto<IMember>
            CreateMap<IMember, ContentItemDto<IMember>>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(content => Udi.Create(Constants.UdiEntityType.Member, content.Key)))
                .ForMember(dest => dest.Owner, opt => opt.ResolveUsing(src => memberOwnerResolver.Resolve(src)))
                .ForMember(dest => dest.Published, opt => opt.Ignore())
                .ForMember(dest => dest.Updater, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
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
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore());
        }

        /// <summary>
        /// Maps the generic tab with custom properties for content
        /// </summary>
        /// <param name="memberService"></param>
        /// <param name="userService"></param>
        /// <param name="member"></param>
        /// <param name="display"></param>
        /// <param name="localizedText"></param>
        /// <remarks>
        /// If this is a new entity and there is an approved field then we'll set it to true by default.
        /// </remarks>
        private static void MapGenericCustomProperties(IMemberService memberService, IUserService userService, IMember member, MemberDisplay display, ILocalizedTextService localizedText)
        {
            var membersProvider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

            //map the tree node url
            if (HttpContext.Current != null)
            {
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                var url = urlHelper.GetUmbracoApiService<MemberTreeController>(controller => controller.GetTreeNode(display.Key.ToString("N"), null));
                display.TreeNodeUrl = url;
            }

            var genericProperties = new List<ContentPropertyDisplay>
            {
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}doctype", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedText.Localize("content/membertype"),
                    Value = localizedText.UmbracoDictionaryTranslate(display.ContentTypeName),
                    View = Current.PropertyEditors[Constants.PropertyEditors.Aliases.NoEdit].GetValueEditor().View
                },
                GetLoginProperty(memberService, member, display, localizedText),
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}email", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedText.Localize("general/email"),
                    Value = display.Email,
                    View = "email",
                    Validation = {Mandatory = true}
                },
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}password", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedText.Localize("password"),
                    //NOTE: The value here is a json value - but the only property we care about is the generatedPassword one if it exists, the newPassword exists
                    // only when creating a new member and we want to have a generated password pre-filled.
                    Value = new Dictionary<string, object>
                    {
                        // fixme why ignoreCase, what are we doing here?!
                        {"generatedPassword", member.GetAdditionalDataValueIgnoreCase("GeneratedPassword", null)},
                        {"newPassword", member.GetAdditionalDataValueIgnoreCase("NewPassword", null)},
                    },
                    //TODO: Hard coding this because the changepassword doesn't necessarily need to be a resolvable (real) property editor
                    View = "changepassword",
                    //initialize the dictionary with the configuration from the default membership provider
                    Config = new Dictionary<string, object>(membersProvider.GetConfiguration(userService))
                    {
                        //the password change toggle will only be displayed if there is already a password assigned.
                        {"hasPassword", member.RawPasswordValue.IsNullOrWhiteSpace() == false}
                    }
                },
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}membergroup", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedText.Localize("content/membergroup"),
                    Value = GetMemberGroupValue(display.Username),
                    View = "membergroups",
                    Config = new Dictionary<string, object> {{"IsRequired", true}}
                }
            };

            TabsAndPropertiesResolver.MapGenericProperties(member, display, localizedText, genericProperties, properties =>
            {
                if (HttpContext.Current != null && UmbracoContext.Current != null && UmbracoContext.Current.Security.CurrentUser != null
                    && UmbracoContext.Current.Security.CurrentUser.AllowedSections.Any(x => x.Equals(Constants.Applications.Settings)))
                {
                    var memberTypeLink = string.Format("#/member/memberTypes/edit/{0}", member.ContentTypeId);

                    //Replace the doctype property
                    var docTypeProperty = properties.First(x => x.Alias == string.Format("{0}doctype", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                    docTypeProperty.Value = new List<object>
                    {
                        new
                        {
                            linkText = member.ContentType.Name,
                            url = memberTypeLink,
                            target = "_self",
                            icon = "icon-item-arrangement"
                        }
                    };
                    docTypeProperty.View = "urllist";
                }
            });

            //check if there's an approval field
            var provider = membersProvider as IUmbracoMemberTypeMembershipProvider;
            if (member.HasIdentity == false && provider != null)
            {
                var approvedField = provider.ApprovedPropertyTypeAlias;
                var prop = display.Properties.FirstOrDefault(x => x.Alias == approvedField);
                if (prop != null)
                {
                    prop.Value = 1;
                }
            }
        }

        /// <summary>
        /// Returns the login property display field
        /// </summary>
        /// <param name="memberService"></param>
        /// <param name="member"></param>
        /// <param name="display"></param>
        /// <param name="localizedText"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the membership provider installed is the umbraco membership provider, then we will allow changing the username, however if
        /// the membership provider is a custom one, we cannot allow chaning the username because MembershipProvider's do not actually natively
        /// allow that.
        /// </remarks>
        internal static ContentPropertyDisplay GetLoginProperty(IMemberService memberService, IMember member, MemberDisplay display, ILocalizedTextService localizedText)
        {
            var prop = new ContentPropertyDisplay
            {
                Alias = string.Format("{0}login", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                Label = localizedText.Localize("login"),
                Value = display.Username
            };

            var scenario = memberService.GetMembershipScenario();

            //only allow editing if this is a new member, or if the membership provider is the umbraco one
            if (member.HasIdentity == false || scenario == MembershipScenario.NativeUmbraco)
            {
                prop.View = "textbox";
                prop.Validation.Mandatory = true;
            }
            else
            {
                prop.View = "readonlyvalue";
            }
            return prop;
        }

        internal static IDictionary<string, bool> GetMemberGroupValue(string username)
        {
            var userRoles = username.IsNullOrWhiteSpace() ? null : Roles.GetRolesForUser(username);

            // create a dictionary of all roles (except internal roles) + "false"
            var result = Roles.GetAllRoles().Distinct()
                // if a role starts with __umbracoRole we won't show it as it's an internal role used for public access
                .Where(x => x.StartsWith(Constants.Conventions.Member.InternalRolePrefix) == false)
                .ToDictionary(x => x, x => false);

            // if user has no roles, just return the dictionary
            if (userRoles == null) return result;

            // else update the dictionary to "true" for the user roles (except internal roles)
            foreach (var userRole in userRoles.Where(x => x.StartsWith(Constants.Conventions.Member.InternalRolePrefix) == false))
                result[userRole] = true;

            return result;
        }
    }
}
