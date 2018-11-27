using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Core.Strings;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.Testing;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web.Runtime;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class RenderRouteHandlerTests : BaseWebTest
    {
        public override void SetUp()
        {
            base.SetUp();

            WebRuntimeComponent.CreateRoutes(
                new TestUmbracoContextAccessor(),
                TestObjects.GetGlobalSettings(),
                new SurfaceControllerTypeCollection(Enumerable.Empty<Type>()),
                new UmbracoApiControllerTypeCollection(Enumerable.Empty<Type>()));
        }

        public class TestRuntime : WebRuntime
        {
            public TestRuntime(UmbracoApplicationBase umbracoApplication)
                : base(umbracoApplication)
            { }

            public override void Boot(IContainer container)
            {
                // do it before anything else - this is the only place where it's possible
                var logger = Mock.Of<ILogger>();
                container.RegisterInstance<ILogger>(logger);
                var profiler = Mock.Of<IProfiler>();
                container.RegisterInstance<IProfiler>(profiler);
                container.RegisterInstance<IProfilingLogger>(new ProfilingLogger(logger, profiler));

                base.Boot(container);
            }
        }

        protected override void Compose()
        {
            base.Compose();

            // set the default RenderMvcController
            Current.DefaultRenderMvcControllerType = typeof(RenderMvcController); // fixme WRONG!

            var surfaceControllerTypes = new SurfaceControllerTypeCollection(Current.TypeLoader.GetSurfaceControllers());
            Container.RegisterInstance(surfaceControllerTypes);

            var umbracoApiControllerTypes = new UmbracoApiControllerTypeCollection(Current.TypeLoader.GetUmbracoApiControllers());
            Container.RegisterInstance(umbracoApiControllerTypes);

            Container.RegisterSingleton<IShortStringHelper>(_ => new DefaultShortStringHelper(SettingsForTests.GetDefaultUmbracoSettings()));
        }

        public override void TearDown()
        {
            base.TearDown();
            RouteTable.Routes.Clear();
        }

        Template CreateTemplate(string alias)
        {
            var name = "Template";
            var template = new Template(name, alias);
            template.Content = ""; // else saving throws with a dirty internal error
            Current.Services.FileService.SaveTemplate(template);
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
            var routeData = new RouteData { Route = route };
            var umbracoContext = GetUmbracoContext("~/dummy-page", template.Id, routeData);
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            frequest.PublishedContent = umbracoContext.ContentCache.GetById(1174);
            frequest.TemplateModel = template;
            frequest.RenderingEngine = RenderingEngine.Mvc;

            var handler = new RenderRouteHandler(umbracoContext, new TestControllerFactory(umbracoContext, Mock.Of<ILogger>()));

            handler.GetHandlerForRoute(umbracoContext.HttpContext.Request.RequestContext, frequest);
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
            var umbracoContext = GetUmbracoContext("~/dummy-page", template.Id, routeData, true);
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            frequest.PublishedContent = umbracoContext.ContentCache.GetById(1172);
            frequest.TemplateModel = template;

            var type = new AutoPublishedContentType(22, "CustomDocument", new PublishedPropertyType[] { });
            ContentTypesCache.GetPublishedContentTypeByAlias = alias => type;

            var handler = new RenderRouteHandler(umbracoContext, new TestControllerFactory(umbracoContext, Mock.Of<ILogger>()));

            handler.GetHandlerForRoute(umbracoContext.HttpContext.Request.RequestContext, frequest);
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
            public CustomDocumentController(IGlobalSettings globalSettings, UmbracoContext umbracoContext, ServiceContext services, CacheHelper applicationCache, ILogger logger, IProfilingLogger profilingLogger)
                : base(globalSettings, umbracoContext, services, applicationCache, logger, profilingLogger)
            {
            }

            public ActionResult HomePage(ContentModel model)
            {
                return View();
            }

        }

        #endregion
    }
}
