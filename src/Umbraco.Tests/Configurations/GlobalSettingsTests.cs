using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Configuration;
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
            var mockHostingSettings = Mock.Get(SettingsForTests.GenerateMockHostingSettings());
            mockHostingSettings.Setup(x => x.ApplicationVirtualPath).Returns(rootPath);

            var hostingEnvironment = new AspNetHostingEnvironment(mockHostingSettings.Object);

            var globalSettings = new GlobalSettingsBuilder().WithPath(path).Build();

            Assert.AreEqual(outcome, globalSettings.GetUmbracoMvcAreaNoCache(hostingEnvironment));
        }
    }
}
