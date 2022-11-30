using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
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
    private static ITestDatabase s_dbInstance;
    private static TestDbMeta s_fixtureDbMeta;
    private static int s_testCount = 1;
    private readonly List<Action> _fixtureTeardown = new();
    private readonly Queue<Action> _testTeardown = new();

    private bool _firstTestInFixture = true;

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
        var databaseFactory = serviceProvider.GetRequiredService<IUmbracoDatabaseFactory>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var connectionStrings = serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();

        // This will create a db, install the schema and ensure the app is configured to run
        SetupTestDatabase(testDatabaseFactoryProvider, connectionStrings, databaseFactory, loggerFactory, state);
    }

    private void ConfigureTestDatabaseFactory(
        TestDbMeta meta,
        IUmbracoDatabaseFactory factory,
        IRuntimeState state,
        IOptionsMonitor<ConnectionStrings> connectionStrings)
    {
        // It's just been pulled from container and wasn't used to create test database
        Assert.IsFalse(factory.Configured);

        factory.Configure(meta.ToStronglyTypedConnectionString());
        connectionStrings.CurrentValue.ConnectionString = meta.ConnectionString;
        connectionStrings.CurrentValue.ProviderName = meta.Provider;
        state.DetermineRuntimeLevel();
    }

    private void SetupTestDatabase(
        TestUmbracoDatabaseFactoryProvider testUmbracoDatabaseFactoryProvider,
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        IUmbracoDatabaseFactory databaseFactory,
        ILoggerFactory loggerFactory,
        IRuntimeState runtimeState)
    {
        if (TestOptions.Database == UmbracoTestOptions.Database.None)
        {
            return;
        }

        var db = GetOrCreateDatabase(loggerFactory, testUmbracoDatabaseFactoryProvider);

        switch (TestOptions.Database)
        {
            case UmbracoTestOptions.Database.NewSchemaPerTest:

                // New DB + Schema
                var newSchemaDbMeta = db.AttachSchema();

                // Add teardown callback
                AddOnTestTearDown(() => db.Detach(newSchemaDbMeta));

                ConfigureTestDatabaseFactory(newSchemaDbMeta, databaseFactory, runtimeState, connectionStrings);

                Assert.AreEqual(RuntimeLevel.Run, runtimeState.Level);

                break;
            case UmbracoTestOptions.Database.NewEmptyPerTest:
                var newEmptyDbMeta = db.AttachEmpty();

                // Add teardown callback
                AddOnTestTearDown(() => db.Detach(newEmptyDbMeta));

                ConfigureTestDatabaseFactory(newEmptyDbMeta, databaseFactory, runtimeState, connectionStrings);

                Assert.AreEqual(RuntimeLevel.Install, runtimeState.Level);

                break;
            case UmbracoTestOptions.Database.NewSchemaPerFixture:
                // Only attach schema once per fixture
                // Doing it more than once will block the process since the old db hasn't been detached
                // and it would be the same as NewSchemaPerTest even if it didn't block
                if (_firstTestInFixture)
                {
                    // New DB + Schema
                    var newSchemaFixtureDbMeta = db.AttachSchema();
                    s_fixtureDbMeta = newSchemaFixtureDbMeta;

                    // Add teardown callback
                    AddOnFixtureTearDown(() => db.Detach(newSchemaFixtureDbMeta));
                }

                ConfigureTestDatabaseFactory(s_fixtureDbMeta, databaseFactory, runtimeState, connectionStrings);

                break;
            case UmbracoTestOptions.Database.NewEmptyPerFixture:
                // Only attach schema once per fixture
                // Doing it more than once will block the process since the old db hasn't been detached
                // and it would be the same as NewSchemaPerTest even if it didn't block
                if (_firstTestInFixture)
                {
                    // New DB + Schema
                    var newEmptyFixtureDbMeta = db.AttachEmpty();
                    s_fixtureDbMeta = newEmptyFixtureDbMeta;

                    // Add teardown callback
                    AddOnFixtureTearDown(() => db.Detach(newEmptyFixtureDbMeta));
                }

                ConfigureTestDatabaseFactory(s_fixtureDbMeta, databaseFactory, runtimeState, connectionStrings);

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(TestOptions), TestOptions, null);
        }
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
                SQLServerMasterConnectionString =
                    Configuration.GetValue<string>("Tests:Database:SQLServerMasterConnectionString")
            };

            Directory.CreateDirectory(settings.FilesPath);

            s_dbInstance = TestDatabaseFactory.Create(settings, dbFactory, loggerFactory);

            return s_dbInstance;
        }
    }
}
