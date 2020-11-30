using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Integration.Testing
{
    /// <summary>
    /// Manages a pool of LocalDb databases for integration testing
    /// </summary>
    public class LocalDbTestDatabase : ITestDatabase
    {
        public const string InstanceName = "UmbracoTests";
        public const string DatabaseName = "UmbracoTests";

        private readonly ILoggerFactory _loggerFactory;
        private readonly LocalDb _localDb;
        private readonly IUmbracoVersion _umbracoVersion;
        private static LocalDb.Instance _instance;
        private static string _filesPath;
        private readonly IUmbracoDatabaseFactory _dbFactory;
        private UmbracoDatabase.CommandInfo[] _dbCommands;
        private string _currentCstr;
        private static DatabasePool _emptyPool;
        private static DatabasePool _schemaPool;
        private DatabasePool _currentPool;

        //It's internal because `Umbraco.Core.Persistence.LocalDb` is internal
        internal LocalDbTestDatabase(ILoggerFactory loggerFactory, LocalDb localDb, string filesPath, IUmbracoDatabaseFactory dbFactory)
        {
            _umbracoVersion = new UmbracoVersion();
            _loggerFactory = loggerFactory;
            _localDb = localDb;
            _filesPath = filesPath;
            _dbFactory = dbFactory;

            _instance = _localDb.GetInstance(InstanceName);
            if (_instance != null) return;

            if (_localDb.CreateInstance(InstanceName) == false)
                throw new Exception("Failed to create a LocalDb instance.");
            _instance = _localDb.GetInstance(InstanceName);
        }

        public string ConnectionString => _currentCstr ?? _instance.GetAttachedConnectionString("XXXXXX", _filesPath);

        private void Create()
        {
            var tempName = Guid.NewGuid().ToString("N");
            _instance.CreateDatabase(tempName, _filesPath);
            _instance.DetachDatabase(tempName);

            // there's probably a sweet spot to be found for size / parallel...

            var s = ConfigurationManager.AppSettings["Umbraco.Tests.LocalDbTestDatabase.EmptyPoolSize"];
            var emptySize = s == null ? 1 : int.Parse(s);
            s = ConfigurationManager.AppSettings["Umbraco.Tests.LocalDbTestDatabase.EmptyPoolThreadCount"];
            var emptyParallel = s == null ? 1 : int.Parse(s);
            s = ConfigurationManager.AppSettings["Umbraco.Tests.LocalDbTestDatabase.SchemaPoolSize"];
            var schemaSize = s == null ? 1 : int.Parse(s);
            s = ConfigurationManager.AppSettings["Umbraco.Tests.LocalDbTestDatabase.SchemaPoolThreadCount"];
            var schemaParallel = s == null ? 1 : int.Parse(s);

            _emptyPool = new DatabasePool(_localDb, _instance, DatabaseName + "-Empty", tempName, _filesPath, emptySize, emptyParallel);
            _schemaPool = new DatabasePool(_localDb, _instance, DatabaseName + "-Schema", tempName, _filesPath, schemaSize, schemaParallel, delete: true, prepare: RebuildSchema);
        }

        public int AttachEmpty()
        {
            if (_emptyPool == null)
                Create();

            _currentCstr = _emptyPool.AttachDatabase(out var id);
            _currentPool = _emptyPool;
            return id;
        }

        public int AttachSchema()
        {
            if (_schemaPool == null)
                Create();

            _currentCstr = _schemaPool.AttachDatabase(out var id);
            _currentPool = _schemaPool;
            return id;
        }

        public void Detach(int id)
        {
            _currentPool.DetachDatabase(id);
        }

        private void RebuildSchema(DbConnection conn, IDbCommand cmd)
        {

            if (_dbCommands != null)
            {
                foreach (var dbCommand in _dbCommands)
                {

                    if (dbCommand.Text.StartsWith("SELECT ")) continue;

                    cmd.CommandText = dbCommand.Text;
                    cmd.Parameters.Clear();
                    foreach (var parameterInfo in dbCommand.Parameters)
                        AddParameter(cmd, parameterInfo);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                _dbFactory.Configure(conn.ConnectionString, Constants.DatabaseProviders.SqlServer);

                using var database = (UmbracoDatabase)_dbFactory.CreateDatabase();
                // track each db command ran as part of creating the database so we can replay these
                database.LogCommands = true;

                using var trans = database.GetTransaction();

                var creator = new DatabaseSchemaCreator(database, _loggerFactory.CreateLogger<DatabaseSchemaCreator>(), _loggerFactory, _umbracoVersion);
                creator.InitializeDatabaseSchema();

                trans.Complete(); // commit it

                _dbCommands = database.Commands.ToArray();
            }

        }

        internal static void AddParameter(IDbCommand cmd, UmbracoDatabase.ParameterInfo parameterInfo)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = parameterInfo.Name;
            p.Value = parameterInfo.Value;
            p.DbType = parameterInfo.DbType;
            p.Size = parameterInfo.Size;
            cmd.Parameters.Add(p);
        }


        internal static void ResetLocalDb(IDbCommand cmd)
        {
            // https://stackoverflow.com/questions/536350

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
                declare @n char(1);
                set @n = char(10);
                declare @stmt nvarchar(max);
                -- check constraints
                select @stmt = isnull( @stmt + @n, '' ) +
                    'alter table [' + schema_name(schema_id) + '].[' + object_name( parent_object_id ) + '] drop constraint [' + name + ']'
                from sys.check_constraints;
                -- foreign keys
                select @stmt = isnull( @stmt + @n, '' ) +
                    'alter table [' + schema_name(schema_id) + '].[' + object_name( parent_object_id ) + '] drop constraint [' + name + ']'
                from sys.foreign_keys;
                -- tables
                select @stmt = isnull( @stmt + @n, '' ) +
                    'drop table [' + schema_name(schema_id) + '].[' + name + ']'
                from sys.tables;
                exec sp_executesql @stmt;
            ";

            // rudimentary retry policy since a db can still be in use when we try to drop
            Retry(10, () =>
            {
                cmd.ExecuteNonQuery();
            });
        }

        public static void KillLocalDb()
        {
            _emptyPool?.Stop();
            _schemaPool?.Stop();

            if (_filesPath == null)
                return;

            var filename = Path.Combine(_filesPath, DatabaseName).ToUpper();

            foreach (var database in _instance.GetDatabases())
            {
                if (database.StartsWith(filename))
                    _instance.DropDatabase(database);
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

        internal static void Retry(int maxIterations, Action action)
        {
            for (var i = 0; i < maxIterations; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (SqlException)
                {

                    //Console.Error.WriteLine($"SqlException occured, but we try again {i+1}/{maxIterations}.\n{e}");
                    // This can occur when there's a transaction deadlock which means (i think) that the database is still in use and hasn't been closed properly yet
                    // so we need to just wait a little bit
                    Thread.Sleep(100 * i);
                    if (i == maxIterations - 1)
                    {
                        Debugger.Launch();
                        throw;
                    }
                }
                catch (InvalidOperationException)
                {

                }
            }
        }

        private class DatabasePool
        {
            private readonly LocalDb _localDb;
            private readonly LocalDb.Instance _instance;
            private readonly string _filesPath;
            private readonly string _name;
            private readonly int _size;
            private readonly string[] _cstrs;
            private readonly BlockingCollection<int> _prepareQueue, _readyQueue;
            private readonly Action<DbConnection, IDbCommand> _prepare;
            private int _current;

            public DatabasePool(LocalDb localDb, LocalDb.Instance instance, string name, string tempName, string filesPath, int size, int parallel = 1, Action<DbConnection, IDbCommand> prepare = null, bool delete = false)
            {
                _localDb = localDb;
                _instance = instance;
                _filesPath = filesPath;
                _name = name;
                _size = size;
                _prepare = prepare;
                _prepareQueue = new BlockingCollection<int>();
                _readyQueue = new BlockingCollection<int>();
                _cstrs = new string[_size];

                for (var i = 0; i < size; i++)
                    localDb.CopyDatabaseFiles(tempName, filesPath, targetDatabaseName: name + "-" + i, overwrite: true, delete: delete && i == size - 1);

                if (prepare == null)
                {
                    for (var i = 0; i < size; i++)
                        _readyQueue.Add(i);
                }
                else
                {
                    for (var i = 0; i < size; i++)
                        _prepareQueue.Add(i);
                }

                for (var i = 0; i < parallel; i++)
                {
                    var thread = new Thread(PrepareThread);
                    thread.Start();
                }
            }

            public string AttachDatabase(out int id)
            {
                _current = _readyQueue.Take();
                id = _current;

                return ConnectionString(_current);
            }

            public void DetachDatabase(int id)
            {
                if (id != _current)
                    throw new InvalidOperationException("Cannot detatch the non-current db");

                _prepareQueue.Add(_current);
            }

            private string ConnectionString(int i)
            {
                return _cstrs[i] ?? (_cstrs[i] = _instance.GetAttachedConnectionString(_name + "-" + i, _filesPath));
            }

            private void PrepareThread()
            {
                Retry(10, () =>
                {
                    while (_prepareQueue.IsCompleted == false)
                    {
                        int i;
                        try
                        {
                            i = _prepareQueue.Take();
                        }
                        catch (InvalidOperationException)
                        {
                            continue;
                        }

                        using (var conn = new SqlConnection(ConnectionString(i)))
                        using (var cmd = conn.CreateCommand())
                        {
                            conn.Open();
                            ResetLocalDb(cmd);

                            _prepare?.Invoke(conn, cmd);

                        }

                        if (!_readyQueue.IsAddingCompleted)
                        {
                            _readyQueue.Add(i);
                        }
                    }
                });
            }

            public void Stop()
            {
                int i;
                _prepareQueue.CompleteAdding();
                while (_prepareQueue.TryTake(out i)) { }
                _readyQueue.CompleteAdding();
                while (_readyQueue.TryTake(out i)) { }
            }
        }

    }
}
