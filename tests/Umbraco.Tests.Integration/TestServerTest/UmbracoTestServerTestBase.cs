using System.Linq.Expressions;
using System.Reflection;
using Asp.Versioning;
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
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Delivery.Controllers.Content;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Persistence.Sqlite;
using Umbraco.Cms.Persistence.SqlServer;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.DependencyInjection;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Website.Controllers;

namespace Umbraco.Cms.Tests.Integration.TestServerTest
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console, Boot = true)]
    public abstract class UmbracoTestServerTestBase : UmbracoIntegrationTestBase
    {
        protected HttpClient Client { get; private set; }

        protected LinkGenerator LinkGenerator => Factory.Services.GetRequiredService<LinkGenerator>();

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

        protected virtual void CustomTestAuthSetup(IServiceCollection services)
        {
            // Add a test auth scheme with a test auth handler to authn and assign the user
            services.AddAuthentication(TestAuthHandler.TestAuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.TestAuthenticationScheme, options => { });
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
                {
                    services.AddSingleton<IWebProfilerRepository, TestWebProfilerRepository>();

                    CustomTestAuthSetup(services);
                });
            });

            Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost/", UriKind.Absolute)
            });
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

        protected string GetManagementApiUrl<T>(Expression<Func<T, object>> methodSelector)
            where T : ManagementApiControllerBase
        {
            MethodInfo? method = ExpressionHelper.GetMethodInfo(methodSelector);
            IDictionary<string, object?> methodParams = ExpressionHelper.GetMethodParams(methodSelector) ?? new Dictionary<string, object?>();

            // Remove the CancellationToken from the method params, this is automatically added by the framework
            // So we do not want to add this to the query string
            var cancellationTokenKey = methodParams.FirstOrDefault(x => x.Value is CancellationToken).Key;
            if (cancellationTokenKey is not null)
            {
                methodParams.Remove(cancellationTokenKey);
            }


            methodParams["version"] = method?.GetCustomAttribute<MapToApiVersionAttribute>()?.Versions?.First().MajorVersion.ToString();
            if (method == null)
            {
                throw new MissingMethodException(
                    $"Could not find the method {methodSelector} on type {typeof(T)} or the result ");
            }

            var controllerName = ControllerExtensions.GetControllerName(typeof(T));

            return LinkGenerator.GetUmbracoControllerUrl(method.Name, controllerName, null, methodParams);
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

            TypeLoader typeLoader = services.AddTypeLoader(
                GetType().Assembly,
                TestHelper.ConsoleLoggerFactory,
                AppCaches.NoCache,
                Configuration,
                TestHelper.Profiler);

            services.AddLogger(TestHelper.GetWebHostEnvironment(), Configuration);

            var builder = new UmbracoBuilder(services, Configuration, typeLoader, TestHelper.ConsoleLoggerFactory, TestHelper.Profiler, AppCaches.NoCache);
            builder.Services.AddTransient<IHostedService>(sp => new TestDatabaseHostedLifecycleService(() => UseTestDatabase(sp)));

            builder
                .AddConfiguration()
                .AddUmbracoCore()
                .AddWebComponents()
                .AddUmbracoHybridCache()
                .AddBackOfficeCore()
                .AddBackOfficeAuthentication()
                .AddBackOfficeIdentity()
                .AddMembersIdentity()
                // .AddBackOfficeAuthorizationPolicies(TestAuthHandler.TestAuthenticationScheme)
                .AddMvcAndRazor(mvcBuilding: mvcBuilder =>
                {
                    // Adds Umbraco.Web.Common
                    mvcBuilder.AddApplicationPart(typeof(RenderController).Assembly);

                    // Adds Umbraco.Web.Website
                    mvcBuilder.AddApplicationPart(typeof(SurfaceController).Assembly);

                    // Adds Umbraco.Cms.Api.ManagementApi
                    mvcBuilder.AddApplicationPart(typeof(ModelsBuilderControllerBase).Assembly);

                    // Adds Umbraco.Cms.Api.DeliveryApi
                    mvcBuilder.AddApplicationPart(typeof(ContentApiItemControllerBase).Assembly);

                    // Adds Umbraco.Tests.Integration
                    mvcBuilder.AddApplicationPart(typeof(UmbracoTestServerTestBase).Assembly);

                    CustomMvcSetup(mvcBuilder);
                })
                .AddWebsite()
                .AddUmbracoSqlServerSupport()
                .AddUmbracoSqliteSupport()
                .AddDeliveryApi()
                .AddUmbracoManagementApi()
                .AddComposers()
                .AddTestServices(TestHelper); // This is the important one!

            CustomTestSetup(builder);


            builder.Build();
        }

        protected virtual void CustomMvcSetup(IMvcBuilder mvcBuilder)
        {

        }

        /// <summary>
        ///  Hook for registering test doubles.
        /// </summary>
        protected virtual void ConfigureTestServices(IServiceCollection services)
        {

        }

        protected void Configure(IApplicationBuilder app)
        {
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

public class TestDatabaseHostedLifecycleService : IHostedLifecycleService
{
    private readonly Action _action;

    public TestDatabaseHostedLifecycleService(Action action)
    {
        _action = action;
    }

    public Task StartAsync(CancellationToken cancellationToken)=> Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        _action();
        return Task.CompletedTask;

    }
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
