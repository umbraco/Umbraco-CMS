using NUnit.Framework;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence;

/// <summary>
/// Contains unit tests for the <see cref="SqlAzureDatabaseProviderMetadata"/> class to verify its behavior and functionality.
/// </summary>
[TestFixture]
public class SqlAzureDatabaseProviderMetadataTests
{
    /// <summary>
    /// Verifies that the <see cref="SqlAzureDatabaseProviderMetadata.GenerateConnectionString"/> method correctly builds a standard Azure SQL connection string
    /// using various combinations of server, database, username, and password inputs.
    /// </summary>
    /// <param name="server">The Azure SQL server address, which may include protocol and port information.</param>
    /// <param name="databaseName">The name of the Azure SQL database.</param>
    /// <param name="userName">The username for database authentication, which may include the server name as a suffix.</param>
    /// <param name="password">The password for the specified user.</param>
    [TestCase("MyServer", "MyDatabase", "MyUser", "MyPassword")]
    [TestCase("MyServer", "MyDatabase", "MyUser@MyServer", "MyPassword")]
    [TestCase("tcp:MyServer", "MyDatabase", "MyUser", "MyPassword")]
    [TestCase("tcp:MyServer", "MyDatabase", "MyUser@MyServer", "MyPassword")]
    [TestCase("tcp:MyServer,1433", "MyDatabase", "MyUser", "MyPassword")]
    [TestCase("tcp:MyServer,1433", "MyDatabase", "MyUser@MyServer", "MyPassword")]
    [TestCase("tcp:MyServer.database.windows.net", "MyDatabase", "MyUser", "MyPassword")]
    [TestCase("tcp:MyServer.database.windows.net", "MyDatabase", "MyUser@MyServer", "MyPassword")]
    [TestCase("tcp:MyServer.database.windows.net,1433", "MyDatabase", "MyUser", "MyPassword")]
    [TestCase("tcp:MyServer.database.windows.net,1433", "MyDatabase", "MyUser@MyServer", "MyPassword")]
    public void Build_Azure_Connection_String_Regular(string server, string databaseName, string userName, string password)
    {
        var settings = new DatabaseModel
        {
            Server = server,
            DatabaseName = databaseName,
            Login = userName,
            Password = password,
        };

        var sut = new SqlAzureDatabaseProviderMetadata();
        var connectionString = sut.GenerateConnectionString(settings);
        Assert.AreEqual(connectionString, "Server=tcp:MyServer.database.windows.net,1433;Database=MyDatabase;User ID=MyUser@MyServer;Password=MyPassword");
    }

    /// <summary>
    /// Tests building an Azure SQL connection string with a custom server.
    /// </summary>
    /// <param name="server">The server address, including protocol and port.</param>
    /// <param name="databaseName">The name of the database.</param>
    /// <param name="userName">The user name for authentication.</param>
    /// <param name="password">The password for authentication.</param>
    [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com,1433", "MyDatabase", "MyUser", "MyPassword")]
    [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com,1433", "MyDatabase", "MyUser@kzeej5z8ty", "MyPassword")]
    [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com", "MyDatabase", "MyUser", "MyPassword")]
    [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com", "MyDatabase", "MyUser@kzeej5z8ty", "MyPassword")]
    public void Build_Azure_Connection_String_CustomServer(string server, string databaseName, string userName, string password)
    {
        var settings = new DatabaseModel
        {
            Server = server,
            DatabaseName = databaseName,
            Login = userName,
            Password = password,
        };

        var sut = new SqlAzureDatabaseProviderMetadata();
        var connectionString = sut.GenerateConnectionString(settings);
        Assert.AreEqual(connectionString, "Server=tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com,1433;Database=MyDatabase;User ID=MyUser@kzeej5z8ty;Password=MyPassword");
    }
}
