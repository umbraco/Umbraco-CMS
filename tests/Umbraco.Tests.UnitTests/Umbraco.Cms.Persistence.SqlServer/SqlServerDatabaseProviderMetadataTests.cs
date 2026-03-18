using NUnit.Framework;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.SqlServer;

/// <summary>
/// Contains unit tests for the <see cref="SqlServerDatabaseProviderMetadata"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class SqlServerDatabaseProviderMetadataTests
{
    /// <summary>
    /// Generates a SQL Server connection string based on the provided parameters.
    /// </summary>
    /// <param name="server">The name or network address of the instance of SQL Server to which to connect.</param>
    /// <param name="databaseName">The name of the database.</param>
    /// <param name="login">The login username for SQL Server authentication.</param>
    /// <param name="password">The password for SQL Server authentication.</param>
    /// <param name="integratedAuth">If true, uses integrated security; otherwise, uses SQL Server authentication.</param>
    /// <returns>A connection string formatted for SQL Server.</returns>
    [Test]
    [TestCase("myServer", "myDatabase", "myLogin", "myPassword", true, ExpectedResult = "Server=myServer;Database=myDatabase;Integrated Security=true")]
    [TestCase("myServer", "myDatabase", "myLogin", "myPassword", false, ExpectedResult = "Server=myServer;Database=myDatabase;User Id=myLogin;Password=myPassword")]
    public string GenerateConnectionString(string server, string databaseName, string login, string password, bool integratedAuth)
    {
        var sut = new SqlServerDatabaseProviderMetadata();
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
    /// Determines whether the specified connection string can be recognized as a SQL Server connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to test.</param>
    /// <returns><c>true</c> if the connection string is recognized as SQL Server; otherwise, <c>false</c>.</returns>
    [Test]
    [TestCase("Server=myServer;Database=myDatabase;Integrated Security=true", ExpectedResult = true)] // SqlServer
    [TestCase("Server=myServer;Database=myDatabase;User Id=myLogin;Password=myPassword", ExpectedResult = true)] // SqlServer
    [TestCase("Server=tcp:cmstest27032000.database.windows.net,1433;Database=test_27032000;User ID=asdasdas@cmstest27032000;Password=123456879", ExpectedResult = true)] // Azure
    [TestCase("Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True", ExpectedResult = false)] // Sqlite
    [TestCase("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=aspnet-MvcMovie;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|\\umbraco.mdf", ExpectedResult = false)] // localDB
    public bool CanRecognizeConnectionString(string connectionString)
    {
        var sut = new SqlServerDatabaseProviderMetadata();
        return sut.CanRecognizeConnectionString(connectionString);
    }
}
