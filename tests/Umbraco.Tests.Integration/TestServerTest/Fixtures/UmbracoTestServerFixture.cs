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
using Umbraco.Cms.Api.Delivery.Controllers.Content;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Persistence.Sqlite;
using Umbraco.Cms.Persistence.SqlServer;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.DependencyInjection;
using Umbraco.Cms.Tests.Integration.Implementations;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Testing.Fixtures;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Website.Controllers;

namespace Umbraco.Cms.Tests.Integration.TestServerTest.Fixtures;

/// <summary>
///     Fixture-agnostic base class for HTTP server integration tests.
///     Unlike <see cref="UmbracoTestServerTestBase"/>, this class does NOT use NUnit lifecycle attributes,
///     allowing it to be used from both [TestFixture] and [SetUpFixture] contexts.
/// </summary>
/// <remarks>
///     Call <see cref="BuildAndStartWebApplication"/> to boot the test server and
///     <see cref="DisposeClientAndFactory"/> to tear it down.
/// </remarks>
public abstract class UmbracoTestServerFixture : UmbracoIntegrationFixtureBase
{
    private static readonly Dictionary<string, IWebApplicationFactoryAdapter> s_factoryCache = new();

    protected HttpClient Client { get; private set; }

    protected IWebApplicationFactoryAdapter Factory { get; private set; }

    protected IServiceProvider Services => Factory?.Services;

    /// <summary>
    ///     When non-null, uses the swapper for database setup instead of the normal
    ///     <see cref="UmbracoIntegrationFixtureBase.UseTestDatabase"/> flow.
    ///     This enables swapping databases on a running host without restarting it.
    /// </summary>
    protected TestDatabaseSwapper DatabaseSwapper { get; set; }

    protected LinkGenerator LinkGenerator => Factory.Services.GetRequiredService<LinkGenerator>();

    protected virtual void CustomMvcSetup(IMvcBuilder mvcBuilder)
    {
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
    }

    /// <summary>
    ///     Hook for altering UmbracoBuilder setup
    /// </summary>
    /// <remarks>
    ///     Can also be used for registering test doubles.
    /// </remarks>
    protected virtual void CustomTestSetup(IUmbracoBuilder builder)
    {
    }

    protected virtual void CustomTestAuthSetup(IServiceCollection services)
    {
        // Add a test auth scheme with a test auth handler to authn and assign the user
        services.AddAuthentication(TestAuthHandler.TestAuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestAuthenticationScheme,
                options => { });
    }

