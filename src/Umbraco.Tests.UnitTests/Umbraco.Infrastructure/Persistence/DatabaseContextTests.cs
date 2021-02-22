using NUnit.Framework;
using Umbraco.Core.Migrations.Install;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence
{
    [TestFixture]
    public class DatabaseContextTests
    {

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
            var connectionString = DatabaseBuilder.GetAzureConnectionString(server, databaseName, userName, password);
            Assert.AreEqual(connectionString, "Server=tcp:MyServer.database.windows.net,1433;Database=MyDatabase;User ID=MyUser@MyServer;Password=MyPassword");
        }

        [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com,1433", "MyDatabase", "MyUser", "MyPassword")]
        [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com,1433", "MyDatabase", "MyUser@kzeej5z8ty", "MyPassword")]
        [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com", "MyDatabase", "MyUser", "MyPassword")]
        [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com", "MyDatabase", "MyUser@kzeej5z8ty", "MyPassword")]
        public void Build_Azure_Connection_String_CustomServer(string server, string databaseName, string userName, string password)
        {
            var connectionString = DatabaseBuilder.GetAzureConnectionString(server, databaseName, userName, password);
            Assert.AreEqual(connectionString, "Server=tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com,1433;Database=MyDatabase;User ID=MyUser@kzeej5z8ty;Password=MyPassword");
        }
    }
}
