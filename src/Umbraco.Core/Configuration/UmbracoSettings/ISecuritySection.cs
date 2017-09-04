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
    }
}