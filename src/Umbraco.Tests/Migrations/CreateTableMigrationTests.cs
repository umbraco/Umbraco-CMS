using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    [DatabaseTestBehavior(DatabaseBehavior.EmptyDbFilePerTest)]
    public class CreateTableMigrationTests : BaseDatabaseFactoryTest
    {
        [Test]
        public void CreateTableOfTDto()
        {
            var logger = new DebugDiagnosticsLogger();

            var migrationRunner = new MigrationRunner(
                Mock.Of<IMigrationEntryService>(),
                logger,
                new SemVersion(0, 0, 0),
                new SemVersion(1, 0, 0),
                "Test",

                // explicit migrations
                new CreateTableOfTDtoMigration(SqlSyntax, logger)
            );

            var db = new UmbracoDatabase("Datasource=|DataDirectory|UmbracoPetaPocoTests.sdf;Flush Interval=1;", Constants.DatabaseProviders.SqlCe, Logger);

            var upgraded = migrationRunner.Execute(db, DatabaseProviders.SqlServerCE, true);
            Assert.IsTrue(upgraded);

            var helper = new DatabaseSchemaHelper(db, logger, SqlSyntax);
            var exists = helper.TableExist("umbracoNode");
            Assert.IsTrue(exists);
        }

        [Migration("1.0.0", 0, "Test")]
        public class CreateTableOfTDtoMigration : MigrationBase
        {
            public CreateTableOfTDtoMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger)
                : base(sqlSyntax, logger)
            { }

            public override void Up()
            {
                Create.Table<NodeDto>();
            }

            public override void Down()
            {
                throw new NotImplementedException();
            }
        }
    }
}
