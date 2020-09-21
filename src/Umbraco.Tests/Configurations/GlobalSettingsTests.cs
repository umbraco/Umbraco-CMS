using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Tests.Common.Builders;
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
            var hostingEnvironment = new AspNetHostingEnvironment(Microsoft.Extensions.Options.Options.Create(new HostingSettings()
            {
                ApplicationVirtualPath = rootPath
            }));

            var globalSettings = new GlobalSettings { UmbracoPath = path };

            Assert.AreEqual(outcome, globalSettings.GetUmbracoMvcAreaNoCache(hostingEnvironment));
        }
    }
}
