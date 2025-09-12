using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
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
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.DependencyInjection;
using Umbraco.Cms.Tests.Integration.Extensions;

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

    protected IIdKeyMap IdKeyMap => Services.GetRequiredService<IIdKeyMap>();

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
    public void TearDownAsync()
    {
        _host.StopAsync();
        Services.DisposeIfDisposable();
    }

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
        services.AddSingleton<IWebProfilerRepository, TestWebProfilerRepository>();

        services.AddLogger(webHostEnvironment, Configuration);

        // Register a keyed service to verify that all calls to ServiceDescriptor.ImplementationType
        // are guarded by checking IsKeyedService first.
        // Failure to check this when accessing a keyed service descriptor's ImplementationType property
        // throws a InvalidOperationException.
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
            // TODO: Should these just be called from within AddUmbracoCore/AddWebComponents?
            builder
                .AddCoreMappingProfiles();
        }

        services.RemoveAll(x=>x.ImplementationType == typeof(DocumentUrlServiceInitializerNotificationHandler));
        services.AddSignalR();
        services.AddMvc();

        CustomTestSetup(builder);
        ExecuteBuilderAttributes(builder);

        // custom helper services that might be moved out of tests eventually to benefit the community
        services.AddSingleton<IContentEditingModelFactory, ContentEditingModelFactory>();

        builder.Build();
    }

    private void ExecuteBuilderAttributes(IUmbracoBuilder builder)
    {
        Type? testClassType = GetTestClassType()
            ?? throw new Exception($"Could not find test class for {TestContext.CurrentContext.Test.FullName} in order to execute builder attributes.");

        // Execute builder attributes defined on method.
        foreach (ConfigureBuilderAttribute builderAttribute in GetConfigureBuilderAttributes<ConfigureBuilderAttribute>(testClassType))
        {
            builderAttribute.Execute(builder);
        }

        // Execute builder attributes defined on method with param value pass through from test case.
        foreach (ConfigureBuilderTestCaseAttribute builderAttribute in GetConfigureBuilderAttributes<ConfigureBuilderTestCaseAttribute>(testClassType))
        {
            builderAttribute.Execute(builder);
        }
    }

    private static Type? GetTestClassType()
    {
        string testClassName = TestContext.CurrentContext.Test.ClassName;

        // Try resolving the type name directly (which will work for tests in this assembly).
        Type testClass = Type.GetType(testClassName);
        if (testClass is not null)
        {
            return testClass;
        }

        // Try scanning the loaded assemblies to see if we can find the class by full name. This will be necessary
        // for integration test projects using the base classess provided by Umbraco.
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        return assemblies
            .SelectMany(a => a.GetTypes().Where(t => t.FullName == testClassName))
            .FirstOrDefault();
    }

    private static IEnumerable<TAttribute> GetConfigureBuilderAttributes<TAttribute>(Type testClassType)
        where TAttribute : Attribute =>
        testClassType
            .GetMethods().First(m => m.Name == TestContext.CurrentContext.Test.MethodName)
            .GetCustomAttributes(typeof(TAttribute), true)
            .Cast<TAttribute>();

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
