using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Current = Umbraco.Core.DI.Current;

namespace Umbraco.Tests.Web.Mvc
{
    [TestFixture]
    public class SurfaceControllerTests
    {
        [SetUp]
        public void SetUp()
        {
            Umbraco.Web.Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();
        }

        [TearDown]
        public void TearDown()
        {
            Current.Reset();
        }

        [Test]
        public void Can_Construct_And_Get_Result()
        {
            //var appCtx = new ApplicationContext(
            //    CacheHelper.CreateDisabledCacheHelper(),
            //    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                Mock.Of<IFacadeService>(),
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var ctrl = new TestSurfaceController { UmbracoContext = umbCtx };

            var result = ctrl.Index();

            Assert.IsNotNull(result);
        }

        [Test]
        public void Umbraco_Context_Not_Null()
        {
            //var appCtx = new ApplicationContext(
            //    CacheHelper.CreateDisabledCacheHelper(),
            //    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            //Current.ApplicationContext = appCtx; // FIXME horrible

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                Mock.Of<IFacadeService>(),
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var ctrl = new TestSurfaceController { UmbracoContext = umbCtx };

            Assert.IsNotNull(ctrl.UmbracoContext);
        }

        [Test]
        public void Umbraco_Helper_Not_Null()
        {
            var databaseFactory = TestObjects.GetIDatabaseFactoryMock();

            //var appCtx = new ApplicationContext(
            //    new DatabaseContext(databaseFactory, Mock.Of<ILogger>(), Mock.Of<IRuntimeState>(), Mock.Of<IMigrationEntryService>()),
            //    TestObjects.GetServiceContextMock(),
            //    CacheHelper.CreateDisabledCacheHelper(),
            //    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                Mock.Of<IFacadeService>(),
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var ctrl = new TestSurfaceController { UmbracoContext = umbCtx };

            Assert.IsNotNull(ctrl.Umbraco);
        }

        [Test]
        public void Can_Lookup_Content()
        {
            //var appCtx = new ApplicationContext(
            //    CacheHelper.CreateDisabledCacheHelper(),
            //    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var facade = new Mock<IFacade>();
            facade.Setup(x => x.MemberCache).Returns(Mock.Of<IPublishedMemberCache>());
            var facadeService = new Mock<IFacadeService>();
            facadeService.Setup(x => x.CreateFacade(It.IsAny<string>())).Returns(facade.Object);

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                facadeService.Object,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == "AutoLegacy")),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var helper = new UmbracoHelper(
                umbCtx,
                Mock.Of<IPublishedContent>(),
                Mock.Of<IPublishedContentQuery>(query => query.Content(It.IsAny<int>()) ==
                                                         //return mock of IPublishedContent for any call to GetById
                                                         Mock.Of<IPublishedContent>(content => content.Id == 2)),
                Mock.Of<ITagQuery>(),
                Mock.Of<IDataTypeService>(),
                new UrlProvider(umbCtx, Enumerable.Empty<IUrlProvider>()),
                Mock.Of<ICultureDictionary>(),
                Mock.Of<IUmbracoComponentRenderer>(),
                new MembershipHelper(umbCtx, Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>()),
                new ServiceContext(),
                CacheHelper.CreateDisabledCacheHelper());

            var ctrl = new TestSurfaceController { UmbracoContext = umbCtx, Umbraco = helper };
            var result = ctrl.GetContent(2) as PublishedContentResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Content.Id);
        }

        [Test]
        public void Mock_Current_Page()
        {
            //var appCtx = new ApplicationContext(
            //    CacheHelper.CreateDisabledCacheHelper(),
            //    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            var webRoutingSettings = Mock.Of<IWebRoutingSection>(section => section.UrlProviderMode == "AutoLegacy");

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                Mock.Of<IFacadeService>(),
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == webRoutingSettings),
                Enumerable.Empty<IUrlProvider>(),
                true);

            var content = Mock.Of<IPublishedContent>(publishedContent => publishedContent.Id == 12345);

            var contextBase = umbCtx.HttpContext;
            var facadeRouter = BaseWebTest.CreateFacadeRouter();
            var frequest = facadeRouter.CreateRequest(umbCtx, new Uri("http://localhost/test"));
            frequest.PublishedContent = content;

            var routeDefinition = new RouteDefinition
            {
                PublishedContentRequest = frequest
            };

            var routeData = new RouteData();
            routeData.DataTokens.Add(Core.Constants.Web.UmbracoRouteDefinitionDataToken, routeDefinition);

            var ctrl = new TestSurfaceController { UmbracoContext = umbCtx, Umbraco = new UmbracoHelper() };
            ctrl.ControllerContext = new ControllerContext(contextBase, routeData, ctrl);

            var result = ctrl.GetContentFromCurrentPage() as PublishedContentResult;

            Assert.AreEqual(12345, result.Content.Id);
        }

        public class TestSurfaceController : SurfaceController
        {
            public ActionResult Index()
            {
                return View();
            }

            public ActionResult GetContent(int id)
            {
                var content = Umbraco.Content(id);

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