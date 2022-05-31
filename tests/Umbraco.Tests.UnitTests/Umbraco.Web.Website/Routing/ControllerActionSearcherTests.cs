using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Website.Routing;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants.Web.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Routing;

[TestFixture]
public class ControllerActionSearcherTests
{
    private ControllerActionDescriptor GetDescriptor<T>(string action)
        => new()
        {
            ActionName = action,
            ControllerName = ControllerExtensions.GetControllerName<T>(),
            ControllerTypeInfo = typeof(RenderController).GetTypeInfo(),
            DisplayName = $"{ControllerExtensions.GetControllerName<T>()}.{action}",
        };

    private IReadOnlyList<ControllerActionDescriptor> GetActionDescriptors() => new List<ControllerActionDescriptor>
    {
        GetDescriptor<RenderController>(nameof(RenderController.Index)),
        GetDescriptor<Render1Controller>(nameof(Render1Controller.Index)),
        GetDescriptor<Render1Controller>(nameof(Render1Controller.Custom)),
        GetDescriptor<Render2Controller>(nameof(Render2Controller.Index)),
    };

    private class Render1Controller : ControllerBase, IRenderController
    {
        public IActionResult Index => Content("hello world");

        public IActionResult Custom => Content("hello world");
    }

    private class Render2Controller : RenderController
    {
        public Render2Controller(
            ILogger<Render2Controller> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
        }
    }

    [TestCase("Index", "RenderNotFound", null, false)]
    [TestCase("index", "Render", nameof(RenderController.Index), true)]
    [TestCase("Index", "Render1", nameof(RenderController.Index), true)]
    [TestCase("Index", "render2", nameof(Render2Controller.Index), true)]
    [TestCase("NotFound", "Render", nameof(RenderController.Index), true)]
    [TestCase("NotFound", "Render1", nameof(Render1Controller.Index), true)]
    [TestCase("NotFound", "Render2", nameof(Render2Controller.Index), true)]
    [TestCase("Custom", "Render1", nameof(Render1Controller.Custom), true)]
    public void Matches_Controller(string action, string controller, string resultAction, bool matches)
    {
        var descriptors = GetActionDescriptors();

        var actionSelector = new Mock<IActionSelector>();
        actionSelector.Setup(x => x.SelectCandidates(It.IsAny<RouteContext>()))
            .Returns((RouteContext r) =>
            {
                // our own rudimentary search
                var controller = r.RouteData.Values[ControllerToken].ToString();
                var action = r.RouteData.Values[ActionToken].ToString();
                return descriptors.Where(x =>
                    x.ControllerName.InvariantEquals(controller) && x.ActionName.InvariantEquals(action)).ToList();
            });

        var query = new ControllerActionSearcher(
            new NullLogger<ControllerActionSearcher>(),
            actionSelector.Object);

        var httpContext = new DefaultHttpContext();

        var result = query.Find<IRenderController>(httpContext, controller, action);
        Assert.IsTrue(matches == (result != null));
        if (matches)
        {
            Assert.IsTrue(result.ActionName.InvariantEquals(resultAction), "expected {0} does not match resulting action {1}", resultAction, result.ActionName);
            Assert.IsTrue(result.ControllerName.InvariantEquals(controller), "expected {0} does not match resulting controller {1}", controller, result.ControllerName);
        }
    }
}
