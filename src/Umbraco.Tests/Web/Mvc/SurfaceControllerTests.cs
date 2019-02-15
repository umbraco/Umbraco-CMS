using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
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

            var umbracoContextFactory = new UmbracoContextFactory(
                Current.UmbracoContextAccessor,
                Mock.Of<IPublishedSnapshotService>(),
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                TestObjects.GetUmbracoSettings(),
                globalSettings,
                Enumerable.Empty<IUrlProvider>(),
                Mock.Of<IUserService>());

            var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext(Mock.Of<HttpContextBase>());
            var umbracoContext = umbracoContextReference.UmbracoContext;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);

            var ctrl = new TestSurfaceController(umbracoContextAccessor);

            var result = ctrl.Index();

            Assert.IsNotNull(result);
        }

        [Test]
        public void Umbraco_Context_Not_Null()
        {
            var globalSettings = TestObjects.GetGlobalSettings();

            var umbracoContextFactory = new UmbracoContextFactory(
                Current.UmbracoContextAccessor,
                Mock.Of<IPublishedSnapshotService>(),
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                TestObjects.GetUmbracoSettings(),
                globalSettings,
                Enumerable.Empty<IUrlProvider>(),
                Mock.Of<IUserService>());

            var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext(Mock.Of<HttpContextBase>());
            var umbCtx = umbracoContextReference.UmbracoContext;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbCtx);

            var ctrl = new TestSurfaceController(umbracoContextAccessor);

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

            var umbracoContextFactory = new UmbracoContextFactory(
                Current.UmbracoContextAccessor,
                publishedSnapshotService.Object,
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == "Auto")),
                globalSettings,
                Enumerable.Empty<IUrlProvider>(),
                Mock.Of<IUserService>());

            var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext(Mock.Of<HttpContextBase>());
            var umbracoContext = umbracoContextReference.UmbracoContext;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);

            var helper = new UmbracoHelper(
                umbracoContext,
                Mock.Of<ITagQuery>(),
                Mock.Of<ICultureDictionaryFactory>(),
                Mock.Of<IUmbracoComponentRenderer>(),
                Mock.Of<IPublishedContentQuery>(query => query.Content(2) == content.Object),
                new MembershipHelper(new TestUmbracoContextAccessor(umbracoContext), Mock.Of<MembershipProvider>(), Mock.Of<RoleProvider>(), Mock.Of<IMemberService>(), Mock.Of<IMemberTypeService>(), Mock.Of<IUserService>(), Mock.Of<IPublicAccessService>(), Mock.Of<AppCaches>(), Mock.Of<ILogger>()));

            var ctrl = new TestSurfaceController(umbracoContextAccessor, helper);
            var result = ctrl.GetContent(2) as PublishedContentResult;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Content);
            Assert.AreEqual(2, result.Content.Id);
        }

        [Test]
        public void Mock_Current_Page()
        {
            var webRoutingSettings = Mock.Of<IWebRoutingSection>(section => section.UrlProviderMode == "Auto");
            var globalSettings = TestObjects.GetGlobalSettings();

            var umbracoContextFactory = new UmbracoContextFactory(
                Current.UmbracoContextAccessor,
                Mock.Of<IPublishedSnapshotService>(),
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == webRoutingSettings),
                globalSettings,
                Enumerable.Empty<IUrlProvider>(),
                Mock.Of<IUserService>());

            var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext(Mock.Of<HttpContextBase>());
            var umbracoContext = umbracoContextReference.UmbracoContext;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);

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

            var ctrl = new TestSurfaceController(umbracoContextAccessor, new UmbracoHelper());
            ctrl.ControllerContext = new ControllerContext(contextBase, routeData, ctrl);

            var result = ctrl.GetContentFromCurrentPage() as PublishedContentResult;

            Assert.AreEqual(12345, result.Content.Id);
        }

        public class TestSurfaceController : SurfaceController
        {
            public TestSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, UmbracoHelper helper = null)
                : base(umbracoContextAccessor, null, ServiceContext.CreatePartial(), Mock.Of<AppCaches>(), null, null, helper)
            {
            }

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
