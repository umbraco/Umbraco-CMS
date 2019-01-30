using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Tests.Web.Mvc
{
    [TestFixture]
    public class RenderIndexActionSelectorAttributeTests
    {
        [SetUp]
        public void SetUp()
        {
            Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();
            Core.Composing.Current.Factory = Mock.Of<IFactory>();
        }

        [TearDown]
        public void TearDown()
        {
            Current.Reset();
        }

        private TestObjects TestObjects = new TestObjects(null);

        private MethodInfo GetRenderMvcControllerIndexMethodFromCurrentType(Type currType)
        {
            return currType.GetMethods().Single(x =>
            {
                if (x.Name != "Index") return false;
                if (x.ReturnParameter == null || x.ReturnParameter.ParameterType != typeof (ActionResult)) return false;
                var p = x.GetParameters();
                if (p.Length != 1) return false;
                if (p[0].ParameterType != typeof (ContentModel)) return false;
                return true;
            });
        }

        [Test]
        public void Matches_Default_Index()
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            var attr = new RenderIndexActionSelectorAttribute();
            var req = new RequestContext();
            //var appCtx = new ApplicationContext(
            //    CacheHelper.CreateDisabledCacheHelper(),
            //    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var umbCtx = UmbracoContext.EnsureContext(
                Current.UmbracoContextAccessor,
                Mock.Of<HttpContextBase>(),
                Mock.Of<IPublishedSnapshotService>(),
                new Mock<WebSecurity>(null, null, globalSettings).Object,
                TestObjects.GetUmbracoSettings(),
                Enumerable.Empty<IUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor(),
                true);
            var ctrl = new MatchesDefaultIndexController { UmbracoContext = umbCtx };
            var controllerCtx = new ControllerContext(req, ctrl);
            var result = attr.IsValidForRequest(controllerCtx,
                GetRenderMvcControllerIndexMethodFromCurrentType(ctrl.GetType()));

            Assert.IsTrue(result);
        }

        [Test]
        public void Matches_Overriden_Index()
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            var attr = new RenderIndexActionSelectorAttribute();
            var req = new RequestContext();
            var umbCtx = UmbracoContext.EnsureContext(
                Current.UmbracoContextAccessor,
                Mock.Of<HttpContextBase>(),
                Mock.Of<IPublishedSnapshotService>(),
                new Mock<WebSecurity>(null, null, globalSettings).Object,
                TestObjects.GetUmbracoSettings(),
                Enumerable.Empty<IUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor(),
                true);
            var ctrl = new MatchesOverriddenIndexController { UmbracoContext = umbCtx };
            var controllerCtx = new ControllerContext(req, ctrl);
            var result = attr.IsValidForRequest(controllerCtx,
                GetRenderMvcControllerIndexMethodFromCurrentType(ctrl.GetType()));

            Assert.IsTrue(result);
        }

        [Test]
        public void Matches_Custom_Index()
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            var attr = new RenderIndexActionSelectorAttribute();
            var req = new RequestContext();
            var umbCtx = UmbracoContext.EnsureContext(
                Current.UmbracoContextAccessor,
                Mock.Of<HttpContextBase>(),
                Mock.Of<IPublishedSnapshotService>(),
                new Mock<WebSecurity>(null, null, globalSettings).Object,
                TestObjects.GetUmbracoSettings(),
                Enumerable.Empty<IUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor(),
                true);
            var ctrl = new MatchesCustomIndexController { UmbracoContext = umbCtx };
            var controllerCtx = new ControllerContext(req, ctrl);
            var result = attr.IsValidForRequest(controllerCtx,
                GetRenderMvcControllerIndexMethodFromCurrentType(ctrl.GetType()));

            Assert.IsFalse(result);
        }

        [Test]
        public void Matches_Async_Index_Same_Signature()
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            var attr = new RenderIndexActionSelectorAttribute();
            var req = new RequestContext();
            var umbCtx = UmbracoContext.EnsureContext(
                Current.UmbracoContextAccessor,
                Mock.Of<HttpContextBase>(),
                Mock.Of<IPublishedSnapshotService>(),
                new Mock<WebSecurity>(null, null, globalSettings).Object,
                TestObjects.GetUmbracoSettings(),
                Enumerable.Empty<IUrlProvider>(),
                globalSettings,
                new TestVariationContextAccessor(),
                true);
            var ctrl = new MatchesAsyncIndexController { UmbracoContext = umbCtx };
            var controllerCtx = new ControllerContext(req, ctrl);
            var result = attr.IsValidForRequest(controllerCtx,
                GetRenderMvcControllerIndexMethodFromCurrentType(ctrl.GetType()));

            Assert.IsFalse(result);
        }

        public class MatchesDefaultIndexController : RenderMvcController
        {
        }

        public class MatchesOverriddenIndexController : RenderMvcController
        {
            public override ActionResult Index(ContentModel model)
            {
                return base.Index(model);
            }
        }

        public class MatchesCustomIndexController : RenderMvcController
        {
            public ActionResult Index(ContentModel model, int page)
            {
                return base.Index(model);
            }
        }

        public class MatchesAsyncIndexController : RenderMvcController
        {
            public new async Task<ActionResult> Index(ContentModel model)
            {
                return await Task.FromResult(base.Index(model));
            }
        }
    }
}
