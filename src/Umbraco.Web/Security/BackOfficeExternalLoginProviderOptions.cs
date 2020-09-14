using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Umbraco.Web.Security
{

    /// <summary>
    /// Options used to configure back office external login providers
    /// </summary>
    public class BackOfficeExternalLoginProviderOptions
    {
        /// <summary>
        /// When specified this will be called to retrieve the <see cref="AuthenticationProperties"/> used during the authentication Challenge response.
        /// </summary>
        /// <remarks>
        /// This will generally not be needed since OpenIdConnect.RedirectToIdentityProvider options should be used instead
        /// </remarks>
        [IgnoreDataMember]
        public Func<IOwinContext, AuthenticationProperties> OnChallenge { get; set; }

        /// <summary>
        /// Options used to control how users can be auto-linked/created/updated based on the external login provider
        /// </summary>
        [IgnoreDataMember] // we are ignoring this one from serialization for backwards compat since these options are manually incuded in the response separately
        public ExternalSignInAutoLinkOptions AutoLinkOptions { get; set; } = new ExternalSignInAutoLinkOptions();

        /// <summary>
        /// When set to true will disable all local user login functionality
        /// </summary>
        public bool DenyLocalLogin { get; set; }

        /// <summary>
        /// When specified this will automatically redirect to the OAuth login provider instead of prompting the user to click on the OAuth button first.
        /// </summary>
        /// <remarks>
        /// This is generally used in conjunction with <see cref="DenyLocalLogin"/>. If more than one OAuth provider specifies this, the last registered
        /// provider's redirect settings will win.
        /// </remarks>
        public bool AutoRedirectLoginToExternalProvider { get; set; }

        /// <summary>
        /// A virtual path to a custom angular view that is used to replace the entire UI that renders the external login button that the user interacts with
        /// </summary>
        /// <remarks>
        /// If this view is specified it is 100% up to the user to render the html responsible for rendering the link/un-link buttons along with showing any errors
        /// that occur. This overrides what Umbraco normally does by default.
        /// </remarks>
        public string CustomBackOfficeView { get; set; }
    }
}
