using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using System.Linq;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Security;
using UserProfile = Umbraco.Web.Models.ContentEditing.UserProfile;

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
            config.CreateMap<MembershipUser, MemberDisplay>().ConvertUsing<MembershipUserTypeConverter>();

            //FROM MembershipUser TO IMember - used when using a non-umbraco membership provider
            config.CreateMap<MembershipUser, IMember>()
                .ConstructUsing(user => MemberService.CreateGenericMembershipProviderMember(user.UserName, user.Email, user.UserName, ""))
                //we're giving this entity an ID of 0 - we cannot really map it but it needs an id so the system knows it's not a new entity
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
                .ForMember(member => member.DeletedDate, expression => expression.Ignore())
                //TODO: Support these eventually
                .ForMember(member => member.PasswordQuestion, expression => expression.Ignore())
                .ForMember(member => member.RawPasswordAnswerValue, expression => expression.Ignore());

            //FROM IMember TO MediaItemDisplay
            config.CreateMap<IMember, MemberDisplay>()
                .ForMember(display => display.Udi, expression => expression.MapFrom(content => Udi.Create(Constants.UdiEntityType.Member, content.Key)))
                .ForMember(display => display.Owner, expression => expression.ResolveUsing(new OwnerResolver<IMember>()))
                .ForMember(display => display.Icon, expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(display => display.ContentTypeAlias, expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(display => display.ContentTypeName, expression => expression.MapFrom(content => content.ContentType.Name))
                .ForMember(display => display.Properties, expression => expression.Ignore())
                .ForMember(display => display.Tabs, expression => expression.ResolveUsing(new MemberTabsAndPropertiesResolver(applicationContext.Services.TextService, applicationContext.Services.MemberService, applicationContext.Services.UserService)))
                .ForMember(display => display.MemberProviderFieldMapping, expression => expression.ResolveUsing(new MemberProviderFieldMappingResolver()))
                .ForMember(display => display.MembershipScenario,
                    expression => expression.ResolveUsing(new MembershipScenarioMappingResolver(applicationContext.Services.MemberTypeService)))
                .ForMember(display => display.Notifications, expression => expression.Ignore())
                .ForMember(display => display.Errors, expression => expression.Ignore())
                .ForMember(display => display.Published, expression => expression.Ignore())
                .ForMember(display => display.Updater, expression => expression.Ignore())
                .ForMember(display => display.Alias, expression => expression.Ignore())
                .ForMember(display => display.IsChildOfListView, expression => expression.Ignore())
                .ForMember(display => display.Trashed, expression => expression.Ignore())
                .ForMember(display => display.IsContainer, expression => expression.Ignore())
                .ForMember(display => display.TreeNodeUrl, opt => opt.ResolveUsing(new MemberTreeNodeUrlResolver()))
                .ForMember(display => display.HasPublishedVersion, expression => expression.Ignore());
                //.AfterMap((member, display) => MapGenericCustomProperties(applicationContext.Services.MemberService, applicationContext.Services.UserService, member, display, applicationContext.Services.TextService));

            //FROM IMember TO MemberBasic
            config.CreateMap<IMember, MemberBasic>()
                .ForMember(display => display.Udi,
                    expression =>
                        expression.MapFrom(content => Udi.Create(Constants.UdiEntityType.Member, content.Key)))
                .ForMember(dto => dto.Owner, expression => expression.ResolveUsing(new OwnerResolver<IMember>()))
                .ForMember(dto => dto.Icon, expression => expression.MapFrom(content => content.ContentType.Icon))
                .ForMember(dto => dto.ContentTypeAlias,
                    expression => expression.MapFrom(content => content.ContentType.Alias))
                .ForMember(dto => dto.Email, expression => expression.MapFrom(content => content.Email))
                .ForMember(dto => dto.Username, expression => expression.MapFrom(content => content.Username))
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(dto => dto.Published, expression => expression.Ignore())
                .ForMember(dto => dto.Updater, expression => expression.Ignore())
                .ForMember(dto => dto.Alias, expression => expression.Ignore())
                .ForMember(dto => dto.HasPublishedVersion, expression => expression.Ignore())
                .ForMember(dto => dto.Properties, expression => expression.ResolveUsing(new MemberBasicPropertiesResolver()));

            //FROM MembershipUser TO MemberBasic
            config.CreateMap<MembershipUser, MemberBasic>()
                //we're giving this entity an ID of 0 - we cannot really map it but it needs an id so the system knows it's not a new entity
                .ForMember(member => member.Id, expression => expression.MapFrom(user => int.MaxValue))
                .ForMember(display => display.Udi, expression => expression.Ignore())
                .ForMember(member => member.CreateDate, expression => expression.MapFrom(user => user.CreationDate))
                .ForMember(member => member.UpdateDate, expression => expression.MapFrom(user => user.LastActivityDate))
                .ForMember(member => member.Key, expression => expression.MapFrom(user => user.ProviderUserKey.TryConvertTo<Guid>().Result.ToString("N")))
                .ForMember(member => member.Owner, expression => expression.UseValue(new UserProfile { Name = "Admin", UserId = 0 }))
                .ForMember(member => member.Icon, expression => expression.UseValue("icon-user"))
                .ForMember(member => member.Name, expression => expression.MapFrom(user => user.UserName))
                .ForMember(member => member.Email, expression => expression.MapFrom(content => content.Email))
                .ForMember(member => member.Username, expression => expression.MapFrom(content => content.UserName))
                .ForMember(member => member.Properties, expression => expression.Ignore())
                .ForMember(member => member.ParentId, expression => expression.Ignore())
                .ForMember(member => member.Path, expression => expression.Ignore())
                .ForMember(member => member.SortOrder, expression => expression.Ignore())
                .ForMember(member => member.AdditionalData, expression => expression.Ignore())
                .ForMember(member => member.Published, expression => expression.Ignore())
                .ForMember(member => member.Updater, expression => expression.Ignore())
                .ForMember(member => member.Trashed, expression => expression.Ignore())
                .ForMember(member => member.Alias, expression => expression.Ignore())
                .ForMember(member => member.ContentTypeAlias, expression => expression.Ignore())
                .ForMember(member => member.HasPublishedVersion, expression => expression.Ignore());

            //FROM IMember TO ContentItemDto<IMember>
            config.CreateMap<IMember, ContentItemDto<IMember>>()
                .ForMember(display => display.Udi, expression => expression.MapFrom(content => Udi.Create(Constants.UdiEntityType.Member, content.Key)))
                .ForMember(dto => dto.Owner, expression => expression.ResolveUsing(new OwnerResolver<IMember>()))
                .ForMember(dto => dto.Published, expression => expression.Ignore())
                .ForMember(dto => dto.Updater, expression => expression.Ignore())
                .ForMember(dto => dto.Icon, expression => expression.Ignore())
                .ForMember(dto => dto.Alias, expression => expression.Ignore())
                .ForMember(dto => dto.HasPublishedVersion, expression => expression.Ignore())
                //do no map the custom member properties (currently anyways, they were never there in 6.x)
                .ForMember(dto => dto.Properties, expression => expression.ResolveUsing(new MemberDtoPropertiesValueResolver()));
        }

        /// <summary>
        /// Returns the login property display field
        /// </summary>
        /// <param name="memberService"></param>
        /// <param name="member"></param>
        /// <param name="localizedText"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the membership provider installed is the umbraco membership provider, then we will allow changing the username, however if
        /// the membership provider is a custom one, we cannot allow chaning the username because MembershipProvider's do not actually natively
        /// allow that.
        /// </remarks>
        internal static ContentPropertyDisplay GetLoginProperty(IMemberService memberService, IMember member, ILocalizedTextService localizedText)
        {
            var prop = new ContentPropertyDisplay
            {
                Alias = string.Format("{0}login", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                Label = localizedText.Localize("login"),
                Value = member.Username
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
        /// 
        /// This also ensures that the IsSensitive property display value is set based on the configured IMemberType property type
        /// </remarks>
        internal class MemberTabsAndPropertiesResolver : TabsAndPropertiesResolver<IMember>
        {
            private readonly ILocalizedTextService _localizedTextService;
            private readonly IMemberService _memberService;
            private readonly IUserService _userService;

            public MemberTabsAndPropertiesResolver(ILocalizedTextService localizedTextService, IMemberService memberService, IUserService userService)
                : base(localizedTextService)
            {
                _localizedTextService = localizedTextService;
                _memberService = memberService;
                _userService = userService;
            }

            public MemberTabsAndPropertiesResolver(ILocalizedTextService localizedTextService,
                IEnumerable<string> ignoreProperties, IMemberService memberService, IUserService userService) : base(localizedTextService, ignoreProperties)
            {
                _localizedTextService = localizedTextService;
                _memberService = memberService;
                _userService = userService;
            }
            
            /// <summary>
            /// Overridden to deal with custom member properties and permissions
            /// </summary>
            /// <param name="umbracoContext"></param>
            /// <param name="content"></param>
            /// <returns></returns>
            protected override List<Tab<ContentPropertyDisplay>> ResolveCore(UmbracoContext umbracoContext, IMember content)
            {
                var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
                
                IgnoreProperties = content.PropertyTypes
                    .Where(x => x.HasIdentity == false)
                    .Select(x => x.Alias)
                    .ToArray();

                var result = base.ResolveCore(umbracoContext, content);

                if (provider.IsUmbracoMembershipProvider() == false)
                {
                    //it's a generic provider so update the locked out property based on our known constant alias
                    var isLockedOutProperty = result.SelectMany(x => x.Properties).FirstOrDefault(x => x.Alias == Constants.Conventions.Member.IsLockedOut);
                    if (isLockedOutProperty?.Value != null && isLockedOutProperty.Value.ToString() != "1")
                    {
                        isLockedOutProperty.View = "readonlyvalue";
                        isLockedOutProperty.Value = _localizedTextService.Localize("general/no");
                    }
                }
                else
                {
                    var umbracoProvider = (IUmbracoMemberTypeMembershipProvider)provider;

                    //This is kind of a hack because a developer is supposed to be allowed to set their property editor - would have been much easier
                    // if we just had all of the membeship provider fields on the member table :(
                    // TODO: But is there a way to map the IMember.IsLockedOut to the property ? i dunno.
                    var isLockedOutProperty = result.SelectMany(x => x.Properties).FirstOrDefault(x => x.Alias == umbracoProvider.LockPropertyTypeAlias);
                    if (isLockedOutProperty?.Value != null && isLockedOutProperty.Value.ToString() != "1")
                    {
                        isLockedOutProperty.View = "readonlyvalue";
                        isLockedOutProperty.Value = _localizedTextService.Localize("general/no");
                    }
                }

                if (umbracoContext != null && umbracoContext.Security.CurrentUser != null
                    && umbracoContext.Security.CurrentUser.AllowedSections.Any(x => x.Equals(Constants.Applications.Settings)))
                {
                    var memberTypeLink = string.Format("#/member/memberTypes/edit/{0}", content.ContentTypeId);

                    //Replace the doctype property
                    var docTypeProperty = result.SelectMany(x => x.Properties)
                        .First(x => x.Alias == string.Format("{0}doctype", Constants.PropertyEditors.InternalGenericPropertiesPrefix));
                    docTypeProperty.Value = new List<object>
                    {
                        new
                        {
                            linkText = content.ContentType.Name,
                            url = memberTypeLink,
                            target = "_self",
                            icon = "icon-item-arrangement"
                        }
                    };
                    docTypeProperty.View = "urllist";
                }

                //check if there's an approval field
                var legacyProvider = provider as global::umbraco.providers.members.UmbracoMembershipProvider;
                if (content.HasIdentity == false && legacyProvider != null)
                {
                    var approvedField = legacyProvider.ApprovedPropertyTypeAlias;
                    var prop = result.SelectMany(x => x.Properties).FirstOrDefault(x => x.Alias == approvedField);
                    if (prop != null)
                    {
                        prop.Value = 1;
                    }
                }

                return result;
            }

            protected override IEnumerable<ContentPropertyDisplay> GetCustomGenericProperties(IContentBase content)
            {
                var member = (IMember) content;
                var membersProvider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

                var genericProperties = new List<ContentPropertyDisplay>
                {
                    new ContentPropertyDisplay
                    {
                        Alias = string.Format("{0}id", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                        Label = _localizedTextService.Localize("general/id"),
                        Value = new List<string> {member.Id.ToString(), member.Key.ToString()},
                        View = "idwithguid"
                    },
                    new ContentPropertyDisplay
                    {
                        Alias = string.Format("{0}doctype", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                        Label = _localizedTextService.Localize("content/membertype"),
                        Value = _localizedTextService.UmbracoDictionaryTranslate(member.ContentType.Name),
                        View = PropertyEditorResolver.Current.GetByAlias(Constants.PropertyEditors.NoEditAlias).ValueEditor.View
                    },
                    GetLoginProperty(_memberService, member, _localizedTextService),
                    new ContentPropertyDisplay
                    {
                        Alias = string.Format("{0}email", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                        Label = _localizedTextService.Localize("general/email"),
                        Value = member.Email,
                        View = "email",
                        Validation = {Mandatory = true}
                    },
                    new ContentPropertyDisplay
                    {
                        Alias = string.Format("{0}password", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                        Label = _localizedTextService.Localize("password"),
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
                        Config = new Dictionary<string, object>(membersProvider.GetConfiguration(_userService))
                        {
                            //the password change toggle will only be displayed if there is already a password assigned.
                            {"hasPassword", member.RawPasswordValue.IsNullOrWhiteSpace() == false}
                        }
                    },
                    new ContentPropertyDisplay
                    {
                        Alias = string.Format("{0}membergroup", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                        Label = _localizedTextService.Localize("content/membergroup"),
                        Value = GetMemberGroupValue(member.Username),
                        View = "membergroups",
                        Config = new Dictionary<string, object> {{"IsRequired", true}}
                    }
                };

                return genericProperties;
            }
            
            /// <summary>
            /// Overridden to assign the IsSensitive property values
            /// </summary>
            /// <param name="umbracoContext"></param>
            /// <param name="content"></param>
            /// <param name="properties"></param>
            /// <returns></returns>
            protected override List<ContentPropertyDisplay> MapProperties(UmbracoContext umbracoContext, IContentBase content, List<Property> properties)
            {
                var result = base.MapProperties(umbracoContext, content, properties);
                var member = (IMember)content;
                var memberType = member.ContentType;

                //now update the IsSensitive value
                foreach (var prop in result)
                {
                    //check if this property is flagged as sensitive
                    var isSensitiveProperty = memberType.IsSensitiveProperty(prop.Alias);
                    //check permissions for viewing sensitive data
                    if (isSensitiveProperty && umbracoContext.Security.CurrentUser.HasAccessToSensitiveData() == false)
                    {
                        //mark this property as sensitive
                        prop.IsSensitive = true;
                        //mark this property as readonly so that it does not post any data
                        prop.Readonly = true;
                        //replace this editor with a sensitivevalue
                        prop.View = "sensitivevalue";
                        //clear the value
                        prop.Value = null;
                    }
                }
                return result;
            }
        }

        internal class MembershipScenarioMappingResolver : ValueResolver<IMember, MembershipScenario>
        {
            private readonly IMemberTypeService _memberTypeService;

            public MembershipScenarioMappingResolver(IMemberTypeService memberTypeService)
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
                var memberType = _memberTypeService.Get(Constants.Conventions.MemberTypes.DefaultAlias);
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

        /// <summary>
        /// A converter to go from a <see cref="MembershipUser"/> to a <see cref="MemberDisplay"/>
        /// </summary>
        internal class MembershipUserTypeConverter : ITypeConverter<MembershipUser, MemberDisplay>
        {
            public MemberDisplay Convert(ResolutionContext context)
            {
                var source = (MembershipUser)context.SourceValue;
                //first convert to IMember
                var member = Mapper.Map<MembershipUser, IMember>(source);
                //then convert to MemberDisplay
                return AutoMapperExtensions.MapWithUmbracoContext<IMember, MemberDisplay>(member, context.GetUmbracoContext());
            }
        }

        /// <summary>
        /// A resolver to map <see cref="IMember"/> properties to a collection of <see cref="ContentPropertyBasic"/>
        /// </summary>
        internal class MemberBasicPropertiesResolver : IValueResolver
        {
            public ResolutionResult Resolve(ResolutionResult source)
            {
                if (source.Value != null && (source.Value is IMember) == false)
                    throw new AutoMapperMappingException(string.Format("Value supplied is of type {0} but expected {1}.\nChange the value resolver source type, or redirect the source value supplied to the value resolver using FromMember.", new object[]
                    {
                    source.Value.GetType(),
                    typeof (IMember)
                    }));
                return source.New(
                    //perform the mapping with the current umbraco context
                    ResolveCore(source.Context.GetUmbracoContext(), (IMember)source.Value), typeof(IEnumerable<ContentPropertyDisplay>));
            }

            private IEnumerable<ContentPropertyBasic> ResolveCore(UmbracoContext umbracoContext, IMember content)
            {
                var result = Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyBasic>>(
                    // Sort properties so items from different compositions appear in correct order (see U4-9298). Map sorted properties.
                    content.Properties.OrderBy(prop => prop.PropertyType.SortOrder))
                .ToList();

                var member = (IMember)content;
                var memberType = member.ContentType;

                //now update the IsSensitive value
                foreach (var prop in result)
                {
                    //check if this property is flagged as sensitive
                    var isSensitiveProperty = memberType.IsSensitiveProperty(prop.Alias);
                    //check permissions for viewing sensitive data
                    if (isSensitiveProperty && umbracoContext.Security.CurrentUser.HasAccessToSensitiveData() == false)
                    {
                        //mark this property as sensitive
                        prop.IsSensitive = true;
                        //clear the value
                        prop.Value = null;
                    }
                }
                return result;
            }
        }
    }
}
