using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Persistence.Sqlite;
using Umbraco.Cms.Persistence.SqlServer;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Integration.DependencyInjection;
using Umbraco.Cms.Tests.Integration.Extensions;
using Umbraco.Cms.Web.Common.Hosting;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Testing;

/// <summary>
///     Abstract class for integration tests
/// </summary>
/// <remarks>
///     This will use a Host Builder to boot and install Umbraco ready for use
/// </remarks>
public abstract class UmbracoIntegrationTest : UmbracoIntegrationTestBase
{
    private IHost _host;

    protected IServiceProvider Services => _host.Services;

    /// <summary>
    ///     Gets the <see cref="IScopeProvider" />
    /// </summary>
    protected IScopeProvider ScopeProvider => Services.GetRequiredService<IScopeProvider>();

    /// <summary>
    ///     Gets the <see cref="IScopeAccessor" />
    /// </summary>
    protected IScopeAccessor ScopeAccessor => Services.GetRequiredService<IScopeAccessor>();

    /// <summary>
    ///     Gets the <see cref="ILoggerFactory" />
    /// </summary>
    protected ILoggerFactory LoggerFactory => Services.GetRequiredService<ILoggerFactory>();

    protected AppCaches AppCaches => Services.GetRequiredService<AppCaches>();

    protected IIOHelper IOHelper => Services.GetRequiredService<IIOHelper>();

    protected IShortStringHelper ShortStringHelper => Services.GetRequiredService<IShortStringHelper>();

    protected GlobalSettings GlobalSettings => Services.GetRequiredService<IOptions<GlobalSettings>>().Value;

    protected IMapperCollection Mappers => Services.GetRequiredService<IMapperCollection>();

    protected UserBuilder UserBuilderInstance { get; } = new();

    protected UserGroupBuilder UserGroupBuilderInstance { get; } = new();

    [SetUp]
    public void Setup()
    {
        InMemoryConfiguration[Constants.Configuration.ConfigUnattended + ":" + nameof(UnattendedSettings.InstallUnattended)] = "true";
        var hostBuilder = CreateHostBuilder();

        _host = hostBuilder.Build();
        UseTestDatabase(_host.Services);
        _host.Start();

        if (TestOptions.Boot)
        {
            Services.GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();
        }
    }

    [TearDown]
    public void TearDownAsync() => _host.StopAsync();

    /// <summary>
    ///     Create the Generic Host and execute startup ConfigureServices/Configure calls
    /// </summary>
    private IHostBuilder CreateHostBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureUmbracoDefaults()

            // IMPORTANT: We Cannot use UseStartup, there's all sorts of threads about this with testing. Although this can work
            // if you want to setup your tests this way, it is a bit annoying to do that as the WebApplicationFactory will
            // create separate Host instances. So instead of UseStartup, we just call ConfigureServices/Configure ourselves,
            // and in the case of the UmbracoTestServerTestBase it will use the ConfigureWebHost to Configure the IApplicationBuilder directly.
            .ConfigureAppConfiguration((context, configBuilder) =>
            {
                context.HostingEnvironment = TestHelper.GetWebHostEnvironment();
                configBuilder.Sources.Clear();
                configBuilder.AddInMemoryCollection(InMemoryConfiguration);
                SetUpTestConfiguration(configBuilder);

                Configuration = configBuilder.Build();
            })
            .ConfigureServices((_, services) =>
            {
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

        return hostBuilder;
    }

    protected void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<TestUmbracoDatabaseFactoryProvider>();
        var webHostEnvironment = TestHelper.GetWebHostEnvironment();
        services.AddRequiredNetCoreServices(TestHelper, webHostEnvironment);

        // We register this service because we need it for IRuntimeState, if we don't this breaks 900 tests
        services.AddSingleton<IConflictingRouteService, TestConflictingRouteService>();

        services.AddLogger(webHostEnvironment, Configuration);

        // Add it!
        var hostingEnvironment = TestHelper.GetHostingEnvironment();
        var typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            hostingEnvironment,
            TestHelper.ConsoleLoggerFactory,
            AppCaches.NoCache,
            Configuration,
            TestHelper.Profiler);
        var builder = new UmbracoBuilder(services, Configuration, typeLoader, TestHelper.ConsoleLoggerFactory, TestHelper.Profiler, AppCaches.NoCache, hostingEnvironment);

        builder.AddConfiguration()
            .AddUmbracoCore()
            .AddWebComponents()
            .AddRuntimeMinifier()
            .AddBackOfficeAuthentication()
            .AddBackOfficeIdentity()
            .AddMembersIdentity()
            .AddExamine()
            .AddUmbracoSqlServerSupport()
            .AddUmbracoSqliteSupport()
            .AddTestServices(TestHelper);

        if (TestOptions.Mapper)
        {
            // TODO: Should these just be called from within AddUmbracoCore/AddWebComponents?
            builder
                .AddCoreMappingProfiles()
                .AddWebMappingProfiles();
        }

        services.AddSignalR();
        services.AddMvc();

        CustomTestSetup(builder);

        builder.Build();
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

    /// <summary>
    ///     Hook for registering test doubles.
    /// </summary>
    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
    }

    protected virtual T GetRequiredService<T>() => Services.GetRequiredService<T>();

    protected virtual void SetUpTestConfiguration(IConfigurationBuilder configBuilder)
    {
        if (GlobalSetupTeardown.TestConfiguration is not null)
        {
            configBuilder.AddConfiguration(GlobalSetupTeardown.TestConfiguration);
        }
    }
}
