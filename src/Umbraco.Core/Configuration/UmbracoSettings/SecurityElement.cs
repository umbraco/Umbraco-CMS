using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class SecurityElement : UmbracoConfigurationElement, ISecuritySection
    {
        [ConfigurationProperty("contentSecurityPolicy")]
        internal ContentSecurityPolicyElement ContentSecurityPolicy => (ContentSecurityPolicyElement)this["contentSecurityPolicy"];

        [ConfigurationProperty("keepUserLoggedIn")]
        internal InnerTextConfigurationElement<bool> KeepUserLoggedIn => GetOptionalTextElement("keepUserLoggedIn", true);

        [ConfigurationProperty("hideDisabledUsersInBackoffice")]
        internal InnerTextConfigurationElement<bool> HideDisabledUsersInBackoffice => GetOptionalTextElement("hideDisabledUsersInBackoffice", false);

        /// <summary>
        /// Used to enable/disable the forgot password functionality on the back office login screen
        /// </summary>
        [ConfigurationProperty("allowPasswordReset")]
        internal InnerTextConfigurationElement<bool> AllowPasswordReset => GetOptionalTextElement("allowPasswordReset", true);

        /// <summary>
        /// A boolean indicating that by default the email address will be the username
        /// </summary>
        /// <remarks>
        /// Even if this is true and the username is different from the email in the database, the username field will still be shown.
        /// When this is false, the username and email fields will be shown in the user section.
        /// </remarks>
        [ConfigurationProperty("usernameIsEmail")]
        internal InnerTextConfigurationElement<bool> UsernameIsEmail => GetOptionalTextElement("usernameIsEmail", true);

        [ConfigurationProperty("authCookieName")]
        internal InnerTextConfigurationElement<string> AuthCookieName => GetOptionalTextElement("authCookieName", "UMB_UCONTEXT");

        [ConfigurationProperty("authCookieDomain")]
        internal InnerTextConfigurationElement<string> AuthCookieDomain => GetOptionalTextElement<string>("authCookieDomain", null);

        bool ISecuritySection.KeepUserLoggedIn => KeepUserLoggedIn;

        bool ISecuritySection.HideDisabledUsersInBackoffice => HideDisabledUsersInBackoffice;

        /// <summary>
        /// Used to enable/disable the forgot password functionality on the back office login screen
        /// </summary>
        bool ISecuritySection.AllowPasswordReset => AllowPasswordReset;

        /// <summary>
        /// A boolean indicating that by default the email address will be the username
        /// </summary>
        /// <remarks>
        /// Even if this is true and the username is different from the email in the database, the username field will still be shown.
        /// When this is false, the username and email fields will be shown in the user section.
        /// </remarks>
        bool ISecuritySection.UsernameIsEmail => UsernameIsEmail;

        string ISecuritySection.AuthCookieName => AuthCookieName;

        string ISecuritySection.AuthCookieDomain => AuthCookieDomain;

        IContentSecurityPolicySection ISecuritySection.ContentSecurityPolicy => ContentSecurityPolicy;
    }
}
