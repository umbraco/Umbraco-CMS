// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for security settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigSecurity)]
public class SecuritySettings
{
    /// <summary>
    ///     The default value for bypassing two-factor authentication for external member logins.
    /// </summary>
    internal const bool StaticMemberBypassTwoFactorForExternalLogins = true;

    /// <summary>
    ///     The default value for bypassing two-factor authentication for external user logins.
    /// </summary>
    internal const bool StaticUserBypassTwoFactorForExternalLogins = true;

    /// <summary>
    ///     The default value for keeping users logged in.
    /// </summary>
    internal const bool StaticKeepUserLoggedIn = false;

    /// <summary>
    ///     The default value for hiding disabled users in the back-office.
    /// </summary>
    internal const bool StaticHideDisabledUsersInBackOffice = false;

    /// <summary>
    ///     The default value for allowing password reset.
    /// </summary>
    internal const bool StaticAllowPasswordReset = true;

    /// <summary>
    ///     The default value for allowing edit of invariant properties from non-default language.
    /// </summary>
    internal const bool StaticAllowEditInvariantFromNonDefault = false;

    /// <summary>
    ///     The default value for allowing concurrent logins.
    /// </summary>
    internal const bool StaticAllowConcurrentLogins = false;

    /// <summary>
    ///     The default authentication cookie name.
    /// </summary>
    internal const string StaticAuthCookieName = "UMB_UCONTEXT";

    /// <summary>
    ///     The default authentication cookie SameSite mode.
    /// </summary>
    internal const string StaticAuthCookieSameSite = "Strict";

    /// <summary>
    ///     The default value for using email as username.
    /// </summary>
    internal const bool StaticUsernameIsEmail = true;

    /// <summary>
    ///     The default value for requiring unique email for members.
    /// </summary>
    internal const bool StaticMemberRequireUniqueEmail = true;

    /// <summary>
    ///     The default set of allowed characters for usernames.
    /// </summary>
    internal const string StaticAllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+\\";

    /// <summary>
    ///     The default member lockout time in minutes.
    /// </summary>
    internal const int StaticMemberDefaultLockoutTimeInMinutes = 30 * 24 * 60;

    /// <summary>
    ///     The default user lockout time in minutes.
    /// </summary>
    internal const int StaticUserDefaultLockoutTimeInMinutes = 30 * 24 * 60;

    /// <summary>
    ///     The default duration in milliseconds for failed login attempts.
    /// </summary>
    internal const long StaticUserDefaultFailedLoginDurationInMilliseconds = 1000;

    /// <summary>
    ///     The minimum duration in milliseconds for failed login attempts.
    /// </summary>
    internal const long StaticUserMinimumFailedLoginDurationInMilliseconds = 250;

    /// <summary>
    ///     The default path the back office is served at.
    /// </summary>
    internal const string StaticCallbackPathName = "/umbraco";

    /// <summary>
    ///     The default expiry time for password reset emails.
    /// </summary>
    internal const string StaticPasswordResetEmailExpiry = "01:00:00";

    /// <summary>
    ///     The default expiry time for user invite emails.
    /// </summary>
    internal const string StaticUserInviteEmailExpiry = "3.00:00:00";

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
    ///     Gets or sets the SameSite mode for the back-office authentication cookie.
    /// </summary>
    /// <remarks>
    ///     Valid values are "Strict" (default), "Lax", "None" and "Unspecified".
    ///     <para>
    ///     Keep the default in production. SameSite is what stops a cross-site request from carrying
    ///     the authentication cookie, and the Management API has no antiforgery tokens of its own.
    ///     </para>
    ///     <para>
    ///     Only set "None" when the back office is served from a different origin than the server,
    ///     such as a local front-end dev server or a containerised setup, so the cookie is sent on
    ///     cross-site requests. "None" additionally requires the cookie to be marked Secure, so
    ///     <see cref="GlobalSettings.UseHttps" /> must be enabled - browsers reject a "None" cookie
    ///     that is not Secure, and sign-in then fails with no server-side error. A normal deployment
    ///     serves both from the same origin and does not need it.
    ///     </para>
    /// </remarks>
    [DefaultValue(StaticAuthCookieSameSite)]
    public string AuthCookieSameSite { get; set; } = StaticAuthCookieSameSite;

    /// <summary>
    ///     Gets or sets a value indicating whether the user's email address is to be considered as their username.
    /// </summary>
    [DefaultValue(StaticUsernameIsEmail)]
    public bool UsernameIsEmail { get; set; } = StaticUsernameIsEmail;

    /// <summary>
    ///     Gets or sets a value indicating whether the member's email address must be unique.
    /// </summary>
    [DefaultValue(StaticMemberRequireUniqueEmail)]
    public bool MemberRequireUniqueEmail { get; set; } = StaticMemberRequireUniqueEmail;

    /// <summary>
    ///     Gets or sets the set of allowed characters for a username
    /// </summary>
    [DefaultValue(StaticAllowedUserNameCharacters)]
    public string AllowedUserNameCharacters { get; set; } = StaticAllowedUserNameCharacters;

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
    ///     Gets or sets a value for how long (in minutes) a member is locked out when a lockout occurs.
    /// </summary>
    [DefaultValue(StaticMemberDefaultLockoutTimeInMinutes)]
    public int MemberDefaultLockoutTimeInMinutes { get; set; } = StaticMemberDefaultLockoutTimeInMinutes;

