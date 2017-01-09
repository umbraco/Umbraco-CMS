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

        [ConfigurationProperty("allowPasswordReset")]
        internal InnerTextConfigurationElement<bool> AllowPasswordReset
        {
            get { return GetOptionalTextElement("allowPasswordReset", true); }
        }

        [ConfigurationProperty("authCookieName")]
        internal InnerTextConfigurationElement<string> AuthCookieName
        {
            get { return GetOptionalTextElement("authCookieName", Constants.Web.AuthCookieName); }
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

        bool ISecuritySection.AllowPasswordReset
        {
            get { return AllowPasswordReset; }
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