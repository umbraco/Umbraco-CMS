using System.Collections.Generic;
using System.IO;
using System.Configuration;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Tests.TestHelpers
{
    public class SettingsForTests
    {
        // umbracoSettings

        /// <summary>
        /// Sets the umbraco settings singleton to the object specified
        /// </summary>
        /// <param name="settings"></param>
        public static void ConfigureSettings(IUmbracoSettingsSection settings)
        {
            UmbracoConfig.For.SetUmbracoSettings(settings);
        }

        /// <summary>
        /// Returns generated settings which can be stubbed to return whatever values necessary
        /// </summary>
        /// <returns></returns>
        public static IUmbracoSettingsSection GenerateMockSettings()
        {
            var settings = new Mock<IUmbracoSettingsSection>();

            var content = new Mock<IContentSection>();
            var security = new Mock<ISecuritySection>();
            var requestHandler = new Mock<IRequestHandlerSection>();
            var templates = new Mock<ITemplatesSection>();
            var dev = new Mock<IDeveloperSection>();
            var viewStateMover = new Mock<IViewStateMoverModuleSection>();
            var logging = new Mock<ILoggingSection>();
            var tasks = new Mock<IScheduledTasksSection>();
            var distCall = new Mock<IDistributedCallSection>();
            var repos = new Mock<IRepositoriesSection>();
            var providers = new Mock<IProvidersSection>();
            var help = new Mock<IHelpSection>();
            var routing = new Mock<IWebRoutingSection>();
            var scripting = new Mock<IScriptingSection>();

            settings.Setup(x => x.Content).Returns(content.Object);
            settings.Setup(x => x.Security).Returns(security.Object);
            settings.Setup(x => x.RequestHandler).Returns(requestHandler.Object);
            settings.Setup(x => x.Templates).Returns(templates.Object);
            settings.Setup(x => x.Developer).Returns(dev.Object);
            settings.Setup(x => x.ViewStateMoverModule).Returns(viewStateMover.Object);
            settings.Setup(x => x.Logging).Returns(logging.Object);
            settings.Setup(x => x.ScheduledTasks).Returns(tasks.Object);
            settings.Setup(x => x.DistributedCall).Returns(distCall.Object);
            settings.Setup(x => x.PackageRepositories).Returns(repos.Object);
            settings.Setup(x => x.Providers).Returns(providers.Object);
            settings.Setup(x => x.Help).Returns(help.Object);
            settings.Setup(x => x.WebRouting).Returns(routing.Object);
            settings.Setup(x => x.Scripting).Returns(scripting.Object);

            //Now configure some defaults - the defaults in the config section classes do NOT pertain to the mocked data!!
            settings.Setup(x => x.Content.UseLegacyXmlSchema).Returns(false);
            settings.Setup(x => x.Content.ForceSafeAliases).Returns(true);
            settings.Setup(x => x.Content.ImageAutoFillProperties).Returns(ContentImagingElement.GetDefaultImageAutoFillProperties());
            settings.Setup(x => x.Content.ImageFileTypes).Returns(ContentImagingElement.GetDefaultImageFileTypes());
            settings.Setup(x => x.RequestHandler.AddTrailingSlash).Returns(true);
            settings.Setup(x => x.RequestHandler.UseDomainPrefixes).Returns(false);
            settings.Setup(x => x.RequestHandler.CharCollection).Returns(RequestHandlerElement.GetDefaultCharReplacements());
            settings.Setup(x => x.Content.UmbracoLibraryCacheDuration).Returns(1800);
            settings.Setup(x => x.WebRouting.UrlProviderMode).Returns("AutoLegacy");
            settings.Setup(x => x.Templates.DefaultRenderingEngine).Returns(RenderingEngine.Mvc);
            
            return settings.Object;
        }

        // from appSettings

        private static readonly IDictionary<string, string> SavedAppSettings = new Dictionary<string, string>();

        static void SaveSetting(string key)
        {
            SavedAppSettings[key] = ConfigurationManager.AppSettings[key];
        }

        static void SaveSettings()
        {
            SaveSetting("umbracoHideTopLevelNodeFromPath");
            SaveSetting("umbracoUseDirectoryUrls");
            SaveSetting("umbracoPath");
            SaveSetting("umbracoReservedPaths");
            SaveSetting("umbracoReservedUrls");
            SaveSetting("umbracoConfigurationStatus");
        }

        public static bool HideTopLevelNodeFromPath
        {
            get { return GlobalSettings.HideTopLevelNodeFromPath; }
            set { ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", value ? "true" : "false"); }
        }

        public static bool UseDirectoryUrls
        {
            get { return GlobalSettings.UseDirectoryUrls; }
            set { ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", value ? "true" : "false"); }
        }

        public static string UmbracoPath
        {
            get { return GlobalSettings.Path; }
            set { ConfigurationManager.AppSettings.Set("umbracoPath", value); }
        }

        public static string ReservedPaths
        {
            get { return GlobalSettings.ReservedPaths; }
            set { GlobalSettings.ReservedPaths = value; }
        }

        public static string ReservedUrls
        {
            get { return GlobalSettings.ReservedUrls; }
            set { GlobalSettings.ReservedUrls = value; }
        }

        public static string ConfigurationStatus
        {
            get { return GlobalSettings.ConfigurationStatus; }
            set { ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", value); }
        }

        // reset & defaults

        static SettingsForTests()
        {
            SaveSettings();
        }

        public static void Reset()
        {
            ResetUmbracoSettings();
            GlobalSettings.Reset();

            foreach (var kvp in SavedAppSettings)
                ConfigurationManager.AppSettings.Set(kvp.Key, kvp.Value);

            // set some defaults that are wrong in the config file?!
            // this is annoying, really
            HideTopLevelNodeFromPath = false;
        }

        /// <summary>
        /// This sets all settings back to default settings
        /// </summary>
        private static void ResetUmbracoSettings()
        {
            ConfigureSettings(GetDefault());
        }

        private static IUmbracoSettingsSection _defaultSettings;

        internal static IUmbracoSettingsSection GetDefault()
        {
            if (_defaultSettings == null)
            {
                var config = new FileInfo(TestHelper.MapPathForTest("~/Configurations/UmbracoSettings/web.config"));

                var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = config.FullName };
                var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                _defaultSettings = configuration.GetSection("umbracoConfiguration/defaultSettings") as UmbracoSettingsSection;
            }

            return _defaultSettings;
        }
    }
}
