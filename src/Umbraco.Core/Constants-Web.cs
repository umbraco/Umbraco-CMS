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

        /// <summary>
        ///     The installer cookie name (obsolete).
        /// </summary>
        [Obsolete("InstallerCookieName is no longer used and will be removed in Umbraco 19.")]
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

        /// <summary>
        ///     Contains MVC area and path constants for Umbraco routing.
        /// </summary>
        public static class Mvc
        {
            /// <summary>
            ///     The MVC area name for the Umbraco installer.
            /// </summary>
            public const string InstallArea = "UmbracoInstall";

            /// <summary>
            ///     The path segment prefix for all backoffice controllers.
            /// </summary>
            public const string
                BackOfficePathSegment = "BackOffice"; // The path segment prefix for all back office controllers

            /// <summary>
            ///     The MVC area name for backoffice non-API controllers.
            /// </summary>
            public const string BackOfficeArea = "UmbracoBackOffice"; // Used for area routes of non-api controllers

            /// <summary>
            ///     The MVC area name for backoffice API controllers.
            /// </summary>
            public const string BackOfficeApiArea = "UmbracoApi"; // Same name as v8 so all routing remains the same

            /// <summary>
            ///     The MVC area name for backoffice tree controllers.
            /// </summary>
            public const string BackOfficeTreeArea = "UmbracoTrees"; // Same name as v8 so all routing remains the same

            /// <summary>
            ///     The MVC area name for backoffice login controllers.
            /// </summary>
            public const string BackOfficeLoginArea = "UmbracoLogin"; // Used for area routes of non-api controllers for login
        }

        /// <summary>
        ///     The "base" path to the Management API
        /// </summary>
        public const string ManagementApiPath = "/management/api/";

        /// <summary>
        ///     The SignalR hub path for backoffice real-time communication.
        /// </summary>
        public const string BackofficeSignalRHub = "/backofficeHub";

        /// <summary>
        ///     The SignalR hub path for server-sent events.
        /// </summary>
        public const string ServerEventSignalRHub = "/serverEventHub";

        /// <summary>
        ///     Contains routing token constants used in MVC routing.
        /// </summary>
        public static class Routing
        {
            /// <summary>
            ///     The route token name for the controller.
            /// </summary>
            public const string ControllerToken = "controller";

            /// <summary>
            ///     The route token name for the action.
            /// </summary>
            public const string ActionToken = "action";

            /// <summary>
            ///     The route token name for the area.
            /// </summary>
            public const string AreaToken = "area";

            /// <summary>
            ///     The dynamic route pattern used for Umbraco content routing.
            /// </summary>
            public const string DynamicRoutePattern = "/{**umbracoSlug}";
        }

        /// <summary>
        ///     Contains route path segment constants used in API routing.
        /// </summary>
        public static class RoutePath
        {
            /// <summary>
            ///     The route path segment for tree endpoints.
            /// </summary>
            public const string Tree = "tree";

            /// <summary>
            ///     The route path segment for recycle bin endpoints.
            /// </summary>
            public const string RecycleBin = "recycle-bin";

            /// <summary>
            ///     The route path segment for item endpoints.
            /// </summary>
            public const string Item = "item";

            /// <summary>
            ///     The route path segment for collection endpoints.
            /// </summary>
            public const string Collection = "collection";

            /// <summary>
            ///     The route path segment for filter endpoints.
            /// </summary>
            public const string Filter = "filter";
        }

        /// <summary>
        ///     Contains attribute routing constants.
        /// </summary>
        public static class AttributeRouting
        {
            /// <summary>
            ///     The backoffice route token used in attribute routing.
            /// </summary>
            public const string BackOfficeToken = "umbracoBackOffice";
        }

        /// <summary>
        ///     Contains email type constants used for categorizing outgoing emails.
        /// </summary>
        public static class EmailTypes
        {
            /// <summary>
            ///     The email type for health check notifications.
            /// </summary>
            public const string HealthCheck = "HealthCheck";

            /// <summary>
            ///     The email type for general notifications.
            /// </summary>
            public const string Notification = "Notification";

            /// <summary>
            ///     The email type for password reset emails.
            /// </summary>
            public const string PasswordReset = "PasswordReset";

            /// <summary>
            ///     The email type for two-factor authentication emails.
            /// </summary>
            public const string TwoFactorAuth = "2FA";

            /// <summary>
            ///     The email type for user invitation emails.
            /// </summary>
            public const string UserInvite = "UserInvite";
        }
    }
}
