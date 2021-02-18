// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for security settings.
    /// </summary>
    public class SecuritySettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to keep the user logged in.
        /// </summary>
        public bool KeepUserLoggedIn { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to hide disabled users in the back-office.
        /// </summary>
        public bool HideDisabledUsersInBackOffice { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to allow user password reset.
        /// </summary>
        public bool AllowPasswordReset { get; set; } = true;

        /// <summary>
        /// Gets or sets a value for the authorization cookie name.
        /// </summary>
        public string AuthCookieName { get; set; } = "UMB_UCONTEXT";

        /// <summary>
        /// Gets or sets a value for the authorization cookie domain.
        /// </summary>
        public string AuthCookieDomain { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's email address is to be considered as their username.
        /// </summary>
        public bool UsernameIsEmail { get; set; } = true;

        /// <summary>
        /// Gets or sets a value for the user password settings.
        /// </summary>
        public UserPasswordConfigurationSettings UserPassword { get; set; }

        /// <summary>
        /// Gets or sets a value for the member password settings.
        /// </summary>
        public MemberPasswordConfigurationSettings MemberPassword { get; set; }
    }
}
