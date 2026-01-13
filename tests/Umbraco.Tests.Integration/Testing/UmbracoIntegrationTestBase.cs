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
    private static readonly Lock _dbLocker = new();
    private static ITestDatabase? _dbInstance;
    private static TestDbMeta _fixtureDbMeta;

    // TODO (V18): Rename to s_testCount to follow naming conventions
    protected static int TestCount = 1;

    private readonly List<Action> _fixtureTeardown = new();
    private readonly Queue<Action> _testTeardown = new();
    private bool _firstTestInFixture = true;

    protected Dictionary<string, string> InMemoryConfiguration { get; } = new();

    protected IConfiguration Configuration { get; set; }

    protected UmbracoTestAttribute TestOptions => TestOptionAttributeBase.GetTestOptions<UmbracoTestAttribute>();

    protected TestHelper TestHelper { get; } = new();

    protected void AddOnTestTearDown(Action tearDown) => _testTeardown.Enqueue(tearDown);

    protected void AddOnFixtureTearDown(Action tearDown) => _fixtureTeardown.Add(tearDown);

    [SetUp]
    public virtual void SetUp_Logging() =>
        TestContext.Out.Write($"Start test {TestCount++}: {TestContext.CurrentContext.Test.Name}");

    [TearDown]
    public void TearDown_Logging() =>
        TestContext.Out.Write($"  {TestContext.CurrentContext.Result.Outcome.Status}");

    [OneTimeTearDown]
    public void FixtureTearDown()
    {
        foreach (var a in _fixtureTeardown)
        {
            a();
        }
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
        if (_firstTestInFixture)
        {
            _fixtureDbMeta = withSchema ? db.AttachSchema() : db.AttachEmpty();
            AddOnFixtureTearDown(() => db.Detach(_fixtureDbMeta));
        }

        return _fixtureDbMeta;
    }

    private ITestDatabase GetOrCreateDatabase(ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactory)
    {
        lock (_dbLocker)
        {
            if (_dbInstance != null)
            {
                return _dbInstance;
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

            _dbInstance = TestDatabaseFactory.Create(settings, dbFactory, loggerFactory);

            return _dbInstance;
        }
    }
}
