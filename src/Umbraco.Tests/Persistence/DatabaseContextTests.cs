using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class DatabaseContextTests
    {
	    private DatabaseContext _dbContext;

		[SetUp]
		public void Setup()
		{
			_dbContext = new DatabaseContext(new DefaultDatabaseFactory());

			//unfortunately we have to set this up because the PetaPocoExtensions require singleton access
			ApplicationContext.Current = new ApplicationContext(false)
				{
					DatabaseContext = _dbContext,
					IsReady = true
				};			
		}

		[TearDown]
		public void TearDown()
		{
			_dbContext = null;
			ApplicationContext.Current = null;
		}

        [Test]
        public void Can_Verify_Single_Database_Instance()
        {
			var db1 = _dbContext.Database;
			var db2 = _dbContext.Database;

            Assert.AreSame(db1, db2);
        }

        [Test]
        public void Can_Assert_DatabaseProvider()
        {
			var provider = _dbContext.DatabaseProvider;

            Assert.AreEqual(DatabaseProviders.SqlServerCE, provider);
        }

        [Test]
        public void Can_Assert_Created_Database()
        {
            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            //Delete database file before continueing
            //NOTE: we'll use a custom db file for this test since we're re-using the one created with BaseDatabaseFactoryTest
            string filePath = string.Concat(path, "\\DatabaseContextTests.sdf");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            //Get the connectionstring settings from config
            var settings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];

            //by default the conn string is: Datasource=|DataDirectory|UmbracoPetaPocoTests.sdf;Flush Interval=1;
            //we'll just replace the sdf file with our custom one:
            //Create the Sql CE database
            var engine = new SqlCeEngine(settings.ConnectionString.Replace("UmbracoPetaPocoTests", "DatabaseContextTests"));
            engine.CreateDatabase();

            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            //Create the umbraco database
			_dbContext.Database.CreateDatabaseSchema(false);

			bool umbracoNodeTable = _dbContext.Database.TableExist("umbracoNode");
			bool umbracoUserTable = _dbContext.Database.TableExist("umbracoUser");
			bool cmsTagsTable = _dbContext.Database.TableExist("cmsTags");

            Assert.That(umbracoNodeTable, Is.True);
            Assert.That(umbracoUserTable, Is.True);
            Assert.That(cmsTagsTable, Is.True);
        }

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
            var connectionString = _dbContext.BuildAzureConnectionString(server, databaseName, userName, password);
            Assert.AreEqual(connectionString, "Server=tcp:MyServer.database.windows.net,1433;Database=MyDatabase;User ID=MyUser@MyServer;Password=MyPassword");
        }

        [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com,1433", "MyDatabase", "MyUser", "MyPassword")]
        [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com,1433", "MyDatabase", "MyUser@kzeej5z8ty", "MyPassword")]
        [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com", "MyDatabase", "MyUser", "MyPassword")]
        [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com", "MyDatabase", "MyUser@kzeej5z8ty", "MyPassword")]
        public void Build_Azure_Connection_String_CustomServer(string server, string databaseName, string userName, string password)
        {
            var connectionString = _dbContext.BuildAzureConnectionString(server, databaseName, userName, password);
            Assert.AreEqual(connectionString, "Server=tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com,1433;Database=MyDatabase;User ID=MyUser@kzeej5z8ty;Password=MyPassword");
        }
    }
}