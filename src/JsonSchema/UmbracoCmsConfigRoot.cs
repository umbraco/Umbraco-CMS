using Umbraco.Cms.Core.Configuration.Models;

namespace JsonSchema
{
    /// <summary>
    /// Configuration of Open Source .NET CMS - Umbraco
    /// </summary>
    public class UmbracoCmsConfigRoot
    {
        public Cms CMS { get; set; }


        /// <summary>
        /// Configurations for the Umbraco CMS
        /// </summary>
        public class Cms
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
        public class Forms
        {

        }
    }
}
