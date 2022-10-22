// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Deploy.Core.Configuration.DebugConfiguration;
using Umbraco.Deploy.Core.Configuration.DeployConfiguration;
using Umbraco.Deploy.Core.Configuration.DeployProjectConfiguration;
using Umbraco.Forms.Core.Configuration;
using SecuritySettings = Umbraco.Cms.Core.Configuration.Models.SecuritySettings;

namespace JsonSchema
{
    internal class AppSettings
    {
        /// <summary>
        ///     Gets or sets the Umbraco
        /// </summary>
        public UmbracoDefinition? Umbraco { get; set; }

        /// <summary>
        ///     Configuration of Umbraco CMS and packages
        /// </summary>
        internal class UmbracoDefinition
        {
            // ReSharper disable once InconsistentNaming
            public CmsDefinition? CMS { get; set; }

            public FormsDefinition? Forms { get; set; }

            public DeployDefinition? Deploy { get; set; }

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

            /// <summary>
            ///     Configurations for the Umbraco Forms package to Umbraco CMS
            /// </summary>
            public class FormsDefinition
            {
                public FormDesignSettings? FormDesign { get; set; }

                public PackageOptionSettings? Options { get; set; }

                public Umbraco.Forms.Core.Configuration.SecuritySettings? Security { get; set; }

                public FieldTypesDefinition? FieldTypes { get; set; }

                /// <summary>
                ///     Configurations for the Umbraco Forms Field Types
                /// </summary>
                public class FieldTypesDefinition
                {
                    public DatePickerSettings? DatePicker { get; set; }

                    public Recaptcha2Settings? Recaptcha2 { get; set; }

                    public Recaptcha3Settings? Recaptcha3 { get; set; }
                }
            }

            /// <summary>
            ///     Configurations for the Umbraco Deploy package to Umbraco CMS
            /// </summary>
            public class DeployDefinition
            {
                public DeploySettings? Settings { get; set; }

                public DeployProjectConfig? Project { get; set; }

                public DebugSettings? Debug { get; set; }
            }
        }
    }
}
