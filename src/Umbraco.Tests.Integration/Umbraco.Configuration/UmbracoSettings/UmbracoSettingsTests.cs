using System.Configuration;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Tests.Integration.Implementations;

namespace Umbraco.Tests.Integration.Umbraco.Configuration.UmbracoSettings
{
    public abstract class UmbracoSettingsTests
    {
        protected virtual bool TestingDefaults { get; set; }

        [SetUp]
        public void Init()
        {
            var testHelper = new TestHelper();
            var config = new FileInfo(testHelper.MapPathForTestFiles("~/Umbraco.Configuration/Configurations/web.config"));

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
        private UmbracoSettingsSection Settings { get; set; }

        protected ILoggingSettings LoggingSettings => Settings.Logging;
        protected IWebRoutingSettings WebRoutingSettings => Settings.WebRouting;
        protected IRequestHandlerSettings RequestHandlerSettings => Settings.RequestHandler;
        protected ISecuritySettings SecuritySettings => Settings.Security;
        protected IUserPasswordConfiguration UserPasswordConfiguration => Settings.Security.UserPasswordConfiguration;
        protected IMemberPasswordConfiguration MemberPasswordConfiguration => Settings.Security.MemberPasswordConfiguration;
        protected IContentSettings ContentSettings => Settings.Content;
    }
}
