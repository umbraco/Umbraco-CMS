using System.Configuration;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;
using Umbraco.Core.Sync;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests
{
    [TestFixture]
    public class ServerEnvironmentHelperTests
    {
        private ILogger _logger;

        // note: in tests, read appContext._umbracoApplicationUrl and not the property,
        // because reading the property does run some code, as long as the field is null.

        [TestFixtureSetUp]
        public void InitializeFixture()
        {
            _logger = new Logger(new FileInfo(TestHelper.MapPathForTest("~/unit-test-log4net.config")));
        }

        [Test]
        public void SetApplicationUrlWhenNoSettings()
        {
            var appCtx = new ApplicationContext(
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()))
            {
                UmbracoApplicationUrl = null // NOT set
            };

            

            ConfigurationManager.AppSettings.Set("umbracoUseSSL", "true"); // does not make a diff here

            ServerEnvironmentHelper.TrySetApplicationUrlFromSettings(appCtx, _logger,
                Mock.Of<IUmbracoSettingsSection>(
                    section =>
                        section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                        && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null)
                        && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>()));


            // still NOT set
            Assert.IsNull(appCtx._umbracoApplicationUrl);
        }

        [Test]
        public void SetApplicationUrlFromDcSettingsNoSsl()
        {
            var appCtx = new ApplicationContext(
               CacheHelper.CreateDisabledCacheHelper(),
               new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            ConfigurationManager.AppSettings.Set("umbracoUseSSL", "false");

            ServerEnvironmentHelper.TrySetApplicationUrlFromSettings(appCtx, _logger,
                Mock.Of<IUmbracoSettingsSection>(
                    section =>
                        section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                        && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null)
                        && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == "mycoolhost.com/hello/world/")));


            Assert.AreEqual("http://mycoolhost.com/hello/world", appCtx._umbracoApplicationUrl);
        }

        [Test]
        public void SetApplicationUrlFromDcSettingsSsl()
        {
            var appCtx = new ApplicationContext(
               CacheHelper.CreateDisabledCacheHelper(),
               new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            ConfigurationManager.AppSettings.Set("umbracoUseSSL", "true");

            ServerEnvironmentHelper.TrySetApplicationUrlFromSettings(appCtx, _logger,
                Mock.Of<IUmbracoSettingsSection>(
                    section =>
                        section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                        && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null)
                        && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == "mycoolhost.com/hello/world")));


            Assert.AreEqual("https://mycoolhost.com/hello/world", appCtx._umbracoApplicationUrl);
        }

        [Test]
        public void SetApplicationUrlFromWrSettingsSsl()
        {
            var appCtx = new ApplicationContext(
               CacheHelper.CreateDisabledCacheHelper(),
               new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            ConfigurationManager.AppSettings.Set("umbracoUseSSL", "true"); // does not make a diff here

            ServerEnvironmentHelper.TrySetApplicationUrlFromSettings(appCtx, _logger,
                Mock.Of<IUmbracoSettingsSection>(
                    section =>
                        section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                        && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == "httpx://whatever.com/hello/world/")
                        && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == "mycoolhost.com/hello/world")));


            Assert.AreEqual("httpx://whatever.com/hello/world", appCtx._umbracoApplicationUrl);
        }
    }
}