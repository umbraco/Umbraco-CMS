using System.Linq;
using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Migrations.Syntax.Execute;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
    public class AdvancedMigrationTests : TestWithDatabaseBase
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

                var helper = new DatabaseSchemaCreator(database, logger);
                var exists = helper.TableExists("umbracoNode");
                Assert.IsTrue(exists);

                scope.Complete();
            }
        }

        [Test]
        public void DeleteKeysAndIndexesOfTDto()
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
                    new CreateTableOfTDtoMigration(context),
                    new DeleteKeysAndIndexesMigration(context)
                );

                var upgraded = runner.Execute(context);
                Assert.IsTrue(upgraded);

                scope.Complete();
            }
        }

        [Test]
        public void CreateKeysAndIndexesOfTDto()
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
                    new CreateTableOfTDtoMigration(context),
                    new DeleteKeysAndIndexesMigration(context),
                    new CreateKeysAndIndexesOfTDtoMigration(context)
                );

                var upgraded = runner.Execute(context);
                Assert.IsTrue(upgraded);

                scope.Complete();
            }
        }

        [Test]
        public void CreateKeysAndIndexes()
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
                    new CreateTableOfTDtoMigration(context),
                    new DeleteKeysAndIndexesMigration(context),
                    new CreateKeysAndIndexesMigration(context)
                );

                var upgraded = runner.Execute(context);
                Assert.IsTrue(upgraded);

                scope.Complete();
            }
        }

        [Test]
        public void CreateColumn()
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
                    new CreateTableOfTDtoMigration(context),
                    new CreateColumnMigration(context)
                );

                var upgraded = runner.Execute(context);
                Assert.IsTrue(upgraded);

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
                // creates Node table with keys, indexes, etc
                Create.Table<NodeDto>();
            }
        }

        [Migration("1.0.0", 1, "Test")]
        public class DeleteKeysAndIndexesMigration : MigrationBase
        {
            public DeleteKeysAndIndexesMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Up()
            {
                // drops Node table keys and indexes
                //Execute.DropKeysAndIndexes("umbracoNode");

                // drops *all* tables keys and indexes
                Execute.DropKeysAndIndexes();
            }
        }

        [Migration("1.0.0", 2, "Test")]
        public class CreateKeysAndIndexesOfTDtoMigration : MigrationBase
        {
            public CreateKeysAndIndexesOfTDtoMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Up()
            {
                // creates Node table keys and indexes
                Create.KeysAndIndexes<NodeDto>();
            }
        }

        [Migration("1.0.0", 3, "Test")]
        public class CreateKeysAndIndexesMigration : MigrationBase
        {
            public CreateKeysAndIndexesMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Up()
            {
                // creates *all* tables keys and indexes
                foreach (var x in DatabaseSchemaCreator.OrderedTables)
                {
                    // ok - for tests, restrict to Node
                    if (x.Value != typeof(NodeDto)) continue;

                    Create.KeysAndIndexes(x.Value);
                }
            }
        }

        [Migration("1.0.0", 2, "Test")]
        public class CreateColumnMigration : MigrationBase
        {
            public CreateColumnMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Up()
            {
                // cannot delete the column without this, of course
                Execute.DropKeysAndIndexes();

                Delete.Column("id").FromTable("umbracoNode");

                var table = DefinitionFactory.GetTableDefinition(typeof(NodeDto), SqlSyntax);
                var column = table.Columns.First(x => x.Name == "id");
                var create = SqlSyntax.Format(column); // returns [id] INTEGER NOT NULL IDENTITY(1060,1)
                Execute.Sql($"ALTER TABLE {SqlSyntax.GetQuotedTableName("umbracoNode")} ADD COLUMN " + create);
            }
        }
    }
}
