using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Persistence.Sqlite;
using Umbraco.Cms.Persistence.SqlServer;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.DependencyInjection;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.TestServerTest
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console, Boot = true)]
    public abstract class UmbracoTestServerTestBase : UmbracoIntegrationTestBase
    {
        protected HttpClient Client { get; private set; }

        protected LinkGenerator LinkGenerator { get; private set; }

        protected WebApplicationFactory<UmbracoTestServerTestBase> Factory { get; private set; }

        /// <summary>
        ///  Hook for altering UmbracoBuilder setup
        /// </summary>
        /// <remarks>
        /// Can also be used for registering test doubles.
        /// </remarks>
        protected virtual void CustomTestSetup(IUmbracoBuilder builder)
        {
        }

        [SetUp]
        public void Setup()
        {
            /*
             * It's worth noting that our usage of WebApplicationFactory is non-standard,
             * the intent is that your Startup.ConfigureServices is called just like
             * when the app starts up, then replacements are registered in this class with
             * builder.ConfigureServices (builder.ConfigureTestServices has hung around from before the
             * generic host switchover).
             *
             * This is currently a pain to refactor towards due to UmbracoBuilder+TypeFinder+TypeLoader setup but
             * we should get there one day.
             *
             * However we need to separate the testing framework we provide for downstream projects from our own tests.
             * We cannot use the Umbraco.Web.UI startup yet as that is not available downstream.
             *
             * See https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests
             */
            var factory = new UmbracoWebApplicationFactory<UmbracoTestServerTestBase>(CreateHostBuilder);

            // additional host configuration for web server integration tests
            Factory = factory.WithWebHostBuilder(builder =>
            {
                // Otherwise inferred as $(SolutionDir)/Umbraco.Tests.Integration (note lack of src/tests)
                builder.UseContentRoot(Assembly.GetExecutingAssembly().GetRootDirectorySafe());

                // Executes after the standard ConfigureServices method
                builder.ConfigureTestServices(services =>

                    // Add a test auth scheme with a test auth handler to authn and assign the user
                    services.AddAuthentication(TestAuthHandler.TestAuthenticationScheme)
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestAuthenticationScheme, options => { }));
            });

            Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            LinkGenerator = Factory.Services.GetRequiredService<LinkGenerator>();
        }

        /// <summary>
        /// Prepare a url before using <see cref="Client"/>.
        /// This returns the url but also sets the HttpContext.request into to use this url.
        /// </summary>
        /// <returns>The string URL of the controller action.</returns>
        protected string PrepareApiControllerUrl<T>(Expression<Func<T, object>> methodSelector)
            where T : UmbracoApiController
        {
            var url = LinkGenerator.GetUmbracoApiService(methodSelector);
            return PrepareUrl(url);
        }

        /// <summary>
        /// Prepare a url before using <see cref="Client"/>.
        /// This returns the url but also sets the HttpContext.request into to use this url.
        /// </summary>
        /// <returns>The string URL of the controller action.</returns>
        protected string PrepareSurfaceControllerUrl<T>(Expression<Func<T, object>> methodSelector)
            where T : SurfaceController
        {
            var url = LinkGenerator.GetUmbracoSurfaceUrl(methodSelector);
            return PrepareUrl(url);
        }

        /// <summary>
        /// Prepare a url before using <see cref="Client"/>.
        /// This returns the url but also sets the HttpContext.request into to use this url.
        /// </summary>
        /// <returns>The string URL of the controller action.</returns>
        protected string PrepareUrl(string url)
        {
            IUmbracoContextFactory umbracoContextFactory = GetRequiredService<IUmbracoContextFactory>();
            IHttpContextAccessor httpContextAccessor = GetRequiredService<IHttpContextAccessor>();

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

            umbracoContextFactory.EnsureUmbracoContext();

            return url;
        }

        private IHostBuilder CreateHostBuilder()
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureUmbracoDefaults()
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    context.HostingEnvironment = TestHelper.GetWebHostEnvironment();
                    configBuilder.Sources.Clear();
                    configBuilder.AddInMemoryCollection(InMemoryConfiguration);
                    configBuilder.AddConfiguration(GlobalSetupTeardown.TestConfiguration);

                    Configuration = configBuilder.Build();
                })
                .ConfigureWebHost(builder =>
                {
                    builder.ConfigureServices((context, services) =>
                    {
                        context.HostingEnvironment = TestHelper.GetWebHostEnvironment();

                        ConfigureServices(services);
                        ConfigureTestServices(services);
                        services.AddUnique(CreateLoggerFactory());

                        if (!TestOptions.Boot)
                        {
                            // If boot is false, we don't want the CoreRuntime hosted service to start
                            // So we replace it with a Mock
                            services.AddUnique(Mock.Of<IRuntime>());
                        }
                    });

                    // call startup
                    builder.Configure(Configure);
                })
                .UseDefaultServiceProvider(cfg =>
                {
                    // These default to true *if* WebHostEnvironment.EnvironmentName == Development
                    // When running tests, EnvironmentName used to be null on the mock that we register into services.
                    // Enable opt in for tests so that validation occurs regardless of environment name.
                    // Would be nice to have this on for UmbracoIntegrationTest also but requires a lot more effort to resolve issues.
                    cfg.ValidateOnBuild = true;
                    cfg.ValidateScopes = true;
                });

            return hostBuilder;
        }

        protected virtual IServiceProvider Services => Factory.Services;

        protected virtual T GetRequiredService<T>() => Factory.Services.GetRequiredService<T>();

        protected void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<TestUmbracoDatabaseFactoryProvider>();

            Core.Hosting.IHostingEnvironment hostingEnvironment = TestHelper.GetHostingEnvironment();

            TypeLoader typeLoader = services.AddTypeLoader(
                GetType().Assembly,
                hostingEnvironment,
                TestHelper.ConsoleLoggerFactory,
                AppCaches.NoCache,
                Configuration,
                TestHelper.Profiler);

            services.AddLogger(TestHelper.GetWebHostEnvironment(), Configuration);

            var builder = new UmbracoBuilder(services, Configuration, typeLoader, TestHelper.ConsoleLoggerFactory, TestHelper.Profiler, AppCaches.NoCache, hostingEnvironment);

            builder
                .AddConfiguration()
                .AddUmbracoCore()
                .AddWebComponents()
                .AddNuCache()
                .AddRuntimeMinifier()
                .AddBackOfficeCore()
                .AddBackOfficeAuthentication()
                .AddBackOfficeIdentity()
                .AddMembersIdentity()
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

                    // Adds Umbraco.Tests.Integration
                    mvcBuilder.AddApplicationPart(typeof(UmbracoTestServerTestBase).Assembly);
                })
                .AddWebServer()
                .AddWebsite()
                .AddUmbracoSqlServerSupport()
                .AddUmbracoSqliteSupport()
                .AddTestServices(TestHelper); // This is the important one!

            CustomTestSetup(builder);
            builder.Build();
        }

        /// <summary>
        ///  Hook for registering test doubles.
        /// </summary>
        protected virtual void ConfigureTestServices(IServiceCollection services)
        {
            
        }

        protected void Configure(IApplicationBuilder app)
        {
            UseTestDatabase(app);

            app.UseUmbraco()
                .WithMiddleware(u =>
                {
                    u.UseBackOffice();
                    u.UseWebsite();
                })
                .WithEndpoints(u =>
                {
                    u.UseBackOfficeEndpoints();
                    u.UseWebsiteEndpoints();
                });
        }
    }
}
