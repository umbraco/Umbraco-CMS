// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    /// <summary>
    /// Manages a pool of LocalDb databases for integration testing
    /// </summary>
    public class LocalDbTestDatabase : BaseTestDatabase, ITestDatabase
    {
        public const string InstanceName = "UmbracoTests";
        public const string DatabaseName = "UmbracoTests";

        private readonly TestDatabaseSettings _settings;
        private readonly LocalDb _localDb;
        private static LocalDb.Instance s_localDbInstance;
        private static string s_filesPath;

        public static LocalDbTestDatabase Instance { get; private set; }

        // It's internal because `Umbraco.Core.Persistence.LocalDb` is internal
        internal LocalDbTestDatabase(TestDatabaseSettings settings, ILoggerFactory loggerFactory, LocalDb localDb, string filesPath, IUmbracoDatabaseFactory dbFactory)
        {
            _loggerFactory = loggerFactory;
            _databaseFactory = dbFactory;

            _settings = settings;
            _localDb = localDb;
            s_filesPath = filesPath;

            Instance = this; // For GlobalSetupTeardown.cs

            var counter = 0;

            var schema = Enumerable.Range(0, _settings.SchemaDatabaseCount)
                .Select(x => TestDbMeta.CreateWithoutConnectionString($"{DatabaseName}-{++counter}", false));

            var empty = Enumerable.Range(0, _settings.EmptyDatabasesCount)
                .Select(x => TestDbMeta.CreateWithoutConnectionString($"{DatabaseName}-{++counter}", true));

            _testDatabases = schema.Concat(empty).ToList();

            s_localDbInstance = _localDb.GetInstance(InstanceName);
            if (s_localDbInstance != null)
            {
                return;
            }

            if (_localDb.CreateInstance(InstanceName) == false)
            {
                throw new Exception("Failed to create a LocalDb instance.");
            }

            s_localDbInstance = _localDb.GetInstance(InstanceName);
        }

        protected override void Initialize()
        {
            string tempName = Guid.NewGuid().ToString("N");
            s_localDbInstance.CreateDatabase(tempName, s_filesPath);
            s_localDbInstance.DetachDatabase(tempName);

            _prepareQueue = new BlockingCollection<TestDbMeta>();
            _readySchemaQueue = new BlockingCollection<TestDbMeta>();
            _readyEmptyQueue = new BlockingCollection<TestDbMeta>();

            for (int i = 0; i < _testDatabases.Count; i++)
            {
                TestDbMeta meta = _testDatabases[i];
                bool isLast = i == _testDatabases.Count - 1;

                _localDb.CopyDatabaseFiles(tempName, s_filesPath, targetDatabaseName: meta.Name, overwrite: true, delete: isLast);
                meta.ConnectionString = s_localDbInstance.GetAttachedConnectionString(meta.Name, s_filesPath);
                _prepareQueue.Add(meta);
            }

            for (int i = 0; i < _settings.PrepareThreadCount; i++)
            {
                var thread = new Thread(PrepareDatabase);
                thread.Start();
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

            if (s_filesPath == null)
            {
                return;
            }

            string filename = Path.Combine(s_filesPath, DatabaseName).ToUpper();

            foreach (string database in s_localDbInstance.GetDatabases())
            {
                if (database.StartsWith(filename))
                {
                    s_localDbInstance.DropDatabase(database);
                }
            }

            foreach (string file in Directory.EnumerateFiles(s_filesPath))
            {
                if (file.EndsWith(".mdf") == false && file.EndsWith(".ldf") == false)
                {
                    continue;
                }

                try
                {
                    File.Delete(file);
                }
                catch (IOException)
                {
                    // ignore, must still be in use but nothing we can do
                }
            }
        }
    }
}
