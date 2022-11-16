using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

internal class UmbracoCmsSchema
{
    public UmbracoDefinition? Umbraco { get; set; }

    /// <summary>
    /// Configuration container for all Umbraco products.
    /// </summary>
    public class UmbracoDefinition
    {
        public UmbracoCmsDefinition? CMS { get; set; }
    }

    /// <summary>
    /// Configuration of Umbraco CMS.
    /// </summary>
    public class UmbracoCmsDefinition
    {
        public ContentSettings? Content { get; set; }

        public CoreDebugSettings? Debug { get; set; }

        public ExceptionFilterSettings? ExceptionFilter { get; set; }

        public ModelsBuilderSettings? ModelsBuilder { get; set; }

        public GlobalSettings? Global { get; set; }

        public HealthChecksSettings? HealthChecks { get; set; }

        public HostingSettings? Hosting { get; set; }

        public ImagingSettings? Imaging { get; set; }

        public IndexCreatorSettings? Examine { get; set; }

        public KeepAliveSettings? KeepAlive { get; set; }

        public LoggingSettings? Logging { get; set; }

        public NuCacheSettings? NuCache { get; set; }

        public RequestHandlerSettings? RequestHandler { get; set; }

        public RuntimeSettings? Runtime { get; set; }

        public SecuritySettings? Security { get; set; }

        public TourSettings? Tours { get; set; }

        public TypeFinderSettings? TypeFinder { get; set; }

        public WebRoutingSettings? WebRouting { get; set; }

        public UmbracoPluginSettings? Plugins { get; set; }

        public UnattendedSettings? Unattended { get; set; }

        public RichTextEditorSettings? RichTextEditor { get; set; }

        public RuntimeMinificationSettings? RuntimeMinification { get; set; }

        public BasicAuthSettings? BasicAuth { get; set; }

        public PackageMigrationSettings? PackageMigration { get; set; }

        public LegacyPasswordMigrationSettings? LegacyPasswordMigration { get; set; }

        public ContentDashboardSettings? ContentDashboard { get; set; }

        public HelpPageSettings? HelpPage { get; set; }

        public InstallDefaultDataSettings? DefaultDataCreation { get; set; }

        public DataTypesSettings? DataTypes { get; set; }
    }
}
