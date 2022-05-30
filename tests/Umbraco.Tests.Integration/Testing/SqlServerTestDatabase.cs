// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence;

// ReSharper disable ConvertToUsingDeclaration
namespace Umbraco.Cms.Tests.Integration.Testing;

/// <remarks>
///     It's not meant to be pretty, rushed port of LocalDb.cs + LocalDbTestDatabase.cs
/// </remarks>
public class SqlServerTestDatabase : SqlServerBaseTestDatabase, ITestDatabase
{
    public const string DatabaseName = "UmbracoTests";
    private readonly TestDatabaseSettings _settings;

    public SqlServerTestDatabase(TestDatabaseSettings settings, ILoggerFactory loggerFactory, IUmbracoDatabaseFactory databaseFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));

        _settings = settings;

        var counter = 0;

        var schema = Enumerable.Range(0, _settings.SchemaDatabaseCount)
            .Select(x => TestDbMeta.CreateWithMasterConnectionString($"{DatabaseName}-{++counter}", false, _settings.SQLServerMasterConnectionString));

        var empty = Enumerable.Range(0, _settings.EmptyDatabasesCount)
            .Select(x => TestDbMeta.CreateWithMasterConnectionString($"{DatabaseName}-{++counter}", true, _settings.SQLServerMasterConnectionString));

        _testDatabases = schema.Concat(empty).ToList();
    }

    protected override void Initialize()
    {
        _prepareQueue = new BlockingCollection<TestDbMeta>();
        _readySchemaQueue = new BlockingCollection<TestDbMeta>();
        _readyEmptyQueue = new BlockingCollection<TestDbMeta>();

        foreach (var meta in _testDatabases)
        {
            CreateDatabase(meta);
            _prepareQueue.Add(meta);
        }

        for (var i = 0; i < _settings.PrepareThreadCount; i++)
        {
            var thread = new Thread(PrepareDatabase);
            thread.Start();
        }
    }

    private void CreateDatabase(TestDbMeta meta)
    {
        Drop(meta);

        using (var connection = new SqlConnection(_settings.SQLServerMasterConnectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                SetCommand(command, $@"CREATE DATABASE {LocalDb.QuotedName(meta.Name)}");
                command.ExecuteNonQuery();
            }
        }
    }

    private void Drop(TestDbMeta meta)
    {
        using (var connection = new SqlConnection(_settings.SQLServerMasterConnectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                SetCommand(command, "select count(1) from sys.databases where name = @0", meta.Name);
                var records = (int)command.ExecuteScalar();
                if (records == 0)
                {
                    return;
                }

                var sql = $@"
                        ALTER DATABASE {LocalDb.QuotedName(meta.Name)}
                        SET SINGLE_USER 
                        WITH ROLLBACK IMMEDIATE";
                SetCommand(command, sql);
                command.ExecuteNonQuery();

                SetCommand(command, $@"DROP DATABASE {LocalDb.QuotedName(meta.Name)}");
                command.ExecuteNonQuery();
            }
        }
    }

    public override void TearDown()
    {
        if (_prepareQueue == null)
        {
            return;
        }

        _prepareQueue.CompleteAdding();
        while (_prepareQueue.TryTake(out _))
        {
        }

        _readyEmptyQueue.CompleteAdding();
        while (_readyEmptyQueue.TryTake(out _))
        {
        }

        _readySchemaQueue.CompleteAdding();
        while (_readySchemaQueue.TryTake(out _))
        {
        }

        Parallel.ForEach(_testDatabases, Drop);
    }
}
