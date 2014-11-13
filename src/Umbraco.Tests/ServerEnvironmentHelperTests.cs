using System.Configuration;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Sync;

namespace Umbraco.Tests
{
    [TestFixture]
    public class ServerEnvironmentHelperTests
    {
        [Test]
        public void Get_Base_Url_Single_Server_Orig_Request_Url_No_SSL()
        {
            var appContext = new ApplicationContext(null)
            {
                OriginalRequestUrl = "test.com"
            };

            ConfigurationManager.AppSettings.Set("umbracoUseSSL", "false");

            var result = ServerEnvironmentHelper.GetCurrentServerUmbracoBaseUrl(
                appContext,
                Mock.Of<IUmbracoSettingsSection>(
                    section => 
                        section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                        && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>()));


            Assert.AreEqual("http://test.com/", result);
        }

        [Test]
        public void Get_Base_Url_Single_Server_Orig_Request_Url_With_SSL()
        {
            var appContext = new ApplicationContext(null)
            {
                OriginalRequestUrl = "test.com"
            };

            ConfigurationManager.AppSettings.Set("umbracoUseSSL", "true");

            var result = ServerEnvironmentHelper.GetCurrentServerUmbracoBaseUrl(
                appContext,
                Mock.Of<IUmbracoSettingsSection>(
                    section =>
                        section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                        && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>()));


            Assert.AreEqual("https://test.com/", result);
        }

        [Test]
        public void Get_Base_Url_Single_Server_Via_Config_Url_No_SSL()
        {
            var appContext = new ApplicationContext(null);

            ConfigurationManager.AppSettings.Set("umbracoUseSSL", "false");

            var result = ServerEnvironmentHelper.GetCurrentServerUmbracoBaseUrl(
                appContext,
                Mock.Of<IUmbracoSettingsSection>(
                    section =>
                        section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                        && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == "mycoolhost.com/hello/world")));


            Assert.AreEqual("http://mycoolhost.com/hello/world/", result);
        }

        [Test]
        public void Get_Base_Url_Single_Server_Via_Config_Url_With_SSL()
        {
            var appContext = new ApplicationContext(null);

            ConfigurationManager.AppSettings.Set("umbracoUseSSL", "true");

            var result = ServerEnvironmentHelper.GetCurrentServerUmbracoBaseUrl(
                appContext,
                Mock.Of<IUmbracoSettingsSection>(
                    section =>
                        section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                        && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == "mycoolhost.com/hello/world")));


            Assert.AreEqual("https://mycoolhost.com/hello/world/", result);
        }
    }
}