namespace Umbraco.Cms.Core;

/// <summary>
///     Defines constants.
/// </summary>
public static partial class Constants
{
    /// <summary>
    ///     Defines constants for ModelsBuilder.
    /// </summary>
    public static class HealthChecks
    {
        public static class DocumentationLinks
        {
            public const string SmtpCheck = "https://umbra.co/healthchecks-smtp";

            public static class LiveEnvironment
            {
                public const string CompilationDebugCheck = "https://umbra.co/healthchecks-compilation-debug";
            }

            public static class Configuration
            {
                public const string MacroErrorsCheck = "https://umbra.co/healthchecks-macro-errors";

                public const string TrySkipIisCustomErrorsCheck =
                    "https://umbra.co/healthchecks-skip-iis-custom-errors";

                public const string NotificationEmailCheck = "https://umbra.co/healthchecks-notification-email";
            }

            public static class FolderAndFilePermissionsCheck
            {
                public const string FileWriting = "https://umbra.co/healthchecks-file-writing";
                public const string FolderCreation = "https://umbra.co/healthchecks-folder-creation";
                public const string FileWritingForPackages = "https://umbra.co/healthchecks-file-writing-for-packages";
                public const string MediaFolderCreation = "https://umbra.co/healthchecks-media-folder-creation";
            }

            public static class Security
            {
                public const string UmbracoApplicationUrlCheck =
                    "https://umbra.co/healthchecks-umbraco-application-url";

                public const string ClickJackingCheck = "https://umbra.co/healthchecks-click-jacking";
                public const string HstsCheck = "https://umbra.co/healthchecks-hsts";
                public const string NoSniffCheck = "https://umbra.co/healthchecks-no-sniff";
                public const string XssProtectionCheck = "https://umbra.co/healthchecks-xss-protection";
                public const string ExcessiveHeadersCheck = "https://umbra.co/healthchecks-excessive-headers";

                public static class HttpsCheck
                {
                    public const string CheckIfCurrentSchemeIsHttps = "https://umbra.co/healthchecks-https-request";
                    public const string CheckHttpsConfigurationSetting = "https://umbra.co/healthchecks-https-config";
                    public const string CheckForValidCertificate = "https://umbra.co/healthchecks-valid-certificate";
                }
            }
        }
    }
}
