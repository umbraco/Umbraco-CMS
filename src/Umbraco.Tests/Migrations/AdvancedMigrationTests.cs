using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
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

            var builder = Mock.Of<IMigrationCollectionBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Instanciate(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    if (t != typeof(CreateTableOfTDtoMigration))
                        throw new NotSupportedException();
                    return new CreateTableOfTDtoMigration(c);
                });

            using (var scope = ScopeProvider.CreateScope())
            {
                var runner = new MigrationRunner(
                    ScopeProvider,
                    builder,
                    Mock.Of<IMigrationEntryService>(),
                    logger,
                    new SemVersion(0), // 0.0.0
                    new SemVersion(1), // 1.0.0
                    "Test",

                    // explicit migrations
                    typeof(CreateTableOfTDtoMigration)
                );

                var upgraded = runner.Execute();
                Assert.IsTrue(upgraded);

                var helper = new DatabaseSchemaCreator(scope.Database, logger);
                var exists = helper.TableExists("umbracoNode");
                Assert.IsTrue(exists);

                scope.Complete();
            }
        }

        [Test]
        public void DeleteKeysAndIndexesOfTDto()
        {
            var logger = new DebugDiagnosticsLogger();

            var builder = Mock.Of<IMigrationCollectionBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Instanciate(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "CreateTableOfTDtoMigration":
                            return new CreateTableOfTDtoMigration(c);
                        case "DeleteKeysAndIndexesMigration":
                            return new DeleteKeysAndIndexesMigration(c);
                        default:
                            throw new NotSupportedException();
                    }
                });

            using (var scope = ScopeProvider.CreateScope())
            {
                var runner = new MigrationRunner(
                    ScopeProvider,
                    builder,
                    Mock.Of<IMigrationEntryService>(),
                    logger,
                    new SemVersion(0), // 0.0.0
                    new SemVersion(1), // 1.0.0
                    "Test",

                    // explicit migrations
                    typeof(CreateTableOfTDtoMigration),
                    typeof(DeleteKeysAndIndexesMigration)
                );

                var upgraded = runner.Execute();
                Assert.IsTrue(upgraded);

                scope.Complete();
            }
        }

        [Test]
        public void CreateKeysAndIndexesOfTDto()
        {
            var logger = new DebugDiagnosticsLogger();

            var builder = Mock.Of<IMigrationCollectionBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Instanciate(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "CreateTableOfTDtoMigration":
                            return new CreateTableOfTDtoMigration(c);
                        case "DeleteKeysAndIndexesMigration":
                            return new DeleteKeysAndIndexesMigration(c);
                        case "CreateKeysAndIndexesOfTDtoMigration":
                            return new CreateKeysAndIndexesOfTDtoMigration(c);
                        default:
                            throw new NotSupportedException();
                    }
                });

            using (var scope = ScopeProvider.CreateScope())
            {
                var runner = new MigrationRunner(
                    ScopeProvider,
                    builder,
                    Mock.Of<IMigrationEntryService>(),
                    logger,
                    new SemVersion(0), // 0.0.0
                    new SemVersion(1), // 1.0.0
                    "Test",

                    // explicit migrations
                    typeof(CreateTableOfTDtoMigration),
                    typeof(DeleteKeysAndIndexesMigration),
                    typeof(CreateKeysAndIndexesOfTDtoMigration)
                );

                var upgraded = runner.Execute();
                Assert.IsTrue(upgraded);

                scope.Complete();
            }
        }

        [Test]
        public void CreateKeysAndIndexes()
        {
            var logger = new DebugDiagnosticsLogger();

            var builder = Mock.Of<IMigrationCollectionBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Instanciate(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "CreateTableOfTDtoMigration":
                            return new CreateTableOfTDtoMigration(c);
                        case "DeleteKeysAndIndexesMigration":
                            return new DeleteKeysAndIndexesMigration(c);
                        case "CreateKeysAndIndexesMigration":
                            return new CreateKeysAndIndexesMigration(c);
                        default:
                            throw new NotSupportedException();
                    }
                });

            using (var scope = ScopeProvider.CreateScope())
            {
                var runner = new MigrationRunner(
                    ScopeProvider,
                    builder,
                    Mock.Of<IMigrationEntryService>(),
                    logger,
                    new SemVersion(0), // 0.0.0
                    new SemVersion(1), // 1.0.0
                    "Test",

                    // explicit migrations
                    typeof(CreateTableOfTDtoMigration),
                    typeof(DeleteKeysAndIndexesMigration),
                    typeof(CreateKeysAndIndexesMigration)
                );

                var upgraded = runner.Execute();
                Assert.IsTrue(upgraded);

                scope.Complete();
            }
        }

        [Test]
        public void CreateColumn()
        {
            var logger = new DebugDiagnosticsLogger();

            var builder = Mock.Of<IMigrationCollectionBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Instanciate(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "CreateTableOfTDtoMigration":
                            return new CreateTableOfTDtoMigration(c);
                        case "CreateColumnMigration":
                            return new CreateColumnMigration(c);
                        default:
                            throw new NotSupportedException();
                    }
                });

            using (var scope = ScopeProvider.CreateScope())
            {
                var runner = new MigrationRunner(
                    ScopeProvider,
                    builder,
                    Mock.Of<IMigrationEntryService>(),
                    logger,
                    new SemVersion(0), // 0.0.0
                    new SemVersion(1), // 1.0.0
                    "Test",

                    // explicit migrations
                    typeof(CreateTableOfTDtoMigration),
                    typeof(CreateColumnMigration)
                );

                var upgraded = runner.Execute();
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
                Create.Table<NodeDto>().Do();
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
                Delete.KeysAndIndexes().Do();
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
                Create.KeysAndIndexes<NodeDto>().Do();
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

                    Create.KeysAndIndexes(x.Value).Do();
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
                Delete.KeysAndIndexes().Do();

                Delete.Column("id").FromTable("umbracoNode").Do();

                var table = DefinitionFactory.GetTableDefinition(typeof(NodeDto), SqlSyntax);
                var column = table.Columns.First(x => x.Name == "id");
                var create = SqlSyntax.Format(column); // returns [id] INTEGER NOT NULL IDENTITY(1060,1)
                Database.Execute($"ALTER TABLE {SqlSyntax.GetQuotedTableName("umbracoNode")} ADD COLUMN " + create);
            }
        }
    }
}
