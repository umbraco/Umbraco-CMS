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
using SQLitePCL;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    public class SQLiteTestDatabase : BaseTestDatabase, ITestDatabase
    {
        public static SQLiteTestDatabase Instance { get; private set; }

        private readonly TestDatabaseSettings _settings;
        public const string DatabaseName = "UmbracoTests";

        public SQLiteTestDatabase(TestDatabaseSettings settings, IUmbracoDatabaseFactory dbFactory, ILoggerFactory loggerFactory)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _databaseFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _loggerFactory = loggerFactory;

            var counter = 0;

            var schema = Enumerable.Range(0, _settings.SchemaDatabaseCount)
                .Select(x => CreateSQLiteMeta(++counter, false));

            var empty = Enumerable.Range(0, _settings.SchemaDatabaseCount)
                .Select(x => CreateSQLiteMeta(++counter, true));

            _testDatabases = schema.Concat(empty).ToList();

            Instance = this; // For GlobalSetupTeardown.cs
        }

        protected override void Initialize()
        {
            _prepareQueue = new BlockingCollection<TestDbMeta>();
            _readySchemaQueue = new BlockingCollection<TestDbMeta>();
            _readyEmptyQueue = new BlockingCollection<TestDbMeta>();

            foreach (TestDbMeta meta in _testDatabases)
            {
                Drop(meta);
                _prepareQueue.Add(meta);
            }

            for (int i = 0; i < _settings.PrepareThreadCount; i++)
            {
                var thread = new Thread(PrepareDatabase);
                thread.Start();
            }
        }

        protected override void ResetTestDatabase(TestDbMeta meta)
        {
            using (var connection = GetConnection(meta))
            {
                connection.Open();

                using (var db = new Database(connection))
                {
                    var tables = db.Fetch<string>("select name from sqlite_master where type='table'");
                    foreach (var table in tables.Where(x => !x.StartsWith("sqlite")))
                    {
                        db.Execute($"drop table {table}");
                    }
                }
            }
        }

        protected override DbConnection GetConnection(TestDbMeta meta) => new SqliteConnection(meta.ConnectionString);

        protected override void RebuildSchema(IDbCommand command, TestDbMeta meta)
        {
            base.RebuildSchema(command, meta);

            // Base rebuilds from cached commands, for whatever reason keyvalue table updated column ends up null
            // and DetermineRuntimeLevel is install for second test.
            // TODO: SQLite - Fix all the things.
            command.CommandText = "update umbracoKeyValue set updated = date()";
            command.Parameters.Clear();
            command.ExecuteNonQuery();
        }

        public void Finish()
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
                GC.WaitForPendingFinalizers();
                GC.Collect();
                File.Delete(meta.Path);
            }
            catch (IOException ex)
            {

            }
        } 

        private TestDbMeta CreateSQLiteMeta(int i, bool empty)
        {
            var name = $"{DatabaseName}-{i}.sqlite";
            var path = Path.Combine(_settings.FilesPath, name);
            var connectionString = $"Data Source={path}"; // In memory only? just keep a connection open here to stop it getting wiped.
            // Data Source={name};Mode=Memory;Cache=Shared // The database persists as long as at least one connection to it remains open. 


            return new TestDbMeta(name, empty, connectionString, Constants.DatabaseProviders.SQLite, path);
        }
    }
}
