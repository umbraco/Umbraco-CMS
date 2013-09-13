namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ISecurity
    {
        bool KeepUserLoggedIn { get; }

        bool HideDisabledUsersInBackoffice { get; }

        string AuthCookieName { get; }

        string AuthCookieDomain { get; }
    }
}