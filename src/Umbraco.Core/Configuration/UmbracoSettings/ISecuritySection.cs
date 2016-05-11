﻿namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface ISecuritySection : IUmbracoConfigurationSection
    {
        bool KeepUserLoggedIn { get; }

        bool HideDisabledUsersInBackoffice { get; }

        bool AllowPasswordReset { get; }

        string AuthCookieName { get; }

        string AuthCookieDomain { get; }
    }
}