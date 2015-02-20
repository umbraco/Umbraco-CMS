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

	        public const string StartContentNodeIdClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/startcontentnode";
            public const string StartMediaNodeIdClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/startmedianode";
            public const string AllowedApplicationsClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/allowedapps";
            //public const string UserIdClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/userid";
            public const string CultureClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/culture";
            public const string SessionIdClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/sessionid";

	    }
	}
}