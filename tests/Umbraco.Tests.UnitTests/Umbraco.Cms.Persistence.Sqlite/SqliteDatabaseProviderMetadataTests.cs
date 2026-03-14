using NUnit.Framework;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Persistence.Sqlite.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.Sqlite;

    /// <summary>
    /// Contains unit tests for the <see cref="SqliteDatabaseProviderMetadata"/> class to verify its behavior and functionality.
    /// </summary>
[TestFixture]
public class SqliteDatabaseProviderMetadataTests
{
    /// <summary>
    /// Generates a SQLite connection string for the specified database file name.
    /// </summary>
    /// <param name="server">Unused. Included for interface compatibility; ignored for SQLite.</param>
    /// <param name="databaseName">The name of the SQLite database file.</param>
    /// <param name="login">Unused. Included for interface compatibility; ignored for SQLite.</param>
    /// <param name="password">Unused. Included for interface compatibility; ignored for SQLite.</param>
    /// <param name="integratedAuth">Unused. Included for interface compatibility; ignored for SQLite.</param>
    /// <returns>A connection string targeting the specified SQLite database file.</returns>
    [Test]
    [TestCase("ignored", "myDatabase", "ignored", "ignored", true /*ignored*/, ExpectedResult = "Data Source=|DataDirectory|/myDatabase.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True")]
    [TestCase("ignored", "myDatabase2", "ignored", "ignored", false /*ignored*/, ExpectedResult = "Data Source=|DataDirectory|/myDatabase2.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True")]
    public string GenerateConnectionString(string server, string databaseName, string login, string password, bool integratedAuth)
    {
        var sut = new SqliteDatabaseProviderMetadata();
        return sut.GenerateConnectionString(new DatabaseModel()
        {
            DatabaseName = databaseName,
            Login = login,
            Password = password,
            Server = server,
            IntegratedAuth = integratedAuth
        });
    }

    /// <summary>
    /// Determines whether the specified connection string is recognized as a SQLite connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to evaluate.</param>
    /// <returns>True if the connection string is recognized as SQLite; otherwise, false.</returns>
    [Test]
    [TestCase("Server=myServer;Database=myDatabase;Integrated Security=true", ExpectedResult = false)] // SqlServer
    [TestCase("Server=myServer;Database=myDatabase;User Id=myLogin;Password=myPassword", ExpectedResult = false)] // SqlServer
    [TestCase("Server=tcp:cmstest27032000.database.windows.net,1433;Database=test_27032000;User ID=asdasdas@cmstest27032000;Password=123456879", ExpectedResult = false)] // Azure
    [TestCase("Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True", ExpectedResult = true)] // Sqlite
    [TestCase("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=aspnet-MvcMovie;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|\\umbraco.mdf", ExpectedResult = false)] // localDB
    public bool CanRecognizeConnectionString(string connectionString)
    {
        var sut = new SqliteDatabaseProviderMetadata();
        return sut.CanRecognizeConnectionString(connectionString);
    }
}
