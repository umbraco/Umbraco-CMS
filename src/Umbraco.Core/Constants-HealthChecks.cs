namespace Umbraco.Cms.Core;

/// <summary>
///     Defines constants.
/// </summary>
public static partial class Constants
{
    /// <summary>
    ///     Defines constants for health checks.
    /// </summary>
    public static class HealthChecks
    {
        /// <summary>
        ///     Contains route path constants for health checks.
        /// </summary>
        public static class RoutePath
        {
            /// <summary>
            ///     The route path segment for health check endpoints.
            /// </summary>
            public const string HealthCheck = "health-check";
        }

        /// <summary>
        ///     Contains documentation link constants for health checks.
        /// </summary>
        public static class DocumentationLinks
        {
            /// <summary>
            ///     The documentation link for SMTP health check.
            /// </summary>
            public const string SmtpCheck = "https://umbra.co/healthchecks-smtp";

            /// <summary>
            ///     Contains documentation links for live environment health checks.
            /// </summary>
            public static class LiveEnvironment
            {
                /// <summary>
                ///     The documentation link for compilation debug check.
                /// </summary>
                public const string CompilationDebugCheck = "https://umbra.co/healthchecks-compilation-debug";

                /// <summary>
                ///     The documentation link for runtime mode check.
                /// </summary>
                public const string RuntimeModeCheck = "https://docs.umbraco.com/umbraco-cms/fundamentals/setup/server-setup/runtime-modes";
            }

            /// <summary>
            ///     Contains documentation links for configuration health checks.
            /// </summary>
            public static class Configuration
            {
                /// <summary>
                ///     The documentation link for IIS custom errors check.
                /// </summary>
                public const string TrySkipIisCustomErrorsCheck =
                    "https://umbra.co/healthchecks-skip-iis-custom-errors";

                /// <summary>
                ///     The documentation link for notification email check.
                /// </summary>
                public const string NotificationEmailCheck = "https://umbra.co/healthchecks-notification-email";
            }

            /// <summary>
            ///     Contains documentation links for folder and file permission health checks.
            /// </summary>
            public static class FolderAndFilePermissionsCheck
            {
                /// <summary>
                ///     The documentation link for file writing check.
                /// </summary>
                public const string FileWriting = "https://umbra.co/healthchecks-file-writing";

                /// <summary>
                ///     The documentation link for folder creation check.
                /// </summary>
                public const string FolderCreation = "https://umbra.co/healthchecks-folder-creation";

                /// <summary>
                ///     The documentation link for file writing for packages check.
                /// </summary>
                public const string FileWritingForPackages = "https://umbra.co/healthchecks-file-writing-for-packages";

                /// <summary>
                ///     The documentation link for media folder creation check.
                /// </summary>
                public const string MediaFolderCreation = "https://umbra.co/healthchecks-media-folder-creation";
            }

            /// <summary>
            ///     Contains documentation links for security health checks.
            /// </summary>
            public static class Security
            {
                /// <summary>
                ///     The documentation link for Umbraco application URL check.
                /// </summary>
                public const string UmbracoApplicationUrlCheck =
                    "https://umbra.co/healthchecks-umbraco-application-url";

                /// <summary>
                ///     The documentation link for click jacking check.
                /// </summary>
                public const string ClickJackingCheck = "https://umbra.co/healthchecks-click-jacking";

                /// <summary>
                ///     The documentation link for HSTS check.
                /// </summary>
                public const string HstsCheck = "https://umbra.co/healthchecks-hsts";

                /// <summary>
                ///     The documentation link for no-sniff check.
                /// </summary>
                public const string NoSniffCheck = "https://umbra.co/healthchecks-no-sniff";

                /// <summary>
                ///     The documentation link for excessive headers check.
                /// </summary>
                public const string ExcessiveHeadersCheck = "https://umbra.co/healthchecks-excessive-headers";

                /// <summary>
                ///     The documentation link for CSP header check.
                /// </summary>
                public const string CspHeaderCheck = "https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP";

                /// <summary>
                ///     Contains documentation links for HTTPS health checks.
                /// </summary>
                public static class HttpsCheck
                {
                    /// <summary>
                    ///     The documentation link for HTTPS request scheme check.
                    /// </summary>
                    public const string CheckIfCurrentSchemeIsHttps = "https://umbra.co/healthchecks-https-request";

                    /// <summary>
                    ///     The documentation link for HTTPS configuration setting check.
                    /// </summary>
                    public const string CheckHttpsConfigurationSetting = "https://umbra.co/healthchecks-https-config";

                    /// <summary>
                    ///     The documentation link for valid certificate check.
                    /// </summary>
                    public const string CheckForValidCertificate = "https://umbra.co/healthchecks-valid-certificate";
                }
            }
        }
    }
}
