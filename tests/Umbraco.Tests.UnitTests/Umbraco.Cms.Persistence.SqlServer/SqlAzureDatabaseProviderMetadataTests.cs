using NUnit.Framework;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.SqlServer;

/// <summary>
/// Contains unit tests for the <see cref="SqlAzureDatabaseProviderMetadata"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class SqlAzureDatabaseProviderMetadataTests
{
    /// <summary>
    /// Generates a SQL Azure connection string based on the provided parameters.
    /// </summary>
    /// <param name="server">The SQL Azure server name.</param>
    /// <param name="databaseName">The name of the database.</param>
    /// <param name="login">The login username.</param>
    /// <param name="password">The login password.</param>
    /// <param name="integratedAuth">Indicates whether integrated authentication is used.</param>
    /// <returns>A connection string formatted for SQL Azure.</returns>
    [Test]
    [TestCase("myServer", "myDatabase", "myLogin", "myPassword", true /*ignored*/, ExpectedResult = "Server=tcp:myServer.database.windows.net,1433;Database=myDatabase;User ID=myLogin@myServer;Password=myPassword")]
    [TestCase("myServer", "myDatabase", "myLogin", "myPassword", false, ExpectedResult = "Server=tcp:myServer.database.windows.net,1433;Database=myDatabase;User ID=myLogin@myServer;Password=myPassword")]
    public string GenerateConnectionString(string server, string databaseName, string login, string password, bool integratedAuth)
    {
        var sut = new SqlAzureDatabaseProviderMetadata();
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
    /// Determines whether the specified connection string is recognized as an Azure SQL Database connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to evaluate.</param>
    /// <returns><c>true</c> if the connection string is recognized as an Azure SQL Database connection string; otherwise, <c>false</c>.</returns>
    [Test]
    [TestCase("Server=myServer;Database=myDatabase;Integrated Security=true", ExpectedResult = false)] // SqlServer
    [TestCase("Server=myServer;Database=myDatabase;User Id=myLogin;Password=myPassword", ExpectedResult = false)] // SqlServer
    [TestCase("Server=tcp:cmstest27032000.database.windows.net,1433;Database=test_27032000;User ID=asdasdas@cmstest27032000;Password=123456879", ExpectedResult = true)] // Azure
    [TestCase("Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True", ExpectedResult = false)] // Sqlite
    [TestCase("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=aspnet-MvcMovie;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|\\umbraco.mdf", ExpectedResult = false)] // localDB
    public bool CanRecognizeConnectionString(string connectionString)
    {
        var sut = new SqlAzureDatabaseProviderMetadata();
        return sut.CanRecognizeConnectionString(connectionString);
    }
}
