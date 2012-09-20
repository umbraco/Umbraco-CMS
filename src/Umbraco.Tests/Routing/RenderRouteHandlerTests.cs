using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Tests.Stubs;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class RenderRouteHandlerTests : BaseRoutingTest
	{

		public override void Initialize()
		{
			base.Initialize();
			var webBoot = new WebBootManager(new UmbracoApplication());
			webBoot.CreateRoutes();
		}

		public override void TearDown()
		{
			base.TearDown();
			RouteTable.Routes.Clear();
		}

		/// <summary>
		/// Will route to the default controller and action since no custom controller is defined for this node route
		/// </summary>
		[Test]
		public void Umbraco_Route_Umbraco_Defined_Controller_Action()
		{
			var template = Template.MakeNew("homePage", new User(0));
			var route = RouteTable.Routes["Umbraco_default"];
			var routeData = new RouteData() { Route = route };
			var routingContext = GetRoutingContext("~/dummy-page", template, routeData);
			var docRequest = new DocumentRequest(routingContext.UmbracoContext.UmbracoUrl, routingContext)
			{
				Document = routingContext.PublishedContentStore.GetDocumentById(routingContext.UmbracoContext, 1174),
				Template = template
			};

			var handler = new RenderRouteHandler(new TestControllerFactory());

			handler.GetHandlerForRoute(routingContext.UmbracoContext.HttpContext.Request.RequestContext, docRequest);
			Assert.AreEqual("RenderMvc", routeData.Values["controller"].ToString());
			Assert.AreEqual("Index", routeData.Values["action"].ToString());
		}

		//test all template name styles to match the ActionName
		[TestCase("home-page")]
		[TestCase("Home-Page")]
		[TestCase("HomePage")]
		[TestCase("homePage")]
		public void Umbraco_Route_User_Defined_Controller_Action(string templateName)
		{
			var template = Template.MakeNew(templateName, new User(0));
			var route = RouteTable.Routes["Umbraco_default"];
			var routeData = new RouteData() {Route = route};
			var routingContext = GetRoutingContext("~/dummy-page", template, routeData);
			var docRequest = new DocumentRequest(routingContext.UmbracoContext.UmbracoUrl, routingContext)
				{
					Document = routingContext.PublishedContentStore.GetDocumentById(routingContext.UmbracoContext, 1172), 
					Template = template
				};

			var handler = new RenderRouteHandler(new TestControllerFactory());

			handler.GetHandlerForRoute(routingContext.UmbracoContext.HttpContext.Request.RequestContext, docRequest);
			Assert.AreEqual("CustomDocument", routeData.Values["controller"].ToString());
			Assert.AreEqual("HomePage", routeData.Values["action"].ToString());
		}


		#region Internal classes

		///// <summary>
		///// Used to test a user route (non-umbraco)
		///// </summary>
		//private class CustomUserController : Controller
		//{

		//    public ActionResult Index()
		//    {
		//        return View();
		//    }

		//    public ActionResult Test(int id)
		//    {
		//        return View();
		//    }

		//}

		/// <summary>
		/// Used to test a user route umbraco route
		/// </summary>
		public class CustomDocumentController : RenderMvcController
		{

			public ActionResult HomePage(RenderModel model)
			{
				return View();
			}

		}

		#endregion
	}
}
