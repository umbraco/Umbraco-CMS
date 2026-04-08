using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.Integration.Testing;

public abstract class BaseTestDatabase
{
    protected IUmbracoDatabaseFactory _databaseFactory;

    protected ILoggerFactory _loggerFactory;

    protected BlockingCollection<TestDatabaseInformation> _prepareQueue;
    protected BlockingCollection<TestDatabaseInformation> _readyEmptyQueue;
    protected BlockingCollection<TestDatabaseInformation> _readySchemaQueue;
    protected IList<TestDatabaseInformation> _testDatabases;

    public BaseTestDatabase() => Instance = this;

    public static BaseTestDatabase Instance { get; private set; }

    public static bool IsSqlite() => Instance is SqliteTestDatabase;

    public static bool IsSqlServer() => Instance is SqlServerBaseTestDatabase;

    protected abstract void Initialize();

    public virtual TestDatabaseInformation AttachEmpty()
    {
        if (_prepareQueue == null)
        {
            Initialize();
        }

        return _readyEmptyQueue.Take();
    }

    public virtual TestDatabaseInformation AttachSchema()
    {
        if (_prepareQueue == null)
        {
            Initialize();
        }

        return _readySchemaQueue.Take();
    }

    public virtual void Detach(TestDatabaseInformation meta) => _prepareQueue.TryAdd(meta);

    protected virtual void PrepareDatabase() =>
        Retry(10, () =>
        {
            while (_prepareQueue.IsCompleted == false)
            {
                TestDatabaseInformation meta;
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
        var p = cmd.CreateParameter();
        p.ParameterName = parameterInfo.Name;
        p.Value = parameterInfo.Value;
        p.DbType = parameterInfo.DbType;
        p.Size = parameterInfo.Size;
        cmd.Parameters.Add(p);
    }

    protected abstract DbConnection GetConnection(TestDatabaseInformation meta);

    protected abstract void RebuildSchema(IDbCommand command, TestDatabaseInformation meta);

    protected abstract void ResetTestDatabase(TestDatabaseInformation meta);

    protected static void Retry(int maxIterations, Action action)
    {
        for (var i = 0; i < maxIterations; i++)
        {
            try
            {
                action();
                return;
            }
            catch (DbException)
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
