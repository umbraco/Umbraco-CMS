using System;
using System.Configuration;
using System.Linq;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Misc
{
    [TestFixture]
    public class ApplicationUrlHelperTests
    {
        private IServerRegistrar _registrar;

        // note: in tests, read appContext._umbracoApplicationUrl and not the property,
        // because reading the property does run some code, as long as the field is null.

        private void Initialize(IUmbracoSettingsSection settings, IGlobalSettings globalSettings)
        {
            _registrar = new ConfigServerRegistrar(settings.DistributedCall, Mock.Of<ILogger>(), globalSettings);
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore();
            container.Register(_ => _registrar);
        }

        [TearDown]
        public void Reset()
        {
            Current.Reset();
        }

        [Test]
        public void NoApplicationUrlByDefault()
        {
            var state = new RuntimeState(Mock.Of<ILogger>(), new Lazy<IServerRegistrar>(Mock.Of<IServerRegistrar>), new Lazy<MainDom>(Mock.Of<MainDom>));
            Assert.IsNull(state.ApplicationUrl);
        }

        [Test]
        public void SetApplicationUrlViaProvider()
        {
            // no applicable settings, but a provider

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>());

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            ApplicationUrlHelper.ApplicationUrlProvider = request => "http://server1.com/umbraco";

            Initialize(settings, globalConfig.Object);

            var state = new RuntimeState(Mock.Of<ILogger>(), new Lazy<IServerRegistrar>(Mock.Of<IServerRegistrar>), new Lazy<MainDom>(Mock.Of<MainDom>));

            state.EnsureApplicationUrl(settings, globalConfig.Object);

            Assert.AreEqual("http://server1.com/umbraco", state.ApplicationUrl.ToString());
        }

        [Test]
        public void SetApplicationUrlWhenNoSettings()
        {
            // no applicable settings, cannot set url

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>());

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            Initialize(settings, globalConfig.Object);
            
            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object);

            // still NOT set
            Assert.IsNull(url);
        }

        [Test]
        public void SetApplicationUrlFromDcSettingsSsl1()
        {
            // set from distributed call settings
            // first server is master server

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Enabled && callSection.Servers == new[]
                {
                    Mock.Of<IServer>(server => server.ServerName == NetworkHelper.MachineName && server.ServerAddress == "server1.com"),
                    Mock.Of<IServer>(server => server.ServerName == "ANOTHERNAME" && server.ServerAddress == "server2.com"),
                })
                && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string)null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == (string)null));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            Initialize(settings, globalConfig.Object);

            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object);

            Assert.AreEqual("http://server1.com:80/umbraco", url);

            var role = _registrar.GetCurrentServerRole();
            Assert.AreEqual(ServerRole.Master, role);
        }

        [Test]
        public void SetApplicationUrlFromDcSettingsSsl2()
        {
            // set from distributed call settings
            // other servers are slave servers

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Enabled && callSection.Servers == new[]
                {
                    Mock.Of<IServer>(server => server.ServerName == "ANOTHERNAME" && server.ServerAddress == "server2.com"),
                    Mock.Of<IServer>(server => server.ServerName == NetworkHelper.MachineName && server.ServerAddress == "server1.com"),
                })
                && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string)null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == (string)null));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            Initialize(settings, globalConfig.Object);

            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object);

            Assert.AreEqual("http://server1.com:80/umbraco", url);

            var role = _registrar.GetCurrentServerRole();
            Assert.AreEqual(ServerRole.Slave, role);
        }

        [Test]
        public void SetApplicationUrlFromDcSettingsSsl3()
        {
            // set from distributed call settings
            // cannot set if not enabled

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Enabled == false && callSection.Servers == new[]
                {
                    Mock.Of<IServer>(server => server.ServerName == "ANOTHERNAME" && server.ServerAddress == "server2.com"),
                    Mock.Of<IServer>(server => server.ServerName == NetworkHelper.MachineName && server.ServerAddress == "server1.com"),
                })
                && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string)null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == (string)null));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            Initialize(settings, globalConfig.Object);

            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object);

            Assert.IsNull(url);

            var role = _registrar.GetCurrentServerRole();
            Assert.AreEqual(ServerRole.Single, role);
        }

        [Test]
        public void ServerRoleSingle()
        {
            // distributed call settings disabled, single server

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Enabled == false && callSection.Servers == Enumerable.Empty<IServer>())
                && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string)null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == (string)null));
            
            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            
            Initialize(settings, globalConfig.Object);

            var role = _registrar.GetCurrentServerRole();
            Assert.AreEqual(ServerRole.Single, role);
        }

        [Test]
        public void ServerRoleUnknown1()
        {
            // distributed call enabled but missing servers, unknown server

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Enabled && callSection.Servers == Enumerable.Empty<IServer>())
                && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string)null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == (string)null));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());

            Initialize(settings, globalConfig.Object);

            var role = _registrar.GetCurrentServerRole();
            Assert.AreEqual(ServerRole.Unknown, role);
        }

        [Test]
        public void ServerRoleUnknown2()
        {
            // distributed call enabled, cannot find server, assume it's an undeclared slave

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Enabled && callSection.Servers == new[]
                {
                    Mock.Of<IServer>(server => server.ServerName == "ANOTHERNAME" && server.ServerAddress == "server2.com"),
                })
                && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string)null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == (string)null));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());

            Initialize(settings, globalConfig.Object);

            var role = _registrar.GetCurrentServerRole();
            Assert.AreEqual(ServerRole.Slave, role);
        }

        [Test]
        public void SetApplicationUrlFromStSettingsNoSsl()
        {
            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == "mycoolhost.com/umbraco"));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(false);

            Initialize(settings, globalConfig.Object);
            
            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object);

            Assert.AreEqual("http://mycoolhost.com/umbraco", url);
        }

        [Test]
        public void SetApplicationUrlFromStSettingsSsl()
        {
            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == "mycoolhost.com/umbraco/"));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            Initialize(settings, globalConfig.Object);
            
            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object);

            Assert.AreEqual("https://mycoolhost.com/umbraco", url);
        }

        [Test]
        public void SetApplicationUrlFromWrSettingsSsl()
        {
            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.DistributedCall == Mock.Of<IDistributedCallSection>(callSection => callSection.Servers == Enumerable.Empty<IServer>())
                && section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == "httpx://whatever.com/umbraco/")
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == "mycoolhost.com/umbraco"));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            Initialize(settings, globalConfig.Object);

            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object);

            Assert.AreEqual("httpx://whatever.com/umbraco", url);
        }
    }
}
