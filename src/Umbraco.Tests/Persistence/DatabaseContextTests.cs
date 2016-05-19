using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{

    [TestFixture, RequiresSTA]
    public class DatabaseContextTests
    {
	    private DatabaseContext _dbContext;
        private ILogger _logger;
        private SqlCeSyntaxProvider _sqlCeSyntaxProvider;
        private ISqlSyntaxProvider[] _sqlSyntaxProviders;

		[SetUp]
		public void Setup()
		{
            // create the database factory and database context
            _sqlCeSyntaxProvider = new SqlCeSyntaxProvider();
            _sqlSyntaxProviders = new[] { (ISqlSyntaxProvider) _sqlCeSyntaxProvider };
            _logger = Mock.Of<ILogger>();
            var dbFactory = new DefaultDatabaseFactory(Core.Configuration.GlobalSettings.UmbracoConnectionName, _sqlSyntaxProviders, _logger, new TestScopeContextAdapter(), Mock.Of<IMappingResolver>());
            _dbContext = new DatabaseContext(dbFactory, _logger);
		}

		[TearDown]
		public void TearDown()
		{
			_dbContext = null;
		}

        [Test]
        public void Can_Verify_Single_Database_Instance()
        {
			var db1 = _dbContext.Database;
			var db2 = _dbContext.Database;

            Assert.AreSame(db1, db2);
        }

        [Test]
        public void Can_Assert_DatabaseType()
        {
			var databaseType = _dbContext.Database.DatabaseType;

            Assert.AreEqual(DatabaseType.SQLCe, databaseType);
        }

        [Test]
        public void Can_Assert_Created_Database()
        {
            var path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            // delete database file
            // NOTE: using a custom db file for this test since we're re-using the one created with BaseDatabaseFactoryTest
            var filePath = string.Concat(path, "\\DatabaseContextTests.sdf");
            if (File.Exists(filePath))
                File.Delete(filePath);

            // get the connectionstring settings from config
            var settings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];

            // by default the conn string is: Datasource=|DataDirectory|UmbracoNPocoTests.sdf;Flush Interval=1;
            // replace the SDF file with our own and create the sql ce database
            var engine = new SqlCeEngine(settings.ConnectionString.Replace("UmbracoNPocoTests", "DatabaseContextTests"));
            engine.CreateDatabase();

            // re-create the database factory and database context with proper connection string
            var dbFactory = new DefaultDatabaseFactory(engine.LocalConnectionString, Constants.DbProviderNames.SqlCe, _sqlSyntaxProviders, _logger, new TestScopeContextAdapter(), Mock.Of<IMappingResolver>());
            _dbContext = new DatabaseContext(dbFactory, _logger);

            // create application context
            var appCtx = new ApplicationContext(
                _dbContext,
                new ServiceContext(migrationEntryService: Mock.Of<IMigrationEntryService>()),
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            // create the umbraco database
            var schemaHelper = new DatabaseSchemaHelper(_dbContext.Database, _logger);
            schemaHelper.CreateDatabaseSchema(false, appCtx);

            var umbracoNodeTable = schemaHelper.TableExist("umbracoNode");
            var umbracoUserTable = schemaHelper.TableExist("umbracoUser");
            var cmsTagsTable = schemaHelper.TableExist("cmsTags");

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
            var connectionString = _dbContext.GetAzureConnectionString(server, databaseName, userName, password);
            Assert.AreEqual(connectionString, "Server=tcp:MyServer.database.windows.net,1433;Database=MyDatabase;User ID=MyUser@MyServer;Password=MyPassword");
        }

        [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com,1433", "MyDatabase", "MyUser", "MyPassword")]
        [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com,1433", "MyDatabase", "MyUser@kzeej5z8ty", "MyPassword")]
        [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com", "MyDatabase", "MyUser", "MyPassword")]
        [TestCase("tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com", "MyDatabase", "MyUser@kzeej5z8ty", "MyPassword")]
        public void Build_Azure_Connection_String_CustomServer(string server, string databaseName, string userName, string password)
        {
            var connectionString = _dbContext.GetAzureConnectionString(server, databaseName, userName, password);
            Assert.AreEqual(connectionString, "Server=tcp:kzeej5z8ty.ssmsawacluster4.windowsazure.mscds.com,1433;Database=MyDatabase;User ID=MyUser@kzeej5z8ty;Password=MyPassword");
        }
    }
}