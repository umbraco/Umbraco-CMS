using System.Linq.Expressions;
using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
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
using Umbraco.Cms.Api.Delivery.Controllers.Content;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
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
        private static readonly Dictionary<string, WebApplicationFactory<UmbracoTestServerTestBase>> _factoryCache = new();

        private static int _testCounter;

        protected HttpClient Client { get; private set; }

        protected WebApplicationFactory<UmbracoTestServerTestBase> Factory { get; private set; }

        protected IServiceProvider Services => Factory?.Services;

        protected LinkGenerator LinkGenerator => GetRequiredService<LinkGenerator>();

        protected virtual void CustomTestSetup(IUmbracoBuilder builder)
        {
        }

        protected virtual void CustomMvcSetup(IMvcBuilder mvcBuilder)
        {
        }

        protected virtual void ConfigureTestServices(IServiceCollection services)
        {
        }

        protected virtual void CustomTestAuthSetup(IServiceCollection services)
        {
            services.AddAuthentication(TestAuthHandler.TestAuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestAuthenticationScheme,
                    _ => { });
        }

        [SetUp]
        public virtual void Setup()
        {
            var cacheKey = $"{TestOptions.Database}_{TestOptions.Boot}";

            if (!_factoryCache.TryGetValue(cacheKey, out var cachedFactory))
            {
                cachedFactory = new UmbracoWebApplicationFactory<UmbracoTestServerTestBase>(CreateHostBuilder)
                    .WithWebHostBuilder(builder =>
                    {
                        builder.UseContentRoot(Assembly.GetExecutingAssembly().GetRootDirectorySafe());
                        builder.ConfigureTestServices(services =>
                        {
                            services.AddSingleton<IWebProfilerRepository, TestWebProfilerRepository>();
                            CustomTestAuthSetup(services);
                        });
                    });
                _factoryCache[cacheKey] = cachedFactory;
            }

            Factory = cachedFactory;

            Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false, BaseAddress = new Uri("https://localhost/"),
            });

            if (++_testCounter % 10 == 0)
            {
                GC.Collect();
            }
        }

        [TearDown]
        public void TearDownClient() => Client?.Dispose();

        [OneTimeTearDown]
        public static async Task CleanupFactories()
        {
            foreach (var factory in _factoryCache.Values)
            {
                await factory.DisposeAsync();
            }

            _factoryCache.Clear();
        }

        protected string GetManagementApiUrl<T>(Expression<Func<T, object>> methodSelector)
            where T : ManagementApiControllerBase
        {
            var method = ExpressionHelper.GetMethodInfo(methodSelector);
            var methodParams = ExpressionHelper.GetMethodParams(methodSelector) ?? new Dictionary<string, object>();
            methodParams.Remove(methodParams.FirstOrDefault(x => x.Value is CancellationToken).Key);
            methodParams["version"] = method?.GetCustomAttribute<MapToApiVersionAttribute>()?.Versions?.First()
                .MajorVersion.ToString();
            return LinkGenerator.GetUmbracoControllerUrl(method.Name, ControllerExtensions.GetControllerName(typeof(T)),
                null, methodParams);
        }

        protected string PrepareApiControllerUrl<T>(Expression<Func<T, object>> methodSelector)
            where T : UmbracoApiController
        {
            var url = LinkGenerator.GetUmbracoApiService(methodSelector);
            return PrepareUrl(url);
        }

        protected string PrepareSurfaceControllerUrl<T>(Expression<Func<T, object>> methodSelector)
            where T : SurfaceController
        {
            var url = LinkGenerator.GetUmbracoSurfaceUrl(methodSelector);
            return PrepareUrl(url);
        }

        protected string PrepareUrl(string url)
        {
            GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext
            {
                Request = { Scheme = "https", Host = new HostString("localhost", 80), Path = url },
            };
            GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();
            return url;
        }

        protected T GetRequiredService<T>() => Factory.Services.GetRequiredService<T>();

        private IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureUmbracoDefaults()
                .ConfigureAppConfiguration((context, config) =>
                {
                    context.HostingEnvironment = TestHelper.GetWebHostEnvironment();
                    config.Sources.Clear();
                    config.AddInMemoryCollection(InMemoryConfiguration);
                    config.AddConfiguration(GlobalSetupTeardown.TestConfiguration);
                    Configuration = config.Build();
                })
                .ConfigureWebHost(builder =>
                {
                    builder.ConfigureServices((context, services) =>
                    {
                        context.HostingEnvironment = TestHelper.GetWebHostEnvironment();
                        ConfigureUmbracoServices(services);
                        ConfigureTestServices(services);
                        services.AddUnique(CreateLoggerFactory());
                        if (!TestOptions.Boot)
                        {
                            services.AddUnique(Mock.Of<IRuntime>());
                        }
                    });
                    builder.Configure(app => app.UseUmbraco()
                        .WithMiddleware(u =>
                        {
                            u.UseBackOffice();
                            u.UseWebsite();
                        })
                        .WithEndpoints(u =>
                        {
                            u.UseBackOfficeEndpoints();
                            u.UseWebsiteEndpoints();
                        }));
                })
                .UseDefaultServiceProvider(cfg => { cfg.ValidateOnBuild = cfg.ValidateScopes = true; });

        private void ConfigureUmbracoServices(IServiceCollection services)
        {
            services.AddTransient<TestUmbracoDatabaseFactoryProvider>();

            var hostingEnvironment = TestHelper.GetHostingEnvironment();
            var typeLoader = services.AddTypeLoader(GetType().Assembly, hostingEnvironment,
                TestHelper.ConsoleLoggerFactory, AppCaches.NoCache, Configuration, TestHelper.Profiler);

            services.AddLogger(TestHelper.GetWebHostEnvironment(), Configuration);

            var builder = new UmbracoBuilder(services, Configuration, typeLoader, TestHelper.ConsoleLoggerFactory, TestHelper.Profiler, AppCaches.NoCache, hostingEnvironment);
            builder.Services.AddTransient<IHostedService>(sp => new TestDatabaseHostedService(() => UseTestDatabase(sp)));

            builder
                .AddConfiguration()
                .AddUmbracoCore()
                .AddWebComponents()
                .AddUmbracoHybridCache()
                .AddBackOfficeCore()
                .AddBackOfficeAuthentication()
                .AddBackOfficeIdentity()
                .AddMembersIdentity()
                .AddMvcAndRazor(mvcBuilder =>
                {
                    mvcBuilder.AddApplicationPart(typeof(RenderController).Assembly);
                    mvcBuilder.AddApplicationPart(typeof(SurfaceController).Assembly);
                    mvcBuilder.AddApplicationPart(typeof(ModelsBuilderControllerBase).Assembly);
                    mvcBuilder.AddApplicationPart(typeof(ContentApiItemControllerBase).Assembly);
                    mvcBuilder.AddApplicationPart(typeof(UmbracoTestServerTestBase).Assembly);
                    CustomMvcSetup(mvcBuilder);
                })
                .AddWebServer()
                .AddWebsite()
                .AddUmbracoSqlServerSupport()
                .AddUmbracoSqliteSupport()
                .AddDeliveryApi()
                .AddUmbracoManagementApi()
                .AddComposers()
                .AddTestServices(TestHelper);

            CustomTestSetup(builder);
            builder.Build();
        }

        private class TestDatabaseHostedService : IHostedService
        {
            private readonly Action _action;

            public TestDatabaseHostedService(Action action) => _action = action;

            public Task StartAsync(CancellationToken cancellationToken)
            {
                _action();
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }
    }
}
