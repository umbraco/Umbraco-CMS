// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    public abstract class BaseTestDatabase
    {
        public static bool IsSqlite() => BaseTestDatabase.Instance is SqliteTestDatabase;
        public static bool IsSqlServer() => BaseTestDatabase.Instance is SqlServerBaseTestDatabase;

        protected ILoggerFactory _loggerFactory;
        protected IUmbracoDatabaseFactory _databaseFactory;
        protected IList<TestDbMeta> _testDatabases;

        protected BlockingCollection<TestDbMeta> _prepareQueue;
        protected BlockingCollection<TestDbMeta> _readySchemaQueue;
        protected BlockingCollection<TestDbMeta> _readyEmptyQueue;
        public static BaseTestDatabase Instance { get; private set; }

        public BaseTestDatabase() => Instance = this;

        protected abstract void Initialize();

        public virtual TestDbMeta AttachEmpty()
        {
            if (_prepareQueue == null)
            {
                Initialize();
            }

            return _readyEmptyQueue.Take();
        }

        public virtual TestDbMeta AttachSchema()
        {
            if (_prepareQueue == null)
            {
                Initialize();
            }

            return _readySchemaQueue.Take();
        }

        public virtual void Detach(TestDbMeta meta)
        {
            _prepareQueue.TryAdd(meta);
        }

        protected virtual void PrepareDatabase() =>
            Retry(10, () =>
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

                    ResetTestDatabase(meta);

                    if (!meta.IsEmpty)
                    {
                        using (var conn = GetConnection(meta))
                        {
                            conn.Open();
                            using (var cmd = conn.CreateCommand())
                            {
                                RebuildSchema(cmd, meta);
                            }
                        }

                        _readySchemaQueue.TryAdd(meta);
                    }
                    else
                    {
                        _readyEmptyQueue.TryAdd(meta);
                    }
                }
            });

        protected static void AddParameter(IDbCommand cmd, UmbracoDatabase.ParameterInfo parameterInfo)
        {
            IDbDataParameter p = cmd.CreateParameter();
            p.ParameterName = parameterInfo.Name;
            p.Value = parameterInfo.Value;
            p.DbType = parameterInfo.DbType;
            p.Size = parameterInfo.Size;
            cmd.Parameters.Add(p);
        }

        protected abstract DbConnection GetConnection(TestDbMeta meta);

        protected abstract void RebuildSchema(IDbCommand command, TestDbMeta meta);

        protected abstract void ResetTestDatabase(TestDbMeta meta);

        protected static void Retry(int maxIterations, Action action)
        {
            for (int i = 0; i < maxIterations; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (DbException ex)
                {
                    // Console.Error.WriteLine($"SqlException occured, but we try again {i+1}/{maxIterations}.\n{e}");
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
                    // Ignore
                }
            }
        }

        public abstract void TearDown();
    }
}
