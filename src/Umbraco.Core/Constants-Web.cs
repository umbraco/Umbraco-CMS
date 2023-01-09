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
        ///     The cookie name that is set for angular to use to pass in to the header value for "X-UMB-XSRF-TOKEN"
        /// </summary>
        public const string AngularCookieName = "UMB-XSRF-TOKEN";

        /// <summary>
        ///     The header name that angular uses to pass in the token to validate the cookie
        /// </summary>
        public const string AngularHeadername = "X-UMB-XSRF-TOKEN";

        /// <summary>
        ///     The route name of the page shown when Umbraco has no published content.
        /// </summary>
        public const string NoContentRouteName = "umbraco-no-content";

        /// <summary>
        ///     The default authentication type used for remembering that 2FA is not needed on next login
        /// </summary>
        public const string TwoFactorRememberBrowserCookie = "TwoFactorRememberBrowser";

        public static class Mvc
        {
            public const string InstallArea = "UmbracoInstall";

            public const string
                BackOfficePathSegment = "BackOffice"; // The path segment prefix for all back office controllers

            public const string BackOfficeArea = "UmbracoBackOffice"; // Used for area routes of non-api controllers
            public const string BackOfficeApiArea = "UmbracoApi"; // Same name as v8 so all routing remains the same
            public const string BackOfficeTreeArea = "UmbracoTrees"; // Same name as v8 so all routing remains the same
        }

        public static class Routing
        {
            public const string ControllerToken = "controller";
            public const string ActionToken = "action";
            public const string AreaToken = "area";
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
