using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using umbraco;
using System.Linq;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Security;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for members.
    /// </summary>
    internal class MemberModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            //FROM MembershipUser TO MediaItemDisplay - used when using a non-umbraco membership provider
            config.CreateMap<MembershipUser, MemberDisplay>()
                .ConvertUsing(user =>
                    {
                        var member = Mapper.Map<MembershipUser, IMember>(user);
                        return Mapper.Map<IMember, MemberDisplay>(member);
                    });

            //FROM MembershipUser TO IMember - used when using a non-umbraco membership provider
            config.CreateMap<MembershipUser, IMember>()
                .ConstructUsing(user => MemberService.CreateGenericMembershipProviderMember(user.UserName, user.Email, user.UserName, ""))
                //we're giving this entity an ID - we cannot really map it but it needs an id so the system knows it's not a new entity
                .ForMember(member => member.Id, expression => expression.MapFrom(user => int.MaxValue))
                .ForMember(member => member.Comments, expression => expression.MapFrom(user => user.Comment))
                .ForMember(member => member.CreateDate, expression => expression.MapFrom(user => user.CreationDate))
                .ForMember(member => member.UpdateDate, expression => expression.MapFrom(user => user.LastActivityDate))
                .ForMember(member => member.LastPasswordChangeDate, expression => expression.MapFrom(user => user.LastPasswordChangedDate))
                .ForMember(member => member.Key, expression => expression.MapFrom(user => user.ProviderUserKey.TryConvertTo<Guid>().Result.ToString("N")))
                //This is a special case for password - we don't actually care what the password is but it either needs to be something or nothing
                // so we'll set it to something if the member is actually created, otherwise nothing if it is a new member.
                .ForMember(member => member.RawPasswordValue, expression => expression.MapFrom(user => user.CreationDate > DateTime.MinValue ? Guid.NewGuid().ToString("N") : ""))
                .ForMember(member => member.Properties, expression => expression.Ignore())
                .ForMember(member => member.CreatorId, expression => expression.Ignore())
                .ForMember(member => member.Level, expression => expression.Ignore())
                .ForMember(member => member.Name, expression => expression.Ignore())
                .ForMember(member => member.ParentId, expression => expression.Ignore())
                .ForMember(member => member.Path, expression => expression.Ignore())
                .ForMember(member => member.SortOrder, expression => expression.Ignore())
                .ForMember(member => member.AdditionalData, expression => expression.Ignore())
                .ForMember(member => member.FailedPasswordAttempts, expression => expression.Ignore())
                //TODO: Support these eventually
                .ForMember(member => member.PasswordQuestion, expression => expression.Ignore())
                .ForMember(member => member.RawPasswordAnswerValue, expression => expression.Ignore());

            //FROM IMember TO MediaItemDisplay
            config.CreateMap<IMember, MemberDisplay>()
                .ForMember(
                    dto => dto.Owner,
                    expression => expression.ResolveUsing<OwnerResolver<IMember>>())
                .ForMember(
                    dto => dto.Icon,
                    expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(
                    dto => dto.ContentTypeAlias,
                    expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(
                    dto => dto.ContentTypeName,
                    expression => expression.MapFrom(content => content.ContentType.Name))
                .ForMember(display => display.Properties, expression => expression.Ignore())
                .ForMember(display => display.Tabs,
                    expression => expression.ResolveUsing(new MemberTabsAndPropertiesResolver(applicationContext.Services.TextService)))
                .ForMember(display => display.MemberProviderFieldMapping,
                    expression => expression.ResolveUsing<MemberProviderFieldMappingResolver>())
                .ForMember(display => display.MembershipScenario,
                    expression => expression.ResolveUsing(new MembershipScenarioMappingResolver(new Lazy<IMemberTypeService>(() => applicationContext.Services.MemberTypeService))))
                .ForMember(display => display.Notifications, expression => expression.Ignore())
                .ForMember(display => display.Errors, expression => expression.Ignore())
                .ForMember(display => display.Published, expression => expression.Ignore())
                .ForMember(display => display.Updater, expression => expression.Ignore())
                .ForMember(display => display.Alias, expression => expression.Ignore())
                .ForMember(display => display.IsChildOfListView, expression => expression.Ignore())
                .ForMember(display => display.Trashed, expression => expression.Ignore())
                .ForMember(display => display.IsContainer, expression => expression.Ignore())
                .ForMember(display => display.TreeNodeUrl, expression => expression.Ignore())
                .AfterMap((member, display) => MapGenericCustomProperties(applicationContext.Services.MemberService, member, display, applicationContext.Services.TextService));

            //FROM IMember TO MemberBasic
            config.CreateMap<IMember, MemberBasic>()
                .ForMember(
                    dto => dto.Owner,
                    expression => expression.ResolveUsing<OwnerResolver<IMember>>())
                .ForMember(
                    dto => dto.Icon,
                    expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(
                    dto => dto.ContentTypeAlias,
                    expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(
                    dto => dto.Email,
                    expression => expression.MapFrom(content => content.Email))
                .ForMember(
                    dto => dto.Username,
                    expression => expression.MapFrom(content => content.Username))
                .ForMember(display => display.Trashed, expression => expression.Ignore())
                .ForMember(x => x.Published, expression => expression.Ignore())
                .ForMember(x => x.Updater, expression => expression.Ignore())
                .ForMember(x => x.Alias, expression => expression.Ignore());

            //FROM MembershipUser TO MemberBasic
            config.CreateMap<MembershipUser, MemberBasic>()
                //we're giving this entity an ID - we cannot really map it but it needs an id so the system knows it's not a new entity
                .ForMember(member => member.Id, expression => expression.MapFrom(user => int.MaxValue))
                .ForMember(member => member.CreateDate, expression => expression.MapFrom(user => user.CreationDate))
                .ForMember(member => member.UpdateDate, expression => expression.MapFrom(user => user.LastActivityDate))
                .ForMember(member => member.Key, expression => expression.MapFrom(user => user.ProviderUserKey.TryConvertTo<Guid>().Result.ToString("N")))
                .ForMember(
                    dto => dto.Owner,
                    expression => expression.UseValue(new UserBasic {Name = "Admin", UserId = 0}))
                .ForMember(
                    dto => dto.Icon,
                    expression => expression.UseValue("icon-user"))
                .ForMember(member => member.Name, expression => expression.MapFrom(user => user.UserName))
                .ForMember(
                    dto => dto.Email,
                    expression => expression.MapFrom(content => content.Email))
                .ForMember(
                    dto => dto.Username,
                    expression => expression.MapFrom(content => content.UserName))
                .ForMember(member => member.Properties, expression => expression.Ignore())
                .ForMember(member => member.ParentId, expression => expression.Ignore())
                .ForMember(member => member.Path, expression => expression.Ignore())
                .ForMember(member => member.SortOrder, expression => expression.Ignore())
                .ForMember(member => member.AdditionalData, expression => expression.Ignore())
                .ForMember(x => x.Published, expression => expression.Ignore())
                .ForMember(x => x.Updater, expression => expression.Ignore())
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(x => x.Alias, expression => expression.Ignore())
                .ForMember(x => x.ContentTypeAlias, expression => expression.Ignore());

            //FROM IMember TO ContentItemDto<IMember>
            config.CreateMap<IMember, ContentItemDto<IMember>>()
                .ForMember(
                    dto => dto.Owner,
                    expression => expression.ResolveUsing<OwnerResolver<IMember>>())
                .ForMember(x => x.Published, expression => expression.Ignore())
                .ForMember(x => x.Updater, expression => expression.Ignore())
                .ForMember(x => x.Icon, expression => expression.Ignore())
                .ForMember(x => x.Alias, expression => expression.Ignore())
                //do no map the custom member properties (currently anyways, they were never there in 6.x)
                .ForMember(dto => dto.Properties, expression => expression.ResolveUsing<MemberDtoPropertiesValueResolver>());
        }

        /// <summary>
        /// Maps the generic tab with custom properties for content
        /// </summary>
        /// <param name="memberService"></param>
        /// <param name="member"></param>
        /// <param name="display"></param>
        /// <param name="localizedText"></param>
        /// <remarks>
        /// If this is a new entity and there is an approved field then we'll set it to true by default.
        /// </remarks>
        private static void MapGenericCustomProperties(IMemberService memberService, IMember member, MemberDisplay display, ILocalizedTextService localizedText)
        {
            var membersProvider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

            //map the tree node url
            if (HttpContext.Current != null)
            {
                var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));
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
                    View = PropertyEditorResolver.Current.GetByAlias(Constants.PropertyEditors.NoEditAlias).ValueEditor.View
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
                        {"generatedPassword", member.GetAdditionalDataValueIgnoreCase("GeneratedPassword", null)},
                        {"newPassword", member.GetAdditionalDataValueIgnoreCase("NewPassword", null)},
                    },
                    //TODO: Hard coding this because the changepassword doesn't necessarily need to be a resolvable (real) property editor
                    View = "changepassword",
                    //initialize the dictionary with the configuration from the default membership provider
                    Config = new Dictionary<string, object>(membersProvider.GetConfiguration())
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


            TabsAndPropertiesResolver.MapGenericProperties(member, display, localizedText, genericProperties);
            
            //check if there's an approval field
            var provider = membersProvider as global::umbraco.providers.members.UmbracoMembershipProvider;
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
            var result = new Dictionary<string, bool>();
            foreach (var role in Roles.GetAllRoles().Distinct())
            {
                // if a role starts with __umbracoRole we won't show it as it's an internal role used for public access
                if (role.StartsWith(Constants.Conventions.Member.InternalRolePrefix) == false)
                {
                    result.Add(role, false);
                    if (username.IsNullOrWhiteSpace()) continue;
                    if (Roles.IsUserInRole(username, role))
                    {
                        result[role] = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This ensures that the custom membership provider properties are not mapped - these property values are controller by the membership provider
        /// </summary>
        /// <remarks>
        /// Because these properties don't exist on the form, if we don't remove them for this map we'll get validation errors when posting data
        /// </remarks>
        internal class MemberDtoPropertiesValueResolver : ValueResolver<IMember, IEnumerable<ContentPropertyDto>>
        {
            protected override IEnumerable<ContentPropertyDto> ResolveCore(IMember source)
            {
                var defaultProps = Constants.Conventions.Member.GetStandardPropertyTypeStubs();

                //remove all membership properties, these values are set with the membership provider.
                var exclude = defaultProps.Select(x => x.Value.Alias).ToArray();
                
                return source.Properties
                             .Where(x => exclude.Contains(x.Alias) == false)
                             .Select(Mapper.Map<Property, ContentPropertyDto>);
            }
        }

        /// <summary>
        /// A custom tab/property resolver for members which will ensure that the built-in membership properties are or arent' displayed
        /// depending on if the member type has these properties
        /// </summary>
        /// <remarks>
        /// This also ensures that the IsLocked out property is readonly when the member is not locked out - this is because
        /// an admin cannot actually set isLockedOut = true, they can only unlock. 
        /// </remarks>
        internal class MemberTabsAndPropertiesResolver : TabsAndPropertiesResolver
        {
            private readonly ILocalizedTextService _localizedTextService;

            public MemberTabsAndPropertiesResolver(ILocalizedTextService localizedTextService)
                : base(localizedTextService)
            {
                _localizedTextService = localizedTextService;
            }

            public MemberTabsAndPropertiesResolver(ILocalizedTextService localizedTextService,
                IEnumerable<string> ignoreProperties) : base(localizedTextService, ignoreProperties)
            {
                _localizedTextService = localizedTextService;
            }

            protected override IEnumerable<Tab<ContentPropertyDisplay>> ResolveCore(IContentBase content)
            {
                var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

                IgnoreProperties = content.PropertyTypes
                    .Where(x => x.HasIdentity == false)
                    .Select(x => x.Alias)
                    .ToArray();

                var result = base.ResolveCore(content).ToArray();

                if (provider.IsUmbracoMembershipProvider() == false)
                {
                    //it's a generic provider so update the locked out property based on our known constant alias
                    var isLockedOutProperty = result.SelectMany(x => x.Properties).FirstOrDefault(x => x.Alias == Constants.Conventions.Member.IsLockedOut);
                    if (isLockedOutProperty != null && isLockedOutProperty.Value.ToString() != "1")
                    {
                        isLockedOutProperty.View = "readonlyvalue";
                        isLockedOutProperty.Value = _localizedTextService.Localize("general/no");
                    }

                    return result;    
                }
                else
                {
                    var umbracoProvider = (IUmbracoMemberTypeMembershipProvider)provider;

                    //This is kind of a hack because a developer is supposed to be allowed to set their property editor - would have been much easier
                    // if we just had all of the membeship provider fields on the member table :(
                    // TODO: But is there a way to map the IMember.IsLockedOut to the property ? i dunno.
                    var isLockedOutProperty = result.SelectMany(x => x.Properties).FirstOrDefault(x => x.Alias == umbracoProvider.LockPropertyTypeAlias);
                    if (isLockedOutProperty != null && isLockedOutProperty.Value.ToString() != "1")
                    {
                        isLockedOutProperty.View = "readonlyvalue";
                        isLockedOutProperty.Value = _localizedTextService.Localize("general/no");
                    }

                    return result;    
                }

                
            }
        }

        internal class MembershipScenarioMappingResolver : ValueResolver<IMember, MembershipScenario>
        {
            private readonly Lazy<IMemberTypeService> _memberTypeService;

            public MembershipScenarioMappingResolver(Lazy<IMemberTypeService> memberTypeService)
            {
                _memberTypeService = memberTypeService;
            }

            protected override MembershipScenario ResolveCore(IMember source)
            {
                var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

                if (provider.IsUmbracoMembershipProvider())
                {
                    return MembershipScenario.NativeUmbraco;
                }
                var memberType = _memberTypeService.Value.Get(Constants.Conventions.MemberTypes.DefaultAlias);
                return memberType != null
                           ? MembershipScenario.CustomProviderWithUmbracoLink
                           : MembershipScenario.StandaloneCustomProvider;
            }
        }

        /// <summary>
        /// A resolver to map the provider field aliases
        /// </summary>
        internal class MemberProviderFieldMappingResolver : ValueResolver<IMember, IDictionary<string, string>>
        {
            protected override IDictionary<string, string> ResolveCore(IMember source)
            {
                var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

                if (provider.IsUmbracoMembershipProvider() == false)
                {
                    return new Dictionary<string, string>
                    {
                        {Constants.Conventions.Member.IsLockedOut, Constants.Conventions.Member.IsLockedOut},
                        {Constants.Conventions.Member.IsApproved, Constants.Conventions.Member.IsApproved},
                        {Constants.Conventions.Member.Comments, Constants.Conventions.Member.Comments}
                    };    
                }
                else
                {
                    var umbracoProvider = (IUmbracoMemberTypeMembershipProvider)provider;

                    return new Dictionary<string, string>
                    {
                        {Constants.Conventions.Member.IsLockedOut, umbracoProvider.LockPropertyTypeAlias},
                        {Constants.Conventions.Member.IsApproved, umbracoProvider.ApprovedPropertyTypeAlias},
                        {Constants.Conventions.Member.Comments, umbracoProvider.CommentPropertyTypeAlias}
                    };    
                }

                
            }
        } 

    }
}