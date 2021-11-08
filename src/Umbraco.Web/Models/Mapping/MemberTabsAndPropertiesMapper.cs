using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// A custom tab/property resolver for members which will ensure that the built-in membership properties are or aren't displayed
    /// depending on if the member type has these properties
    /// </summary>
    /// <remarks>
    /// This also ensures that the IsLocked out property is readonly when the member is not locked out - this is because
    /// an admin cannot actually set isLockedOut = true, they can only unlock.
    /// </remarks>
    internal class MemberTabsAndPropertiesMapper : TabsAndPropertiesMapper<IMember>
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IMemberService _memberService;
        private readonly IUserService _userService;

        public MemberTabsAndPropertiesMapper(IUmbracoContextAccessor umbracoContextAccessor, ILocalizedTextService localizedTextService, IMemberService memberService, IUserService userService, IMemberTypeService memberTypeService)
            : base(localizedTextService)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _localizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _memberTypeService = memberTypeService ?? throw new ArgumentNullException(nameof(memberTypeService));
        }

        /// <inheritdoc />
        /// <remarks>Overridden to deal with custom member properties and permissions.</remarks>
        public override IEnumerable<Tab<ContentPropertyDisplay>> Map(IMember source, MapperContext context)
        {
            var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

            var memberType = _memberTypeService.Get(source.ContentTypeId);

            IgnoreProperties = memberType.CompositionPropertyTypes
                .Where(x => x.HasIdentity == false)
                .Select(x => x.Alias)
                .ToArray();

            var resolved = base.Map(source, context);

            // IMember.IsLockedOut can't be set to true, so make it readonly when that's the case (you can only unlock)
            var isLockedOutPropertyAlias = Constants.Conventions.Member.IsLockedOut;
            if (provider.IsUmbracoMembershipProvider() &&
                provider is IUmbracoMemberTypeMembershipProvider umbracoProvider)
            {
                // This is kind of a hack because a developer is supposed to be allowed to set their property editor - would have been much easier
                // if we just had all of the membership provider fields on the member table :(
                // TODO: But is there a way to map the IMember.IsLockedOut to the property ? i dunno.
                isLockedOutPropertyAlias = umbracoProvider.LockPropertyTypeAlias;
            }

            var isLockedOutProperty = resolved.SelectMany(x => x.Properties).FirstOrDefault(x => x.Alias == isLockedOutPropertyAlias);
            if (isLockedOutProperty?.Value != null && isLockedOutProperty.Value.ToString() != "1")
            {
                isLockedOutProperty.Readonly = true;
            }

            return resolved;
        }

        public IEnumerable<ContentPropertyDisplay> MapMembershipProperties(IMember member, MapperContext context)
        {
            var membersProvider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

            var properties = new List<ContentPropertyDisplay>
            {
                GetLoginProperty(_memberService, member, _localizedTextService),
                new ContentPropertyDisplay
                {
                    Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}email",
                    Label = _localizedTextService.Localize("general","email"),
                    Value = member.Email,
                    View = "email",
                    Validation = {Mandatory = true}
                },
                new ContentPropertyDisplay
                {
                    Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}password",
                    Label = _localizedTextService.Localize(null,"password"),
                    // NOTE: The value here is a JSON value - but the only property we care about is the generatedPassword one if it exists, the newPassword exists
                    // only when creating a new member and we want to have a generated password pre-filled.
                    Value = new Dictionary<string, object>
                    {
                        // TODO: why ignoreCase, what are we doing here?!
                        { "generatedPassword", member.GetAdditionalDataValueIgnoreCase("GeneratedPassword", null) },
                        { "newPassword", member.GetAdditionalDataValueIgnoreCase("NewPassword", null) },
                    },
                    View = "changepassword",
                    // Initialize the dictionary with the configuration from the default membership provider
                    Config = new Dictionary<string, object>(membersProvider.GetConfiguration(_userService))
                    {
                        // The password change toggle will only be displayed if there is already a password assigned.
                        { "hasPassword", member.RawPasswordValue.IsNullOrWhiteSpace() == false }
                    }
                },
                new ContentPropertyDisplay
                {
                    Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}membergroup",
                    Label = _localizedTextService.Localize("content","membergroup"),
                    Value = GetMemberGroupValue(member.Username),
                    View = "membergroups",
                    Config = new Dictionary<string, object> {{"IsRequired", true}}
                }
            };

            return properties;
        }

        /// <summary>
        /// Overridden to assign the IsSensitive property values
        /// </summary>
        /// <param name="content"></param>
        /// <param name="properties"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override List<ContentPropertyDisplay> MapProperties(IContentBase content, List<Property> properties, MapperContext context)
        {
            var result = base.MapProperties(content, properties, context);
            var member = (IMember)content;
            var memberType = _memberTypeService.Get(member.ContentTypeId);

            var umbracoContext = _umbracoContextAccessor.UmbracoContext;

            // now update the IsSensitive value
            foreach (var prop in result)
            {
                // check if this property is flagged as sensitive
                var isSensitiveProperty = memberType.IsSensitiveProperty(prop.Alias);
                // check permissions for viewing sensitive data
                if (isSensitiveProperty && (umbracoContext == null || umbracoContext.Security.CurrentUser.HasAccessToSensitiveData() == false))
                {
                    // mark this property as sensitive
                    prop.IsSensitive = true;
                    // mark this property as readonly so that it does not post any data
                    prop.Readonly = true;
                    // replace this editor with a sensitive value
                    prop.View = "sensitivevalue";
                    // clear the value
                    prop.Value = null;
                }
            }
            return result;
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
        /// the membership provider is a custom one, we cannot allow changing the username because MembershipProvider's do not actually natively
        /// allow that.
        /// </remarks>
        internal static ContentPropertyDisplay GetLoginProperty(IMemberService memberService, IMember member, ILocalizedTextService localizedText)
        {
            var prop = new ContentPropertyDisplay
            {
                Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}login",
                Label = localizedText.Localize(null,"login"),
                Value = member.Username
            };

            var scenario = memberService.GetMembershipScenario();

            // only allow editing if this is a new member, or if the membership provider is the Umbraco one
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
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
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
