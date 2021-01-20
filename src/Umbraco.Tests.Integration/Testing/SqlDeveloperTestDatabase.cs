// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Persistence;

// ReSharper disable ConvertToUsingDeclaration
namespace Umbraco.Tests.Integration.Testing
{
    /// <remarks>
    /// It's not meant to be pretty, rushed port of LocalDb.cs + LocalDbTestDatabase.cs
    /// </remarks>
    public class SqlDeveloperTestDatabase : BaseTestDatabase, ITestDatabase
    {
        private readonly TestDatabaseSettings _settings;
        private readonly string _masterConnectionString;
        public const string DatabaseName = "UmbracoTests";

        public static SqlDeveloperTestDatabase Instance { get; private set; }

        public SqlDeveloperTestDatabase(TestDatabaseSettings settings, ILoggerFactory loggerFactory, IUmbracoDatabaseFactory databaseFactory, string masterConnectionString)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));

            _settings = settings;
            _masterConnectionString = masterConnectionString;

            var counter = 0;

            var schema = Enumerable.Range(0, _settings.SchemaDatabaseCount)
                .Select(x => TestDbMeta.CreateWithMasterConnectionString($"{DatabaseName}-{++counter}", false, masterConnectionString));

            var empty = Enumerable.Range(0, _settings.EmptyDatabasesCount)
                .Select(x => TestDbMeta.CreateWithMasterConnectionString($"{DatabaseName}-{++counter}", true, masterConnectionString));

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
                CreateDatabase(meta);
                _prepareQueue.Add(meta);
            }

            for (int i = 0; i < _settings.PrepareThreadCount; i++)
            {
                var thread = new Thread(PrepareDatabase);
                thread.Start();
            }
        }

        private void CreateDatabase(TestDbMeta meta)
        {
            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    SetCommand(command, $@"CREATE DATABASE {LocalDb.QuotedName(meta.Name)}");
                    command.ExecuteNonQuery();
                }
            }
        }

        private void Drop(TestDbMeta meta)
        {
            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    string sql = $@"
                        ALTER DATABASE{LocalDb.QuotedName(meta.Name)}
                        SET SINGLE_USER
                        WITH ROLLBACK IMMEDIATE";
                    SetCommand(command, sql);
                    command.ExecuteNonQuery();

                    SetCommand(command, $@"DROP DATABASE {LocalDb.QuotedName(meta.Name)}");
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Finish()
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

            foreach (TestDbMeta testDatabase in _testDatabases)
            {
                Drop(testDatabase);
            }
        }
    }
}
