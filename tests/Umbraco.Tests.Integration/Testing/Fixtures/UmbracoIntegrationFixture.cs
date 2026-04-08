using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Persistence.Sqlite;
using Umbraco.Cms.Persistence.SqlServer;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.DependencyInjection;
using Umbraco.Cms.Tests.Integration.Extensions;
using Umbraco.Cms.Tests.Integration.Implementations;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Web.Common.Cache;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Testing.Fixtures;

/// <summary>
///     Fixture-agnostic integration test class that boots a full Umbraco host.
///     Unlike <see cref="UmbracoIntegrationTest"/>, this class does NOT use NUnit lifecycle attributes,
///     allowing it to be used from both [TestFixture] and [SetUpFixture] contexts.
/// </summary>
/// <remarks>
///     Call <see cref="BuildAndStartHost"/> to boot Umbraco and <see cref="StopAndDisposeHost"/> to tear it down.
/// </remarks>
public abstract class UmbracoIntegrationFixture : UmbracoIntegrationFixtureBase
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

    protected IIdKeyMap IdKeyMap => Services.GetRequiredService<IIdKeyMap>();

    protected GlobalSettings GlobalSettings => Services.GetRequiredService<IOptions<GlobalSettings>>().Value;

    protected IMapperCollection Mappers => Services.GetRequiredService<IMapperCollection>();

    protected UserBuilder UserBuilderInstance { get; } = new();

    protected UserGroupBuilder UserGroupBuilderInstance { get; } = new();

    /// <summary>
    ///     When non-null, uses the swapper for database setup instead of the normal
    ///     <see cref="UmbracoIntegrationFixtureBase.UseTestDatabase"/> flow.
    ///     This enables swapping databases on a running host without restarting it.
    /// </summary>
    protected TestDatabaseSwapper DatabaseSwapper { get; set; }

    /// <summary>
    ///     Builds and starts the Umbraco host. Call this from [SetUp], [OneTimeSetUp], or any other context.
    /// </summary>
    public void BuildAndStartHost()
    {
        InMemoryConfiguration[Constants.Configuration.ConfigUnattended + ":" + nameof(UnattendedSettings.InstallUnattended)] = "true";
        var hostBuilder = CreateHostBuilder();

        _host = hostBuilder.Build();

        if (DatabaseSwapper is not null)
        {
            DatabaseSwapper.InitialSetup(_host.Services, Configuration, TestHelper);
        }
        else
        {
            UseTestDatabase(_host.Services);
        }

        _host.Start();

        if (TestOptions.Boot)
        {
            Services.GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();
        }
    }

    /// <summary>
    ///     Convenience method: swaps to a seeded database (snapshot-aware).
    ///     Requires <see cref="DatabaseSwapper"/> to be set.
    /// </summary>
    public Task SwapToSeededDatabaseAsync(ITestDatabaseSeedProfile seedProfile)
    {
        if (DatabaseSwapper is null)
        {
            throw new InvalidOperationException(
                "DatabaseSwapper must be set to use SwapToSeededDatabaseAsync.");
        }

        return DatabaseSwapper.SwapToSeededDatabaseAsync(Services, Configuration, TestHelper, seedProfile);
    }

    /// <summary>
    ///     Convenience method: swaps to a fresh database with schema + unattended install.
    ///     Requires <see cref="DatabaseSwapper"/> to be set.
    /// </summary>
    public Task SwapToFreshDatabaseAsync()
    {
        if (DatabaseSwapper is null)
        {
            throw new InvalidOperationException(
                "DatabaseSwapper must be set to use SwapToFreshDatabaseAsync.");
        }

        return DatabaseSwapper.SwapDatabaseAsync(Services, Configuration, TestHelper);
    }

    /// <summary>
    ///     Stops and disposes the Umbraco host. Call this from [TearDown], [OneTimeTearDown], or any other context.
    /// </summary>
    public void StopAndDisposeHost()
    {
        _host?.StopAsync().GetAwaiter().GetResult();
        (_host?.Services as IDisposable)?.Dispose();
        _host = null;
    }

    /// <summary>
    ///     Create the Generic Host and execute startup ConfigureServices/Configure calls
    /// </summary>
    private IHostBuilder CreateHostBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureUmbracoDefaults()
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
        services.AddSingleton<IWebProfilerRepository, TestWebProfilerRepository>();

        services.AddLogger(webHostEnvironment, Configuration);

        // Register a keyed service to verify that all calls to ServiceDescriptor.ImplementationType
        // are guarded by checking IsKeyedService first.
        services.AddKeyedSingleton<object>("key");

        // Add it!
        var hostingEnvironment = TestHelper.GetHostingEnvironment();
        var typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            TestHelper.ConsoleLoggerFactory,
            AppCaches.NoCache,
            Configuration,
            TestHelper.Profiler);
        var builder = new UmbracoBuilder(services, Configuration, typeLoader, TestHelper.ConsoleLoggerFactory, TestHelper.Profiler, AppCaches.NoCache);

        builder.AddConfiguration()
            .AddUmbracoCore()
            .AddWebComponents()
            .AddBackOfficeAuthentication()
            .AddBackOfficeIdentity()
            .AddMembersIdentity()
            .AddExamine()
            .AddUmbracoSqlServerSupport()
            .AddUmbracoSqliteSupport()
            .AddUmbracoHybridCache()
            .AddTestServices(TestHelper);

        if (TestOptions.Mapper)
        {
            builder
                .AddCoreMappingProfiles();
        }

        services.RemoveAll(x => x.ImplementationType == typeof(DocumentUrlServiceInitializerNotificationHandler));
        services.RemoveAll(x => x.ImplementationType == typeof(DocumentUrlAliasServiceInitializerNotificationHandler));
        services.AddSignalR();
        services.AddMvc();

        CustomTestSetup(builder);

        // custom helper services that might be moved out of tests eventually to benefit the community
        services.AddSingleton<IContentEditingModelFactory, ContentEditingModelFactory>();
        services.AddUnique<IRepositoryCacheVersionAccessor, RepositoryCacheVersionAccessor>();

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

    /// <summary>
    ///     Public accessor for resolving services from the host's service provider.
    ///     Useful when accessing the fixture from a [SetUpFixture] static instance.
    /// </summary>
    public T GetService<T>() => Services.GetRequiredService<T>();

    protected virtual void SetUpTestConfiguration(IConfigurationBuilder configBuilder)
    {
        if (GlobalSetupTeardown.TestConfiguration is not null)
        {
            configBuilder.AddConfiguration(GlobalSetupTeardown.TestConfiguration);
        }
    }

    protected void DeleteAllTemplateViewFiles()
    {
        var fileSystems = GetRequiredService<FileSystems>();
        var viewFileSystem = fileSystems.MvcViewsFileSystem!;
        foreach (var file in viewFileSystem.GetFiles(string.Empty).ToArray())
        {
            viewFileSystem.DeleteFile(file);
        }
    }
}
