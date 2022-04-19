using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence
{
    [TestFixture]
    [UmbracoTest]
    [Platform("Win")]
    public class DatabaseBuilderTests : UmbracoIntegrationTest
    {
        private IDbProviderFactoryCreator DbProviderFactoryCreator => GetRequiredService<IDbProviderFactoryCreator>();
        private IUmbracoDatabaseFactory UmbracoDatabaseFactory => GetRequiredService<IUmbracoDatabaseFactory>();
        private IDatabaseCreator EmbeddedDatabaseCreator => GetRequiredService<IDatabaseCreator>();

        public DatabaseBuilderTests()
        {
            TestOptionAttributeBase.ScanAssemblies.Add(typeof(DatabaseBuilderTests).Assembly);
        }

        [Test]
        public void CreateDatabase()
        {
            var path = TestContext.CurrentContext.TestDirectory.Split("bin")[0];
            AppDomain.CurrentDomain.SetData("DataDirectory", path);
            const string dbFile = "DatabaseContextTests.sdf";
            // delete database file
            // NOTE: using a custom db file for this test since we're re-using the one created with BaseDatabaseFactoryTest
            var filePath = string.Concat(path, dbFile);
            if (File.Exists(filePath))
                File.Delete(filePath);

            var connectionString = $"Datasource=|DataDirectory|{dbFile};Flush Interval=1";

            UmbracoDatabaseFactory.Configure(connectionString, Constants.DbProviderNames.SqlCe);
            DbProviderFactoryCreator.CreateDatabase(Constants.DbProviderNames.SqlCe, connectionString);
            UmbracoDatabaseFactory.CreateDatabase();

            // test get database type (requires an actual database)
            using (var database = UmbracoDatabaseFactory.CreateDatabase())
            {
                var databaseType = database.DatabaseType;
                Assert.AreEqual(DatabaseType.SQLCe, databaseType);
            }

            // create application context
            //var appCtx = new ApplicationContext(
            //    _databaseFactory,
            //    new ServiceContext(migrationEntryService: Mock.Of<IMigrationEntryService>()),
            //    CacheHelper.CreateDisabledCacheHelper(),
            //    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            // create the umbraco database
            DatabaseSchemaCreator schemaHelper;
            using (var database = UmbracoDatabaseFactory.CreateDatabase())
            using (var transaction = database.GetTransaction())
            {
                schemaHelper = new DatabaseSchemaCreator(database, Mock.Of<ILogger<DatabaseSchemaCreator>>(), NullLoggerFactory.Instance, new UmbracoVersion(), Mock.Of<IEventAggregator>(), Mock.Of<IOptionsMonitor<InstallDefaultDataSettings>>(x => x.CurrentValue == new InstallDefaultDataSettings()));
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

    }
}
