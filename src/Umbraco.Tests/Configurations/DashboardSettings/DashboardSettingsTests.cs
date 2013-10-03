using System.Configuration;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Configuration.Dashboard;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Configurations.DashboardSettings
{
    [TestFixture]
    public class DashboardSettingsTests
    {
        [SetUp]
        public void Init()
        {
            var config = new FileInfo(TestHelper.MapPathForTest("~/Configurations/DashboardSettings/web.config"));
            
            var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = config.FullName };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            SettingsSection = configuration.GetSection("umbracoConfiguration/dashBoard") as DashboardSection;

            Assert.IsNotNull(SettingsSection);
        }

        protected IDashboardSection SettingsSection { get; private set; }

        [Test]
        public void Test_Sections()
        {
            Assert.AreEqual(5, SettingsSection.Sections.Count());
        }
    }
}