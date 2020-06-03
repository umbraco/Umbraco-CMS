

using NUnit.Framework;
using Umbraco.Tests.Integration.Implementations;

namespace Umbraco.Tests.Security
{
    [TestFixture]
    public class BackOfficeCookieManagerTests 
    {
        [Test]
        public void ShouldAuthenticateRequest_When_Not_Configured()
        {
            var testHelper = new TestHelper();

            //should force app ctx to show not-configured
            ConfigurationManager.AppSettings.Set(Constants.AppSettings.ConfigurationStatus, "");

            var httpContextAccessor = testHelper.GetHttpContextAccessor();
            var globalSettings = testHelper.SettingsForTests.GetDefaultGlobalSettings();
            var umbracoContext = new UmbracoContext(
                httpContextAccessor,
                Mock.Of<IPublishedSnapshotService>(),
                Mock.Of<IWebSecurity>(),
                globalSettings,
                HostingEnvironment,
                new TestVariationContextAccessor(),
                UriUtility,
                new AspNetCookieManager(httpContextAccessor));

            var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Install);
            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(accessor => accessor.UmbracoContext == umbracoContext), runtime, HostingEnvironment, globalSettings, AppCaches.RequestCache);

            var result = mgr.ShouldAuthenticateRequest(Mock.Of<IOwinContext>(), new Uri("http://localhost/umbraco"));

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldAuthenticateRequest_When_Configured()
        {
            var httpContextAccessor = TestHelper.GetHttpContextAccessor();
            var globalSettings = TestObjects.GetGlobalSettings();
            var umbCtx = new UmbracoContext(
                httpContextAccessor,
                Mock.Of<IPublishedSnapshotService>(),
                Mock.Of<IWebSecurity>(),
                globalSettings,
                HostingEnvironment,
                new TestVariationContextAccessor(),
                UriUtility,
                new AspNetCookieManager(httpContextAccessor));

            var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);
            var mgr = new BackOfficeCookieManager(Mock.Of<IUmbracoContextAccessor>(accessor => accessor.UmbracoContext == umbCtx), runtime, HostingEnvironment, globalSettings, AppCaches.RequestCache);

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
