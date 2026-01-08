using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;

internal class UmbracoCmsSchema
{
    public required UmbracoDefinition Umbraco { get; set; }

    /// <summary>
    /// Configuration container for all Umbraco products.
    /// </summary>
    public class UmbracoDefinition
    {
        public required UmbracoCmsDefinition CMS { get; set; }
    }

    /// <summary>
    /// Configuration of Umbraco CMS.
    /// </summary>
    public class UmbracoCmsDefinition
    {
        public required ContentSettings Content { get; set; }

        public required DeliveryApiSettings DeliveryApi { get; set; }

        public required CoreDebugSettings Debug { get; set; }

        public required ExceptionFilterSettings ExceptionFilter { get; set; }

        public required ModelsBuilderSettings ModelsBuilder { get; set; }

        public required GlobalSettings Global { get; set; }

        public required HealthChecksSettings HealthChecks { get; set; }

        public required HostingSettings Hosting { get; set; }

        public required ImagingSettings Imaging { get; set; }

        public required IndexCreatorSettings Examine { get; set; }

        public required IndexingSettings Indexing { get; set; }

        public required LoggingSettings Logging { get; set; }

        public required NuCacheSettings NuCache { get; set; }

        public required RequestHandlerSettings RequestHandler { get; set; }

        public required RuntimeSettings Runtime { get; set; }

        public required SecuritySettings Security { get; set; }

        public required TypeFinderSettings TypeFinder { get; set; }

        public required WebRoutingSettings WebRouting { get; set; }

        public required UmbracoPluginSettings Plugins { get; set; }

        public required UnattendedSettings Unattended { get; set; }

        [Obsolete("Runtime minification is no longer supported. Will be removed entirely in V16.")]
        public required RuntimeMinificationSettings RuntimeMinification { get; set; }

        public required BasicAuthSettings BasicAuth { get; set; }

        public required PackageMigrationSettings PackageMigration { get; set; }

        public required LegacyPasswordMigrationSettings LegacyPasswordMigration { get; set; }

        [Obsolete("Scheduled for removal in v16, dashboard manipulation is now done trough frontend extensions.")]
        public required ContentDashboardSettings ContentDashboard { get; set; }

        public required HelpPageSettings HelpPage { get; set; }

        public required InstallDefaultDataNamedOptions InstallDefaultData { get; set; }

        public required DataTypesSettings DataTypes { get; set; }

        public required MarketplaceSettings Marketplace { get; set; }

        public required WebhookSettings Webhook { get; set; }

        public required CacheSettings Cache { get; set; }
    }

    public class InstallDefaultDataNamedOptions
    {
        public required InstallDefaultDataSettings Languages { get; set; }

        public required InstallDefaultDataSettings DataTypes { get; set; }

        public required InstallDefaultDataSettings MediaTypes { get; set; }

        public required InstallDefaultDataSettings MemberTypes { get; set; }
    }
}
