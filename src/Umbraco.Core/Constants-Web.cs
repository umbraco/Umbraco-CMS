namespace Umbraco.Core
{
	public static partial class Constants
	{		
        /// <summary>
        /// Defines the identifiers for Umbraco system nodes.
        /// </summary>
        public static class Web
        {
            /// <summary>
            /// The preview cookie name
            /// </summary>
            public const string PreviewCookieName = "UMB_PREVIEW";

            /// <summary>
            /// The auth cookie name
            /// </summary>
            public const string AuthCookieName = "UMB_UCONTEXT";

        }

	    public static class Security
	    {

            public const string BackOfficeAuthenticationType = "UmbracoBackOffice";
	        public const string BackOfficeExternalAuthenticationType = "UmbracoExternalCookie";
            public const string BackOfficeExternalCookieName = "UMB_EXTLOGIN";
            public const string BackOfficeTokenAuthenticationType = "UmbracoBackOfficeToken";

            /// <summary>
            /// The prefix used for external identity providers for their authentication type
            /// </summary>
            /// <remarks>
            /// By default we don't want to interfere with front-end external providers and their default setup, for back office the 
            /// providers need to be setup differently and each auth type for the back office will be prefixed with this value
            /// </remarks>
            public const string BackOfficeExternalAuthenticationTypePrefix = "Umbraco.";

	        public const string StartContentNodeIdClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/startcontentnode";
            public const string StartMediaNodeIdClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/startmedianode";
            public const string AllowedApplicationsClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/allowedapp";
            public const string SessionIdClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/sessionid";

	    }
	}
}