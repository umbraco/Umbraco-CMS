using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <summary>
/// A custom tab/property resolver for members which will ensure that the built-in membership properties are or aren't displayed
/// depending on if the member type has these properties
/// </summary>
/// <remarks>
/// This also ensures that the IsLocked out property is readonly when the member is not locked out - this is because
/// an admin cannot actually set isLockedOut = true, they can only unlock.
/// </remarks>
public class MemberTabsAndPropertiesMapper : TabsAndPropertiesMapper<IMember>
{
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly IMemberService _memberService;
    private readonly IMemberGroupService _memberGroupService;
    private readonly MemberPasswordConfigurationSettings _memberPasswordConfiguration;
    private readonly PropertyEditorCollection _propertyEditorCollection;

    public MemberTabsAndPropertiesMapper(
        ICultureDictionary cultureDictionary,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        ILocalizedTextService localizedTextService,
        IMemberTypeService memberTypeService,
        IMemberService memberService,
        IMemberGroupService memberGroupService,
        IOptions<MemberPasswordConfigurationSettings> memberPasswordConfiguration,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        PropertyEditorCollection propertyEditorCollection)
        : base(cultureDictionary, localizedTextService, contentTypeBaseServiceProvider)
    {
        _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
        _localizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        _memberTypeService = memberTypeService ?? throw new ArgumentNullException(nameof(memberTypeService));
        _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
        _memberGroupService = memberGroupService ?? throw new ArgumentNullException(nameof(memberGroupService));
        _memberPasswordConfiguration = memberPasswordConfiguration.Value;
        _propertyEditorCollection = propertyEditorCollection;
    }

    /// <inheritdoc />
    /// <remarks>Overridden to deal with custom member properties and permissions.</remarks>
    public override IEnumerable<Tab<ContentPropertyDisplay>> Map(IMember source, MapperContext context)
    {

        IMemberType? memberType = _memberTypeService.Get(source.ContentTypeId);

        if (memberType is not null)
        {

            IgnoreProperties = memberType.CompositionPropertyTypes
                .Where(x => x.HasIdentity == false)
                .Select(x => x.Alias)
                .ToArray();
        }

        IEnumerable<Tab<ContentPropertyDisplay>> resolved = base.Map(source, context);

        return resolved;
    }

    [Obsolete("Use MapMembershipProperties. Will be removed in Umbraco 10.")]
    protected override IEnumerable<ContentPropertyDisplay> GetCustomGenericProperties(IContentBase content)
    {
        var member = (IMember)content;
        return MapMembershipProperties(member, null);
    }

    private Dictionary<string, object> GetPasswordConfig(IMember member)
    {
        var result = new Dictionary<string, object>(_memberPasswordConfiguration.GetConfiguration(true))
        {
            // the password change toggle will only be displayed if there is already a password assigned.
            {"hasPassword", member.RawPasswordValue.IsNullOrWhiteSpace() == false}
        };

        // This will always be true for members since we always want to allow admins to change a password - so long as that
        // user has access to edit members (but that security is taken care of separately)
        result["allowManuallyChangingPassword"] = true;

        return result;
    }

    /// <summary>
    /// Overridden to assign the IsSensitive property values
    /// </summary>
    /// <param name="content"></param>
    /// <param name="properties"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override List<ContentPropertyDisplay> MapProperties(IContentBase content, List<IProperty> properties, MapperContext context)
    {
        List<ContentPropertyDisplay> result = base.MapProperties(content, properties, context);
        var member = (IMember)content;
        IMemberType? memberType = _memberTypeService.Get(member.ContentTypeId);

        // now update the IsSensitive value
        foreach (ContentPropertyDisplay prop in result)
        {
            // check if this property is flagged as sensitive
            var isSensitiveProperty = memberType?.IsSensitiveProperty(prop.Alias) ?? false;
            // check permissions for viewing sensitive data
            if (isSensitiveProperty && _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.HasAccessToSensitiveData() == false)
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
    /// <param name="member"></param>
    /// <param name="localizedText"></param>
    /// <returns></returns>
    /// <remarks>
    /// If the membership provider installed is the umbraco membership provider, then we will allow changing the username, however if
    /// the membership provider is a custom one, we cannot allow changing the username because MembershipProvider's do not actually natively
    /// allow that.
    /// </remarks>
    internal static ContentPropertyDisplay GetLoginProperty(IMember member, ILocalizedTextService localizedText)
    {
        var prop = new ContentPropertyDisplay
        {
            Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}login",
            Label = localizedText.Localize(null,"login"),
            Value = member.Username
        };

        prop.View = "textbox";
        prop.Validation.Mandatory = true;
        return prop;
    }

    internal IDictionary<string, bool> GetMemberGroupValue(string username)
    {
        IEnumerable<string> userRoles = _memberService.GetAllRoles(username);

        // create a dictionary of all roles (except internal roles) + "false"
        var result = _memberGroupService.GetAll()
            .Select(x => x.Name!)
            // if a role starts with __umbracoRole we won't show it as it's an internal role used for public access
            .Where(x => x?.StartsWith(Constants.Conventions.Member.InternalRolePrefix) == false)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x, x => false);

        // if user has no roles, just return the dictionary
        if (userRoles == null)
        {
            return result;
        }

        // else update the dictionary to "true" for the user roles (except internal roles)
        foreach (var userRole in userRoles.Where(x => x?.StartsWith(Constants.Conventions.Member.InternalRolePrefix) == false))
        {
            result[userRole] = true;
        }

        return result;
    }

