// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    public abstract class BaseTestDatabase
    {
        protected ILoggerFactory _loggerFactory;
        protected IUmbracoDatabaseFactory _databaseFactory;
        protected IList<TestDbMeta> _testDatabases;


        protected UmbracoDatabase.CommandInfo[] _cachedDatabaseInitCommands = new UmbracoDatabase.CommandInfo[0];

        protected BlockingCollection<TestDbMeta> _prepareQueue;
        protected BlockingCollection<TestDbMeta> _readySchemaQueue;
        protected BlockingCollection<TestDbMeta> _readyEmptyQueue;

        protected abstract void Initialize();

        public TestDbMeta AttachEmpty()
        {
            if (_prepareQueue == null)
            {
                Initialize();
            }

            return _readyEmptyQueue.Take();
        }

        public TestDbMeta AttachSchema()
        {
            if (_prepareQueue == null)
            {
                Initialize();
            }

            return _readySchemaQueue.Take();
        }

        public void Detach(TestDbMeta meta)
        {
            _prepareQueue.TryAdd(meta);
        }

        protected void PrepareDatabase() =>
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

                        using (var conn = new SqlConnection(meta.ConnectionString))
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            conn.Open();
                            ResetTestDatabase(cmd);

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

        private void RebuildSchema(IDbCommand command, TestDbMeta meta)
        {
            lock (_cachedDatabaseInitCommands)
            {
                if (!_cachedDatabaseInitCommands.Any())
                {
                    RebuildSchemaFirstTime(meta);
                    return;
                }
            }

            foreach (UmbracoDatabase.CommandInfo dbCommand in _cachedDatabaseInitCommands)
            {
                if (dbCommand.Text.StartsWith("SELECT "))
                {
                    continue;
                }

                command.CommandText = dbCommand.Text;
                command.Parameters.Clear();

                foreach (UmbracoDatabase.ParameterInfo parameterInfo in dbCommand.Parameters)
                {
                    AddParameter(command, parameterInfo);
                }

                command.ExecuteNonQuery();
            }
        }

        private void RebuildSchemaFirstTime(TestDbMeta meta)
        {
            _databaseFactory.Configure(meta.ConnectionString, Constants.DatabaseProviders.SqlServer);

            using (var database = (UmbracoDatabase)_databaseFactory.CreateDatabase())
            {
                database.LogCommands = true;

                using (NPoco.ITransaction transaction = database.GetTransaction())
                {
                    var schemaCreator = new DatabaseSchemaCreator(database, _loggerFactory.CreateLogger<DatabaseSchemaCreator>(), _loggerFactory, new UmbracoVersion(), Mock.Of<IEventAggregator>(), Mock.Of<IOptionsMonitor<InstallDefaultDataSettings>>(x => x.CurrentValue == new InstallDefaultDataSettings()));
                    schemaCreator.InitializeDatabaseSchema();

                    transaction.Complete();

                    _cachedDatabaseInitCommands = database.Commands.ToArray();
                }
            }
        }

        protected static void SetCommand(SqlCommand command, string sql, params object[] args)
        {
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            command.Parameters.Clear();

            for (int i = 0; i < args.Length; i++)
            {
                command.Parameters.AddWithValue("@" + i, args[i]);
            }
        }

        protected static void AddParameter(IDbCommand cmd, UmbracoDatabase.ParameterInfo parameterInfo)
        {
            IDbDataParameter p = cmd.CreateParameter();
            p.ParameterName = parameterInfo.Name;
            p.Value = parameterInfo.Value;
            p.DbType = parameterInfo.DbType;
            p.Size = parameterInfo.Size;
            cmd.Parameters.Add(p);
        }

        protected static void ResetTestDatabase(IDbCommand cmd)
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
            Retry(10, () => cmd.ExecuteNonQuery());
        }

        protected static void Retry(int maxIterations, Action action)
        {
            for (int i = 0; i < maxIterations; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (SqlException)
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
    }
}
