using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using SecurityConstants = Umbraco.Cms.Core.Constants.Security;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Options used to configure auto-linking external OAuth providers
/// </summary>
public class ExternalSignInAutoLinkOptions
{
    private readonly string? _defaultCulture;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalSignInAutoLinkOptions" /> class.
    /// </summary>
    /// <param name="autoLinkExternalAccount"></param>
    /// <param name="defaultUserGroups">If null, the default will be the 'editor' group</param>
    /// <param name="defaultCulture"></param>
    /// <param name="allowManualLinking"></param>
    public ExternalSignInAutoLinkOptions(
        bool autoLinkExternalAccount = false,
        string[]? defaultUserGroups = null,
        string? defaultCulture = null,
        bool allowManualLinking = true)
    {
        DefaultUserGroups = defaultUserGroups ?? new[] { SecurityConstants.EditorGroupAlias };
        AutoLinkExternalAccount = autoLinkExternalAccount;
        AllowManualLinking = allowManualLinking;
        _defaultCulture = defaultCulture;
    }

    /// <summary>
    ///     A callback executed during account auto-linking and before the user is persisted
    /// </summary>
    [IgnoreDataMember]
    public Action<BackOfficeIdentityUser, ExternalLoginInfo>? OnAutoLinking { get; set; }

    /// <summary>
    ///     A callback executed during every time a user authenticates using an external login.
    ///     returns a boolean indicating if sign in should continue or not.
    /// </summary>
    [IgnoreDataMember]
    public Func<BackOfficeIdentityUser, ExternalLoginInfo, bool>? OnExternalLogin { get; set; }

    /// <summary>
    ///     Gets a value indicating whether flag indicating if logging in with the external provider should auto-link/create a
    ///     local user
    /// </summary>
    public bool AutoLinkExternalAccount { get; }

    /// <summary>
    ///     By default this is true which allows the user to manually link and unlink the external provider, if set to false
    ///     the back office user
    ///     will not see and cannot perform manual linking or unlinking of the external provider.
    /// </summary>
    public bool AllowManualLinking { get; protected set; }

    /// <summary>
    ///     The default user groups to assign to the created local user linked
    /// </summary>
    public string[] DefaultUserGroups { get; }

    /// <summary>
    ///     The default Culture to use for auto-linking users
    /// </summary>
    // TODO: Should we use IDefaultCultureAccessor here instead?
    public string GetUserAutoLinkCulture(GlobalSettings globalSettings) =>
        _defaultCulture ?? globalSettings.DefaultUILanguage;
}
