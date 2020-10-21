namespace Umbraco.Core.Configuration.Models
{
    public class SecuritySettings
    {
        public bool KeepUserLoggedIn { get; set; } = false;

        public bool HideDisabledUsersInBackOffice { get; set; } = false;

        public bool AllowPasswordReset { get; set; } = true;

        public string AuthCookieName { get; set; } = "UMB_UCONTEXT";

        public string AuthCookieDomain { get; set; }

        public bool UsernameIsEmail { get; set; } = true;

        public UserPasswordConfigurationSettings UserPassword { get; set; }

        public MemberPasswordConfigurationSettings MemberPassword { get; set; }
    }
}
