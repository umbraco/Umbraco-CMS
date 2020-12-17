using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Integration.Testing
{
    /// <summary>
    /// Manages a pool of LocalDb databases for integration testing
    /// </summary>
    public class LocalDbTestDatabase : BaseTestDatabase, ITestDatabase
    {
        public const string InstanceName = "UmbracoTests";
        public const string DatabaseName = "UmbracoTests";

        private readonly LocalDb _localDb;
        private static LocalDb.Instance _localDbInstance;
        private static string _filesPath;

        private const int _threadCount = 2;

        public static LocalDbTestDatabase Instance { get; private set; }

        //It's internal because `Umbraco.Core.Persistence.LocalDb` is internal
        internal LocalDbTestDatabase(ILoggerFactory loggerFactory, LocalDb localDb, string filesPath, IUmbracoDatabaseFactory dbFactory)
        {
            _loggerFactory = loggerFactory;
            _databaseFactory = dbFactory;

            _localDb = localDb;
            _filesPath = filesPath;

            Instance = this; // For GlobalSetupTeardown.cs

            _testDatabases = new[]
            {
                // With Schema
                TestDbMeta.CreateWithoutConnectionString($"{DatabaseName}-1", false),
                TestDbMeta.CreateWithoutConnectionString($"{DatabaseName}-2", false),

                // Empty (for migration testing etc)
                TestDbMeta.CreateWithoutConnectionString($"{DatabaseName}-3", true),
                TestDbMeta.CreateWithoutConnectionString($"{DatabaseName}-4", true),
            };

            _localDbInstance = _localDb.GetInstance(InstanceName);
            if (_localDbInstance != null)
            {
                return;
            }

            if (_localDb.CreateInstance(InstanceName) == false)
            {
                throw new Exception("Failed to create a LocalDb instance.");
            }

            _localDbInstance = _localDb.GetInstance(InstanceName);
        }

        protected override void Initialize()
        {
            var tempName = Guid.NewGuid().ToString("N");
            _localDbInstance.CreateDatabase(tempName, _filesPath);
            _localDbInstance.DetachDatabase(tempName);

            _prepareQueue = new BlockingCollection<TestDbMeta>();
            _readySchemaQueue = new BlockingCollection<TestDbMeta>();
            _readyEmptyQueue = new BlockingCollection<TestDbMeta>();

            for (var i = 0; i < _testDatabases.Count; i++)
            {
                var meta = _testDatabases[i];
                var isLast = i == _testDatabases.Count - 1;

                _localDb.CopyDatabaseFiles(tempName, _filesPath, targetDatabaseName: meta.Name, overwrite: true, delete: isLast);
                meta.ConnectionString = _localDbInstance.GetAttachedConnectionString(meta.Name, _filesPath);
                _prepareQueue.Add(meta);
            }

            for (var i = 0; i < _threadCount; i++)
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
            { }

            _readyEmptyQueue.CompleteAdding();
            while (_readyEmptyQueue.TryTake(out _))
            { }

            _readySchemaQueue.CompleteAdding();
            while (_readySchemaQueue.TryTake(out _))
            { }

            if (_filesPath == null)
            {
                return;
            }

            var filename = Path.Combine(_filesPath, DatabaseName).ToUpper();

            foreach (var database in _localDbInstance.GetDatabases())
            {
                if (database.StartsWith(filename))
                {
                    _localDbInstance.DropDatabase(database);
                }
            }

            foreach (var file in Directory.EnumerateFiles(_filesPath))
            {
                if (file.EndsWith(".mdf") == false && file.EndsWith(".ldf") == false) continue;
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
