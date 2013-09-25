using System.Configuration;
using System.IO;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    public abstract class UmbracoSettingsTests
    {
        
        protected virtual bool TestingDefaults
        {
            get { return false; }
        }

        [SetUp]
        public void Init()
        {
            var config = new FileInfo(TestHelper.MapPathForTest("~/Configurations/UmbracoSettings/web.config"));
            
            var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = config.FullName };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            if (TestingDefaults)
            {
                SettingsSection = configuration.GetSection("umbracoConfiguration/defaultSettings") as UmbracoSettingsSection;
            }
            else
            {
                SettingsSection = configuration.GetSection("umbracoConfiguration/settings") as UmbracoSettingsSection;
            }

            

            Assert.IsNotNull(SettingsSection);
        }

        protected IUmbracoSettingsSection SettingsSection { get; private set; }
    }
}