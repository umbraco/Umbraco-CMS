using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.Integration.Testing;

public abstract class SqlServerBaseTestDatabase : BaseTestDatabase
{
    protected UmbracoDatabase.CommandInfo[] _cachedDatabaseInitCommands = new UmbracoDatabase.CommandInfo[0];

    protected override void ResetTestDatabase(TestDbMeta meta)
    {
        using var connection = GetConnection(meta);
        connection.Open();

        using (var cmd = connection.CreateCommand())
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
    }

    protected static void SetCommand(SqlCommand command, string sql, params object[] args)
    {
        command.CommandType = CommandType.Text;
        command.CommandText = sql;
        command.Parameters.Clear();

        for (var i = 0; i < args.Length; i++)
        {
            command.Parameters.AddWithValue("@" + i, args[i]);
        }
    }


    protected override DbConnection GetConnection(TestDbMeta meta) => new SqlConnection(meta.ConnectionString);

    protected override void RebuildSchema(IDbCommand command, TestDbMeta meta)
    {
        lock (_cachedDatabaseInitCommands)
        {
            if (!_cachedDatabaseInitCommands.Any())
            {
                RebuildSchemaFirstTime(meta);
                return;
            }
        }

        foreach (var dbCommand in _cachedDatabaseInitCommands)
        {
            command.CommandText = dbCommand.Text;
            command.Parameters.Clear();

            foreach (var parameterInfo in dbCommand.Parameters)
            {
                AddParameter(command, parameterInfo);
            }

            command.ExecuteNonQuery();
        }
    }

    private void RebuildSchemaFirstTime(TestDbMeta meta)
    {
        _databaseFactory.Configure(meta.ToStronglyTypedConnectionString());

        using (var database = (UmbracoDatabase)_databaseFactory.CreateDatabase())
        {
            database.LogCommands = true;

            using (var transaction = database.GetTransaction())
            {
                var options =
                    new TestOptionsMonitor<InstallDefaultDataSettings>(
                        new InstallDefaultDataSettings { InstallData = InstallDefaultDataOption.All });

                var schemaCreator = new DatabaseSchemaCreator(
                    database,
                    _loggerFactory.CreateLogger<DatabaseSchemaCreator>(),
                    _loggerFactory,
                    new UmbracoVersion(),
                    Mock.Of<IEventAggregator>(),
                    options);

                schemaCreator.InitializeDatabaseSchema();

                transaction.Complete();

                _cachedDatabaseInitCommands = database.Commands
                    .Where(x => !x.Text.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }
        }
    }
}
