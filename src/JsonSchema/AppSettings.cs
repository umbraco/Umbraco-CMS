using Umbraco.Cms.Core.Configuration.Models;

namespace JsonSchema
{
    public class AppSettings
    {
        public UmbracoDefinition Umbraco { get; set; }

        /// <summary>
        /// Configuration of Umbraco CMS and packages
        /// </summary>
        public class UmbracoDefinition
        {
            public CmsDefinition CMS { get; set; }
            public FormsDefinition Forms { get; set; }
            public DeployDefinition Deploy { get; set; }

            /// <summary>
            /// Configurations for the Umbraco CMS
            /// </summary>
            public class CmsDefinition
            {
                public ActiveDirectorySettings ActiveDirectory { get; set; }
                public ContentSettings Content { get; set; }
                public CoreDebugSettings CoreDebug { get; set; }
                public ExceptionFilterSettings ExceptionFilter { get; set; }
                public ModelsBuilderSettings ModelsBuilder { get; set; }
                public GlobalSettings Global { get; set; }
                public HealthChecksSettings HealthChecks { get; set; }
                public HostingSettings Hosting { get; set; }
                public ImagingSettings Imaging { get; set; }
                public IndexCreatorSettings Examine { get; set; }
                public KeepAliveSettings KeepAlive { get; set; }
                public LoggingSettings Logging { get; set; }
                public MemberPasswordConfigurationSettings MemberPassword { get; set; }
                public NuCacheSettings NuCache { get; set; }
                public RequestHandlerSettings RequestHandler { get; set; }
                public RuntimeSettings Runtime { get; set; }
                public SecuritySettings Security { get; set; }
                public TourSettings Tours { get; set; }
                public TypeFinderSettings TypeFinder { get; set; }
                public UserPasswordConfigurationSettings UserPassword { get; set; }
                public WebRoutingSettings WebRouting { get; set; }
                public UmbracoPluginSettings Plugins { get; set; }
                public UnattendedSettings Unattended { get; set; }
                public RichTextEditorSettings RichTextEditor { get; set; }
                public RuntimeMinificationSettings RuntimeMinification { get; set; }
            }

            /// <summary>
            /// Configurations for the Umbraco Forms package to Umbraco CMS
            /// </summary>
            public class FormsDefinition
            {

            }

            /// <summary>
            /// Configurations for the Umbraco Deploy package to Umbraco CMS
            /// </summary>
            public class DeployDefinition
            {

            }
        }
    }
}
