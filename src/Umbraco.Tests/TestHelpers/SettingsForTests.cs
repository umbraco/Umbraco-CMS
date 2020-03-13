using System.IO;
using System.Configuration;
using System.Linq;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Tests.TestHelpers
{
    public class SettingsForTests
    {
        public static IGlobalSettings GenerateMockGlobalSettings()
        {
            var config = Mock.Of<IGlobalSettings>(
                settings =>
                    settings.ConfigurationStatus == TestHelper.GetUmbracoVersion().SemanticVersion.ToSemanticString() &&
                    settings.UseHttps == false &&
                    settings.HideTopLevelNodeFromPath == false &&
                    settings.Path == TestHelper.IOHelper.ResolveUrl("~/umbraco") &&
                    settings.TimeOutInMinutes == 20 &&
                    settings.DefaultUILanguage == "en" &&
                    settings.ReservedPaths == (GlobalSettings.StaticReservedPaths + "~/umbraco") &&
                    settings.ReservedUrls == GlobalSettings.StaticReservedUrls &&
                    settings.UmbracoPath == "~/umbraco" &&
                    settings.UmbracoMediaPath == "~/media" &&
                    settings.UmbracoCssPath == "~/css" &&
                    settings.UmbracoScriptsPath == "~/scripts"
            );



            return config;
        }

        /// <summary>
        /// Returns generated settings which can be stubbed to return whatever values necessary
        /// </summary>
        /// <returns></returns>
        public static IContentSettings GenerateMockContentSettings()
        {

            var content = new Mock<IContentSettings>();

            //Now configure some defaults - the defaults in the config section classes do NOT pertain to the mocked data!!
            content.Setup(x => x.ImageAutoFillProperties).Returns(ContentImagingElement.GetDefaultImageAutoFillProperties());
            content.Setup(x => x.ImageFileTypes).Returns(ContentImagingElement.GetDefaultImageFileTypes());
            return content.Object;
        }

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

        public static void Reset()
        {
            ResetSettings();
            GlobalSettings.Reset();

            //foreach (var kvp in SavedAppSettings)
            //    ConfigurationManager.AppSettings.Set(kvp.Key, kvp.Value);

            //// set some defaults that are wrong in the config file?!
            //// this is annoying, really
            //HideTopLevelNodeFromPath = false;
        }

        /// <summary>
        /// This sets all settings back to default settings
        /// </summary>
        private static void ResetSettings()
        {
            _defaultGlobalSettings = null;
        }

        private static IGlobalSettings _defaultGlobalSettings;
        private static IHostingSettings _defaultHostingSettings;

        internal static IGlobalSettings GetDefaultGlobalSettings()
        {
            if (_defaultGlobalSettings == null)
            {
                _defaultGlobalSettings = GenerateMockGlobalSettings();
            }
            return _defaultGlobalSettings;
        }

        internal static IHostingSettings GetDefaultHostingSettings()
        {
            if (_defaultHostingSettings == null)
            {
                _defaultHostingSettings = GenerateMockHostingSettings();
            }
            return _defaultHostingSettings;
        }

        private static IHostingSettings GenerateMockHostingSettings()
        {
            var config = Mock.Of<IHostingSettings>(
                settings =>
                    settings.LocalTempStorageLocation == LocalTempStorage.EnvironmentTemp &&
                    settings.DebugMode == false
            );
            return config;
        }

        public static IWebRoutingSettings GenerateMockWebRoutingSettings()
        {
            var mock = new Mock<IWebRoutingSettings>();

            mock.Setup(x => x.DisableRedirectUrlTracking).Returns(false);
            mock.Setup(x => x.InternalRedirectPreservesTemplate).Returns(false);
            mock.Setup(x => x.UrlProviderMode).Returns(UrlMode.Auto.ToString());

            return mock.Object;
        }

        public static IRequestHandlerSettings GenerateMockRequestHandlerSettings()
        {
            var mock = new Mock<IRequestHandlerSettings>();

            mock.Setup(x => x.AddTrailingSlash).Returns(true);
            mock.Setup(x => x.ConvertUrlsToAscii).Returns(false);
            mock.Setup(x => x.TryConvertUrlsToAscii).Returns(false);
            mock.Setup(x => x.CharCollection).Returns(RequestHandlerElement.GetDefaultCharReplacements);

            return mock.Object;
        }

        public static ISecuritySettings GenerateMockSecuritySettings()
        {
            var security = new Mock<ISecuritySettings>();

            return security.Object;
        }

        public static IUserPasswordConfiguration GenerateMockUserPasswordConfiguration()
        {
            var mock = new Mock<IUserPasswordConfiguration>();

            return mock.Object;
        }

        public static IMemberPasswordConfiguration GenerateMockMemberPasswordConfiguration()
        {
            var mock = new Mock<IMemberPasswordConfiguration>();

            return mock.Object;
        }
    }
}
