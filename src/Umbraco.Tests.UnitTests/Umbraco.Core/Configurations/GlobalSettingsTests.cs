using AutoFixture.NUnit3;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Tests.UnitTests.AutoFixture;
using Umbraco.Web.Common.AspNetCore;


namespace Umbraco.Tests.Configurations
{
    [TestFixture]
    public class GlobalSettingsTests 
    {
        [InlineAutoMoqData("~/umbraco", "/", "umbraco")]
        [InlineAutoMoqData("~/umbraco", "/MyVirtualDir", "umbraco")]
        [InlineAutoMoqData("~/customPath", "/MyVirtualDir/", "custompath")]
        [InlineAutoMoqData("~/some-wacky/nestedPath", "/MyVirtualDir", "some-wacky-nestedpath")]
        [InlineAutoMoqData("~/some-wacky/nestedPath", "/MyVirtualDir/NestedVDir/", "some-wacky-nestedpath")]
        public void Umbraco_Mvc_Area(string path, string rootPath, string outcome, [Frozen] IOptionsMonitor<HostingSettings> hostingSettings,  AspNetCoreHostingEnvironment hostingEnvironment)
        {
            hostingSettings.CurrentValue.ApplicationVirtualPath = rootPath;

            var globalSettings = new GlobalSettings { UmbracoPath = path };

            Assert.AreEqual(outcome, globalSettings.GetUmbracoMvcAreaNoCache(hostingEnvironment));
        }
    }
}
