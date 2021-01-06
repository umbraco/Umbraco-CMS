using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Tests.Common;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Current = Umbraco.Web.Composing.Current;

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
            var httpContextAccessor = TestHelper.GetHttpContextAccessor();

            var umbracoContextFactory = new UmbracoContextFactory(
                Current.UmbracoContextAccessor,
                Mock.Of<IPublishedSnapshotService>(),
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                globalSettings,
                Mock.Of<IUserService>(),
                HostingEnvironment,
                UriUtility,
                httpContextAccessor,
                new AspNetCookieManager(httpContextAccessor));

            var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
            var umbracoContext = umbracoContextReference.UmbracoContext;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);

            var ctrl = new TestSurfaceController(umbracoContextAccessor, Mock.Of<IPublishedContentQuery>());

            var result = ctrl.Index();

            Assert.IsNotNull(result);
        }

        [Test]
        public void Umbraco_Context_Not_Null()
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            var httpContextAccessor = TestHelper.GetHttpContextAccessor();

            var umbracoContextFactory = new UmbracoContextFactory(
                Current.UmbracoContextAccessor,
                Mock.Of<IPublishedSnapshotService>(),
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                globalSettings,
                Mock.Of<IUserService>(),
                HostingEnvironment,
                UriUtility,
                httpContextAccessor,
                new AspNetCookieManager(httpContextAccessor));

            var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
            var umbCtx = umbracoContextReference.UmbracoContext;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbCtx);

            var ctrl = new TestSurfaceController(umbracoContextAccessor, Mock.Of<IPublishedContentQuery>());

            Assert.IsNotNull(ctrl.UmbracoContext);
        }

        [Test]
        public void Can_Lookup_Content()
        {
            var publishedSnapshot = new Mock<IPublishedSnapshot>();
            publishedSnapshot.Setup(x => x.Members).Returns(Mock.Of<IPublishedMemberCache>());
            var content = new Mock<IPublishedContent>();
            content.Setup(x => x.Id).Returns(2);
            var publishedSnapshotService = new Mock<IPublishedSnapshotService>();
            var globalSettings = TestObjects.GetGlobalSettings();
            var httpContextAccessor = TestHelper.GetHttpContextAccessor();

            var umbracoContextFactory = new UmbracoContextFactory(
                Current.UmbracoContextAccessor,
                publishedSnapshotService.Object,
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                globalSettings,
                Mock.Of<IUserService>(),
                HostingEnvironment,
                UriUtility,
                httpContextAccessor,
                new AspNetCookieManager(httpContextAccessor));

            var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
            var umbracoContext = umbracoContextReference.UmbracoContext;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);

            var publishedContentQuery = Mock.Of<IPublishedContentQuery>(query => query.Content(2) == content.Object);

            var ctrl = new TestSurfaceController(umbracoContextAccessor,publishedContentQuery);
            var result = ctrl.GetContent(2) as PublishedContentResult;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Content);
            Assert.AreEqual(2, result.Content.Id);
        }

        [Test]
        public void Mock_Current_Page()
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            var httpContextAccessor = TestHelper.GetHttpContextAccessor();

            var umbracoContextFactory = new UmbracoContextFactory(
                Current.UmbracoContextAccessor,
                Mock.Of<IPublishedSnapshotService>(),
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                globalSettings,
                Mock.Of<IUserService>(),
                HostingEnvironment,
                UriUtility,
                httpContextAccessor,
                new AspNetCookieManager(httpContextAccessor));

            var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
            var umbracoContext = umbracoContextReference.UmbracoContext;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);

            var content = Mock.Of<IPublishedContent>(publishedContent => publishedContent.Id == 12345);

            var webRoutingSettings = new WebRoutingSettings();
            var publishedRouter = BaseWebTest.CreatePublishedRouter(umbracoContextAccessor, webRoutingSettings);
            var frequest = publishedRouter.CreateRequest(new Uri("http://localhost/test"));
            frequest.SetPublishedContent(content);

            var routeDefinition = new RouteDefinition
            {
                PublishedRequest = frequest.Build()
            };

            var routeData = new RouteData();
            routeData.Values.Add(Core.Constants.Web.UmbracoRouteDefinitionDataToken, routeDefinition);

            var ctrl = new TestSurfaceController(umbracoContextAccessor, Mock.Of<IPublishedContentQuery>());
            ctrl.ControllerContext = new ControllerContext(Mock.Of<HttpContextBase>(), routeData, ctrl);

            var result = ctrl.GetContentFromCurrentPage() as PublishedContentResult;

            Assert.AreEqual(12345, result.Content.Id);
        }

        public class TestSurfaceController : SurfaceController
        {
            private readonly IPublishedContentQuery _publishedContentQuery;

            public TestSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IPublishedContentQuery publishedContentQuery)
                : base(umbracoContextAccessor, null, ServiceContext.CreatePartial(), AppCaches.Disabled, null)
            {
                _publishedContentQuery = publishedContentQuery;
            }

            public ActionResult Index()
            {
                // ReSharper disable once Mvc.ViewNotResolved
                return View();
            }

            public ActionResult GetContent(int id)
            {
                var content = _publishedContentQuery.Content(id);

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
