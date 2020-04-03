using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Hosting;

namespace Umbraco.Tests.Configurations
{

    [TestFixture]
    public class GlobalSettingsTests : BaseWebTest
    {
        [TestCase("~/umbraco", "/", "umbraco")]
        [TestCase("~/umbraco", "/MyVirtualDir", "umbraco")]
        [TestCase("~/customPath", "/MyVirtualDir/", "custompath")]
        [TestCase("~/some-wacky/nestedPath", "/MyVirtualDir", "some-wacky-nestedpath")]
        [TestCase("~/some-wacky/nestedPath", "/MyVirtualDir/NestedVDir/", "some-wacky-nestedpath")]
        public void Umbraco_Mvc_Area(string path, string rootPath, string outcome)
        {

            var globalSettings = SettingsForTests.GenerateMockGlobalSettings();
            var mockHostingSettings = Mock.Get(SettingsForTests.GetDefaultHostingSettings());
            mockHostingSettings.Setup(x => x.ApplicationVirtualPath).Returns(rootPath);

            var hostingEnvironment = new AspNetHostingEnvironment(mockHostingSettings.Object);

            var globalSettingsMock = Mock.Get(globalSettings);
            globalSettingsMock.Setup(x => x.UmbracoPath).Returns(() => path);

            Assert.AreEqual(outcome, globalSettingsMock.Object.GetUmbracoMvcAreaNoCache(hostingEnvironment));
        }
    }
}
