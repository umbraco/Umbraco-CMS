using System;
using System.Linq.Expressions;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.DependencyInjection;
using Umbraco.Extensions;
using Umbraco.Infrastructure.PublishedCache.DependencyInjection;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.DependencyInjection;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.DependencyInjection;
using Umbraco.Web.Website.Controllers;

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
            var factory = new UmbracoWebApplicationFactory<UmbracoTestServerTestBase>(CreateHostBuilder, BeforeHostStart);

            // additional host configuration for web server integration tests
            Factory = factory.WithWebHostBuilder(builder =>
            {
                // Executes after the standard ConfigureServices method
                builder.ConfigureTestServices(services =>
                {
                    // Add a test auth scheme with a test auth handler to authn and assign the user
                    services.AddAuthentication(TestAuthHandler.TestAuthenticationScheme)
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestAuthenticationScheme, options => { });
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

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<TestUmbracoDatabaseFactoryProvider>();
            var typeLoader = services.AddTypeLoader(
                GetType().Assembly,
                TestHelper.GetWebHostEnvironment(),
                TestHelper.GetHostingEnvironment(),
                TestHelper.ConsoleLoggerFactory,
                AppCaches.NoCache,
                Configuration,
                TestHelper.Profiler);

            var builder = new UmbracoBuilder(services, Configuration, typeLoader);

            builder
                .AddConfiguration()
                .AddTestCore(TestHelper) // This is the important one!
                .AddWebComponents()
                .AddRuntimeMinifier()
                .AddBackOfficeAuthentication()
                .AddBackOfficeIdentity()
                .AddBackOfficeAuthorizationPolicies(TestAuthHandler.TestAuthenticationScheme)
                .AddPreviewSupport()
                .AddMvcAndRazor(mvcBuilding: mvcBuilder =>
                {
                    // Adds Umbraco.Web.BackOffice
                    mvcBuilder.AddApplicationPart(typeof(ContentController).Assembly);

                    // Adds Umbraco.Web.Common
                    mvcBuilder.AddApplicationPart(typeof(RenderController).Assembly);

                    // Adds Umbraco.Web.Website
                    mvcBuilder.AddApplicationPart(typeof(SurfaceController).Assembly);
                })
                .AddWebServer()
                .Build();
        }

        public override void Configure(IApplicationBuilder app)
        {
            app.UseUmbraco();
            app.UseUmbracoBackOffice();
            app.UseUmbracoWebsite();
        }
    }
}
