using System;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionEight;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class MigrationIssuesTests : TestWithDatabaseBase
    {
        [Test]
        public void Issue8361Test()
        {
            var logger = new DebugDiagnosticsLogger();

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                var database = scope.Database;

                var migrationContext = new MigrationContext(database, Logger);

                //Setup the MigrationRunner
                var migrationRunner = new MigrationRunner(
                    Mock.Of<IMigrationCollectionBuilder>(),
                    Mock.Of<IMigrationEntryService>(),
                    logger,
                    new SemVersion(7, 5, 0),
                    new SemVersion(8, 0, 0),
                    Constants.System.UmbracoMigrationName,

                    //pass in explicit migrations
                    new DeleteRedirectUrlTable(migrationContext),
                    new AddRedirectUrlTable(migrationContext)
                );

                var upgraded = migrationRunner.Execute(migrationContext, true);
                Assert.IsTrue(upgraded);
            }
        }

        [Migration("8.0.0", 99, Constants.System.UmbracoMigrationName)]
        public class DeleteRedirectUrlTable : MigrationBase
        {
            public DeleteRedirectUrlTable(IMigrationContext context)
                : base(context)
            { }

            public override void Up()
            {
                Delete.Table("umbracoRedirectUrl");
            }

            public override void Down()
            { }
        }
    }
}
