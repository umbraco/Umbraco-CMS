using Microsoft.Data.SqlClient;
using Umbraco.Cms.Infrastructure.Persistence;
using static Umbraco.Cms.Persistence.SqlServer.TSqlQuoting;

namespace Umbraco.Cms.Persistence.SqlServer.Services;

/// <summary>
///     Implements <see cref="IDatabaseCreator" /> for SQL Server.
/// </summary>
public class SqlServerDatabaseCreator : IDatabaseCreator
{
    /// <inheritdoc />
    public string ProviderName => Constants.ProviderName;

    /// <summary>
    ///     Creates a SQL Server database, either from a data file or by name.
    /// </summary>
    /// <param name="connectionString">The connection string to use for creating the database.</param>
    /// <remarks>
    ///     <para>
    ///         When the connection string carries an <c>AttachDbFilename</c> that does not yet exist, the database is
    ///         created at that path and then detached, leaving the data and log files in place for the runtime to
    ///         attach on demand. Otherwise a database is created by name, if one does not already exist.
    ///     </para>
    ///     <para>
    ///         Neither statement can be parameterised, so every value is quoted with
    ///         <see cref="TSqlQuoting.QuotedName" />.
    ///     </para>
    /// </remarks>
    public void Create(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        // Get connection string without database specific information.
        var masterBuilder = new SqlConnectionStringBuilder(builder.ConnectionString)
        {
            AttachDBFilename = string.Empty,
            InitialCatalog = string.Empty,
        };
        var masterConnectionString = masterBuilder.ConnectionString;

        string fileName = builder.AttachDBFilename,
            database = builder.InitialCatalog;

        // Create database.
        if (!string.IsNullOrEmpty(fileName) && !File.Exists(fileName))
        {
            if (string.IsNullOrWhiteSpace(database))
            {
                // Use a temporary database name
                database = "Umbraco-" + Guid.NewGuid();
            }

            // Specify the log file explicitly rather than letting SQL Server derive it from the data
            // file path. That derivation mishandles dot segments: one starting with a single dot is
            // merged into the segment before it, so "C:\repo\.tools" becomes "C:\repo.tools", and
            // ".." consumes one segment too many. Where the resulting directory does not exist,
            // creation fails with operating system error 3; where it happens to exist, creation
            // succeeds and the log is written outside the data file's directory, leaving a database
            // whose files are no longer together. Affected engines are 2017 and later, so a
            // correctly derived path on an earlier one is not evidence that this is unnecessary.
            var logName = GetLogName(fileName);
            var logFileName = GetLogFileName(fileName);

            using var connection = new SqlConnection(masterConnectionString);
            connection.Open();

            using var command = new SqlCommand(
                $"CREATE DATABASE {QuotedName(database)} " +
                $"ON (NAME=N{QuotedName(database, '\'')}, FILENAME=N{QuotedName(fileName, '\'')}) " +
                $"LOG ON (NAME=N{QuotedName(logName, '\'')}, FILENAME=N{QuotedName(logFileName, '\'')});" +
                $"ALTER DATABASE {QuotedName(database)} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" +
                $"EXEC sp_detach_db @dbname=N{QuotedName(database, '\'')};",
                connection);
            command.ExecuteNonQuery();

            connection.Close();
        }
        else if (!string.IsNullOrEmpty(database))
        {
            using var connection = new SqlConnection(masterConnectionString);
            connection.Open();

            using var command = new SqlCommand(
                $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N{QuotedName(database, '\'')}) " +
                $"CREATE DATABASE {QuotedName(database)};",
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
    ///     Gets the log (LDF) file path for the specified data file, beside the data file.
    /// </summary>
    /// <param name="dataFileName">The data (MDF) file name.</param>
    /// <returns>The log file path, for example "C:\Data\Umbraco_log.ldf" for "C:\Data\Umbraco.mdf".</returns>
    /// <remarks>
    ///     This is where SQL Server places the log itself for the paths it derives correctly. Stating
    ///     it explicitly avoids the derivation described in <see cref="Create" />, so this deliberately
    ///     does not reproduce what SQL Server derives for the paths it gets wrong.
    /// </remarks>
    internal static string GetLogFileName(string dataFileName)
    {
        var logFileName = GetLogName(dataFileName) + ".ldf";
        var directory = Path.GetDirectoryName(dataFileName);

        return string.IsNullOrEmpty(directory) ? logFileName : Path.Combine(directory, logFileName);
    }
}
