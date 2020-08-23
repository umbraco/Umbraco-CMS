using System.Collections.Generic;
using Moq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Legacy;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Tests.Common
{
    public class SettingsForTests
    {
        public SettingsForTests()
        {
        }

        public IGlobalSettings GenerateMockGlobalSettings(IUmbracoVersion umbVersion = null)
        {
            var semanticVersion = umbVersion?.SemanticVersion ?? new SemVersion(9);

            var config = Mock.Of<IGlobalSettings>(
                settings =>
                    settings.UseHttps == false &&
                    settings.HideTopLevelNodeFromPath == false &&
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
        public IContentSettings GenerateMockContentSettings()
        {

            var content = new Mock<IContentSettings>();

            //Now configure some defaults - the defaults in the config section classes do NOT pertain to the mocked data!!
            content.Setup(x => x.ImageAutoFillProperties).Returns(ContentImagingElement.GetDefaultImageAutoFillProperties());
            content.Setup(x => x.ImageFileTypes).Returns(ContentImagingElement.GetDefaultImageFileTypes());
            return content.Object;
        }

        //// from appSettings

        //private readonly IDictionary<string, string> SavedAppSettings = new Dictionary<string, string>();

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

        public void Reset()
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
        private void ResetSettings()
        {
            _defaultGlobalSettings.Clear();
            _defaultHostingSettings = null;
        }

        private readonly Dictionary<SemVersion, IGlobalSettings> _defaultGlobalSettings = new Dictionary<SemVersion, IGlobalSettings>();
        private IHostingSettings _defaultHostingSettings;

        public IGlobalSettings GetDefaultGlobalSettings(IUmbracoVersion umbVersion)
        {
            if (_defaultGlobalSettings.TryGetValue(umbVersion.SemanticVersion, out var settings))
                return settings;

            settings = GenerateMockGlobalSettings(umbVersion);
            _defaultGlobalSettings[umbVersion.SemanticVersion] = settings;
            return settings;
        }

        public IHostingSettings DefaultHostingSettings => _defaultHostingSettings ?? (_defaultHostingSettings = GenerateMockHostingSettings());

        public IHostingSettings GenerateMockHostingSettings()
        {
            var config = Mock.Of<IHostingSettings>(
                settings =>
                    settings.LocalTempStorageLocation == LocalTempStorage.EnvironmentTemp &&
                    settings.DebugMode == false
            );
            return config;
        }

        public IWebRoutingSettings GenerateMockWebRoutingSettings()
        {
            var mock = new Mock<IWebRoutingSettings>();

            mock.Setup(x => x.DisableRedirectUrlTracking).Returns(false);
            mock.Setup(x => x.InternalRedirectPreservesTemplate).Returns(false);
            mock.Setup(x => x.UrlProviderMode).Returns(UrlMode.Auto.ToString());

            return mock.Object;
        }

        public IRequestHandlerSettings GenerateMockRequestHandlerSettings()
        {
            var mock = new Mock<IRequestHandlerSettings>();

            mock.Setup(x => x.AddTrailingSlash).Returns(true);
            mock.Setup(x => x.ConvertUrlsToAscii).Returns(false);
            mock.Setup(x => x.TryConvertUrlsToAscii).Returns(false);
            mock.Setup(x => x.CharCollection).Returns(RequestHandlerElement.GetDefaultCharReplacements);

            return mock.Object;
        }

        public ISecuritySettings GenerateMockSecuritySettings()
        {
            var security = new Mock<ISecuritySettings>();

            return security.Object;
        }

        public IUserPasswordConfiguration GenerateMockUserPasswordConfiguration()
        {
            var mock = new Mock<IUserPasswordConfiguration>();

            return mock.Object;
        }

        public IMemberPasswordConfiguration GenerateMockMemberPasswordConfiguration()
        {
            var mock = new Mock<IMemberPasswordConfiguration>();

            return mock.Object;
        }
    }
}
