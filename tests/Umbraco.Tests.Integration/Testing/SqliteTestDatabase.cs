using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using NPoco;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Persistence.Sqlite.Mappers;
using Umbraco.Cms.Persistence.Sqlite.Services;

namespace Umbraco.Cms.Tests.Integration.Testing;

public class SqliteTestDatabase : BaseTestDatabase, ITestDatabase
{
    private readonly TestDatabaseSettings _settings;
    private readonly TestUmbracoDatabaseFactoryProvider _dbFactoryProvider;
    public const string DatabaseName = "UmbracoTests";

    protected UmbracoDatabase.CommandInfo[] _cachedDatabaseInitCommands = new UmbracoDatabase.CommandInfo[0];

    public SqliteTestDatabase(TestDatabaseSettings settings, TestUmbracoDatabaseFactoryProvider dbFactoryProvider,
        ILoggerFactory loggerFactory)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _dbFactoryProvider = dbFactoryProvider;
        _databaseFactory = dbFactoryProvider.Create();
        _loggerFactory = loggerFactory;

        var counter = 0;

        var schema = Enumerable.Range(0, _settings.SchemaDatabaseCount)
            .Select(x => CreateSqLiteMeta(++counter, false));

        var empty = Enumerable.Range(0, _settings.EmptyDatabasesCount)
            .Select(x => CreateSqLiteMeta(++counter, true));

        _testDatabases = schema.Concat(empty).ToList();
    }

    protected override void Initialize()
    {
        _prepareQueue = new BlockingCollection<TestDbMeta>();
        _readySchemaQueue = new BlockingCollection<TestDbMeta>();
        _readyEmptyQueue = new BlockingCollection<TestDbMeta>();

        foreach (var file in Directory.GetFiles(_settings.FilesPath))
        {
            if (!Path.GetFileName(file).StartsWith(DatabaseName))
            {
                continue;
            }

            File.Delete(file);
        }

        var creator = new SqliteDatabaseCreator();
        foreach (TestDbMeta meta in _testDatabases)
        {
            creator.Create(meta.ConnectionString);
            _prepareQueue.Add(meta);
        }

        for (var i = 0; i < _settings.PrepareThreadCount; i++)
        {
            var thread = new Thread(PrepareDatabase);
            thread.Start();
        }
    }

    protected override void ResetTestDatabase(TestDbMeta meta) => Drop(meta);

    protected override DbConnection GetConnection(TestDbMeta meta) => new SqliteConnection(meta.ConnectionString);

    protected override void RebuildSchema(IDbCommand command, TestDbMeta meta)
    {
        lock (_cachedDatabaseInitCommands)
        {
            if (!_cachedDatabaseInitCommands.Any())
            {
                RebuildSchemaFirstTime(meta);
                return;
            }
        }

        new SqliteDatabaseCreator().Create(meta.ConnectionString);
        using var connection = GetConnection(meta);
        connection.Open();

        // Get NPoco to handle all the type mappings (e.g. dates) for us.
        var database = new Database(connection, DatabaseType.SQLite);
        database.BeginTransaction();

        database.Mappers.Add(new NullableDateMapper());
        database.Mappers.Add(new SqlitePocoGuidMapper());

        foreach (UmbracoDatabase.CommandInfo dbCommand in _cachedDatabaseInitCommands)
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

        using NPoco.ITransaction transaction = database.GetTransaction();

        var schemaCreator = new DatabaseSchemaCreator(
            database,
            _loggerFactory.CreateLogger<DatabaseSchemaCreator>(), _loggerFactory,
            new UmbracoVersion(),
            Mock.Of<IEventAggregator>());

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
        while (_prepareQueue.TryTake(out _)) { }

        _readyEmptyQueue.CompleteAdding();
        while (_readyEmptyQueue.TryTake(out _)) { }

        _readySchemaQueue.CompleteAdding();
        while (_readySchemaQueue.TryTake(out _)) { }

        Parallel.ForEach(_testDatabases, Drop);
    }

    private void Drop(TestDbMeta meta)
    {
        // DO something... In memory only?
        try
        {
            foreach (var file in Directory.GetFiles(_settings.FilesPath))
            {
                if (!Path.GetFileName(file).StartsWith(meta.Name))
                {
                    continue;
                }

                File.Delete(file);
            }
        }
        catch (IOException ex)
        {
        }
    }

    private TestDbMeta CreateSqLiteMeta(int i, bool empty)
    {
        var name = $"{DatabaseName}-{i}.sqlite";
        var path = Path.Combine(_settings.FilesPath, name);

        var builder = new SqliteConnectionStringBuilder()
        {
            DataSource = path,
            Cache = SqliteCacheMode.Private,
            Pooling = false // TODO: PMJ - Breaks if connection pooling is on, why?
        };

        return new TestDbMeta(name, empty, builder.ConnectionString, Persistence.Sqlite.Constants.ProviderName, path);
    }
}
