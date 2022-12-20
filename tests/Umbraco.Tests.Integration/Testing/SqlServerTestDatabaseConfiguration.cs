// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using Microsoft.Data.SqlClient;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Persistence.SqlServer;

// ReSharper disable ConvertToUsingDeclaration
namespace Umbraco.Cms.Tests.Integration.Testing;

/// <remarks>
///     It's not meant to be pretty, rushed port of LocalDb.cs + LocalDbTestDatabase.cs
/// </remarks>
public class SqlServerTestDatabaseConfiguration : ITestDatabaseConfiguration
{
    private Guid _key;
    private readonly string _connectionString;

    public SqlServerTestDatabaseConfiguration(string connectionString) => _connectionString = connectionString;

    public ConnectionStrings InitializeConfiguration()
    {
        _key = Guid.NewGuid();
        CreateDatabase();

        return new ConnectionStrings
        {
            ConnectionString = _connectionString + _key + ";TrustServerCertificate=true;",
            ProviderName = "Microsoft.Data.SqlClient"
        };
    }

    private void CreateDatabase()
    {
        Teardown();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                SetCommand(command, $@"CREATE DATABASE {LocalDb.QuotedName(_key.ToString())}");
                command.ExecuteNonQuery();
            }
        }
    }

    public void Teardown()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                SetCommand(command, "select count(1) from sys.databases where name = @0", _key.ToString());
                var records = (int)command.ExecuteScalar();
                if (records == 0)
                {
                    return;
                }

                var sql = $@"
                        ALTER DATABASE {LocalDb.QuotedName(_key.ToString())}
                        SET SINGLE_USER 
                        WITH ROLLBACK IMMEDIATE";
                SetCommand(command, sql);
                command.ExecuteNonQuery();

                SetCommand(command, $@"DROP DATABASE {LocalDb.QuotedName(_key.ToString())}");
                command.ExecuteNonQuery();
            }
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
}
