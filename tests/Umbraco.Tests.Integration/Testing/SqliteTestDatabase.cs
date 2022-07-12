using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using NPoco;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Persistence.Sqlite;
using Umbraco.Cms.Persistence.Sqlite.Mappers;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.Integration.Testing;

public class SqliteTestDatabase : BaseTestDatabase, ITestDatabase
{
    public const string DatabaseName = "UmbracoTests";
    private readonly TestUmbracoDatabaseFactoryProvider _dbFactoryProvider;
    private readonly TestDatabaseSettings _settings;

    protected UmbracoDatabase.CommandInfo[] _cachedDatabaseInitCommands = new UmbracoDatabase.CommandInfo[0];

    public SqliteTestDatabase(TestDatabaseSettings settings, TestUmbracoDatabaseFactoryProvider dbFactoryProvider, ILoggerFactory loggerFactory)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _dbFactoryProvider = dbFactoryProvider;
        _databaseFactory = dbFactoryProvider.Create();
        _loggerFactory = loggerFactory;

        var schema = Enumerable.Range(0, _settings.SchemaDatabaseCount)
            .Select(x => CreateSqLiteMeta(false));

        var empty = Enumerable.Range(0, _settings.EmptyDatabasesCount)
            .Select(x => CreateSqLiteMeta(true));

        _testDatabases = schema.Concat(empty).ToList();
    }

    public override void Detach(TestDbMeta meta)
    {
        meta.Connection.Close();
        _prepareQueue.TryAdd(CreateSqLiteMeta(meta.IsEmpty));
    }

    protected override void Initialize()
    {
        _prepareQueue = new BlockingCollection<TestDbMeta>();
        _readySchemaQueue = new BlockingCollection<TestDbMeta>();
        _readyEmptyQueue = new BlockingCollection<TestDbMeta>();

        foreach (var meta in _testDatabases)
        {
            _prepareQueue.Add(meta);
        }

        for (var i = 0; i < _settings.PrepareThreadCount; i++)
        {
            var thread = new Thread(PrepareDatabase);
            thread.Start();
        }
    }

    protected override void ResetTestDatabase(TestDbMeta meta)
    {
        // Database survives in memory until all connections closed.
        meta.Connection = GetConnection(meta);
        meta.Connection.Open();
    }

    protected override DbConnection GetConnection(TestDbMeta meta) => new SqliteConnection(meta.ConnectionString);

    protected override void RebuildSchema(IDbCommand command, TestDbMeta meta)
    {
        using var connection = GetConnection(meta);
        connection.Open();

        lock (_cachedDatabaseInitCommands)
        {
            if (!_cachedDatabaseInitCommands.Any())
            {
                RebuildSchemaFirstTime(meta);
                return;
            }
        }

        // Get NPoco to handle all the type mappings (e.g. dates) for us.
        var database = new Database(connection, DatabaseType.SQLite);
        database.BeginTransaction();

        database.Mappers.Add(new NullableDateMapper());
        database.Mappers.Add(new SqlitePocoGuidMapper());

        foreach (var dbCommand in _cachedDatabaseInitCommands)
        {
            database.Execute(dbCommand.Text, dbCommand.Parameters.Select(x => x.Value).ToArray());
        }

        database.CompleteTransaction();
    }

    private void RebuildSchemaFirstTime(TestDbMeta meta)
    {
        var dbFactory = _dbFactoryProvider.Create();
        dbFactory.Configure(meta.ToStronglyTypedConnectionString());

        using var database = (UmbracoDatabase)dbFactory.CreateDatabase();
        database.LogCommands = true;

        using var transaction = database.GetTransaction();

        var options =
            new TestOptionsMonitor<InstallDefaultDataSettings>(
                new InstallDefaultDataSettings { InstallData = InstallDefaultDataOption.All });

        var schemaCreator = new DatabaseSchemaCreator(
            database,
            _loggerFactory.CreateLogger<DatabaseSchemaCreator>(),
            _loggerFactory,
            new UmbracoVersion(),
            Mock.Of<IEventAggregator>(),
            options);

        schemaCreator.InitializeDatabaseSchema();
        transaction.Complete();

        _cachedDatabaseInitCommands = database.Commands
            .Where(x => !x.Text.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public override void TearDown()
    {
        if (_prepareQueue == null)
        {
            return;
        }

        _prepareQueue.CompleteAdding();
        while (_prepareQueue.TryTake(out _))
        { }

        _readyEmptyQueue.CompleteAdding();
        while (_readyEmptyQueue.TryTake(out _))
        { }

        _readySchemaQueue.CompleteAdding();
        while (_readySchemaQueue.TryTake(out _))
        { }
    }

    private TestDbMeta CreateSqLiteMeta(bool empty)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = $"{Guid.NewGuid()}",
            Mode = SqliteOpenMode.Memory,
            ForeignKeys = true,
            Pooling = false, // When pooling true, files kept open after connections closed, bad for cleanup.
            Cache = SqliteCacheMode.Shared
        };

        return new TestDbMeta(builder.DataSource, empty, builder.ConnectionString, Constants.ProviderName, "InMemory");
    }
}
