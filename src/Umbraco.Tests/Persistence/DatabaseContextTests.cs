using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Threading;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class DatabaseContextTests
    {
        private IUmbracoDatabaseFactory _databaseFactory;
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
            _databaseFactory = new UmbracoDatabaseFactory(_logger, new Lazy<IMapperCollection>(() => Mock.Of<IMapperCollection>()));
        }

        [TearDown]
        public void TearDown()
        {
            _databaseFactory = null;
        }

        [Test]
        public void GetDatabaseType()
        {
            using (var database = _databaseFactory.CreateDatabase())
            {
                var databaseType = database.DatabaseType;
                Assert.AreEqual(DatabaseType.SQLCe, databaseType);
            }
        }

        [Test]
        public void CreateDatabase() // fixme - move to DatabaseBuilderTest!
        {
            var path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            // delete database file
            // NOTE: using a custom db file for this test since we're re-using the one created with BaseDatabaseFactoryTest
            var filePath = string.Concat(path, "\\DatabaseContextTests.sdf");
            if (File.Exists(filePath))
                File.Delete(filePath);

            // get the connectionstring settings from config
            var settings = ConfigurationManager.ConnectionStrings[Constants.System.UmbracoConnectionName];

            // by default the conn string is: Datasource=|DataDirectory|UmbracoNPocoTests.sdf;Flush Interval=1;
            // replace the SDF file with our own and create the sql ce database
            var connString = settings.ConnectionString.Replace("UmbracoNPocoTests", "DatabaseContextTests");
            using (var engine = new SqlCeEngine(connString))
            {
                engine.CreateDatabase();
            }

            // re-create the database factory and database context with proper connection string
            _databaseFactory = new UmbracoDatabaseFactory(connString, Constants.DbProviderNames.SqlCe, _logger, new Lazy<IMapperCollection>(() => Mock.Of<IMapperCollection>()));

            // create application context
            //var appCtx = new ApplicationContext(
            //    _databaseFactory,
            //    new ServiceContext(migrationEntryService: Mock.Of<IMigrationEntryService>()),
            //    CacheHelper.CreateDisabledCacheHelper(),
            //    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            // create the umbraco database
            DatabaseSchemaCreator schemaHelper;
            using (var database = _databaseFactory.CreateDatabase())
            using (var transaction = database.GetTransaction())
            {
                schemaHelper = new DatabaseSchemaCreator(database, _logger);
                schemaHelper.InitializeDatabaseSchema();
                transaction.Complete();
            }

            var umbracoNodeTable = schemaHelper.TableExists("umbracoNode");
            var umbracoUserTable = schemaHelper.TableExists("umbracoUser");
            var cmsTagsTable = schemaHelper.TableExists("cmsTags");

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