    /// <summary>
    ///     Builds and starts the web application factory and creates an HttpClient.
    ///     Call this from [SetUp], [OneTimeSetUp], or any other context.
    /// </summary>
    public virtual void BuildAndStartWebApplication()
    {
        // Don't cache factory if using per-test database
        if (TestOptions.Database == UmbracoTestOptions.Database.NewSchemaPerTest ||
            TestOptions.Database == UmbracoTestOptions.Database.NewEmptyPerTest)
        {
            Factory = CreateNewFactory();
        }
        else
        {
            // Use cached factory for per-fixture database options
            var cacheKey = $"{TestOptions.Database}_{TestOptions.Boot}";

            if (!s_factoryCache.TryGetValue(cacheKey, out var cachedFactory))
            {
                cachedFactory = CreateNewFactory();
                s_factoryCache[cacheKey] = cachedFactory;
            }

            Factory = cachedFactory;
        }

        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost/", UriKind.Absolute),
        });
    }

    /// <summary>
    ///     Disposes the HttpClient and, if using per-test databases, the factory.
    ///     Call this from [TearDown] or after each test.
    /// </summary>
    public virtual void DisposeClientAndFactory()
    {
        Client?.Dispose();

        // Dispose the factory if using per-test database
        if (TestOptions.Database == UmbracoTestOptions.Database.NewSchemaPerTest ||
            TestOptions.Database == UmbracoTestOptions.Database.NewEmptyPerTest)
        {
            Factory?.Dispose();
            Factory = null;
        }
    }

    /// <summary>
    ///     Swaps to a fresh database on the running host.
    ///     Requires <see cref="DatabaseSwapper"/> to be set.
    /// </summary>
    public async Task SwapToFreshDatabaseAsync()
    {
        if (DatabaseSwapper is null)
        {
            throw new InvalidOperationException(
                "DatabaseSwapper must be set to use SwapToFreshDatabaseAsync.");
        }

        await DatabaseSwapper.SwapDatabaseAsync(Services, Configuration, TestHelper);
    }

    /// <summary>
    ///     Swaps to a seeded database on the running host.
    ///     If a snapshot for the seed profile already exists, restores from it instantly.
    ///     Otherwise, seeds the database and creates a snapshot for future callers.
    ///     Requires <see cref="DatabaseSwapper"/> to be set.
    /// </summary>
    public async Task SwapToSeededDatabaseAsync(ITestDatabaseSeedProfile seedProfile)
    {
        if (DatabaseSwapper is null)
        {
            throw new InvalidOperationException(
                "DatabaseSwapper must be set to use SwapToSeededDatabaseAsync.");
        }

        await DatabaseSwapper.SwapToSeededDatabaseAsync(Services, Configuration, TestHelper, seedProfile);
    }

    /// <summary>
    ///     Detaches the current database without attaching a new one.
    ///     Requires <see cref="DatabaseSwapper"/> to be set.
    /// </summary>
    public void DetachDatabase()
    {
        DatabaseSwapper?.DetachCurrentDatabase(Services, Configuration, TestHelper);
    }

    /// <summary>
    ///     Disposes all cached factories. Call from [OneTimeTearDown] or at the end of a SetUpFixture.
    /// </summary>
    public static async Task CleanupFactories()
    {
        foreach (var factory in s_factoryCache.Values)
        {
            await factory.DisposeAsync();
        }

        s_factoryCache.Clear();
    }

    private IWebApplicationFactoryAdapter CreateNewFactory()
    {
        return WebApplicationFactoryAdapter<UmbracoTestServerFixture>.Create(
            GetType(),
            CreateHostBuilder,
            builder =>
            {
                builder.UseContentRoot(Assembly.GetExecutingAssembly().GetRootDirectorySafe());
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IWebProfilerRepository, TestWebProfilerRepository>();
                    CustomTestAuthSetup(services);
                });
            });
    }

    /// <summary>
    ///     Prepare a url before using <see cref="Client"/>.
    ///     This returns the url but also sets the HttpContext.request into to use this url.
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
        methodParams.Remove(methodParams.FirstOrDefault(x => x.Value is CancellationToken).Key);
        methodParams["version"] = method?.GetCustomAttribute<MapToApiVersionAttribute>()?.Versions[0].MajorVersion.ToString();
        return LinkGenerator.GetUmbracoControllerUrl(method.Name, ControllerExtensions.GetControllerName<T>(), null, methodParams);
    }

    /// <summary>
    ///     Prepare a url before using <see cref="Client"/>.
    ///     This returns the url but also sets the HttpContext.request into to use this url.
    /// </summary>
    /// <returns>The string URL of the controller action.</returns>
    protected string PrepareSurfaceControllerUrl<T>(Expression<Func<T, object>> methodSelector)
        where T : SurfaceController
    {
        var url = LinkGenerator.GetUmbracoSurfaceUrl(methodSelector);
        return PrepareUrl(url);
    }

    /// <summary>
    ///     Prepare a url before using <see cref="Client"/>.
    ///     This returns the url but also sets the HttpContext.request into to use this url.
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
                cfg.ValidateOnBuild = true;
                cfg.ValidateScopes = true;
            });

        return hostBuilder;
    }

    protected virtual T GetRequiredService<T>() => Factory.Services.GetRequiredService<T>();

    protected void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<TestUmbracoDatabaseFactoryProvider>();

        TypeLoader typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            TestHelper.ConsoleLoggerFactory,
            Configuration);

        services.AddLogger(TestHelper.GetWebHostEnvironment(), Configuration);

        var builder = new UmbracoBuilder(services, Configuration, typeLoader, TestHelper.ConsoleLoggerFactory, TestHelper.Profiler, AppCaches.NoCache);
        if (DatabaseSwapper is not null)
        {
            var swapper = DatabaseSwapper;
            var config = Configuration;
            var helper = TestHelper;
            builder.Services.AddTransient<IHostedService>(sp =>
                new TestDatabaseHostedLifecycleService(() => swapper.InitialSetup(sp, config, helper)));
        }
        else
        {
            builder.Services.AddTransient<IHostedService>(sp =>
                new TestDatabaseHostedLifecycleService(() => UseTestDatabase(sp)));
        }
        builder
            .AddConfiguration()
            .AddUmbracoCore()
            .AddWebComponents()
            .AddUmbracoHybridCache()
            .AddBackOfficeCore()
            .AddBackOfficeAuthentication()
            .AddBackOfficeIdentity()
            .AddMembersIdentity()
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
                mvcBuilder.AddApplicationPart(typeof(UmbracoTestServerFixture).Assembly);

                CustomMvcSetup(mvcBuilder);
            })
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

    protected virtual void Configure(IApplicationBuilder app)
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
