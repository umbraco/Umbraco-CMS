using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
    public class CreateTableMigrationTests : TestWithDatabaseBase
    {
        [Test]
        public void CreateTableOfTDto()
        {
            var logger = new DebugDiagnosticsLogger();

            using (var scope = ScopeProvider.CreateScope())
            {
                var database = scope.Database;

                var context = new MigrationContext(database, logger);

                var runner = new MigrationRunner(
                    Mock.Of<IMigrationCollectionBuilder>(),
                    Mock.Of<IMigrationEntryService>(),
                    logger,
                    new SemVersion(0), // 0.0.0
                    new SemVersion(1), // 1.0.0
                    "Test",

                    // explicit migrations
                    new CreateTableOfTDtoMigration(context)
                );

                var upgraded = runner.Execute(context);
                Assert.IsTrue(upgraded);

                var helper = new DatabaseSchemaHelper(database, logger);
                var exists = helper.TableExist("umbracoNode");
                Assert.IsTrue(exists);

                scope.Complete();
            }
        }

        [Migration("1.0.0", 0, "Test")]
        public class CreateTableOfTDtoMigration : MigrationBase
        {
            public CreateTableOfTDtoMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Up()
            {
                Create.Table<NodeDto>();
            }

            public override void Down()
            {
                throw new WontImplementException();
            }
        }
    }
}