    /// <summary>
    ///     Gets or sets a value for how long (in minutes) a user is locked out when a lockout occurs.
    /// </summary>
    [DefaultValue(StaticUserDefaultLockoutTimeInMinutes)]
    public int UserDefaultLockoutTimeInMinutes { get; set; } = StaticUserDefaultLockoutTimeInMinutes;

    /// <summary>
    ///     Gets or sets a value indicating whether to allow concurrent logins.
    /// </summary>
    [DefaultValue(StaticAllowConcurrentLogins)]
    public bool AllowConcurrentLogins { get; set; } = StaticAllowConcurrentLogins;

    /// <summary>
    ///     Gets or sets a value indicating whether to allow concurrent logins for backoffice users.
    ///     When <c>null</c> (default), the value of <see cref="AllowConcurrentLogins"/> is used.
    /// </summary>
    public bool? UserAllowConcurrentLogins { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to allow concurrent logins for members.
    ///     When <c>null</c> (default), the value of <see cref="AllowConcurrentLogins"/> is used.
    /// </summary>
    public bool? MemberAllowConcurrentLogins { get; set; }

    /// <summary>
    ///     Gets the effective concurrent login setting for backoffice users.
    /// </summary>
    public bool GetUserAllowConcurrentLogins() => UserAllowConcurrentLogins ?? AllowConcurrentLogins;

    /// <summary>
    ///     Gets the effective concurrent login setting for members.
    /// </summary>
    public bool GetMemberAllowConcurrentLogins() => MemberAllowConcurrentLogins ?? AllowConcurrentLogins;

    /// <summary>
    /// Gets or sets the default duration (in milliseconds) of failed login attempts.
    /// </summary>
    /// <value>
    /// The default duration (in milliseconds) of failed login attempts.
    /// </value>
    /// <remarks>
    /// The user login endpoint ensures that failed login attempts take at least as long as the average successful login.
    /// However, if no successful logins have occurred, this value is used as the default duration.
    /// </remarks>
    [Range(0, long.MaxValue)]
    [DefaultValue(StaticUserDefaultFailedLoginDurationInMilliseconds)]
    public long UserDefaultFailedLoginDurationInMilliseconds { get; set; } = StaticUserDefaultFailedLoginDurationInMilliseconds;

    /// <summary>
    /// Gets or sets the minimum duration (in milliseconds) of failed login attempts.
    /// </summary>
    /// <value>
    /// The minimum duration (in milliseconds) of failed login attempts.
    /// </value>
    [Range(0, long.MaxValue)]
    [DefaultValue(StaticUserMinimumFailedLoginDurationInMilliseconds)]
    public long UserMinimumFailedLoginDurationInMilliseconds { get; set; } = StaticUserMinimumFailedLoginDurationInMilliseconds;

    /// <summary>
    ///     Gets or sets a value of the back-office host URI. Use this when running the back-office client and the Management API on different hosts. Leave empty when running both on the same host.
    /// </summary>
    public Uri? BackOfficeHost { get; set; }

    /// <summary>
    ///     Gets or sets the path at which the back-office client is served. Used as the default
    ///     post-login return target: appended to <see cref="BackOfficeHost" /> when that is set (e.g.
    ///     "/" when the client runs at the root of a separate host such as a dev server), and used
    ///     as-is on the current host otherwise. The logout and error paths are derived from it - see
    ///     <see cref="GetEffectiveLogoutPathName" /> and <see cref="GetEffectiveErrorPathName" />.
    ///     Defaults to the standard back-office path.
    /// </summary>
    [DefaultValue(StaticCallbackPathName)]
    public string CallbackPathName { get; set; } = StaticCallbackPathName;

    /// <summary>
    ///     Gets or sets the expiry time for password reset emails.
    /// </summary>
    [DefaultValue(StaticPasswordResetEmailExpiry)]
    public TimeSpan PasswordResetEmailExpiry { get; set; } = TimeSpan.Parse(StaticPasswordResetEmailExpiry);

    /// <summary>
    ///     Gets or sets the expiry time for user invite emails.
    /// </summary>
    [DefaultValue(StaticUserInviteEmailExpiry)]
    public TimeSpan UserInviteEmailExpiry { get; set; } = TimeSpan.Parse(StaticUserInviteEmailExpiry);

    /// <summary>
    ///     Gets or sets the password configuration settings for users.
    /// </summary>
    /// <remarks>
    ///     This property exists to enable IntelliSense/autocomplete in appsettings.json
    ///     for the <c>Umbraco:CMS:Security:UserPassword</c> configuration section.
    /// </remarks>
    public UserPasswordConfigurationSettings UserPassword { get; set; } = new();

    /// <summary>
    ///     Gets or sets the password configuration settings for members.
    /// </summary>
    /// <remarks>
    ///     This property exists to enable IntelliSense/autocomplete in appsettings.json
    ///     for the <c>Umbraco:CMS:Security:MemberPassword</c> configuration section.
    /// </remarks>
    public MemberPasswordConfigurationSettings MemberPassword { get; set; } = new();

    /// <summary>
    ///     Gets the path to the back-office client's logout route, derived from <see cref="CallbackPathName" />.
    /// </summary>
    /// <returns>The logout path.</returns>
    public string GetEffectiveLogoutPathName() => CallbackPathName.EnsureEndsWith('/') + "logout";

    /// <summary>
    ///     Gets the path to the back-office client's error route, derived from <see cref="CallbackPathName" />.
    /// </summary>
    /// <returns>The error path.</returns>
    public string GetEffectiveErrorPathName() => CallbackPathName.EnsureEndsWith('/') + "error";
}
