

using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Hosting;
using Umbraco.Tests.Integration.Implementations;
using Umbraco.Web;
using Umbraco.Web.BackOffice.Security;

namespace Umbraco.Tests.Security
{
    [TestFixture]
    public class BackOfficeCookieManagerTests 
    {
        [Test]
        public void ShouldAuthenticateRequest_When_Not_Configured()
        {
            var testHelper = new TestHelper();

            var httpContextAccessor = testHelper.GetHttpContextAccessor();
            var globalSettings = testHelper.SettingsForTests.GenerateMockGlobalSettings();
            
            var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Install);
            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(),
                runtime,
                Mock.Of<IHostingEnvironment>(),
                globalSettings,
                Mock.Of<IRequestCache>(),
                Mock.Of<LinkGenerator>());

            var result = mgr.ShouldAuthenticateRequest(new Uri("http://localhost/umbraco"));

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldAuthenticateRequest_When_Configured()
        {
            var testHelper = new TestHelper();


            //hostingEnvironment.ToAbsolute(globalSettings.UmbracoPath);

            var httpContextAccessor = testHelper.GetHttpContextAccessor();
            var globalSettings = testHelper.SettingsForTests.GenerateMockGlobalSettings();
            
            var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);
            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(),
                runtime,
                Mock.Of<IHostingEnvironment>(x => x.ApplicationVirtualPath == "/" && x.ToAbsolute(globalSettings.UmbracoPath) == "/umbraco"),
                globalSettings,
                Mock.Of<IRequestCache>(),
                Mock.Of<LinkGenerator>());

            var result = mgr.ShouldAuthenticateRequest(new Uri("http://localhost/umbraco"));

            Assert.IsTrue(result);
        }

        // TODO: Write remaining tests for `ShouldAuthenticateRequest`
    }
}
