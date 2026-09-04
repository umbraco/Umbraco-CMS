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

            // Specify the log file explicitly rather than letting SQL Server derive it from the
            // data file path. That derivation drops the directory separator in front of a directory
            // whose name starts with a dot, so a data file in "C:\repo\.tools\Umbraco.mdf" yields
            // the log file "C:\repo.tools\Umbraco_log.ldf", which fails with operating system error
            // 3 because the directory does not exist. The name and location used here are the ones
            // SQL Server derives itself for any path without such a directory.
            var logName = GetLogName(fileName);
            var logFileName = GetLogFileName(fileName);

            using var connection = new SqlConnection(masterConnectionString);
            connection.Open();

            using var command = new SqlCommand(
                $"CREATE DATABASE [{database}] ON (NAME='{database}', FILENAME='{fileName}') " +
                $"LOG ON (NAME='{logName}', FILENAME='{logFileName}');" +
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

    /// <summary>
    ///     Gets the logical name SQL Server assigns to the log file of a database created from the
    ///     specified data file.
    /// </summary>
    /// <param name="dataFileName">The data (MDF) file name.</param>
    /// <returns>The logical log file name, for example "Umbraco_log" for "Umbraco.mdf".</returns>
    /// <remarks>
    ///     SQL Server derives this from the data file name rather than from the database name, and
    ///     strips only the last extension, so "My.Site.mdf" becomes "My.Site_log".
    /// </remarks>
    internal static string GetLogName(string dataFileName)
        => Path.GetFileNameWithoutExtension(dataFileName) + "_log";

    /// <summary>
    ///     Gets the log (LDF) file path SQL Server assigns to a database created from the specified
    ///     data file, which sits next to the data file.
    /// </summary>
    /// <param name="dataFileName">The data (MDF) file name.</param>
    /// <returns>The log file path, for example "C:\Data\Umbraco_log.ldf" for "C:\Data\Umbraco.mdf".</returns>
    internal static string GetLogFileName(string dataFileName)
    {
        var logFileName = GetLogName(dataFileName) + ".ldf";
        var directory = Path.GetDirectoryName(dataFileName);

        return string.IsNullOrEmpty(directory) ? logFileName : Path.Combine(directory, logFileName);
    }
}
