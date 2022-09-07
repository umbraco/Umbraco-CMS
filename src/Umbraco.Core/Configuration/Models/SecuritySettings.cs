// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for security settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigSecurity)]
public class SecuritySettings
{
    internal const bool StaticMemberBypassTwoFactorForExternalLogins = true;
    internal const bool StaticUserBypassTwoFactorForExternalLogins = true;
    internal const bool StaticKeepUserLoggedIn = false;
    internal const bool StaticHideDisabledUsersInBackOffice = false;
    internal const bool StaticAllowPasswordReset = true;
    internal const bool StaticAllowEditInvariantFromNonDefault = false;
    internal const string StaticAuthCookieName = "UMB_UCONTEXT";

    internal const string StaticAllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+\\";

    /// <summary>
    ///     Gets or sets a value indicating whether to keep the user logged in.
    /// </summary>
    [DefaultValue(StaticKeepUserLoggedIn)]
    public bool KeepUserLoggedIn { get; set; } = StaticKeepUserLoggedIn;

    /// <summary>
    ///     Gets or sets a value indicating whether to hide disabled users in the back-office.
    /// </summary>
    [DefaultValue(StaticHideDisabledUsersInBackOffice)]
    public bool HideDisabledUsersInBackOffice { get; set; } = StaticHideDisabledUsersInBackOffice;

    /// <summary>
    ///     Gets or sets a value indicating whether to allow user password reset.
    /// </summary>
    [DefaultValue(StaticAllowPasswordReset)]
    public bool AllowPasswordReset { get; set; } = StaticAllowPasswordReset;

    /// <summary>
    ///     Gets or sets a value for the authorization cookie name.
    /// </summary>
    [DefaultValue(StaticAuthCookieName)]
    public string AuthCookieName { get; set; } = StaticAuthCookieName;

    /// <summary>
    ///     Gets or sets a value for the authorization cookie domain.
    /// </summary>
    public string? AuthCookieDomain { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the user's email address is to be considered as their username.
    /// </summary>
    public bool UsernameIsEmail { get; set; } = true;

    /// <summary>
    ///     Gets or sets the set of allowed characters for a username
    /// </summary>
    [DefaultValue(StaticAllowedUserNameCharacters)]
    public string AllowedUserNameCharacters { get; set; } = StaticAllowedUserNameCharacters;

    /// <summary>
    ///     Gets or sets a value for the user password settings.
    /// </summary>
    public UserPasswordConfigurationSettings? UserPassword { get; set; }

    /// <summary>
    ///     Gets or sets a value for the member password settings.
    /// </summary>
    public MemberPasswordConfigurationSettings? MemberPassword { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to bypass the two factor requirement in Umbraco when using external login
    ///     for members. Thereby rely on the External login and potential 2FA at that provider.
    /// </summary>
    [DefaultValue(StaticMemberBypassTwoFactorForExternalLogins)]
    public bool MemberBypassTwoFactorForExternalLogins { get; set; } = StaticMemberBypassTwoFactorForExternalLogins;

    /// <summary>
    ///     Gets or sets a value indicating whether to bypass the two factor requirement in Umbraco when using external login
    ///     for users. Thereby rely on the External login and potential 2FA at that provider.
    /// </summary>
    [DefaultValue(StaticUserBypassTwoFactorForExternalLogins)]
    public bool UserBypassTwoFactorForExternalLogins { get; set; } = StaticUserBypassTwoFactorForExternalLogins;

    /// <summary>
    /// Gets or sets a value indicating whether to allow editing invariant properties from a non-default language variation.
    /// </summary>
    [Obsolete("Use ContentSettings.AllowEditFromInvariant instead")]
    [DefaultValue(StaticAllowEditInvariantFromNonDefault)]
    public bool AllowEditInvariantFromNonDefault { get; set; } = StaticAllowEditInvariantFromNonDefault;
}
