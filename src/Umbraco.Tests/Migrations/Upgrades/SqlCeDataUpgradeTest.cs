using System;
using Moq;
using NUnit.Framework;
using Semver;
using SQLCE4Umbraco;
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
            var provider = GetDatabaseProvider();
            var db = GetConfiguredDatabase();

            var fix = new PublishAfterUpgradeToVersionSixth();

            //Setup the MigrationRunner
            var migrationRunner = new MigrationRunner(
                Mock.Of<IMigrationEntryService>(),
                Mock.Of<ILogger>(), configuredVersion, targetVersion, GlobalSettings.UmbracoMigrationName);

            bool upgraded = migrationRunner.Execute(db, provider, true);

            Assert.That(upgraded, Is.True);

            bool hasTabTable = db.TableExist("cmsTab");
            bool hasPropertyTypeGroupTable = db.TableExist("cmsPropertyTypeGroup");
            bool hasAppTreeTable = db.TableExist("umbracoAppTree");

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

        public override ISqlSyntaxProvider GetSyntaxProvider()
        {
            return new SqlCeSyntaxProvider();
        }

        public override UmbracoDatabase GetConfiguredDatabase()
        {
            return new UmbracoDatabase("Datasource=|DataDirectory|UmbracoPetaPocoTests.sdf;Flush Interval=1;", "System.Data.SqlServerCe.4.0", Mock.Of<ILogger>());
        }

        public override DatabaseProviders GetDatabaseProvider()
        {
            return DatabaseProviders.SqlServerCE;
        }

        public override string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.SqlCe_SchemaAndData_4110;
        } 
    }
}