using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class SecurityElement : UmbracoConfigurationElement, ISecuritySection
    {
        [ConfigurationProperty("keepUserLoggedIn")]
        internal InnerTextConfigurationElement<bool> KeepUserLoggedIn
        {
            get { return GetOptionalTextElement("keepUserLoggedIn", true); }
        }

        [ConfigurationProperty("hideDisabledUsersInBackoffice")]
        internal InnerTextConfigurationElement<bool> HideDisabledUsersInBackoffice
        {
            get { return GetOptionalTextElement("hideDisabledUsersInBackoffice", false); }
        }

        /// <summary>
        /// Used to enable/disable the forgot password functionality on the back office login screen
        /// </summary>
        [ConfigurationProperty("allowPasswordReset")]
        internal InnerTextConfigurationElement<bool> AllowPasswordReset
        {
            get { return GetOptionalTextElement("allowPasswordReset", true); }
        }

        /// <summary>
        /// A boolean indicating that by default the email address will be the username
        /// </summary>
        /// <remarks>
        /// Even if this is true and the username is different from the email in the database, the username field will still be shown.
        /// When this is false, the username and email fields will be shown in the user section.
        /// </remarks>
        [ConfigurationProperty("usernameIsEmail")]
        internal InnerTextConfigurationElement<bool> UsernameIsEmail
        {
            get { return GetOptionalTextElement("usernameIsEmail", true); }
        }

        [ConfigurationProperty("authCookieName")]
        internal InnerTextConfigurationElement<string> AuthCookieName
        {
            get { return GetOptionalTextElement("authCookieName", "UMB_UCONTEXT"); }
        }

        [ConfigurationProperty("authCookieDomain")]
        internal InnerTextConfigurationElement<string> AuthCookieDomain
        {
            get { return GetOptionalTextElement<string>("authCookieDomain", null); }
        }

        bool ISecuritySection.KeepUserLoggedIn
        {
            get { return KeepUserLoggedIn; }
        }

        bool ISecuritySection.HideDisabledUsersInBackoffice
        {
            get { return HideDisabledUsersInBackoffice; }
        }

        /// <summary>
        /// Used to enable/disable the forgot password functionality on the back office login screen
        /// </summary>
        bool ISecuritySection.AllowPasswordReset
        {
            get { return AllowPasswordReset; }
        }

        /// <summary>
        /// A boolean indicating that by default the email address will be the username
        /// </summary>
        /// <remarks>
        /// Even if this is true and the username is different from the email in the database, the username field will still be shown.
        /// When this is false, the username and email fields will be shown in the user section.
        /// </remarks>
        bool ISecuritySection.UsernameIsEmail
        {
            get { return UsernameIsEmail; }
        }

        string ISecuritySection.AuthCookieName
        {
            get { return AuthCookieName; }
        }

        string ISecuritySection.AuthCookieDomain
        {
            get { return AuthCookieDomain; }
        }
    }
}
