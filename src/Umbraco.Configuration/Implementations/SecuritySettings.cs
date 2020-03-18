using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Implementations
{
    internal class SecuritySettings : ConfigurationManagerConfigBase, ISecuritySettings
    {
        public bool KeepUserLoggedIn => UmbracoSettingsSection.Security.KeepUserLoggedIn;
        public bool HideDisabledUsersInBackoffice => UmbracoSettingsSection.Security.HideDisabledUsersInBackoffice;
        public bool AllowPasswordReset => UmbracoSettingsSection.Security.AllowPasswordReset;
        public string AuthCookieName => UmbracoSettingsSection.Security.AuthCookieName;
        public string AuthCookieDomain => UmbracoSettingsSection.Security.AuthCookieDomain;
        public bool UsernameIsEmail => UmbracoSettingsSection.Security.UsernameIsEmail;
    }
}
