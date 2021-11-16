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

            /// Client-side cookie that determines whether the user has accepted to be in Preview Mode when visiting the website.
            /// </summary>
            public const string AcceptPreviewCookieName = "UMB-WEBSITE-PREVIEW-ACCEPT";

            public const string InstallerCookieName = "umb_installId";
        }
    }
}
