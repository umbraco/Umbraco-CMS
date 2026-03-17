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
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Testing.Fixtures;

/// <summary>
///     Fixture-agnostic base class for integration tests.
///     Unlike <see cref="UmbracoIntegrationTestBase"/>, this class does NOT use NUnit lifecycle attributes,
///     allowing it to be used from both [TestFixture] and [SetUpFixture] contexts.
/// </summary>
[SingleThreaded]
[NonParallelizable]
public abstract class UmbracoIntegrationFixtureBase
{
    private static readonly Lock s_dbLocker = new();
    private static ITestDatabase? s_dbInstance;
    private static TestDbMeta s_fixtureDbMeta;

    protected static int TestCount = 1;

    private readonly List<Action> _fixtureTeardown = new();
    private readonly Queue<Action> _testTeardown = new();

    protected bool IsFirstTestInFixture { get; set; } = true;

    protected Dictionary<string, string> InMemoryConfiguration { get; } = new();

    protected IConfiguration Configuration { get; set; }

    protected UmbracoTestAttribute TestOptions => TestOptionAttributeBase.GetTestOptions<UmbracoTestAttribute>();

    protected TestHelper TestHelper { get; } = new();

    protected void AddOnTestTearDown(Action tearDown) => _testTeardown.Enqueue(tearDown);

    protected void AddOnFixtureTearDown(Action tearDown) => _fixtureTeardown.Add(tearDown);

    /// <summary>
    ///     Log the start of a test. Call from [SetUp] or [OneTimeSetUp] as needed.
    /// </summary>
    protected void OnSetUpLogging() =>
        TestContext.Out.Write($"Start test {TestCount++}: {TestContext.CurrentContext.Test.Name}");

    /// <summary>
    ///     Log the result of a test. Call from [TearDown] or [OneTimeTearDown] as needed.
    /// </summary>
    protected void OnTearDownLogging() =>
        TestContext.Out.Write($"  {TestContext.CurrentContext.Result.Outcome.Status}");

    /// <summary>
    ///     Execute per-test teardown actions. Call from [TearDown] or after each test in a shared fixture.
    /// </summary>
    protected void OnTestTearDown()
    {
        IsFirstTestInFixture = false;

        while (_testTeardown.TryDequeue(out var a))
        {
            a();
        }
    }

    /// <summary>
    ///     Execute per-fixture teardown actions. Call from [OneTimeTearDown].
    /// </summary>
    protected void OnFixtureTearDown()
    {
        foreach (var a in _fixtureTeardown)
        {
            a();
        }
    }

    protected ILoggerFactory CreateLoggerFactory()
    {
        try
        {
            switch (TestOptions.Logger)
            {
                case UmbracoTestOptions.Logger.Mock:
                    return NullLoggerFactory.Instance;
                case UmbracoTestOptions.Logger.Serilog:
                    return LoggerFactory.Create(builder =>
                    {
                        var path = Path.Combine(TestHelper.WorkingDirectory, "logs", "umbraco_integration_tests_.txt");

                        Log.Logger = new LoggerConfiguration()
                            .WriteTo.File(path, rollingInterval: RollingInterval.Day)
                            .MinimumLevel.Debug()
                            .ReadFrom.Configuration(Configuration)
                            .CreateLogger();

                        builder.AddSerilog(Log.Logger);
                    });
                case UmbracoTestOptions.Logger.Console:
                    return LoggerFactory.Create(builder =>
                    {
                        builder.AddConfiguration(Configuration.GetSection("Logging"))
                            .AddConsole();
                    });
            }
        }
        catch
        {
            // ignored
        }

        return NullLoggerFactory.Instance;
    }

    protected void UseTestDatabase(IServiceProvider serviceProvider)
    {
        if (TestOptions.Database == UmbracoTestOptions.Database.None)
        {
            return;
        }

        var state = serviceProvider.GetRequiredService<IRuntimeState>();
        var databaseFactory = serviceProvider.GetRequiredService<IUmbracoDatabaseFactory>();
        var connectionStrings = serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();

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
        serviceProvider.GetRequiredService<IEventAggregator>().Publish(new UnattendedInstallNotification());
    }

    private TestDbMeta SetupPerTestDatabase(ITestDatabase db, bool withSchema)
    {
        var meta = withSchema ? db.AttachSchema() : db.AttachEmpty();
        AddOnTestTearDown(() => db.Detach(meta));
        return meta;
    }

    private TestDbMeta SetupPerFixtureDatabase(ITestDatabase db, bool withSchema)
    {
        if (IsFirstTestInFixture)
        {
            s_fixtureDbMeta = withSchema ? db.AttachSchema() : db.AttachEmpty();
            AddOnFixtureTearDown(() => db.Detach(s_fixtureDbMeta));
        }

        return s_fixtureDbMeta;
    }

    private ITestDatabase GetOrCreateDatabase(ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactory)
    {
        lock (s_dbLocker)
        {
            if (s_dbInstance != null)
            {
                return s_dbInstance;
            }

            var settings = new TestDatabaseSettings
            {
                FilesPath = Path.Combine(TestHelper.WorkingDirectory, "databases"),
                DatabaseType =
                    Configuration.GetValue<TestDatabaseSettings.TestDatabaseType>("Tests:Database:DatabaseType"),
                PrepareThreadCount = Configuration.GetValue<int>("Tests:Database:PrepareThreadCount"),
                EmptyDatabasesCount = Configuration.GetValue<int>("Tests:Database:EmptyDatabasesCount"),
                SchemaDatabaseCount = Configuration.GetValue<int>("Tests:Database:SchemaDatabaseCount"),
                SQLServerMasterConnectionString = Configuration.GetValue<string>("Tests:Database:SQLServerMasterConnectionString"),
            };

            Directory.CreateDirectory(settings.FilesPath);

            s_dbInstance = TestDatabaseFactory.Create(settings, dbFactory, loggerFactory);

            return s_dbInstance;
        }
    }
}
