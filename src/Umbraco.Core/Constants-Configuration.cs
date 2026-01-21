namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains configuration key constants for Umbraco settings.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Case insensitive prefix for all configurations.
        /// </summary>
        /// <remarks>
        /// ":" is used as marker for nested objects in JSON, e.g. <c>"Umbraco:CMS:" = {"Umbraco":{"CMS":{...}}</c>.
        /// </remarks>
        public const string ConfigPrefix = "Umbraco:CMS:";

        /// <summary>
        ///     The configuration key prefix for content settings.
        /// </summary>
        public const string ConfigContentPrefix = ConfigPrefix + "Content:";

        /// <summary>
        ///     The configuration key prefix for content notification settings.
        /// </summary>
        public const string ConfigContentNotificationsPrefix = ConfigContentPrefix + "Notifications:";

        /// <summary>
        ///     The configuration key prefix for core settings.
        /// </summary>
        public const string ConfigCorePrefix = ConfigPrefix + "Core:";

        /// <summary>
        ///     The configuration key prefix for custom error settings.
        /// </summary>
        public const string ConfigCustomErrorsPrefix = ConfigPrefix + "CustomErrors:";

        /// <summary>
        ///     The configuration key prefix for global settings.
        /// </summary>
        public const string ConfigGlobalPrefix = ConfigPrefix + "Global:";

        /// <summary>
        ///     The configuration key for the global ID setting.
        /// </summary>
        public const string ConfigGlobalId = ConfigGlobalPrefix + "Id";

        /// <summary>
        ///     The configuration key for the distributed locking mechanism setting.
        /// </summary>
        public const string ConfigGlobalDistributedLockingMechanism = ConfigGlobalPrefix + "DistributedLockingMechanism";

        /// <summary>
        ///     The configuration key prefix for hosting settings.
        /// </summary>
        public const string ConfigHostingPrefix = ConfigPrefix + "Hosting:";

        /// <summary>
        ///     The configuration key prefix for ModelsBuilder settings.
        /// </summary>
        public const string ConfigModelsBuilderPrefix = ConfigPrefix + "ModelsBuilder:";

        /// <summary>
        ///     The configuration key prefix for security settings.
        /// </summary>
        public const string ConfigSecurityPrefix = ConfigPrefix + "Security:";

        /// <summary>
        ///     The configuration key for content notification email settings.
        /// </summary>
        public const string ConfigContentNotificationsEmail = ConfigContentNotificationsPrefix + "Email";

        /// <summary>
        ///     The configuration key for the global HTTPS setting.
        /// </summary>
        public const string ConfigGlobalUseHttps = ConfigGlobalPrefix + "UseHttps";

        /// <summary>
        ///     The configuration key for the hosting debug setting.
        /// </summary>
        public const string ConfigHostingDebug = ConfigHostingPrefix + "Debug";

        /// <summary>
        ///     The configuration key for the custom errors mode setting.
        /// </summary>
        public const string ConfigCustomErrorsMode = ConfigCustomErrorsPrefix + "Mode";

        /// <summary>
        ///     The configuration key for Active Directory settings.
        /// </summary>
        public const string ConfigActiveDirectory = ConfigPrefix + "ActiveDirectory";

        /// <summary>
        ///     The configuration key for Marketplace settings.
        /// </summary>
        public const string ConfigMarketplace = ConfigPrefix + "Marketplace";

        /// <summary>
        ///     The configuration key for legacy password migration settings.
        /// </summary>
        public const string ConfigLegacyPasswordMigration = ConfigPrefix + "LegacyPasswordMigration";

        /// <summary>
        ///     The configuration key for system date migration settings.
        /// </summary>
        public const string ConfigSystemDateMigration = ConfigPrefix + "SystemDateMigration";

        /// <summary>
        ///     The configuration key for content settings.
        /// </summary>
        public const string ConfigContent = ConfigPrefix + "Content";

        /// <summary>
        ///     The configuration key for Delivery API settings.
        /// </summary>
        public const string ConfigDeliveryApi = ConfigPrefix + "DeliveryApi";

        /// <summary>
        ///     The configuration key for core debug settings.
        /// </summary>
        public const string ConfigCoreDebug = ConfigCorePrefix + "Debug";

        /// <summary>
        ///     The configuration key for exception filter settings.
        /// </summary>
        public const string ConfigExceptionFilter = ConfigPrefix + "ExceptionFilter";

        /// <summary>
        ///     The configuration key for global settings.
        /// </summary>
        public const string ConfigGlobal = ConfigPrefix + "Global";

        /// <summary>
        ///     The configuration key for unattended installation settings.
        /// </summary>
        public const string ConfigUnattended = ConfigPrefix + "Unattended";

        /// <summary>
        ///     The configuration key for health check settings.
        /// </summary>
        public const string ConfigHealthChecks = ConfigPrefix + "HealthChecks";

        /// <summary>
        ///     The configuration key for hosting settings.
        /// </summary>
        public const string ConfigHosting = ConfigPrefix + "Hosting";

        /// <summary>
        ///     The configuration key for imaging settings.
        /// </summary>
        public const string ConfigImaging = ConfigPrefix + "Imaging";

        /// <summary>
        ///     The configuration key for Examine search settings.
        /// </summary>
        public const string ConfigExamine = ConfigPrefix + "Examine";

        /// <summary>
        ///     The configuration key for indexing settings.
        /// </summary>
        public const string ConfigIndexing = ConfigPrefix + "Indexing";

        /// <summary>
        ///     The configuration key for logging settings.
        /// </summary>
        public const string ConfigLogging = ConfigPrefix + "Logging";

        /// <summary>
        ///     The configuration key for long running operations settings.
        /// </summary>
        public const string ConfigLongRunningOperations = ConfigPrefix + "LongRunningOperations";

        /// <summary>
        ///     The configuration key for member password settings.
        /// </summary>
        public const string ConfigMemberPassword = ConfigPrefix + "Security:MemberPassword";

        /// <summary>
        ///     The configuration key for ModelsBuilder settings.
        /// </summary>
        public const string ConfigModelsBuilder = ConfigPrefix + "ModelsBuilder";

        /// <summary>
        ///     The configuration key for the ModelsBuilder mode setting.
        /// </summary>
        public const string ConfigModelsMode = ConfigModelsBuilder + ":ModelsMode";

        /// <summary>
        ///     The configuration key for NuCache settings.
        /// </summary>
        public const string ConfigNuCache = ConfigPrefix + "NuCache";

        /// <summary>
        ///     The configuration key for plugins settings.
        /// </summary>
        public const string ConfigPlugins = ConfigPrefix + "Plugins";

        /// <summary>
        ///     The configuration key for request handler settings.
        /// </summary>
        public const string ConfigRequestHandler = ConfigPrefix + "RequestHandler";

        /// <summary>
        ///     The configuration key for runtime settings.
        /// </summary>
        public const string ConfigRuntime = ConfigPrefix + "Runtime";

        /// <summary>
        ///     The configuration key for the runtime mode setting.
        /// </summary>
        public const string ConfigRuntimeMode = ConfigRuntime + ":Mode";

        /// <summary>
        ///     The configuration key for security settings.
        /// </summary>
        public const string ConfigSecurity = ConfigPrefix + "Security";

        /// <summary>
        ///     The configuration key for basic authentication settings.
        /// </summary>
        public const string ConfigBasicAuth = ConfigPrefix + "BasicAuth";

        /// <summary>
        ///     The configuration key for type finder settings.
        /// </summary>
        public const string ConfigTypeFinder = ConfigPrefix + "TypeFinder";

        /// <summary>
        ///     The configuration key for web routing settings.
        /// </summary>
        public const string ConfigWebRouting = ConfigPrefix + "WebRouting";

        /// <summary>
        ///     The configuration key for user password settings.
        /// </summary>
        public const string ConfigUserPassword = ConfigPrefix + "Security:UserPassword";

        /// <summary>
        ///     The configuration key for rich text editor settings.
        /// </summary>
        public const string ConfigRichTextEditor = ConfigPrefix + "RichTextEditor";

        /// <summary>
        ///     The configuration key for package migration settings.
        /// </summary>
        public const string ConfigPackageMigration = ConfigPrefix + "PackageMigration";

        /// <summary>
        ///     The configuration key for content dashboard settings.
        /// </summary>
        [Obsolete("No longer used in Umbraco. Scheduled to be removed in Umbraco 19.")]
        public const string ConfigContentDashboard = ConfigPrefix + "ContentDashboard";

        /// <summary>
        ///     The configuration key for help page settings.
        /// </summary>
        public const string ConfigHelpPage = ConfigPrefix + "HelpPage";

        /// <summary>
        ///     The configuration key for install default data settings.
        /// </summary>
        public const string ConfigInstallDefaultData = ConfigPrefix + "InstallDefaultData";

        /// <summary>
        ///     The configuration key for data types settings.
        /// </summary>
        public const string ConfigDataTypes = ConfigPrefix + "DataTypes";

        /// <summary>
        ///     The configuration key for package manifests settings.
        /// </summary>
        public const string ConfigPackageManifests = ConfigPrefix + "PackageManifests";

        /// <summary>
        ///     The configuration key for webhook settings.
        /// </summary>
        public const string ConfigWebhook = ConfigPrefix + "Webhook";

        /// <summary>
        ///     The configuration key for webhook payload type settings.
        /// </summary>
        public const string ConfigWebhookPayloadType = ConfigWebhook + ":PayloadType";

        /// <summary>
        ///     The configuration key for cache settings.
        /// </summary>
        public const string ConfigCache = ConfigPrefix + "Cache";

        /// <summary>
        ///     The configuration key for distributed jobs settings.
        /// </summary>
        public const string ConfigDistributedJobs = ConfigPrefix + "DistributedJobs";

        /// <summary>
        ///     The configuration key for backoffice token cookie settings.
        /// </summary>
        public const string ConfigBackOfficeTokenCookie = ConfigSecurity + ":BackOfficeTokenCookie";

        /// <summary>
        ///     Contains constants for named options used in configuration.
        /// </summary>
        public static class NamedOptions
        {
            /// <summary>
            ///     Contains named option constants for install default data configuration.
            /// </summary>
            public static class InstallDefaultData
            {
                /// <summary>
                ///     The named option for languages default data.
                /// </summary>
                public const string Languages = "Languages";

                /// <summary>
                ///     The named option for data types default data.
                /// </summary>
                public const string DataTypes = "DataTypes";

                /// <summary>
                ///     The named option for media types default data.
                /// </summary>
                public const string MediaTypes = "MediaTypes";

                /// <summary>
                ///     The named option for member types default data.
                /// </summary>
                public const string MemberTypes = "MemberTypes";
            }
        }
    }
}
