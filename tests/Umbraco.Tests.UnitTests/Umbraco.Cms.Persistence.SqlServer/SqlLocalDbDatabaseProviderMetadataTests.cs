using NUnit.Framework;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.SqlServer;

/// <summary>
/// Contains unit tests for the <see cref="SqlLocalDbDatabaseProviderMetadata"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class SqlLocalDbDatabaseProviderMetadataTests
{
    /// <summary>
    /// Generates a connection string for a SQL LocalDB database.
    /// </summary>
    /// <param name="server">The server name (ignored for LocalDB).</param>
    /// <param name="databaseName">The name of the database.</param>
    /// <param name="login">The login username (ignored for LocalDB).</param>
    /// <param name="password">The login password (ignored for LocalDB).</param>
    /// <param name="integratedAuth">Whether to use integrated authentication.</param>
    /// <returns>A connection string for the specified LocalDB database.</returns>
    [Test]
    [TestCase("ignored", "myDatabase", "ignored", "ignored", true, ExpectedResult = "Data Source=(localdb)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\myDatabase.mdf;Integrated Security=True")]
    [TestCase("ignored", "myDatabase2", "ignored", "ignored", false /*ignored*/, ExpectedResult = "Data Source=(localdb)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\myDatabase2.mdf;Integrated Security=True")]
    public string GenerateConnectionString(string server, string databaseName, string login, string password, bool integratedAuth)
    {
        var sut = new SqlLocalDbDatabaseProviderMetadata();
        return sut.GenerateConnectionString(new DatabaseModel()
        {
            DatabaseName = databaseName,
            Login = login,
            Password = password,
            Server = server,
            IntegratedAuth = integratedAuth,
        });
    }

    /// <summary>
    /// Determines whether the specified connection string can be recognized as a LocalDb connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to test.</param>
    /// <returns><c>true</c> if the connection string is recognized as LocalDb; otherwise, <c>false</c>.</returns>
    [Test]
    [TestCase("Server=myServer;Database=myDatabase;Integrated Security=true", ExpectedResult = false)] // SqlServer
    [TestCase("Server=myServer;Database=myDatabase;User Id=myLogin;Password=myPassword", ExpectedResult = false)] // SqlServer
    [TestCase("Server=tcp:cmstest27032000.database.windows.net,1433;Database=test_27032000;User ID=asdasdas@cmstest27032000;Password=123456879", ExpectedResult = false)] // Azure
    [TestCase("Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True", ExpectedResult = false)] // Sqlite
    [TestCase("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=aspnet-MvcMovie;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|\\umbraco.mdf", ExpectedResult = true)] // localDB
    public bool CanRecognizeConnectionString(string connectionString)
    {
        var sut = new SqlLocalDbDatabaseProviderMetadata();
        return sut.CanRecognizeConnectionString(connectionString);
    }
}
