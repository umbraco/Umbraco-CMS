using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Extensions;
using Umbraco.Web;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Website.Routing;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Website.Routing
{
    [TestFixture]
    public class HijackedRouteEvaluatorTests
    {
        private class TestActionDescriptorCollectionProvider : ActionDescriptorCollectionProvider
        {
            private readonly IEnumerable<ActionDescriptor> _actions;

            public TestActionDescriptorCollectionProvider(IEnumerable<ActionDescriptor> actions) => _actions = actions;

            public override ActionDescriptorCollection ActionDescriptors => new ActionDescriptorCollection(_actions.ToList(), 1);

            public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;
        }

        private IActionDescriptorCollectionProvider GetActionDescriptors() => new TestActionDescriptorCollectionProvider(
            new ActionDescriptor[]
            {
                new ControllerActionDescriptor
                {
                    ActionName = "Index",
                    ControllerName = ControllerExtensions.GetControllerName<RenderController>(),
                    ControllerTypeInfo = typeof(RenderController).GetTypeInfo()
                },
                new ControllerActionDescriptor
                {
                    ActionName = "Index",
                    ControllerName = ControllerExtensions.GetControllerName<Render1Controller>(),
                    ControllerTypeInfo = typeof(Render1Controller).GetTypeInfo()
                },
                new ControllerActionDescriptor
                {
                    ActionName = "Index",
                    ControllerName = ControllerExtensions.GetControllerName<Render2Controller>(),
                    ControllerTypeInfo = typeof(Render2Controller).GetTypeInfo()
                }
            });

        private class Render1Controller : ControllerBase, IRenderController
        {
            public IActionResult Index => Content("hello world");
        }

        private class Render2Controller : RenderController
        {
            public Render2Controller(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor)
                : base(logger, compositeViewEngine, umbracoContextAccessor)
            {
            }
        }

        [TestCase("index", "Render", "Index", true)]
        [TestCase("Index", "Render1", "Index", true)]
        [TestCase("Index", "render2", "Index", true)]
        [TestCase("NotFound", "Render", "Index", true)]
        [TestCase("NotFound", "Render1", "Index", true)]
        [TestCase("NotFound", "Render2", "Index", true)]
        public void Matches_Controller(string action, string controller, string resultAction, bool matches)
        {
            var evaluator = new HijackedRouteEvaluator(
                new NullLogger<HijackedRouteEvaluator>(),
                GetActionDescriptors());

            HijackedRouteResult result = evaluator.Evaluate(controller, action);
            Assert.AreEqual(matches, result.Success);
            if (matches)
            {
                Assert.IsTrue(result.ActionName.InvariantEquals(resultAction), "expected {0} does not match resulting action {1}", resultAction, result.ActionName);
                Assert.IsTrue(result.ControllerName.InvariantEquals(controller), "expected {0} does not match resulting controller {1}", controller, result.ControllerName);
            }
        }
    }
}
