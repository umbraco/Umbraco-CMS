using System.CodeDom;
using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.ObjectResolution;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Mvc
{
    [TestFixture]
    public class SurfaceControllerTests
    {
        [Test]
        public void Can_Construct_And_Get_Result()
        {
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());
            ApplicationContext.EnsureContext(appCtx, true);

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                true);

            var ctrl = new TestSurfaceController(umbCtx);

            var result = ctrl.Index();

            Assert.IsNotNull(result);
        }

        [Test]
        public void Umbraco_Context_Not_Null()
        {
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());
            ApplicationContext.EnsureContext(appCtx, true);

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                true);

            var ctrl = new TestSurfaceController(umbCtx);

            Assert.IsNotNull(ctrl.UmbracoContext);
        }

        [Test]
        public void Umbraco_Helper_Not_Null()
        {
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());
            ApplicationContext.EnsureContext(appCtx, true);

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                true);

            var ctrl = new TestSurfaceController(umbCtx);

            Assert.IsNotNull(ctrl.Umbraco);
        }

        [Test]
        public void Can_Lookup_Content()
        {
            //init app context
            
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper());

            //TODO: Need to either make this public or make all methods on the UmbracoHelper or 
            // in v7 the PublishedContentQuery object virtual so we can just mock the methods

            var contentCaches = new Mock<IPublishedCaches>();
            
            //init content resolver
            //TODO: This is not public so people cannot actually do this!
            
            PublishedCachesResolver.Current = new PublishedCachesResolver(contentCaches.Object);

            //init umb context

            var umbCtx = UmbracoContext.EnsureContext(
                new Mock<HttpContextBase>().Object,
                appCtx,
                true);

            //setup the mock

            contentCaches.Setup(caches => caches.CreateContextualContentCache(It.IsAny<UmbracoContext>()))
                .Returns(new ContextualPublishedContentCache(
                    Mock.Of<IPublishedContentCache>(cache =>
                        cache.GetById(It.IsAny<UmbracoContext>(), false, It.IsAny<int>()) ==
                            //return mock of IPublishedContent for any call to GetById
                            Mock.Of<IPublishedContent>(content => content.Id == 2)),
                    umbCtx));

            

            
            
            using (var uTest = new DisposableUmbracoTest(appCtx))
            {
                var ctrl = new TestSurfaceController(uTest.UmbracoContext);
                var result = ctrl.GetContent(2) as PublishedContentResult;

                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Content.Id);    
            }
        }

        public class TestSurfaceController : SurfaceController
        {
            public TestSurfaceController(UmbracoContext umbracoContext)
                : base(umbracoContext)
            {
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