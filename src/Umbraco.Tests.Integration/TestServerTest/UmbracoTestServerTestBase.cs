
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Extensions;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.Common.Builder;
using Umbraco.Web.Common.Controllers;
using Microsoft.Extensions.Hosting;
using Umbraco.Core.Cache;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;
using Umbraco.Web.BackOffice.Controllers;

namespace Umbraco.Tests.Integration.TestServerTest
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console, Boot = true)]
    public abstract class UmbracoTestServerTestBase : UmbracoIntegrationTest
    {
        [SetUp]
        public override void Setup()
        {
            InMemoryConfiguration["ConnectionStrings:" + Constants.System.UmbracoConnectionName] = null;
            InMemoryConfiguration["Umbraco:CMS:Hosting:Debug"] = "true";

            // create new WebApplicationFactory specifying 'this' as the IStartup instance
            var factory = new UmbracoWebApplicationFactory<UmbracoTestServerTestBase>(CreateHostBuilder);

            // additional host configuration for web server integration tests
            Factory = factory.WithWebHostBuilder(builder =>
            {
                // Executes after the standard ConfigureServices method
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                });
            });

            Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            LinkGenerator = Factory.Services.GetRequiredService<LinkGenerator>();
        }

        public override IHostBuilder CreateHostBuilder()
        {
            var builder = base.CreateHostBuilder();
            builder.ConfigureWebHost(builder =>
            {
                 // need to configure the IWebHostEnvironment too
                 builder.ConfigureServices((c, s) =>
                 {
                     c.HostingEnvironment = TestHelper.GetWebHostEnvironment();
                 });

                 // call startup
                 builder.Configure(app =>
                 {
                     UseTestLocalDb(app.ApplicationServices);
                     Services = app.ApplicationServices;
                     Configure(app);
                 });
             }).UseEnvironment(Environments.Development);

            return builder;
        }

        /// <summary>
        /// Prepare a url before using <see cref="Client"/>.
        /// This returns the url but also sets the HttpContext.request into to use this url.
        /// </summary>
        /// <returns>The string URL of the controller action.</returns>
        protected string PrepareUrl<T>(Expression<Func<T, object>> methodSelector)
            where T : UmbracoApiController
        {
            var url = LinkGenerator.GetUmbracoApiService<T>(methodSelector);

            var backofficeSecurityFactory = GetRequiredService<IBackOfficeSecurityFactory>();
            var umbracoContextFactory = GetRequiredService<IUmbracoContextFactory>();
            var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();

            httpContextAccessor.HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "https",
                    Host = new HostString("localhost", 80),
                    Path = url,
                    QueryString = new QueryString(string.Empty)
                }
            };

            backofficeSecurityFactory.EnsureBackOfficeSecurity();
            umbracoContextFactory.EnsureUmbracoContext();

            return url;
        }

        protected HttpClient Client { get; private set; }
        protected LinkGenerator LinkGenerator { get; private set; }
        protected WebApplicationFactory<UmbracoTestServerTestBase> Factory { get; private set; }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            base.TerminateCoreRuntime();

            Factory.Dispose();
        }

        #region IStartup

        public override void ConfigureServices(IServiceCollection services)
        {
            var typeLoader = services.AddTypeLoader(GetType().Assembly, TestHelper.GetWebHostEnvironment(),
                TestHelper.ConsoleLoggerFactory, AppCaches.NoCache, Configuration);

            var builder = new UmbracoBuilder(services, Configuration, typeLoader);
    
            builder
                .AddConfiguration()
                .AddTestCore(TestHelper) // This is the important one!
                .AddWebComponents()
                .AddRuntimeMinifier()
                .AddBackOffice()
                .AddBackOfficeIdentity()
                .AddPreviewSupport()
                //.WithMiniProfiler() // we don't want this running in tests
                .AddMvcAndRazor(mvcBuilding: mvcBuilder =>
                {
                    mvcBuilder.AddApplicationPart(typeof(ContentController).Assembly);
                })
                .AddWebServer()
                .Build();
        }

        public override void Configure(IApplicationBuilder app)
        {
            app.UseUmbraco();
        }

        #endregion


    }
}
