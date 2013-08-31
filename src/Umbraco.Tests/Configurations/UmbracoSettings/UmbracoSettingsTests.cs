using System.Configuration;
using System.IO;
using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    public abstract class UmbracoSettingsTests
    {
        //TODO: Need to test defaults after all this is done.

        [SetUp]
        public void Init()
        {
            var config = new FileInfo(TestHelper.MapPathForTest("~/Configurations/UmbracoSettings/web.config"));

            var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = config.FullName };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            Section = configuration.GetSection("umbracoConfiguration/settings") as UmbracoSettingsSection;

            Assert.IsNotNull(Section);
        }

        protected UmbracoSettingsSection Section { get; private set; }
    }
}