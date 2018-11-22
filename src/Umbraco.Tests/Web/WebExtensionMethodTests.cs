using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Web
{
    [TestFixture]
    public class WebExtensionMethodTests
    {

        [Test]
        public void UmbracoContextExtensions_GetUmbracoContext()
        {
            var appCtx = new ApplicationContext(
               CacheHelper.CreateDisabledCacheHelper(),
               new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var umbCtx = UmbracoContext.CreateContext(
                Mock.Of<HttpContextBase>(),
                appCtx,
                new WebSecurity(Mock.Of<HttpContextBase>(), appCtx),
                Mock.Of<IUmbracoSettingsSection>(),
                new List<IUrlProvider>(),
                false);
            var items = new Dictionary<object, object>()
            {
                {UmbracoContext.HttpContextItemName, umbCtx}
            };
            var http = new Mock<HttpContextBase>();
            http.Setup(x => x.Items).Returns(items);

            var result = http.Object.GetUmbracoContext();

            Assert.IsNotNull(result);
            Assert.AreSame(umbCtx, result);
        }

        [Test]
        public void RouteDataExtensions_GetUmbracoContext()
        {
            var appCtx = new ApplicationContext(
               CacheHelper.CreateDisabledCacheHelper(),
               new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var umbCtx = UmbracoContext.CreateContext(
                Mock.Of<HttpContextBase>(),
                appCtx,
                new WebSecurity(Mock.Of<HttpContextBase>(), appCtx),
                Mock.Of<IUmbracoSettingsSection>(),
                new List<IUrlProvider>(),
                false);
            var r1 = new RouteData();
            r1.DataTokens.Add(Core.Constants.Web.UmbracoContextDataToken, umbCtx);

            var result = r1.GetUmbracoContext();

            Assert.IsNotNull(result);
            Assert.AreSame(umbCtx, result);
        }

        [Test]
        public void ControllerContextExtensions_GetUmbracoContext_From_RouteValues()
        {
            var appCtx = new ApplicationContext(
               CacheHelper.CreateDisabledCacheHelper(),
               new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var umbCtx = UmbracoContext.CreateContext(
                Mock.Of<HttpContextBase>(),
                appCtx,
                new WebSecurity(Mock.Of<HttpContextBase>(), appCtx),
                Mock.Of<IUmbracoSettingsSection>(),
                new List<IUrlProvider>(),
                false);

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
        public void ControllerContextExtensions_GetUmbracoContext_From_HttpContext()
        {
            var appCtx = new ApplicationContext(
               CacheHelper.CreateDisabledCacheHelper(),
               new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var umbCtx = UmbracoContext.CreateContext(
                Mock.Of<HttpContextBase>(),
                appCtx,
                new WebSecurity(Mock.Of<HttpContextBase>(), appCtx),
                Mock.Of<IUmbracoSettingsSection>(),
                new List<IUrlProvider>(),
                false);
            var items = new Dictionary<object, object>()
            {
                {UmbracoContext.HttpContextItemName, umbCtx}
            };
            var http = new Mock<HttpContextBase>();
            http.Setup(x => x.Items).Returns(items);

            var r1 = new RouteData();
            var ctx1 = CreateViewContext(new ControllerContext(http.Object, r1, new MyController()));
            var r2 = new RouteData();
            r2.DataTokens.Add("ParentActionViewContext", ctx1);
            var ctx2 = CreateViewContext(new ControllerContext(http.Object, r2, new MyController()));
            var r3 = new RouteData();
            r3.DataTokens.Add("ParentActionViewContext", ctx2);
            var ctx3 = CreateViewContext(new ControllerContext(http.Object, r3, new MyController()));

            var result = ctx3.GetUmbracoContext();

            Assert.IsNotNull(result);
            Assert.AreSame(umbCtx, result);
        }

        [Test]
        public void ControllerContextExtensions_GetDataTokenInViewContextHierarchy()
        {
            var r1 = new RouteData();
            r1.DataTokens.Add("hello", "world");
            var ctx1 = CreateViewContext(new ControllerContext(Mock.Of<HttpContextBase>(), r1, new MyController()));
            var r2 = new RouteData();
            r2.DataTokens.Add("ParentActionViewContext", ctx1);
            var ctx2 = CreateViewContext(new ControllerContext(Mock.Of<HttpContextBase>(), r2, new MyController()));
            var r3 = new RouteData();
            r3.DataTokens.Add("ParentActionViewContext", ctx2);
            var ctx3 = CreateViewContext(new ControllerContext(Mock.Of<HttpContextBase>(), r3, new MyController()));

            var result = ctx3.GetDataTokenInViewContextHierarchy("hello");

            Assert.IsNotNull(result as string);
            Assert.AreEqual((string)result, "world");
        }

        private ViewContext CreateViewContext(ControllerContext ctx)
        {
            return new ViewContext(ctx, Mock.Of<IView>(),
                new ViewDataDictionary(), new TempDataDictionary(), new StringWriter());
        }

        private class MyController : Controller
        {
            
        }
    }
}
