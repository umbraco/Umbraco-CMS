using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Web.Mvc
{
    [TestFixture]
    public class SurfaceControllerTests
    {
        [Test]
        public void Can_Construct_And_Get_Result()
        {
            var appCtx = new ApplicationContext(
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var ctrl = new TestSurfaceController(umbCtx);

            var result = ctrl.Index();

            Assert.IsNotNull(result);
        }

        [Test]
        public void Umbraco_Context_Not_Null()
        {
            var appCtx = new ApplicationContext(
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            ApplicationContext.EnsureContext(appCtx, true);

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var ctrl = new TestSurfaceController(umbCtx);

            Assert.IsNotNull(ctrl.UmbracoContext);
        }

        [Test]
        public void Umbraco_Helper_Not_Null()
        {
            var appCtx = new ApplicationContext(
                new DatabaseContext(new Mock<IDatabaseFactory>().Object, Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), "test"),
                MockHelper.GetMockedServiceContext(),
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var ctrl = new TestSurfaceController(umbCtx);

            Assert.IsNotNull(ctrl.Umbraco);
        }

        [Test]
        public void Can_Lookup_Content()
        {
            var appCtx = new ApplicationContext(
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == "AutoLegacy")),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var helper = new UmbracoHelper(
                umbCtx,
                Mock.Of<IPublishedContent>(),
                Mock.Of<ITypedPublishedContentQuery>(query => query.TypedContent(It.IsAny<int>()) ==
                    //return mock of IPublishedContent for any call to GetById
                    Mock.Of<IPublishedContent>(content => content.Id == 2)),
                Mock.Of<IDynamicPublishedContentQuery>(),
                Mock.Of<ITagQuery>(),
                Mock.Of<IDataTypeService>(),
                new UrlProvider(umbCtx, Enumerable.Empty<IUrlProvider>()),
                Mock.Of<ICultureDictionary>(),
                Mock.Of<IUmbracoComponentRenderer>(),
                new MembershipHelper(umbCtx, Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>()));

            var ctrl = new TestSurfaceController(umbCtx, helper);
            var result = ctrl.GetContent(2) as PublishedContentResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Content.Id);
        }

        [Test]
        public void Mock_Current_Page()
        {
            var appCtx = new ApplicationContext(
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var webRoutingSettings = Mock.Of<IWebRoutingSection>(section => section.UrlProviderMode == "AutoLegacy");

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == webRoutingSettings),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var content = Mock.Of<IPublishedContent>(publishedContent => publishedContent.Id == 12345);

            var contextBase = umbCtx.HttpContext;
            var pcr = new PublishedContentRequest(new Uri("http://localhost/test"),
                umbCtx.RoutingContext,
                webRoutingSettings,
                s => Enumerable.Empty<string>())
            {
                PublishedContent = content
            };

            var routeDefinition = new RouteDefinition
            {
                PublishedContentRequest = pcr
            };

            var routeData = new RouteData();
            routeData.DataTokens.Add(Umbraco.Core.Constants.Web.UmbracoRouteDefinitionDataToken, routeDefinition);

            var ctrl = new TestSurfaceController(umbCtx, new UmbracoHelper());
            ctrl.ControllerContext = new ControllerContext(contextBase, routeData, ctrl);

            var result = ctrl.GetContentFromCurrentPage() as PublishedContentResult;

            Assert.AreEqual(12345, result.Content.Id);
        }

        public class TestSurfaceController : SurfaceController
        {
            private readonly UmbracoHelper _umbracoHelper;

            public TestSurfaceController(UmbracoContext umbracoContext)
                : base(umbracoContext)
            {
            }

            public TestSurfaceController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper)
                : base(umbracoContext)
            {
                _umbracoHelper = umbracoHelper;
            }

            /// <summary>
            /// Returns an UmbracoHelper object
            /// </summary>
            public override UmbracoHelper Umbraco
            {
                get { return _umbracoHelper ?? base.Umbraco; }
            }

            public ActionResult Index()
            {
                return View();
            }

            public ActionResult GetContent(int id)
            {
                var content = Umbraco.TypedContent(id);

                return new PublishedContentResult(content);
            }

            public ActionResult GetContentFromCurrentPage()
            {
                var content = CurrentPage;

                return new PublishedContentResult(content);
            }
        }

        public class PublishedContentResult : ActionResult
        {
            public IPublishedContent Content { get; set; }

            public PublishedContentResult(IPublishedContent content)
            {
                Content = content;
            }

            public override void ExecuteResult(ControllerContext context)
            {
            }

        }
    }
}