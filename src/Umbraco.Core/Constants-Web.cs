namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Defines the identifiers for Umbraco system nodes.
    /// </summary>
    public static class Web
    {
        /// <summary>
        ///     The preview cookie name
        /// </summary>
        public const string PreviewCookieName = "UMB_PREVIEW";

        /// <summary>
        ///     Client-side cookie that determines whether the user has accepted to be in Preview Mode when visiting the website.
        /// </summary>
        public const string AcceptPreviewCookieName = "UMB-WEBSITE-PREVIEW-ACCEPT";

        public const string InstallerCookieName = "umb_installId";

        /// <summary>
        ///     The cookie name that is used to store the validation value
        /// </summary>
        public const string CsrfValidationCookieName = "UMB-XSRF-V";

        /// <summary>
        ///     The route name of the page shown when Umbraco has no published content.
        /// </summary>
        public const string NoContentRouteName = "umbraco-no-content";

        /// <summary>
        ///     The default authentication type used for remembering that 2FA is not needed on next login
        /// </summary>
        public const string TwoFactorRememberBrowserCookie = "TwoFactorRememberBrowser";

        /// <summary>
        ///     The token used to replace the cache buster hash in web assets.
        /// </summary>
        public const string CacheBusterToken = "%CACHE_BUSTER%";

        public static class Mvc
        {
            public const string InstallArea = "UmbracoInstall";

            public const string
                BackOfficePathSegment = "BackOffice"; // The path segment prefix for all back office controllers

            public const string BackOfficeArea = "UmbracoBackOffice"; // Used for area routes of non-api controllers
            public const string BackOfficeApiArea = "UmbracoApi"; // Same name as v8 so all routing remains the same
            public const string BackOfficeTreeArea = "UmbracoTrees"; // Same name as v8 so all routing remains the same
            public const string BackOfficeLoginArea = "UmbracoLogin"; // Used for area routes of non-api controllers for login
        }

        /// <summary>
        ///     The "base" path to the Management API
        /// </summary>
        public const string ManagementApiPath = "/management/api/";
        public const string BackofficeSignalRHub = "/backofficeHub";
        public const string ServerEventSignalRHub = "/serverEventHub";

        public static class Routing
        {
            public const string ControllerToken = "controller";
            public const string ActionToken = "action";
            public const string AreaToken = "area";
            public const string DynamicRoutePattern = "/{**umbracoSlug}";
        }

        public static class RoutePath
        {
            public const string Tree = "tree";
            public const string RecycleBin = "recycle-bin";
            public const string Item = "item";
            public const string Collection = "collection";
            public const string Filter = "filter";
        }

        public static class AttributeRouting
        {
            public const string BackOfficeToken = "umbracoBackOffice";
        }

        public static class EmailTypes
        {
            public const string HealthCheck = "HealthCheck";
            public const string Notification = "Notification";
            public const string PasswordReset = "PasswordReset";
            public const string TwoFactorAuth = "2FA";
            public const string UserInvite = "UserInvite";
        }
    }
}