    public IEnumerable<ContentPropertyDisplay> MapMembershipProperties(IMember member, MapperContext? context)
    {
        var properties = new List<ContentPropertyDisplay>
        {
            GetLoginProperty(member, _localizedTextService),
            new()
            {
                Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}email",
                Label = _localizedTextService.Localize("general","email"),
                Value = member.Email,
                View = "email",
                Validation = { Mandatory = true }
            },
            new()
            {
                Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}password",
                Label = _localizedTextService.Localize(null,"password"),
                Value = new Dictionary<string, object?>
                {
                    // TODO: why ignoreCase, what are we doing here?!
                    { "newPassword", member.GetAdditionalDataValueIgnoreCase("NewPassword", null) }
                },
                View = "changepassword",
                Config = GetPasswordConfig(member) // Initialize the dictionary with the configuration from the default membership provider
            },
            new()
            {
                Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}membergroup",
                Label = _localizedTextService.Localize("content","membergroup"),
                Value = GetMemberGroupValue(member.Username),
                View = "membergroups",
                Config = new Dictionary<string, object>
                {
                    { "IsRequired", true }
                },
            },

            // These properties used to live on the member as property data, defaulting to sensitive, so we set them to sensitive here too
            new()
            {
                Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}failedPasswordAttempts",
                Label = _localizedTextService.Localize("user", "failedPasswordAttempts"),
                Value = member.FailedPasswordAttempts,
                View = "readonlyvalue",
                IsSensitive = true,
            },

            new()
            {
                Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}approved",
                Label = _localizedTextService.Localize("user", "stateApproved"),
                Value = member.IsApproved,
                View = "boolean",
                IsSensitive = true,
                Readonly = false,
            },

            new()
            {
                Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}lockedOut",
                Label = _localizedTextService.Localize("user", "stateLockedOut"),
                Value = member.IsLockedOut,
                View = "boolean",
                IsSensitive = true,
                Readonly = !member.IsLockedOut, // IMember.IsLockedOut can't be set to true, so make it readonly when that's the case (you can only unlock)
            },

            new()
            {
                Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}lastLockoutDate",
                Label = _localizedTextService.Localize("user", "lastLockoutDate"),
                Value = member.LastLockoutDate?.ToString(),
                View = "readonlyvalue",
                IsSensitive = true,
            },

            new()
            {
                Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}lastLoginDate",
                Label = _localizedTextService.Localize("user", "lastLogin"),
                Value = member.LastLoginDate?.ToString(),
                View = "readonlyvalue",
                IsSensitive = true,
            },

            new()
            {
                Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}lastPasswordChangeDate",
                Label = _localizedTextService.Localize("user", "lastPasswordChangeDate"),
                Value = member.LastPasswordChangeDate?.ToString(),
                View = "readonlyvalue",
                IsSensitive = true,
            },
        };

        if (_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.HasAccessToSensitiveData() is false)
        {
            // Current user doesn't have access to sensitive data so explicitly set the views and remove the value from sensitive data
            foreach (ContentPropertyDisplay property in properties)
            {
                if (property.IsSensitive)
                {
                    property.Value = null;
                    property.View = "sensitivevalue";
                    property.Readonly = true;
                }
            }
        }

        return properties;
    }
}
