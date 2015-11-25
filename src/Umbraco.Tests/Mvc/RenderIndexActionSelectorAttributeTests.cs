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
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Mvc
{
    [TestFixture]
    public class RenderIndexActionSelectorAttributeTests
    {
        private MethodInfo GetRenderMvcControllerIndexMethodFromCurrentType(Type currType)
        {
            return currType.GetMethods().Single(x =>
            {
                if (x.Name != "Index") return false;
                if (x.ReturnParameter == null || x.ReturnParameter.ParameterType != typeof (ActionResult)) return false;
                var p = x.GetParameters();
                if (p.Length != 1) return false;
                if (p[0].ParameterType != typeof (RenderModel)) return false;
                return true;
            });
        }

        [Test]
        public void Matches_Default_Index()
        {
            var attr = new RenderIndexActionSelectorAttribute();
            var req = new RequestContext();
            var appCtx = new ApplicationContext(
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var umbCtx = UmbracoContext.EnsureContext(
                Mock.Of<HttpContextBase>(),
                appCtx,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);
            var ctrl = new MatchesDefaultIndexController(umbCtx);
            var controllerCtx = new ControllerContext(req, ctrl);
            var result = attr.IsValidForRequest(controllerCtx,
                GetRenderMvcControllerIndexMethodFromCurrentType(ctrl.GetType()));

            Assert.IsTrue(result);
        }

        [Test]
        public void Matches_Overriden_Index()
        {
            var attr = new RenderIndexActionSelectorAttribute();
            var req = new RequestContext();
            var appCtx = new ApplicationContext(
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var umbCtx = UmbracoContext.EnsureContext(
                Mock.Of<HttpContextBase>(),
                appCtx,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);
            var ctrl = new MatchesOverriddenIndexController(umbCtx);
            var controllerCtx = new ControllerContext(req, ctrl);
            var result = attr.IsValidForRequest(controllerCtx,
                GetRenderMvcControllerIndexMethodFromCurrentType(ctrl.GetType()));

            Assert.IsTrue(result);
        }

        [Test]
        public void Matches_Custom_Index()
        {
            var attr = new RenderIndexActionSelectorAttribute();
            var req = new RequestContext();
            var appCtx = new ApplicationContext(
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var umbCtx = UmbracoContext.EnsureContext(
                Mock.Of<HttpContextBase>(),
                appCtx,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);
            var ctrl = new MatchesCustomIndexController(umbCtx);
            var controllerCtx = new ControllerContext(req, ctrl);
            var result = attr.IsValidForRequest(controllerCtx,
                GetRenderMvcControllerIndexMethodFromCurrentType(ctrl.GetType()));

            Assert.IsFalse(result);
        }

        [Test]
        public void Matches_Async_Index_Same_Signature()
        {
            var attr = new RenderIndexActionSelectorAttribute();
            var req = new RequestContext();
            var appCtx = new ApplicationContext(
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            var umbCtx = UmbracoContext.EnsureContext(
                Mock.Of<HttpContextBase>(),
                appCtx,
                new Mock<WebSecurity>(null, null).Object,
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>(),
                true);
            var ctrl = new MatchesAsyncIndexController(umbCtx);
            var controllerCtx = new ControllerContext(req, ctrl);
            var result = attr.IsValidForRequest(controllerCtx,
                GetRenderMvcControllerIndexMethodFromCurrentType(ctrl.GetType()));

            Assert.IsFalse(result);
        }

        public class MatchesDefaultIndexController : RenderMvcController
        {
            public MatchesDefaultIndexController(UmbracoContext umbracoContext) : base(umbracoContext)
            {
            }
        }

        public class MatchesOverriddenIndexController : RenderMvcController
        {
            public MatchesOverriddenIndexController(UmbracoContext umbracoContext) : base(umbracoContext)
            {
            }
            
            public override ActionResult Index(RenderModel model)
            {
                return base.Index(model);
            }
        }

        public class MatchesCustomIndexController : RenderMvcController
        {
            public MatchesCustomIndexController(UmbracoContext umbracoContext) : base(umbracoContext)
            {
            }

            public ActionResult Index(RenderModel model, int page)
            {
                return base.Index(model);
            }
        }

        public class MatchesAsyncIndexController : RenderMvcController
        {
            public MatchesAsyncIndexController(UmbracoContext umbracoContext) : base(umbracoContext)
            {
            }

            public new async Task<ActionResult> Index(RenderModel model)
            {
                return await Task.FromResult(base.Index(model));
            }
        }
    }
}