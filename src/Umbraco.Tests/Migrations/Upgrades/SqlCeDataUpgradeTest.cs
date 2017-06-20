using System.Data.Common;
using Moq;
using NPoco;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Web.Strategies.Migrations;

namespace Umbraco.Tests.Migrations.Upgrades
{
    [TestFixture, NUnit.Framework.Ignore]
    public class SqlCeDataUpgradeTest : BaseUpgradeTest
    {

        [Test, NUnit.Framework.Ignore]
        public override void Can_Upgrade_From_470_To_600()
        {
            var configuredVersion = new SemVersion(4, 11, 0);
            var targetVersion = new SemVersion(6, 0, 0);
            var db = GetConfiguredDatabase();

            var fix = new PublishAfterUpgradeToVersionSixth();
            MigrationRunner.Migrated += fix.Migrated;

            //Setup the MigrationRunner
            var migrationContext = new MigrationContext(db, Mock.Of<ILogger>());
            var migrationRunner = new MigrationRunner(
                Mock.Of<IMigrationCollectionBuilder>(),
                Mock.Of<IMigrationEntryService>(),
                Mock.Of<ILogger>(), configuredVersion, targetVersion, Constants.System.UmbracoMigrationName);

            bool upgraded = migrationRunner.Execute(migrationContext /*, true*/);

            Assert.That(upgraded, Is.True);

            var schemaHelper = new DatabaseSchemaHelper(db, Mock.Of<ILogger>());

            bool hasTabTable = schemaHelper.TableExist("cmsTab");
            bool hasPropertyTypeGroupTable = schemaHelper.TableExist("cmsPropertyTypeGroup");
            bool hasAppTreeTable = schemaHelper.TableExist("umbracoAppTree");

            MigrationRunner.Migrated -= fix.Migrated;

            Assert.That(hasTabTable, Is.False);
            Assert.That(hasPropertyTypeGroupTable, Is.True);
            Assert.That(hasAppTreeTable, Is.False);
        }

        public override void DatabaseSpecificSetUp()
        {
        }

        public override void DatabaseSpecificTearDown()
        {
            //legacy API database connection close
            //SqlCeContextGuardian.CloseBackgroundConnection();
        }

        public override IUmbracoDatabase GetConfiguredDatabase()
        {
            var dbProviderFactory = DbProviderFactories.GetFactory(Constants.DbProviderNames.SqlCe);
            var sqlContext = new SqlContext(new SqlCeSyntaxProvider(), Mock.Of<IPocoDataFactory>(), DatabaseType.SQLCe);
            return new UmbracoDatabase("Datasource=|DataDirectory|UmbracoNPocoTests.sdf;Flush Interval=1;", sqlContext, dbProviderFactory, Mock.Of<ILogger>());
        }

        public override string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.SqlCe_SchemaAndData_4110;
        }
    }
}