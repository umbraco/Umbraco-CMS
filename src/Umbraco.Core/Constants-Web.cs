using System;
using System.ComponentModel;

namespace Umbraco.Core
{
	public static partial class Constants
	{		
        /// <summary>
        /// Defines the identifiers for Umbraco system nodes.
        /// </summary>
        public static class Web
        {
            public const string UmbracoContextDataToken = "umbraco-context";
            public const string UmbracoDataToken = "umbraco";
            public const string PublishedDocumentRequestDataToken = "umbraco-doc-request";
            public const string CustomRouteDataToken = "umbraco-custom-route";
            public const string UmbracoRouteDefinitionDataToken = "umbraco-route-def";

            /// <summary>
            /// The preview cookie name
            /// </summary>
            public const string PreviewCookieName = "UMB_PREVIEW";

            /// <summary>
            /// The auth cookie name
            /// </summary>
            [Obsolete("DO NOT USE THIS, USE ISecuritySection.AuthCookieName, this will be removed in future versions")]
            [EditorBrowsable(EditorBrowsableState.Never)]
            public const string AuthCookieName = "UMB_UCONTEXT";

        }
        
	}
}