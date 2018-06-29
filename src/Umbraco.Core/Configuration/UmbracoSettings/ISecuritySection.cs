namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ISecuritySection : IUmbracoConfigurationSection
    {
        bool KeepUserLoggedIn { get; }

        bool HideDisabledUsersInBackoffice { get; }

        /// <summary>
        /// Used to enable/disable the forgot password functionality on the back office login screen
        /// </summary>
        bool AllowPasswordReset { get; }

        string AuthCookieName { get; }

        string AuthCookieDomain { get; }

        /// <summary>
        /// A boolean indicating that by default the email address will be the username
        /// </summary>
        /// <remarks>
        /// Even if this is true and the username is different from the email in the database, the username field will still be shown.
        /// When this is false, the username and email fields will be shown in the user section.
        /// </remarks>
        bool UsernameIsEmail { get; }
    }
}
