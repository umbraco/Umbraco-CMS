using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;

// ReSharper disable ConvertToUsingDeclaration

namespace Umbraco.Tests.Integration.Testing
{
    /// <remarks>
    /// It's not meant to be pretty, rushed port of LocalDb.cs + LocalDbTestDatabase.cs
    /// </remarks>
    public class SqlDeveloperTestDatabase : ITestDatabase
    {

        // This is gross but it's how the other one works and I don't want to refactor everything.
        public string ConnectionString { get; private set; }

        private readonly string _masterConnectionString;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _log;
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private readonly IDictionary<int, TestDbMeta> _testDatabases;
        private UmbracoDatabase.CommandInfo[] _cachedDatabaseInitCommands;

        private BlockingCollection<TestDbMeta> _prepareQueue;
        private BlockingCollection<TestDbMeta> _readySchemaQueue;
        private BlockingCollection<TestDbMeta> _readyEmptyQueue;

        private const string _databasePrefix = "UmbracoTest";
        private const int _threadCount = 2;

        public static SqlDeveloperTestDatabase Instance;

        public SqlDeveloperTestDatabase(ILoggerFactory loggerFactory, IUmbracoDatabaseFactory databaseFactory, string masterConnectionString)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));
            _masterConnectionString = masterConnectionString;
            _log = loggerFactory.CreateLogger<SqlDeveloperTestDatabase>();

            _testDatabases = new[]
            {
                new TestDbMeta(1, false, masterConnectionString),
                new TestDbMeta(2, false, masterConnectionString),

                new TestDbMeta(3, true, masterConnectionString),
            }.ToDictionary(x => x.Id);

            Instance = this; // For GlobalSetupTeardown.cs
        }

        public int AttachEmpty()
        {
            if (_prepareQueue == null)
            {
                Initialize();
            }

            var meta = _readyEmptyQueue.Take();

            ConnectionString = meta.ConnectionString;

            return meta.Id;
        }

        public int AttachSchema()
        {
            if (_prepareQueue == null)
            {
                Initialize();
            }

            var meta = _readySchemaQueue.Take();

            ConnectionString = meta.ConnectionString;

            return meta.Id;
        }

        public void Detach(int id)
        {
            _prepareQueue.TryAdd(_testDatabases[id]);
        }

        private void CreateDatabase(TestDbMeta meta)
        {
            _log.LogInformation($"Creating database {meta.Name}");
            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    SetCommand(command, $@"CREATE DATABASE {LocalDb.QuotedName(meta.Name)}");
                    command.ExecuteNonQuery();
                }
            }
        }

        private static string ConstructConnectionString(string masterConnectionString, string databaseName)
        {
            var prefix = Regex.Replace(masterConnectionString, "Database=.+?;", string.Empty);
            var connectionString = $"{prefix};Database={databaseName};";
            return connectionString.Replace(";;", ";");
        }

        private static void SetCommand(SqlCommand command, string sql, params object[] args)
        {
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            command.Parameters.Clear();

            for (var i = 0; i < args.Length; i++)
            {
                command.Parameters.AddWithValue("@" + i, args[i]);
            }
        }

        private void RebuildSchema(IDbCommand command, TestDbMeta meta)
        {
            if (_cachedDatabaseInitCommands != null)
            {
                foreach (var dbCommand in _cachedDatabaseInitCommands)
                {

                    if (dbCommand.Text.StartsWith("SELECT "))
                    {
                        continue;
                    }

                    command.CommandText = dbCommand.Text;
                    command.Parameters.Clear();

                    foreach (var parameterInfo in dbCommand.Parameters)
                    {
                        LocalDbTestDatabase.AddParameter(command, parameterInfo);
                    }

                    command.ExecuteNonQuery();
                }
            }
            else
            {
                _databaseFactory.Configure(meta.ConnectionString, Constants.DatabaseProviders.SqlServer);

                using (var database = (UmbracoDatabase)_databaseFactory.CreateDatabase())
                {
                    database.LogCommands = true;

                    using (var transaction = database.GetTransaction())
                    {
                        var schemaCreator = new DatabaseSchemaCreator(database, _loggerFactory.CreateLogger<DatabaseSchemaCreator>(), _loggerFactory, new UmbracoVersion());
                        schemaCreator.InitializeDatabaseSchema();

                        transaction.Complete();

                        _cachedDatabaseInitCommands = database.Commands.ToArray();
                    }
                }
            }
        }

        private void Initialize()
        {
            _prepareQueue = new BlockingCollection<TestDbMeta>();
            _readySchemaQueue = new BlockingCollection<TestDbMeta>();
            _readyEmptyQueue = new BlockingCollection<TestDbMeta>();

            foreach (var meta in _testDatabases.Values)
            {
                CreateDatabase(meta);
                _prepareQueue.Add(meta);
            }

            for (var i = 0; i < _threadCount; i++)
            {
                var thread = new Thread(PrepareThread);
                thread.Start();
            }
        }

        private void Drop(TestDbMeta meta)
        {
            _log.LogInformation($"Dropping database {meta.Name}");
            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    SetCommand(command, $@"
                        ALTER DATABASE{LocalDb.QuotedName(meta.Name)}
                        SET SINGLE_USER
                        WITH ROLLBACK IMMEDIATE
                    ");
                    command.ExecuteNonQuery();

                    SetCommand(command, $@"DROP DATABASE {LocalDb.QuotedName(meta.Name)}");
                    command.ExecuteNonQuery();
                }
            }
        }

        private void PrepareThread()
        {
            LocalDbTestDatabase.Retry(10, () =>
            {
                while (_prepareQueue.IsCompleted == false)
                {
                    TestDbMeta meta;
                    try
                    {
                        meta = _prepareQueue.Take();
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }

                    using (var conn = new SqlConnection(meta.ConnectionString))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        LocalDbTestDatabase.ResetLocalDb(cmd);

                        if (!meta.IsEmpty)
                        {
                            RebuildSchema(cmd, meta);
                        }
                    }

                    if (!meta.IsEmpty)
                    {
                        _readySchemaQueue.TryAdd(meta);
                    }
                    else
                    {
                        _readyEmptyQueue.TryAdd(meta);
                    }
                }
            });
        }

        public void Finish()
        {
            if (_prepareQueue == null)
                return;

            _prepareQueue.CompleteAdding();
            while (_prepareQueue.TryTake(out _)) { }

            _readyEmptyQueue.CompleteAdding();
            while (_readyEmptyQueue.TryTake(out _)) { }

            _readySchemaQueue.CompleteAdding();
            while (_readySchemaQueue.TryTake(out _)) { }

            foreach (var testDatabase in _testDatabases.Values)
            {
                Drop(testDatabase);
            }
        }

        private class TestDbMeta
        {
            public int Id { get; }
            public string Name => $"{_databasePrefix}-{Id}";
            public bool IsEmpty { get; }
            public string ConnectionString { get; }

            public TestDbMeta(int id, bool isEmpty, string masterConnectionString)
            {
                Id = id;
                IsEmpty = isEmpty;
                ConnectionString = ConstructConnectionString(masterConnectionString, Name);
            }
        }
    }
}
