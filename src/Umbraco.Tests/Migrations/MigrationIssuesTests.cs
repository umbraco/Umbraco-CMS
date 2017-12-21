using System;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade.TargetVersionEight;
using Umbraco.Core.Models.Rdbms;
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

            var builder = Mock.Of<IMigrationCollectionBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Instanciate(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "DeleteRedirectUrlTable":
                            return new DeleteRedirectUrlTable(c);
                        case "AddRedirectUrlTable":
                            return new AddRedirectUrlTable(c);
                        default:
                            throw new NotSupportedException();
                    }
                });

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                //Setup the MigrationRunner
                var migrationRunner = new MigrationRunner(
                    ScopeProvider,
                    builder,
                    Mock.Of<IMigrationEntryService>(),
                    logger,
                    new SemVersion(7, 5, 0),
                    new SemVersion(8, 0, 0),
                    Constants.System.UmbracoMigrationName,

                    //pass in explicit migrations
                    typeof(DeleteRedirectUrlTable),
                    typeof(AddRedirectUrlTable)
                );

                var upgraded = migrationRunner.Execute();
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
