using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Web.Mvc
{
    [TestFixture]
    [UmbracoTest(WithApplication = true)]
    public class SurfaceControllerTests : UmbracoTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();
        }

        [Test]
        public void Can_Construct_And_Get_Result()
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            var umbracoContext = UmbracoContext.EnsureContext(
                Current.UmbracoContextAccessor,
                new Mock<HttpContextBase>().Object,
                Mock.Of<IPublishedSnapshotService>(),
                new Mock<WebSecurity>(null, null, globalSettings).Object,
                TestObjects.GetUmbracoSettings(),
                Enumerable.Empty<IUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor(),
                true);

            var ctrl = new TestSurfaceController { UmbracoContext = umbracoContext };

            var result = ctrl.Index();

            Assert.IsNotNull(result);
        }

        [Test]
        public void Umbraco_Context_Not_Null()
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            var umbCtx = UmbracoContext.EnsureContext(
                Current.UmbracoContextAccessor,
                new Mock<HttpContextBase>().Object,
                Mock.Of<IPublishedSnapshotService>(),
                new Mock<WebSecurity>(null, null, globalSettings).Object,
                TestObjects.GetUmbracoSettings(),
                Enumerable.Empty<IUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor(),
                true);

            var ctrl = new TestSurfaceController { UmbracoContext = umbCtx };

            Assert.IsNotNull(ctrl.UmbracoContext);
        }

        [Test]
        public void Umbraco_Helper_Not_Null()
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            var umbracoContext = UmbracoContext.EnsureContext(
                Current.UmbracoContextAccessor,
                new Mock<HttpContextBase>().Object,
                Mock.Of<IPublishedSnapshotService>(),
                new Mock<WebSecurity>(null, null, globalSettings).Object,
                TestObjects.GetUmbracoSettings(),
                Enumerable.Empty<IUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor(),
                true);

            var controller = new TestSurfaceController { UmbracoContext = umbracoContext };
            Container.Register(_ => umbracoContext);
            Container.InjectProperties(controller);

            Assert.IsNotNull(controller.Umbraco);
        }

        [Test]
        public void Can_Lookup_Content()
        {
            var publishedSnapshot = new Mock<IPublishedSnapshot>();
            publishedSnapshot.Setup(x => x.Members).Returns(Mock.Of<IPublishedMemberCache>());
            var contentCache = new Mock<IPublishedContentCache>();
            var content = new Mock<IPublishedContent>();
            content.Setup(x => x.Id).Returns(2);
            contentCache.Setup(x => x.GetById(It.IsAny<int>())).Returns(content.Object);
            var mediaCache = new Mock<IPublishedMediaCache>();
            publishedSnapshot.Setup(x => x.Content).Returns(contentCache.Object);
            publishedSnapshot.Setup(x => x.Media).Returns(mediaCache.Object);
            var publishedSnapshotService = new Mock<IPublishedSnapshotService>();
            publishedSnapshotService.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>())).Returns(publishedSnapshot.Object);
            var globalSettings = TestObjects.GetGlobalSettings();

            var umbracoContext = UmbracoContext.EnsureContext(
                Current.UmbracoContextAccessor,
                new Mock<HttpContextBase>().Object,
                publishedSnapshotService.Object,
                new Mock<WebSecurity>(null, null, globalSettings).Object,
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == "AutoLegacy")),
                Enumerable.Empty<IUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor(),
                true);

            var helper = new UmbracoHelper(
                umbracoContext,
                Mock.Of<IPublishedContent>(),
                Mock.Of<ITagQuery>(),
                Mock.Of<ICultureDictionary>(),
                Mock.Of<IUmbracoComponentRenderer>(),
                new MembershipHelper(umbracoContext, Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>()),
                new ServiceContext());

            var ctrl = new TestSurfaceController { UmbracoContext = umbracoContext, Umbraco = helper };
            var result = ctrl.GetContent(2) as PublishedContentResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Content.Id);
        }

        [Test]
        public void Mock_Current_Page()
        {
            var webRoutingSettings = Mock.Of<IWebRoutingSection>(section => section.UrlProviderMode == "AutoLegacy");
            var globalSettings = TestObjects.GetGlobalSettings();

            var umbracoContext = UmbracoContext.EnsureContext(
                Current.UmbracoContextAccessor,
                new Mock<HttpContextBase>().Object,
                Mock.Of<IPublishedSnapshotService>(),
                new Mock<WebSecurity>(null, null, globalSettings).Object,
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == webRoutingSettings),
                Enumerable.Empty<IUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor(),
                true);

            var content = Mock.Of<IPublishedContent>(publishedContent => publishedContent.Id == 12345);

            var contextBase = umbracoContext.HttpContext;
            var publishedRouter = BaseWebTest.CreatePublishedRouter(TestObjects.GetUmbracoSettings().WebRouting);
            var frequest = publishedRouter.CreateRequest(umbracoContext, new Uri("http://localhost/test"));
            frequest.PublishedContent = content;

            var routeDefinition = new RouteDefinition
            {
                PublishedRequest = frequest
            };

            var routeData = new RouteData();
            routeData.DataTokens.Add(Core.Constants.Web.UmbracoRouteDefinitionDataToken, routeDefinition);

            var ctrl = new TestSurfaceController { UmbracoContext = umbracoContext, Umbraco = new UmbracoHelper() };
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
