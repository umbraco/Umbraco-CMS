using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Tests.TestHelpers
{
    public class SettingsForTests
    {
        private static Common.SettingsForTests _settingsForTests = new Common.SettingsForTests();

        public static IGlobalSettings GenerateMockGlobalSettings() => _settingsForTests.GenerateMockGlobalSettings(TestHelper.GetUmbracoVersion(), TestHelper.IOHelper);

        /// <summary>
        /// Returns generated settings which can be stubbed to return whatever values necessary
        /// </summary>
        /// <returns></returns>
        public static IContentSettings GenerateMockContentSettings() => _settingsForTests.GenerateMockContentSettings();

        //// from appSettings

        //private static readonly IDictionary<string, string> SavedAppSettings = new Dictionary<string, string>();

        //static void SaveSetting(string key)
        //{
        //    SavedAppSettings[key] = ConfigurationManager.AppSettings[key];
        //}

        //static void SaveSettings()
        //{
        //    SaveSetting("umbracoHideTopLevelNodeFromPath");
        //    SaveSetting("umbracoUseDirectoryUrls");
        //    SaveSetting("umbracoPath");
        //    SaveSetting("umbracoReservedPaths");
        //    SaveSetting("umbracoReservedUrls");
        //    SaveSetting("umbracoConfigurationStatus");
        //}



        // reset & defaults

        //static SettingsForTests()
        //{
        //    //SaveSettings();
        //}

        public static void Reset() => _settingsForTests.Reset();

        internal static IGlobalSettings GetDefaultGlobalSettings() => _settingsForTests.GetDefaultGlobalSettings(TestHelper.GetUmbracoVersion(), TestHelper.IOHelper);

        internal static IHostingSettings GetDefaultHostingSettings() => _settingsForTests.GetDefaultHostingSettings();

        public static IWebRoutingSettings GenerateMockWebRoutingSettings() => _settingsForTests.GenerateMockWebRoutingSettings();

        public static IRequestHandlerSettings GenerateMockRequestHandlerSettings() => _settingsForTests.GenerateMockRequestHandlerSettings();

        public static ISecuritySettings GenerateMockSecuritySettings() => _settingsForTests.GenerateMockSecuritySettings();

        public static IUserPasswordConfiguration GenerateMockUserPasswordConfiguration() => _settingsForTests.GenerateMockUserPasswordConfiguration();

        public static IMemberPasswordConfiguration GenerateMockMemberPasswordConfiguration() => _settingsForTests.GenerateMockMemberPasswordConfiguration();
    }
}
