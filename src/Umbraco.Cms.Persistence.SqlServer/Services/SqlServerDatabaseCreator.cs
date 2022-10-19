using Microsoft.Data.SqlClient;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.SqlServer.Services;

public class SqlServerDatabaseCreator : IDatabaseCreator
{
    public string ProviderName => Constants.ProviderName;

    public void Create(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        // Get connection string without database specific information
        var masterBuilder = new SqlConnectionStringBuilder(builder.ConnectionString)
        {
            AttachDBFilename = string.Empty,
            InitialCatalog = string.Empty
        };
        var masterConnectionString = masterBuilder.ConnectionString;

        string fileName = builder.AttachDBFilename,
            database = builder.InitialCatalog;

        // Create database
        if (!string.IsNullOrEmpty(fileName) && !File.Exists(fileName))
        {
            if (string.IsNullOrWhiteSpace(database))
            {
                // Use a temporary database name
                database = "Umbraco-" + Guid.NewGuid();
            }

            using var connection = new SqlConnection(masterConnectionString);
            connection.Open();

            using var command = new SqlCommand(
                $"CREATE DATABASE [{database}] ON (NAME='{database}', FILENAME='{fileName}');" +
                $"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" +
                $"EXEC sp_detach_db @dbname='{database}';",
                connection);
            command.ExecuteNonQuery();

            connection.Close();
        }
        else if (!string.IsNullOrEmpty(database))
        {
            using var connection = new SqlConnection(masterConnectionString);
            connection.Open();

            using var command = new SqlCommand(
                $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{database}') " +
                $"CREATE DATABASE [{database}];",
                connection);
            command.ExecuteNonQuery();

            connection.Close();
        }
    }
}
