using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Serilog;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.EFCore;
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
    protected static ITestDatabase? s_dbInstance;
    protected static TestDbMeta s_fixtureDbMeta;
    protected static ConnectionStrings s_connectionStrings;
    private static int s_testCount = 1;
    private readonly List<Action> _fixtureTeardown = new();
    private readonly Queue<Action> _testTeardown = new();

    private bool _firstTestInFixture = true;

    protected IUmbracoDatabase BaseTestDatabase { get; set; }
    protected Dictionary<string, string> InMemoryConfiguration { get; } = new();

    protected IConfiguration Configuration { get; set; }

    protected UmbracoTestAttribute TestOptions =>
        TestOptionAttributeBase.GetTestOptions<UmbracoTestAttribute>();

    protected TestHelper TestHelper { get; } = new();

    private void AddOnTestTearDown(Action tearDown) => _testTeardown.Enqueue(tearDown);

    private void AddOnFixtureTearDown(Action tearDown) => _fixtureTeardown.Add(tearDown);

    [SetUp]
    public void SetUp_Logging() =>
        TestContext.Progress.Write($"Start test {s_testCount++}: {TestContext.CurrentContext.Test.Name}");

    [TearDown]
    public void TearDown_Logging() =>
        TestContext.Progress.Write($"  {TestContext.CurrentContext.Result.Outcome.Status}");

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

    protected void UseTestDatabase(IApplicationBuilder app)
        => UseTestDatabase(app.ApplicationServices);

    protected void UseTestDatabase(IServiceProvider serviceProvider)
    {
        var state = serviceProvider.GetRequiredService<IRuntimeState>();
        var testDatabaseFactoryProvider = serviceProvider.GetRequiredService<TestUmbracoDatabaseFactoryProvider>();
        var umbracoDatabaseFactory = serviceProvider.GetRequiredService<IUmbracoDatabaseFactory>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var umbracoDbContextFactory = serviceProvider.GetRequiredService<UmbracoDbContextFactory>();
        var connectionStrings = serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();
        var databaseSchemaCreatorFactory = serviceProvider.GetRequiredService<IDatabaseSchemaCreatorFactory>();
        var databaseDataCreator = serviceProvider.GetRequiredService<IDatabaseDataCreator>();
        var testDatabaseFactory = serviceProvider.GetRequiredService<UmbracoTestDatabaseFactory>();

        // This will create a db, install the schema and ensure the app is configured to run
        SetupTestDatabase(testDatabaseFactoryProvider, connectionStrings, umbracoDatabaseFactory, loggerFactory, state, umbracoDbContextFactory, databaseSchemaCreatorFactory, databaseDataCreator, testDatabaseFactory);
    }

    private void SetupTestDatabase(
        TestUmbracoDatabaseFactoryProvider testUmbracoDatabaseFactoryProvider,
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        IUmbracoDatabaseFactory databaseFactory,
        ILoggerFactory loggerFactory,
        IRuntimeState runtimeState,
        UmbracoDbContextFactory umbracoDbContextFactory, 
        IDatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
        IDatabaseDataCreator databaseDataCreator,
        UmbracoTestDatabaseFactory testDatabaseFactory)
    {
        if (TestOptions.Database == UmbracoTestOptions.Database.None)
        {
            return;
        }

        switch (TestOptions.Database)
        {
            case UmbracoTestOptions.Database.NewSchemaPerTest:

                // New DB + Schema
                var newSchemaPerTestDb = testDatabaseFactory.CreateTestDatabase();
                s_connectionStrings = newSchemaPerTestDb.Initialize();
                AddOnTestTearDown(() => newSchemaPerTestDb.Teardown());
                CreateDatabaseWithSchema(databaseFactory, databaseSchemaCreatorFactory);
                databaseDataCreator.SeedDataAsync().GetAwaiter().GetResult();

                runtimeState.DetermineRuntimeLevel();

                Assert.AreEqual(RuntimeLevel.Run, runtimeState.Level);

                break;
            case UmbracoTestOptions.Database.NewEmptyPerTest:

                var newEmptyPerTestDatabase = testDatabaseFactory.CreateTestDatabase();
                s_connectionStrings = newEmptyPerTestDatabase.Initialize();
                AddOnTestTearDown(() => newEmptyPerTestDatabase.Teardown());

                CreateDatabaseWithoutSchema(databaseFactory, databaseSchemaCreatorFactory);
                runtimeState.DetermineRuntimeLevel();
                Assert.AreEqual(RuntimeLevel.Install, runtimeState.Level);

                break;
            case UmbracoTestOptions.Database.NewSchemaPerFixture:
                // Only attach schema once per fixture
                // Doing it more than once will block the process since the old db hasn't been detached
                // and it would be the same as NewSchemaPerTest even if it didn't block
                if (_firstTestInFixture)
                {
                    // New DB + Schema
                    var newSchemaPerFixtureDb = testDatabaseFactory.CreateTestDatabase();
                    s_connectionStrings = newSchemaPerFixtureDb.Initialize();
                    AddOnFixtureTearDown(() => newSchemaPerFixtureDb.Teardown());

                    CreateDatabaseWithSchema(databaseFactory, databaseSchemaCreatorFactory);
                    databaseDataCreator.SeedDataAsync().GetAwaiter().GetResult();
                }
                else
                {
                    connectionStrings.CurrentValue.ConnectionString = s_connectionStrings.ConnectionString;
                    connectionStrings.CurrentValue.ProviderName = s_connectionStrings.ProviderName;
                    databaseFactory.Configure(s_connectionStrings);
                }


                break;
            case UmbracoTestOptions.Database.NewEmptyPerFixture:
                // Only attach schema once per fixture
                // Doing it more than once will block the process since the old db hasn't been detached
                // and it would be the same as NewSchemaPerTest even if it didn't block
                if (_firstTestInFixture)
                {
                    var newEmptyPerFixtureDb = testDatabaseFactory.CreateTestDatabase();
                    s_connectionStrings = newEmptyPerFixtureDb.Initialize();
                    AddOnFixtureTearDown(() => newEmptyPerFixtureDb.Teardown());
                    CreateDatabaseWithoutSchema(databaseFactory, databaseSchemaCreatorFactory);
                }
                else
                {
                    connectionStrings.CurrentValue.ConnectionString = s_connectionStrings.ConnectionString;
                    connectionStrings.CurrentValue.ProviderName = s_connectionStrings.ProviderName;
                    databaseFactory.Configure(s_connectionStrings);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(TestOptions), TestOptions, null);
        }
    }

    private void CreateDatabaseWithSchema(IUmbracoDatabaseFactory umbracoDatabaseFactory, IDatabaseSchemaCreatorFactory databaseSchemaCreatorFactory)
    {
        BaseTestDatabase = umbracoDatabaseFactory.CreateDatabase();
        BaseTestDatabase.BeginTransaction();
        IDatabaseSchemaCreator creator = databaseSchemaCreatorFactory.Create(BaseTestDatabase);
        creator.InitializeDatabaseSchema(false).GetAwaiter().GetResult();
        BaseTestDatabase.CompleteTransaction();
    }

    private void CreateDatabaseWithoutSchema(IUmbracoDatabaseFactory umbracoDatabaseFactory, IDatabaseSchemaCreatorFactory databaseSchemaCreatorFactory)
    {
        BaseTestDatabase = umbracoDatabaseFactory.CreateDatabase();
        BaseTestDatabase.BeginTransaction();
        databaseSchemaCreatorFactory.Create(BaseTestDatabase);
        BaseTestDatabase.CompleteTransaction();
    }

    private void CreateDatabase(IUmbracoDatabaseFactory databaseFactory, IOptionsMonitor<ConnectionStrings> connectionStrings)
    {
        string? projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
        string tempFolder = @"TEMP\databases";
        var tempFolderPath = Path.Combine(projectDirectory!, tempFolder);
        var absoluteDbPath = Path.Combine(tempFolderPath, Guid.NewGuid().ToString());
    
        switch (TestOptions.Database)
        {
            case UmbracoTestOptions.Database.NewSchemaPerTest:
                AddOnTestTearDown(() => TryDeleteFile(absoluteDbPath));
                break;
            case UmbracoTestOptions.Database.NewEmptyPerTest:
                AddOnTestTearDown(() => TryDeleteFile(absoluteDbPath));
                break;
            case UmbracoTestOptions.Database.NewSchemaPerFixture:
                if (_firstTestInFixture)
                {
                    AddOnFixtureTearDown(() => TryDeleteFile(absoluteDbPath));
                }
    
                break;
            case UmbracoTestOptions.Database.NewEmptyPerFixture:
                if (_firstTestInFixture)
                {
                    AddOnFixtureTearDown(() => TryDeleteFile(absoluteDbPath));
                }
    
                break;
        }
    
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = $"{absoluteDbPath}",
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
            Pooling = false, // When pooling true, files kept open after connections closed, bad for cleanup.
            Cache = SqliteCacheMode.Shared,
        };
    
        s_connectionStrings = new ConnectionStrings
        {
            ConnectionString = builder.ConnectionString,
            ProviderName = "Microsoft.Data.Sqlite",
        };
    
        connectionStrings.CurrentValue.ConnectionString = s_connectionStrings.ConnectionString;
        connectionStrings.CurrentValue.ProviderName = s_connectionStrings.ProviderName;
    
        databaseFactory.Configure(s_connectionStrings);
    }

    private void TryDeleteFile(string filePath)
    {
        const int maxRetries = 5;
        var retries = 0;
        var retry = true;
        do
        {
            try
            {
                File.Delete(filePath);
                retry = false;
            }
            catch (IOException)
            {
                retries++;
                if (retries >= maxRetries)
                {
                    throw;
                }

                Thread.Sleep(500);
            }
        }
        while (retry);
    }
}
