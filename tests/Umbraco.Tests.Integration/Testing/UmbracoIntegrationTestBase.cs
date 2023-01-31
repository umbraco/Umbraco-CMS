using System.ComponentModel;
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
    protected static ConnectionStrings s_connectionStrings;
    private static int s_testCount = 1;
    private readonly List<Action> _fixtureTeardown = new();

    private bool _firstTestInFixture = true;

    protected IUmbracoDatabase BaseTestDatabase { get; set; }
    protected Dictionary<string, string> InMemoryConfiguration { get; } = new();

    protected IConfiguration Configuration { get; set; }

    protected UmbracoTestAttribute TestOptions =>
        TestOptionAttributeBase.GetTestOptions<UmbracoTestAttribute>();

    protected TestHelper TestHelper { get; } = new();

    [SetUp]
    public void SetUp_Logging() =>
        TestContext.Progress.Write($"Start test {s_testCount++}: {TestContext.CurrentContext.Test.Name}");

    [TearDown]
    public void TearDown_Logging() =>
        TestContext.Progress.Write($"  {TestContext.CurrentContext.Result.Outcome.Status}");

    [TearDown]
    public void TearDown()
    {
        _firstTestInFixture = false;
    }

    [OneTimeTearDown]
    public void FixtureTearDown()
    {
        var bw = new BackgroundWorker();

        bw.DoWork += delegate
        {
            Parallel.ForEach(_fixtureTeardown, a =>
            {
                a();
            });
        };

        bw.RunWorkerAsync();

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
        var umbracoDatabaseFactory = serviceProvider.GetRequiredService<IUmbracoDatabaseFactory>();
        var connectionStrings = serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();
        var databaseSchemaCreatorFactory = serviceProvider.GetRequiredService<IDatabaseSchemaCreatorFactory>();
        var databaseDataCreator = serviceProvider.GetRequiredService<IDatabaseDataCreator>();
        var testDatabaseFactory = serviceProvider.GetRequiredService<UmbracoTestDatabaseConfigurationFactory>();

        // This will create a db, install the schema and ensure the app is configured to run
        SetupTestDatabase(connectionStrings, umbracoDatabaseFactory, state, databaseSchemaCreatorFactory, databaseDataCreator, testDatabaseFactory);
    }

    private void SetupTestDatabase(
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        IUmbracoDatabaseFactory databaseFactory,
        IRuntimeState runtimeState,
        IDatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
        IDatabaseDataCreator databaseDataCreator,
        UmbracoTestDatabaseConfigurationFactory testDatabaseConfigurationFactory)
    {
        if (TestOptions.Database == UmbracoTestOptions.Database.None)
        {
            return;
        }

        switch (TestOptions.Database)
        {
            case UmbracoTestOptions.Database.NewSchemaPerTest:

                ConfigureDatabase(testDatabaseConfigurationFactory, connectionStrings, databaseFactory);

                CreateDatabaseWithSchema(databaseFactory, databaseSchemaCreatorFactory);
                databaseDataCreator.SeedDataAsync().GetAwaiter().GetResult();

                runtimeState.DetermineRuntimeLevel();

                Assert.AreEqual(RuntimeLevel.Run, runtimeState.Level);

                break;
            case UmbracoTestOptions.Database.NewEmptyPerTest:

                ConfigureDatabase(testDatabaseConfigurationFactory, connectionStrings, databaseFactory);

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
                    ConfigureDatabase(testDatabaseConfigurationFactory, connectionStrings, databaseFactory);

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
                    ConfigureDatabase(testDatabaseConfigurationFactory, connectionStrings, databaseFactory);

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
    }

    private void ConfigureDatabase(UmbracoTestDatabaseConfigurationFactory testDatabaseConfigurationFactory, IOptionsMonitor<ConnectionStrings> connectionStrings, IUmbracoDatabaseFactory databaseFactory)
    {
        var newSchemaPerTestDb = testDatabaseConfigurationFactory.CreateTestDatabaseConfiguration();
        s_connectionStrings = newSchemaPerTestDb.InitializeConfiguration();
        connectionStrings.CurrentValue.ConnectionString = s_connectionStrings.ConnectionString;
        connectionStrings.CurrentValue.ProviderName = s_connectionStrings.ProviderName;

        databaseFactory.Configure(s_connectionStrings);
        _fixtureTeardown.Add(() => newSchemaPerTestDb.Teardown(newSchemaPerTestDb.Key));
    }
}
