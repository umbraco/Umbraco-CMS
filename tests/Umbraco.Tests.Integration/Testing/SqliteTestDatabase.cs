using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Persistence.Sqlite.Services;

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

    protected override DbConnection GetConnection(TestDbMeta meta) => new SQLiteConnection(meta.ConnectionString);

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

        using var connection = GetConnection(meta);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        foreach (UmbracoDatabase.CommandInfo dbCommand in _cachedDatabaseInitCommands)
        {
            command.Connection = connection;
            command.CommandText = dbCommand.Text;
            command.Parameters.Clear();

            foreach (UmbracoDatabase.ParameterInfo parameterInfo in dbCommand.Parameters)
            {
                AddParameter(command, parameterInfo);
            }

            command.ExecuteNonQuery();
        }

        transaction.Commit();
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

        var builder = new SQLiteConnectionStringBuilder
        {
            DataSource = path
        };

        return new TestDbMeta(name, empty, builder.ConnectionString, Persistence.Sqlite.Constants.ProviderName, path);
    }
}
