using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Tests.Web
{
    [TestFixture]
    [UmbracoTest(WithApplication = true)]
    public class WebExtensionMethodTests : UmbracoTestBase
    {
        [Test]
        public void RouteDataExtensions_GetUmbracoContext()
        {
            var umbCtx = new UmbracoContext(
                Mock.Of<HttpContextBase>(),
                Mock.Of<IPublishedSnapshotService>(),
                new WebSecurity(Mock.Of<HttpContextBase>(), Current.Services.UserService, TestObjects.GetGlobalSettings()),
                TestObjects.GetUmbracoSettings(),
                new List<IUrlProvider>(),
                Enumerable.Empty<IMediaUrlProvider>(),
                TestObjects.GetGlobalSettings(),
                new TestVariationContextAccessor());
            var r1 = new RouteData();
            r1.DataTokens.Add(Core.Constants.Web.UmbracoContextDataToken, umbCtx);

            Assert.IsTrue(r1.DataTokens.ContainsKey(Core.Constants.Web.UmbracoContextDataToken));
            Assert.AreSame(umbCtx, r1.DataTokens[Core.Constants.Web.UmbracoContextDataToken]);
        }

        [Test]
        public void ControllerContextExtensions_GetUmbracoContext_From_RouteValues()
        {
            var umbCtx = new UmbracoContext(
                Mock.Of<HttpContextBase>(),
                Mock.Of<IPublishedSnapshotService>(),
                new WebSecurity(Mock.Of<HttpContextBase>(), Current.Services.UserService, TestObjects.GetGlobalSettings()),
                TestObjects.GetUmbracoSettings(),
                new List<IUrlProvider>(),
                Enumerable.Empty<IMediaUrlProvider>(),
                TestObjects.GetGlobalSettings(),
                new TestVariationContextAccessor());

            var r1 = new RouteData();
            r1.DataTokens.Add(Core.Constants.Web.UmbracoContextDataToken, umbCtx);
            var ctx1 = CreateViewContext(new ControllerContext(Mock.Of<HttpContextBase>(), r1, new MyController()));
            var r2 = new RouteData();
            r2.DataTokens.Add("ParentActionViewContext", ctx1);
            var ctx2 = CreateViewContext(new ControllerContext(Mock.Of<HttpContextBase>(), r2, new MyController()));
            var r3 = new RouteData();
            r3.DataTokens.Add("ParentActionViewContext", ctx2);
            var ctx3 = CreateViewContext(new ControllerContext(Mock.Of<HttpContextBase>(), r3, new MyController()));

            var result = ctx3.GetUmbracoContext();

            Assert.IsNotNull(result);
            Assert.AreSame(umbCtx, result);
        }

        [Test]
        public void ControllerContextExtensions_GetUmbracoContext_From_Current()
        {
            var umbCtx = new UmbracoContext(
                Mock.Of<HttpContextBase>(),
                Mock.Of<IPublishedSnapshotService>(),
                new WebSecurity(Mock.Of<HttpContextBase>(), Current.Services.UserService, TestObjects.GetGlobalSettings()),
                TestObjects.GetUmbracoSettings(),
                new List<IUrlProvider>(),
                Enumerable.Empty<IMediaUrlProvider>(),
                TestObjects.GetGlobalSettings(),
                new TestVariationContextAccessor());

            var httpContext = Mock.Of<HttpContextBase>();

            var r1 = new RouteData();
            var ctx1 = CreateViewContext(new ControllerContext(httpContext, r1, new MyController()));
            var r2 = new RouteData();
            r2.DataTokens.Add("ParentActionViewContext", ctx1);
            var ctx2 = CreateViewContext(new ControllerContext(httpContext, r2, new MyController()));
            var r3 = new RouteData();
            r3.DataTokens.Add("ParentActionViewContext", ctx2);
            var ctx3 = CreateViewContext(new ControllerContext(httpContext, r3, new MyController()));

            Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();
            Current.UmbracoContextAccessor.UmbracoContext = umbCtx;

            var result = ctx3.GetUmbracoContext();

            Assert.IsNotNull(result);
            Assert.AreSame(umbCtx, result);
        }

        [Test]
        public void ControllerContextExtensions_GetDataTokenInViewContextHierarchy()
        {
            var r1 = new RouteData();
            r1.DataTokens.Add("hello", "world");
            r1.DataTokens.Add("r", "1");
            var ctx1 = CreateViewContext(new ControllerContext(Mock.Of<HttpContextBase>(), r1, new MyController()));
            var r2 = new RouteData();
            r2.DataTokens.Add("ParentActionViewContext", ctx1);
            r2.DataTokens.Add("r", "2");
            var ctx2 = CreateViewContext(new ControllerContext(Mock.Of<HttpContextBase>(), r2, new MyController()));
            var r3 = new RouteData();
            r3.DataTokens.Add("ParentActionViewContext", ctx2);
            r3.DataTokens.Add("r", "3");
            var ctx3 = CreateViewContext(new ControllerContext(Mock.Of<HttpContextBase>(), r3, new MyController()));

            var result = ctx3.GetDataTokenInViewContextHierarchy("hello");

            Assert.IsNotNull(result as string);
            Assert.AreEqual((string) result, "world");
        }

        private static ViewContext CreateViewContext(ControllerContext ctx)
        {
            return new ViewContext(ctx, Mock.Of<IView>(), new ViewDataDictionary(), new TempDataDictionary(), new StringWriter());
        }

        private class MyController : Controller
        { }
    }
}
