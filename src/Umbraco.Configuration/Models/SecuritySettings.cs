using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Models
{
    internal class SecuritySettings : ISecuritySettings
    {
        private readonly IConfiguration _configuration;

        public SecuritySettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool KeepUserLoggedIn => _configuration.GetValue("Umbraco:CMS:Security:KeepUserLoggedIn", true);

        public bool HideDisabledUsersInBackoffice =>
            _configuration.GetValue("Umbraco:CMS:Security:HideDisabledUsersInBackoffice", false);

        public bool AllowPasswordReset =>
            _configuration.GetValue("Umbraco:CMS:Security:AllowPasswordResetAllowPasswordReset", true);

        public string AuthCookieName =>
            _configuration.GetValue("Umbraco:CMS:Security:AuthCookieName", "UMB_UCONTEXT");

        public string AuthCookieDomain =>
            _configuration.GetValue<string>("Umbraco:CMS:Security:AuthCookieDomain");

        public bool UsernameIsEmail => _configuration.GetValue("Umbraco:CMS:Security:UsernameIsEmail", true);
    }
}
