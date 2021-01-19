// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
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
        private readonly string _masterConnectionString;
        public const string DatabaseName = "UmbracoTests";

        public static SqlDeveloperTestDatabase Instance { get; private set; }

        public SqlDeveloperTestDatabase(ILoggerFactory loggerFactory, IUmbracoDatabaseFactory databaseFactory, string masterConnectionString)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));

            _masterConnectionString = masterConnectionString;

            _testDatabases = new[]
            {
                // With Schema
                TestDbMeta.CreateWithMasterConnectionString($"{DatabaseName}-1", false, masterConnectionString),
                TestDbMeta.CreateWithMasterConnectionString($"{DatabaseName}-2", false, masterConnectionString),

                // Empty (for migration testing etc)
                TestDbMeta.CreateWithMasterConnectionString($"{DatabaseName}-3", true, masterConnectionString),
                TestDbMeta.CreateWithMasterConnectionString($"{DatabaseName}-4", true, masterConnectionString),
            };

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

            for (int i = 0; i < ThreadCount; i++)
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
