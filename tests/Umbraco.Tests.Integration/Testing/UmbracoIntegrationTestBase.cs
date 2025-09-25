using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Serilog;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Implementations;

namespace Umbraco.Cms.Tests.Integration.Testing;

/// <summary>
///     Base class for all UmbracoIntegrationTests
/// </summary>
[SingleThreaded]
[NonParallelizable]
public abstract class UmbracoIntegrationTestBase
{
    private static readonly object s_dbLocker = new();
    private static ITestDatabase? s_dbInstance;
    private static TestDbMeta s_fixtureDbMeta;
    private readonly List<Action> _fixtureTeardown = new();
    private readonly Queue<Action> _testTeardown = new();
    private bool _firstTestInFixture = true;

    protected Dictionary<string, string> InMemoryConfiguration { get; } = new();

    protected IConfiguration Configuration { get; set; }

    protected UmbracoTestAttribute TestOptions => TestOptionAttributeBase.GetTestOptions<UmbracoTestAttribute>();

    protected TestHelper TestHelper { get; } = new();

    protected void AddOnTestTearDown(Action tearDown) => _testTeardown.Enqueue(tearDown);

    protected void AddOnFixtureTearDown(Action tearDown) => _fixtureTeardown.Add(tearDown);

    [OneTimeTearDown]
    public void FixtureTearDown()
    {
        TestHelper?.DisposeIfDisposable();
        foreach (var a in _fixtureTeardown)
        {
            a();
        }

        (s_dbInstance as IDisposable)?.Dispose();
        _firstTestInFixture = true;
    }

    [TearDown]
    public void TearDown()
    {
        _firstTestInFixture = false;

        while (_testTeardown.TryDequeue(out var a))
        {
            a();
        }
    }

    protected ILoggerFactory CreateLoggerFactory()
    {
        try
        {
            return TestOptions.Logger switch
            {
                UmbracoTestOptions.Logger.Serilog => LoggerFactory.Create(builder =>
                    builder.AddSerilog(new LoggerConfiguration()
                        .WriteTo.File(
                            Path.Combine(TestHelper.WorkingDirectory, "logs", "umbraco_integration_tests_.txt"),
                            rollingInterval: RollingInterval.Day)
                        .MinimumLevel.Warning()
                        .CreateLogger())),
                UmbracoTestOptions.Logger.Console => LoggerFactory.Create(builder => builder.AddConsole()),
                _ => NullLoggerFactory.Instance,
            };
        }
        catch
        {
            return NullLoggerFactory.Instance;
        }
    }

    protected void UseTestDatabase(IApplicationBuilder app)
        => UseTestDatabase(app.ApplicationServices);

    protected void UseTestDatabase(IServiceProvider serviceProvider)
    {
        var databaseFactory = serviceProvider.GetRequiredService<IUmbracoDatabaseFactory>();
        var connectionStrings = serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();
        var state = serviceProvider.GetRequiredService<IRuntimeState>();

        if (TestOptions.Database == UmbracoTestOptions.Database.None)
        {
            return;
        }

        var db = GetOrCreateDatabase(
            serviceProvider.GetRequiredService<ILoggerFactory>(),
            serviceProvider.GetRequiredService<TestUmbracoDatabaseFactoryProvider>());

        TestDbMeta meta = TestOptions.Database switch
        {
            UmbracoTestOptions.Database.NewSchemaPerTest => SetupPerTestDatabase(db, true),
            UmbracoTestOptions.Database.NewEmptyPerTest => SetupPerTestDatabase(db, false),
            UmbracoTestOptions.Database.NewSchemaPerFixture => SetupPerFixtureDatabase(db, true),
            UmbracoTestOptions.Database.NewEmptyPerFixture => SetupPerFixtureDatabase(db, false),
            _ => throw new ArgumentOutOfRangeException(),
        };

        databaseFactory.Configure(meta.ToStronglyTypedConnectionString());
        connectionStrings.CurrentValue.ConnectionString = meta.ConnectionString;
        connectionStrings.CurrentValue.ProviderName = meta.Provider;
        state.DetermineRuntimeLevel();

        if (TestOptions.Database != UmbracoTestOptions.Database.None)
        {
            serviceProvider.GetRequiredService<IEventAggregator>().Publish(new UnattendedInstallNotification());
        }
    }

    private TestDbMeta SetupPerTestDatabase(ITestDatabase db, bool withSchema)
    {
        var meta = withSchema ? db.AttachSchema() : db.AttachEmpty();
        AddOnTestTearDown(() => db.Detach(meta));
        return meta;
    }

    private TestDbMeta SetupPerFixtureDatabase(ITestDatabase db, bool withSchema)
    {
        if (_firstTestInFixture)
        {
            s_fixtureDbMeta = withSchema ? db.AttachSchema() : db.AttachEmpty();
            AddOnFixtureTearDown(() => db.Detach(s_fixtureDbMeta));
        }

        return s_fixtureDbMeta;
    }

    private ITestDatabase GetOrCreateDatabase(ILoggerFactory loggerFactory,
        TestUmbracoDatabaseFactoryProvider dbFactory)
    {
        lock (s_dbLocker)
        {
            return s_dbInstance ??= TestDatabaseFactory.Create(
                new TestDatabaseSettings
                {
                    FilesPath = Path.Combine(TestHelper.WorkingDirectory, "databases"),
                    DatabaseType =
                        Configuration.GetValue<TestDatabaseSettings.TestDatabaseType>("Tests:Database:DatabaseType"),
                    PrepareThreadCount = Configuration.GetValue<int>("Tests:Database:PrepareThreadCount"),
                    EmptyDatabasesCount = Configuration.GetValue<int>("Tests:Database:EmptyDatabasesCount"),
                    SchemaDatabaseCount = Configuration.GetValue<int>("Tests:Database:SchemaDatabaseCount"),
                    SQLServerMasterConnectionString =
                        Configuration.GetValue<string>("Tests:Database:SQLServerMasterConnectionString"),
                }, dbFactory, loggerFactory);
        }
    }
}
