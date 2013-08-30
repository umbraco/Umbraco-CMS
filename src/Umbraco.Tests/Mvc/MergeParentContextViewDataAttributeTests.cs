using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Mvc;

namespace Umbraco.Tests.Mvc
{
    [TestFixture]
    public class MergeParentContextViewDataAttributeTests
    {
        [Test]
        public void Ensure_All_Ancestor_ViewData_Is_Merged()
        {
            var http = new FakeHttpContextFactory("http://localhost");

            //setup an heirarchy
            var rootViewCtx = new ViewContext {Controller = new MyController(), RequestContext = http.RequestContext, ViewData = new ViewDataDictionary()};
            var parentViewCtx = new ViewContext { Controller = new MyController(), RequestContext = http.RequestContext, RouteData = new RouteData(), ViewData = new ViewDataDictionary() };
            parentViewCtx.RouteData.DataTokens.Add("ParentActionViewContext", rootViewCtx);
            var controllerCtx = new ControllerContext(http.RequestContext, new MyController()) {RouteData = new RouteData()};
            controllerCtx.RouteData.DataTokens.Add("ParentActionViewContext", parentViewCtx);

            //set up the view data
            controllerCtx.Controller.ViewData["Test1"] = "Test1";
            controllerCtx.Controller.ViewData["Test2"] = "Test2";
            controllerCtx.Controller.ViewData["Test3"] = "Test3";
            parentViewCtx.ViewData["Test4"] = "Test4";
            parentViewCtx.ViewData["Test5"] = "Test5";
            parentViewCtx.ViewData["Test6"] = "Test6";
            rootViewCtx.ViewData["Test7"] = "Test7";
            rootViewCtx.ViewData["Test8"] = "Test8";
            rootViewCtx.ViewData["Test9"] = "Test9";

            var filter = new ResultExecutingContext(controllerCtx, new ContentResult()) {RouteData = controllerCtx.RouteData};
            var att = new MergeParentContextViewDataAttribute();
            
            Assert.IsTrue(filter.IsChildAction);
            att.OnResultExecuting(filter);

            Assert.AreEqual(9, controllerCtx.Controller.ViewData.Count);
        }

        [Test]
        public void Ensure_All_Ancestor_ViewData_Is_Merged_Without_Data_Loss()
        {
            var http = new FakeHttpContextFactory("http://localhost");

            //setup an heirarchy
            var rootViewCtx = new ViewContext { Controller = new MyController(), RequestContext = http.RequestContext, ViewData = new ViewDataDictionary() };
            var parentViewCtx = new ViewContext { Controller = new MyController(), RequestContext = http.RequestContext, RouteData = new RouteData(), ViewData = new ViewDataDictionary() };
            parentViewCtx.RouteData.DataTokens.Add("ParentActionViewContext", rootViewCtx);
            var controllerCtx = new ControllerContext(http.RequestContext, new MyController()) { RouteData = new RouteData() };
            controllerCtx.RouteData.DataTokens.Add("ParentActionViewContext", parentViewCtx);

            //set up the view data with overlapping keys
            controllerCtx.Controller.ViewData["Test1"] = "Test1";
            controllerCtx.Controller.ViewData["Test2"] = "Test2";
            controllerCtx.Controller.ViewData["Test3"] = "Test3";
            parentViewCtx.ViewData["Test2"] = "Test4";
            parentViewCtx.ViewData["Test3"] = "Test5";
            parentViewCtx.ViewData["Test4"] = "Test6";
            rootViewCtx.ViewData["Test3"] = "Test7";
            rootViewCtx.ViewData["Test4"] = "Test8";
            rootViewCtx.ViewData["Test5"] = "Test9";

            var filter = new ResultExecutingContext(controllerCtx, new ContentResult()) { RouteData = controllerCtx.RouteData };
            var att = new MergeParentContextViewDataAttribute();

            Assert.IsTrue(filter.IsChildAction);
            att.OnResultExecuting(filter);

            Assert.AreEqual(5, controllerCtx.Controller.ViewData.Count);
            Assert.AreEqual("Test1", controllerCtx.Controller.ViewData["Test1"]);
            Assert.AreEqual("Test2", controllerCtx.Controller.ViewData["Test2"]);
            Assert.AreEqual("Test3", controllerCtx.Controller.ViewData["Test3"]);
            Assert.AreEqual("Test6", controllerCtx.Controller.ViewData["Test4"]);
            Assert.AreEqual("Test9", controllerCtx.Controller.ViewData["Test5"]);
        }

        internal class MyController : Controller
        {
            
        }
    }
}