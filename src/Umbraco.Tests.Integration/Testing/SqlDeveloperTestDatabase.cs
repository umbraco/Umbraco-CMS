using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;

// ReSharper disable ConvertToUsingDeclaration

namespace Umbraco.Tests.Integration.Testing
{
    /// <remarks>
    /// It's not meant to be pretty, rushed port of LocalDb.cs + LocalDbTestDatabase.cs
    /// </remarks>
    public class SqlDeveloperTestDatabase : ITestDatabase
    {
        private readonly string _masterConnectionString;
        private readonly string _databaseName;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _log;
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private UmbracoDatabase.CommandInfo[] _cachedDatabaseInitCommands;

        public SqlDeveloperTestDatabase(ILoggerFactory loggerFactory, IUmbracoDatabaseFactory databaseFactory, string masterConnectionString)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));
            _masterConnectionString = masterConnectionString;
            _databaseName = $"Umbraco_Integration_{Guid.NewGuid()}".Replace("-", string.Empty);
            _log = loggerFactory.CreateLogger<SqlDeveloperTestDatabase>();
        }

        public string ConnectionString { get; private set; }

        public int AttachEmpty()
        {
            CreateDatabase();
            return -1;
        }

        public int AttachSchema()
        {
            CreateDatabase();

            _log.LogInformation($"Attaching schema {_databaseName}");

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    RebuildSchema(command);
                }
            }

            return -1;
        }

        public void Detach(int id)
        {
            _log.LogInformation($"Dropping database {_databaseName}");
            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    SetCommand(command, $@"
                        ALTER DATABASE{LocalDb.QuotedName(_databaseName)}
                        SET SINGLE_USER
                        WITH ROLLBACK IMMEDIATE
                    ");
                    command.ExecuteNonQuery();

                    SetCommand(command, $@"DROP DATABASE {LocalDb.QuotedName(_databaseName)}");
                    command.ExecuteNonQuery();
                }
            }
        }

        private void CreateDatabase()
        {
            _log.LogInformation($"Creating database {_databaseName}");
            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    SetCommand(command, $@"CREATE DATABASE {LocalDb.QuotedName(_databaseName)}");
                    var unused = command.ExecuteNonQuery();
                }
            }

            ConnectionString = ConstructConnectionString(_masterConnectionString, _databaseName);
        }

        private static string ConstructConnectionString(string masterConnectionString, string databaseName)
        {
            var prefix = Regex.Replace(masterConnectionString, "Database=.+?;", string.Empty);
            var connectionString = $"{prefix};Database={databaseName};";
            return connectionString.Replace(";;", ";");
        }

        private static void SetCommand(SqlCommand command, string sql, params object[] args)
        {
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            command.Parameters.Clear();

            for (var i = 0; i < args.Length; i++)
            {
                command.Parameters.AddWithValue("@" + i, args[i]);
            }
        }

        private void RebuildSchema(IDbCommand command)
        {
            if (_cachedDatabaseInitCommands != null)
            {
                foreach (var dbCommand in _cachedDatabaseInitCommands)
                {

                    if (dbCommand.Text.StartsWith("SELECT "))
                    {
                        continue;
                    }

                    command.CommandText = dbCommand.Text;
                    command.Parameters.Clear();

                    foreach (var parameterInfo in dbCommand.Parameters)
                    {
                        LocalDbTestDatabase.AddParameter(command, parameterInfo);
                    }

                    command.ExecuteNonQuery();
                }
            }
            else
            {
                _databaseFactory.Configure(ConnectionString, Constants.DatabaseProviders.SqlServer);

                using (var database = (UmbracoDatabase)_databaseFactory.CreateDatabase())
                {
                    database.LogCommands = true;

                    using (var transaction = database.GetTransaction())
                    {
                        var schemaCreator = new DatabaseSchemaCreator(database, _loggerFactory.CreateLogger<DatabaseSchemaCreator>(), _loggerFactory, new UmbracoVersion());
                        schemaCreator.InitializeDatabaseSchema();

                        transaction.Complete();

                        _cachedDatabaseInitCommands = database.Commands.ToArray();
                    }
                }
            }
        }
    }
}
