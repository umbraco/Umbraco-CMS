using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Models
{
    internal class SecuritySettings : ISecuritySettings
    {
        private const string Prefix = Constants.Configuration.ConfigSecurityPrefix;
        private readonly IConfiguration _configuration;

        public SecuritySettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool KeepUserLoggedIn => _configuration.GetValue(Prefix + "KeepUserLoggedIn", false);

        public bool HideDisabledUsersInBackoffice =>
            _configuration.GetValue(Prefix + "HideDisabledUsersInBackoffice", false);

        public bool AllowPasswordReset =>
            _configuration.GetValue(Prefix + "AllowPasswordResetAllowPasswordReset", true);

        public string AuthCookieName =>
            _configuration.GetValue(Prefix + "AuthCookieName", "UMB_UCONTEXT");

        public string AuthCookieDomain =>
            _configuration.GetValue<string>(Prefix + "AuthCookieDomain");

        public bool UsernameIsEmail => _configuration.GetValue(Prefix + "UsernameIsEmail", true);
    }
}
