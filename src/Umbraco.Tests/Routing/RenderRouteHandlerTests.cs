using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.WebApi;
using umbraco.BusinessLogic;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.Routing
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
	[TestFixture]
	public class RenderRouteHandlerTests : BaseRoutingTest
	{

		public override void Initialize()
		{                       
			base.Initialize();

		    SettingsForTests.UmbracoPath = "~/umbraco";
            
			var webBoot = new WebBootManager(new UmbracoApplication(), true);
			//webBoot.Initialize();
			//webBoot.Startup(null); -> don't call startup, we don't want any other application event handlers to bind for this test.
			//webBoot.Complete(null);
			webBoot.CreateRoutes();
		}

        protected override void FreezeResolution()
        {
            DefaultRenderMvcControllerResolver.Current = new DefaultRenderMvcControllerResolver(typeof(RenderMvcController));

            SurfaceControllerResolver.Current = new SurfaceControllerResolver(
                new ActivatorServiceProvider(), Logger,
                PluginManager.Current.ResolveSurfaceControllers());
            UmbracoApiControllerResolver.Current = new UmbracoApiControllerResolver(
                new ActivatorServiceProvider(), Logger,
                PluginManager.Current.ResolveUmbracoApiControllers());
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(new LegacyShortStringHelper());

            base.FreezeResolution();
        }

		public override void TearDown()
		{
			base.TearDown();
		    UmbracoContext.Current = null;
			RouteTable.Routes.Clear();			
		}

        Template CreateTemplate(string alias)
        {
            var path = "template";
            var name = "Template";
            var template = new Template(path, name, alias);
            template.Content = ""; // else saving throws with a dirty internal error
            ApplicationContext.Services.FileService.SaveTemplate(template);
            return template;
        }

		/// <summary>
		/// Will route to the default controller and action since no custom controller is defined for this node route
		/// </summary>
		[Test]
		public void Umbraco_Route_Umbraco_Defined_Controller_Action()
		{
            var template = CreateTemplate("homePage");
			var route = RouteTable.Routes["Umbraco_default"];
			var routeData = new RouteData() { Route = route };
			var routingContext = GetRoutingContext("~/dummy-page", template.Id, routeData);
			var docRequest = new PublishedContentRequest(routingContext.UmbracoContext.CleanedUmbracoUrl, routingContext)
			{
                PublishedContent = routingContext.UmbracoContext.ContentCache.GetById(1174),
				TemplateModel = template,
                RenderingEngine = RenderingEngine.Mvc
			};

			var handler = new RenderRouteHandler(
                new TestControllerFactory(routingContext.UmbracoContext, Mock.Of<ILogger>()), 
                routingContext.UmbracoContext);

			handler.GetHandlerForRoute(routingContext.UmbracoContext.HttpContext.Request.RequestContext, docRequest);
			Assert.AreEqual("RenderMvc", routeData.Values["controller"].ToString());
            //the route action will still be the one we've asked for because our RenderActionInvoker is the thing that decides
            // if the action matches.
            Assert.AreEqual("homePage", routeData.Values["action"].ToString());
		}

		//test all template name styles to match the ActionName

        //[TestCase("home-\\234^^*32page")]        //TODO: This fails!
        [TestCase("home-page")]
        [TestCase("home-page")]
        [TestCase("home-page")]
		[TestCase("Home-Page")] 
		[TestCase("HomePage")]
		[TestCase("homePage")]
        [TestCase("site1/template2")]
        [TestCase("site1\\template2")]
        public void Umbraco_Route_User_Defined_Controller_Action(string templateName)
		{
            // NOTE - here we create templates with crazy aliases... assuming that these
            // could exist in the database... yet creating templates should sanitize
            // aliases one way or another...

            var template = CreateTemplate(templateName);
            var route = RouteTable.Routes["Umbraco_default"];
			var routeData = new RouteData() {Route = route};
			var routingContext = GetRoutingContext("~/dummy-page", template.Id, routeData, true);
			var docRequest = new PublishedContentRequest(routingContext.UmbracoContext.CleanedUmbracoUrl, routingContext)
				{
                    PublishedContent = routingContext.UmbracoContext.ContentCache.GetById(1172), 
					TemplateModel = template
				};

			var handler = new RenderRouteHandler(
                new TestControllerFactory(routingContext.UmbracoContext, Mock.Of<ILogger>()), 
                routingContext.UmbracoContext);

			handler.GetHandlerForRoute(routingContext.UmbracoContext.HttpContext.Request.RequestContext, docRequest);
			Assert.AreEqual("CustomDocument", routeData.Values["controller"].ToString());
		    Assert.AreEqual(
                //global::umbraco.cms.helpers.Casing.SafeAlias(template.Alias),
                template.Alias.ToSafeAlias(),
		        routeData.Values["action"].ToString());
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
		    public CustomDocumentController(UmbracoContext umbracoContext) : base(umbracoContext)
		    {
		    }

		    public ActionResult HomePage(RenderModel model)
			{
				return View();
			}

		}

		#endregion
	}
}
