using System;
using System.Data.Common;
using Moq;
using NPoco;
using NUnit.Framework;
using Semver;
using SQLCE4Umbraco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Web.Strategies.Migrations;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

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

            //Setup the MigrationRunner
            var migrationContext = new MigrationContext(db, Mock.Of<ILogger>());
            var migrationRunner = new MigrationRunner(
                Mock.Of<IMigrationResolver>(),
                Mock.Of<IMigrationEntryService>(),
                Mock.Of<ILogger>(), configuredVersion, targetVersion, GlobalSettings.UmbracoMigrationName);

            bool upgraded = migrationRunner.Execute(migrationContext /*, true*/);

            Assert.That(upgraded, Is.True);

            var schemaHelper = new DatabaseSchemaHelper(db, Mock.Of<ILogger>());

            bool hasTabTable = schemaHelper.TableExist("cmsTab");
            bool hasPropertyTypeGroupTable = schemaHelper.TableExist("cmsPropertyTypeGroup");
            bool hasAppTreeTable = schemaHelper.TableExist("umbracoAppTree");

            fix.Unsubscribe();

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
            SqlCeContextGuardian.CloseBackgroundConnection();
        }

        public override UmbracoDatabase GetConfiguredDatabase()
        {
            var databaseType = DatabaseType.SQLCe;
            var sqlSyntax = new SqlCeSyntaxProvider();
            var dbProviderFactory = DbProviderFactories.GetFactory(Constants.DbProviderNames.SqlCe);
            return new UmbracoDatabase("Datasource=|DataDirectory|UmbracoNPocoTests.sdf;Flush Interval=1;", sqlSyntax, databaseType, dbProviderFactory, Mock.Of<ILogger>());
        }

        public override string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.SqlCe_SchemaAndData_4110;
        }
    }
}