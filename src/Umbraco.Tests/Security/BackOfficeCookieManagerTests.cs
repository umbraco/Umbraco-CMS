using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Microsoft.Owin;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Tests.Testing;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;


namespace Umbraco.Tests.Security
{
    [TestFixture]
    [UmbracoTest(WithApplication = true)]
    public class BackOfficeCookieManagerTests : UmbracoTestBase
    {
        [Test]
        public void ShouldAuthenticateRequest_When_Not_Configured()
        {
            //should force app ctx to show not-configured
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", "");

            var globalSettings = TestObjects.GetGlobalSettings();
            var umbracoContext = new UmbracoContext(
                Mock.Of<HttpContextBase>(),
                Mock.Of<IPublishedSnapshotService>(),
                new WebSecurity(Mock.Of<HttpContextBase>(), Current.Services.UserService, globalSettings),
                TestObjects.GetUmbracoSettings(), new List<IUrlProvider>(),globalSettings,
                new TestVariationContextAccessor());

            var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Install);
            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(accessor => accessor.UmbracoContext == umbracoContext), runtime, TestObjects.GetGlobalSettings());

            var result = mgr.ShouldAuthenticateRequest(Mock.Of<IOwinContext>(), new Uri("http://localhost/umbraco"));

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldAuthenticateRequest_When_Configured()
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            var umbCtx = new UmbracoContext(
                Mock.Of<HttpContextBase>(),
                Mock.Of<IPublishedSnapshotService>(),
                new WebSecurity(Mock.Of<HttpContextBase>(), Current.Services.UserService, globalSettings),
                TestObjects.GetUmbracoSettings(), new List<IUrlProvider>(), globalSettings,
                new TestVariationContextAccessor());

            var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);
            var mgr = new BackOfficeCookieManager(Mock.Of<IUmbracoContextAccessor>(accessor => accessor.UmbracoContext == umbCtx), runtime, TestObjects.GetGlobalSettings());

            var request = new Mock<OwinRequest>();
            request.Setup(owinRequest => owinRequest.Uri).Returns(new Uri("http://localhost/umbraco"));

            var result = mgr.ShouldAuthenticateRequest(
                Mock.Of<IOwinContext>(context => context.Request == request.Object),
                new Uri("http://localhost/umbraco"));

            Assert.IsTrue(result);
        }

        // TODO: Write remaining tests for `ShouldAuthenticateRequest`
    }
}
