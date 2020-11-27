using System;
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
        // note: in tests, read appContext._umbracoApplicationUrl and not the property,
        // because reading the property does run some code, as long as the field is null.

        [TearDown]
        public void Reset()
        {
            Current.Reset();
        }

        [Test]
        public void NoApplicationUrlByDefault()
        {
            var state = new RuntimeState(Mock.Of<ILogger>(), Mock.Of<IUmbracoSettingsSection>(), Mock.Of<IGlobalSettings>(), new Lazy<IMainDom>(), new Lazy<IServerRegistrar>());
            Assert.IsNull(state.ApplicationUrl);
        }

        [Test]
        public void SetApplicationUrlViaServerRegistrar()
        {
            // no applicable settings, but a provider

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string)null));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            var registrar = new Mock<IServerRegistrar>();
            registrar.Setup(x => x.GetCurrentServerUmbracoApplicationUrl()).Returns("http://server1.com/umbraco");

            var state = new RuntimeState(Mock.Of<ILogger>(), settings, globalConfig.Object, new Lazy<IMainDom>(), new Lazy<IServerRegistrar>(() => registrar.Object));

            state.EnsureApplicationUrl();

            Assert.AreEqual("http://server1.com/umbraco", state.ApplicationUrl.ToString());
        }

        [Test]
        public void SetApplicationUrlViaProvider()
        {
            // no applicable settings, but a provider

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            ApplicationUrlHelper.ApplicationUrlProvider = request => "http://server1.com/umbraco";



            var state = new RuntimeState(Mock.Of<ILogger>(), settings, globalConfig.Object, new Lazy<IMainDom>(), new Lazy<IServerRegistrar>(() => Mock.Of<IServerRegistrar>()));

            state.EnsureApplicationUrl();

            Assert.AreEqual("http://server1.com/umbraco", state.ApplicationUrl.ToString());
        }

        [Test]
        public void SetApplicationUrlWhenNoSettings()
        {
            // no applicable settings, cannot set URL

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object, Mock.Of<IServerRegistrar>());

            // still NOT set
            Assert.IsNull(url);
        }
        
        [Test]
        public void SetApplicationUrlFromWrSettingsSsl()
        {
            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == "httpx://whatever.com/umbraco/"));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);



            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object, Mock.Of<IServerRegistrar>());

            Assert.AreEqual("httpx://whatever.com/umbraco", url);
        }
    }
}
