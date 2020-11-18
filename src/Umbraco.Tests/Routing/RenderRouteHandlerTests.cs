using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Runtime;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Tests.Common;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.Runtime;
using Umbraco.Web.WebApi;
using ConnectionStrings = Umbraco.Core.Configuration.Models.ConnectionStrings;
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

            WebInitialComponent.CreateRoutes(
                new TestUmbracoContextAccessor(),
                TestObjects.GetGlobalSettings(),
                ShortStringHelper,
              //  new SurfaceControllerTypeCollection(Enumerable.Empty<Type>()),
                new UmbracoApiControllerTypeCollection(Enumerable.Empty<Type>()),
                HostingEnvironment);
        }

        public class TestRuntimeBootstrapper : CoreRuntimeBootstrapper
        {
            public TestRuntimeBootstrapper(GlobalSettings globalSettings, ConnectionStrings connectionStrings, IUmbracoVersion umbracoVersion, IIOHelper ioHelper, IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo)
                : base(globalSettings, connectionStrings,umbracoVersion, ioHelper,  NullLoggerFactory.Instance, Mock.Of<IProfiler>(), new AspNetUmbracoBootPermissionChecker(), hostingEnvironment, backOfficeInfo, TestHelper.DbProviderFactoryCreator, TestHelper.MainDom, TestHelper.GetTypeFinder(), AppCaches.NoCache)
            {
            }

        }

        protected override void Compose()
        {
            base.Compose();

            // set the default RenderMvcController
            Current.DefaultRenderMvcControllerType = typeof(RenderMvcController); // FIXME: Wrong!

            // var surfaceControllerTypes = new SurfaceControllerTypeCollection(Composition.TypeLoader.GetSurfaceControllers());
            // Composition.Services.AddUnique(surfaceControllerTypes);

            var umbracoApiControllerTypes = new UmbracoApiControllerTypeCollection(Builder.TypeLoader.GetUmbracoApiControllers());
            Builder.Services.AddUnique(umbracoApiControllerTypes);

            var requestHandlerSettings = new RequestHandlerSettings();
            Builder.Services.AddUnique<IShortStringHelper>(_ => new DefaultShortStringHelper(Microsoft.Extensions.Options.Options.Create(requestHandlerSettings)));
        }

        public override void TearDown()
        {
            base.TearDown();
            RouteTable.Routes.Clear();
        }

        Template CreateTemplate(string alias)
        {
            var name = "Template";
            var template = new Template(ShortStringHelper, name, alias);
            template.Content = ""; // else saving throws with a dirty internal error
            ServiceContext.FileService.SaveTemplate(template);
            return template;
        }

        /// <summary>
        /// Will route to the default controller and action since no custom controller is defined for this node route
        /// </summary>
        [Test]
        public void Umbraco_Route_Umbraco_Defined_Controller_Action()
        {
            var url = "~/dummy-page";
            var template = CreateTemplate("homePage");
            var route = RouteTable.Routes["Umbraco_default"];
            var routeData = new RouteData { Route = route };
            var umbracoContext = GetUmbracoContext(url, template.Id, routeData);
            var httpContext = GetHttpContextFactory(url, routeData).HttpContext;
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            frequest.PublishedContent = umbracoContext.Content.GetById(1174);
            frequest.TemplateModel = template;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
            var handler = new RenderRouteHandler(umbracoContext, new TestControllerFactory(umbracoContextAccessor, Mock.Of<ILogger<TestControllerFactory>>()), ShortStringHelper);

            handler.GetHandlerForRoute(httpContext.Request.RequestContext, frequest);
            Assert.AreEqual("RenderMvc", routeData.Values["controller"].ToString());
            //the route action will still be the one we've asked for because our RenderActionInvoker is the thing that decides
            // if the action matches.
            Assert.AreEqual("homePage", routeData.Values["action"].ToString());
        }

        //test all template name styles to match the ActionName

        //[TestCase("home-\\234^^*32page")]        // TODO: This fails!
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

            var url = "~/dummy-page";
            var template = CreateTemplate(templateName);
            var route = RouteTable.Routes["Umbraco_default"];
            var routeData = new RouteData() { Route = route };
            var umbracoContext = GetUmbracoContext("~/dummy-page", template.Id, routeData, true);
            var httpContext = GetHttpContextFactory(url, routeData).HttpContext;
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            frequest.PublishedContent = umbracoContext.Content.GetById(1172);
            frequest.TemplateModel = template;

            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
            var type = new AutoPublishedContentType(Guid.NewGuid(), 22, "CustomDocument", new PublishedPropertyType[] { });
            ContentTypesCache.GetPublishedContentTypeByAlias = alias => type;

            var handler = new RenderRouteHandler(umbracoContext, new TestControllerFactory(umbracoContextAccessor, Mock.Of<ILogger<TestControllerFactory>>(), context =>
                {

                  return new CustomDocumentController(Factory.GetRequiredService<IOptions<GlobalSettings>>(),
                        umbracoContextAccessor,
                        Factory.GetRequiredService<ServiceContext>(),
                        Factory.GetRequiredService<AppCaches>(),
                        Factory.GetRequiredService<IProfilingLogger>(),
                        Factory.GetRequiredService<ILoggerFactory>());
                }), ShortStringHelper);

            handler.GetHandlerForRoute(httpContext.Request.RequestContext, frequest);
            Assert.AreEqual("CustomDocument", routeData.Values["controller"].ToString());
            Assert.AreEqual(
                //global::umbraco.cms.helpers.Casing.SafeAlias(template.Alias),
                template.Alias.ToSafeAlias(ShortStringHelper),
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
            public CustomDocumentController(IOptions<GlobalSettings> globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, ILoggerFactory loggerFactory)

            {
            }

            public ActionResult HomePage(ContentModel model)
            {
                // ReSharper disable once Mvc.ViewNotResolved
                return View();
            }

        }

        #endregion
    }
}
