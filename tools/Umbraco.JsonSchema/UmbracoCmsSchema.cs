using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

internal class UmbracoCmsSchema
{
    public UmbracoDefinition Umbraco { get; set; } = null!;

    /// <summary>
    /// Configuration container for all Umbraco products.
    /// </summary>
    public class UmbracoDefinition
    {
        public UmbracoCmsDefinition CMS { get; set; } = null!;
    }

    /// <summary>
    /// Configuration of Umbraco CMS.
    /// </summary>
    public class UmbracoCmsDefinition
    {
        public ContentSettings Content { get; set; } = null!;

        public DeliveryApiSettings DeliveryApi { get; set; } = null!;

        public CoreDebugSettings Debug { get; set; } = null!;

        public ExceptionFilterSettings ExceptionFilter { get; set; } = null!;

        public ModelsBuilderSettings ModelsBuilder { get; set; } = null!;

        public GlobalSettings Global { get; set; } = null!;

        public HealthChecksSettings HealthChecks { get; set; } = null!;

        public HostingSettings Hosting { get; set; } = null!;

        public ImagingSettings Imaging { get; set; } = null!;

        public IndexCreatorSettings Examine { get; set; } = null!;
        public IndexingSettings Indexing { get; set; } = null!;

        public KeepAliveSettings KeepAlive { get; set; } = null!;

        public LoggingSettings Logging { get; set; } = null!;

        public NuCacheSettings NuCache { get; set; } = null!;

        public RequestHandlerSettings RequestHandler { get; set; } = null!;

        public RuntimeSettings Runtime { get; set; } = null!;

        public SecuritySettings Security { get; set; } = null!;

        public TourSettings Tours { get; set; } = null!;

        public TypeFinderSettings TypeFinder { get; set; } = null!;

        public WebRoutingSettings WebRouting { get; set; } = null!;

        public UmbracoPluginSettings Plugins { get; set; } = null!;

        public UnattendedSettings Unattended { get; set; } = null!;

        public RichTextEditorSettings RichTextEditor { get; set; } = null!;

        public RuntimeMinificationSettings RuntimeMinification { get; set; } = null!;

        public BasicAuthSettings BasicAuth { get; set; } = null!;

        public PackageMigrationSettings PackageMigration { get; set; } = null!;

        public LegacyPasswordMigrationSettings LegacyPasswordMigration { get; set; } = null!;

        public ContentDashboardSettings ContentDashboard { get; set; } = null!;

        public HelpPageSettings HelpPage { get; set; } = null!;

        public InstallDefaultDataSettings DefaultDataCreation { get; set; } = null!;

        public DataTypesSettings DataTypes { get; set; } = null!;

        public MarketplaceSettings Marketplace { get; set; } = null!;

        public WebhookSettings Webhook { get; set; } = null!;
    }
}
