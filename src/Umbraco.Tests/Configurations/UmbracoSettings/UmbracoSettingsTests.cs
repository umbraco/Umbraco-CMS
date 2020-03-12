using System.Configuration;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    public abstract class UmbracoSettingsTests
    {
        protected virtual bool TestingDefaults { get; set; }

        [SetUp]
        public void Init()
        {
            var config = new FileInfo(TestHelper.MapPathForTest("~/Configurations/UmbracoSettings/web.config"));

            var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = config.FullName };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            Debug.WriteLine("Testing defaults? {0}", TestingDefaults);
            if (TestingDefaults)
            {
                Settings = configuration.GetSection("umbracoConfiguration/defaultSettings") as UmbracoSettingsSection;
            }
            else
            {
                Settings = configuration.GetSection("umbracoConfiguration/settings") as UmbracoSettingsSection;
            }

            Assert.IsNotNull(Settings);
        }

        protected IUmbracoSettingsSection SettingsSection => Settings;
        private UmbracoSettingsSection Settings { get; set; }

        protected ILoggingSettings LoggingSettings => Settings.Logging;
    }
}
