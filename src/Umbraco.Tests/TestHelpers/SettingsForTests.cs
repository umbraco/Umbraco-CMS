using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Configuration;
using Rhino.Mocks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Tests.Configurations.UmbracoSettings;

namespace Umbraco.Tests.TestHelpers
{
    class SettingsForTests
    {
        // umbracoSettings

        /// <summary>
        /// Sets the umbraco settings singleton to the object specified
        /// </summary>
        /// <param name="settings"></param>
        public static void ConfigureSettings(IUmbracoSettingsSection settings)
        {
            UmbracoConfiguration.Current.UmbracoSettings = settings;
        }

        /// <summary>
        /// Returns generated settings which can be stubbed to return whatever values necessary
        /// </summary>
        /// <returns></returns>
        public static IUmbracoSettingsSection GetMockSettings()
        {
            var settings = MockRepository.GenerateStub<IUmbracoSettingsSection>();

            var content = MockRepository.GenerateStub<IContentSection>();
            var security = MockRepository.GenerateStub<ISecuritySection>();
            var requestHandler = MockRepository.GenerateStub<IRequestHandlerSection>();
            var templates = MockRepository.GenerateStub<ITemplatesSection>();
            var dev = MockRepository.GenerateStub<IDeveloperSection>();
            var viewStateMover = MockRepository.GenerateStub<IViewStateMoverModuleSection>();
            var logging = MockRepository.GenerateStub<ILoggingSection>();
            var tasks = MockRepository.GenerateStub<IScheduledTasksSection>();
            var distCall = MockRepository.GenerateStub<IDistributedCallSection>();
            var repos = MockRepository.GenerateStub<IRepositoriesSection>();
            var providers = MockRepository.GenerateStub<IProvidersSection>();
            var help = MockRepository.GenerateStub<IHelpSection>();
            var routing = MockRepository.GenerateStub<IWebRoutingSection>();
            var scripting = MockRepository.GenerateStub<IScriptingSection>();

            settings.Stub(x => x.Content).Return(content);
            settings.Stub(x => x.Security).Return(security);
            settings.Stub(x => x.RequestHandler).Return(requestHandler);
            settings.Stub(x => x.Templates).Return(templates);
            settings.Stub(x => x.Developer).Return(dev);
            settings.Stub(x => x.ViewStateMoverModule).Return(viewStateMover);
            settings.Stub(x => x.Logging).Return(logging);
            settings.Stub(x => x.ScheduledTasks).Return(tasks);
            settings.Stub(x => x.DistributedCall).Return(distCall);
            settings.Stub(x => x.PackageRepositories).Return(repos);
            settings.Stub(x => x.Providers).Return(providers);
            settings.Stub(x => x.Help).Return(help);
            settings.Stub(x => x.WebRouting).Return(routing);
            settings.Stub(x => x.Scripting).Return(scripting);

            //Now configure some defaults - the defaults in the config section classes do NOT pertain to the mocked data!!
            settings.Content.Stub(x => x.UseLegacyXmlSchema).Return(false);
            settings.Content.Stub(x => x.ForceSafeAliases).Return(true);
            settings.Content.Stub(x => x.ImageAutoFillProperties).Return(ContentImagingElement.GetDefaultImageAutoFillProperties());
            settings.Content.Stub(x => x.ImageFileTypes).Return(ContentImagingElement.GetDefaultImageFileTypes());
            settings.RequestHandler.Stub(x => x.AddTrailingSlash).Return(true);
            settings.RequestHandler.Stub(x => x.UseDomainPrefixes).Return(false);
            settings.RequestHandler.Stub(x => x.CharCollection).Return(RequestHandlerElement.GetDefaultCharReplacements());
            settings.Content.Stub(x => x.UmbracoLibraryCacheDuration).Return(1800);
            settings.WebRouting.Stub(x => x.UrlProviderMode).Return("Auto");

            

            return settings;
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

        internal static IUmbracoSettingsSection GetDefault()
        {
            var config = new FileInfo(TestHelper.MapPathForTest("~/Configurations/UmbracoSettings/web.config"));

            var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = config.FullName };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            return configuration.GetSection("umbracoConfiguration/defaultSettings") as UmbracoSettingsSection;
        }
    }
}
