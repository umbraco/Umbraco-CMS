﻿namespace Umbraco.Core
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

            public const string InstallerCookieName = "umb_installId";

            /// <summary>
            /// The cookie name that is used to store the validation value
            /// </summary>
            public const string CsrfValidationCookieName = "UMB-XSRF-V";

            /// <summary>
            /// The cookie name that is set for angular to use to pass in to the header value for "X-UMB-XSRF-TOKEN"
            /// </summary>
            public const string AngularCookieName = "UMB-XSRF-TOKEN";

            /// <summary>
            /// The header name that angular uses to pass in the token to validate the cookie
            /// </summary>
            public const string AngularHeadername = "X-UMB-XSRF-TOKEN";
        }
    }
}
