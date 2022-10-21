// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

namespace JsonSchema;

internal class AppSettings
{
    // ReSharper disable once InconsistentNaming
    public CmsDefinition? CMS { get; set; }

    /// <summary>
    ///     Configurations for the Umbraco CMS
    /// </summary>
    public class CmsDefinition
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
