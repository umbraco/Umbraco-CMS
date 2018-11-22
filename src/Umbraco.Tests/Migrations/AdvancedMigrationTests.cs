using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;
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

            var builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    if (t != typeof(CreateTableOfTDtoMigration))
                        throw new NotSupportedException();
                    return new CreateTableOfTDtoMigration(c);
                });

            using (var scope = ScopeProvider.CreateScope())
            {
                var upgrader = new MigrationTests.TestUpgrader(ScopeProvider, builder, Mock.Of<IKeyValueService>(), new PostMigrationCollection(Enumerable.Empty<IPostMigration>()), logger,
                    new MigrationPlan("test",  builder, logger)
                        .Add<CreateTableOfTDtoMigration>(string.Empty, "done"));

                upgrader.Execute();

                var helper = new DatabaseSchemaCreator(scope.Database, logger);
                var exists = helper.TableExists("umbracoUser");
                Assert.IsTrue(exists);

                scope.Complete();
            }
        }

        [Test]
        public void DeleteKeysAndIndexesOfTDto()
        {
            var logger = new DebugDiagnosticsLogger();

            var builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
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
                var upgrader = new MigrationTests.TestUpgrader(ScopeProvider, builder, Mock.Of<IKeyValueService>(), new PostMigrationCollection(Enumerable.Empty<IPostMigration>()), logger,
                    new MigrationPlan("test", builder, logger)
                        .Add<CreateTableOfTDtoMigration>(string.Empty, "a")
                        .Add<DeleteKeysAndIndexesMigration>("a", "done"));

                upgrader.Execute();
                scope.Complete();
            }
        }

        [Test]
        public void CreateKeysAndIndexesOfTDto()
        {
            var logger = new DebugDiagnosticsLogger();

            var builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
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
                var upgrader = new MigrationTests.TestUpgrader(ScopeProvider, builder, Mock.Of<IKeyValueService>(), new PostMigrationCollection(Enumerable.Empty<IPostMigration>()), logger,
                    new MigrationPlan("test",  builder, logger)
                        .Add<CreateTableOfTDtoMigration>(string.Empty, "a")
                        .Add<DeleteKeysAndIndexesMigration>("a", "b")
                        .Add<CreateKeysAndIndexesOfTDtoMigration>("b", "done"));

                upgrader.Execute();
                scope.Complete();
            }
        }

        [Test]
        public void CreateKeysAndIndexes()
        {
            var logger = new DebugDiagnosticsLogger();

            var builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
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
                var upgrader = new MigrationTests.TestUpgrader(ScopeProvider, builder, Mock.Of<IKeyValueService>(), new PostMigrationCollection(Enumerable.Empty<IPostMigration>()), logger,
                    new MigrationPlan("test",  builder, logger)
                        .Add<CreateTableOfTDtoMigration>(string.Empty, "a")
                        .Add<DeleteKeysAndIndexesMigration>("a", "b")
                        .Add<CreateKeysAndIndexesMigration>("b", "done"));

                upgrader.Execute();
                scope.Complete();
            }
        }

        [Test]
        public void CreateColumn()
        {
            var logger = new DebugDiagnosticsLogger();

            var builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
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
                var upgrader = new MigrationTests.TestUpgrader(ScopeProvider, builder, Mock.Of<IKeyValueService>(), new PostMigrationCollection(Enumerable.Empty<IPostMigration>()), logger,
                    new MigrationPlan("test", builder, logger)
                        .Add<CreateTableOfTDtoMigration>(string.Empty, "a")
                        .Add<CreateColumnMigration>("a", "done"));

                upgrader.Execute();
                scope.Complete();
            }
        }

        public class CreateTableOfTDtoMigration : MigrationBase
        {
            public CreateTableOfTDtoMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // creates User table with keys, indexes, etc
                Create.Table<UserDto>().Do();
            }
        }

        public class DeleteKeysAndIndexesMigration : MigrationBase
        {
            public DeleteKeysAndIndexesMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // drops User table keys and indexes
                //Execute.DropKeysAndIndexes("umbracoUser");

                // drops *all* tables keys and indexes
                Delete.KeysAndIndexes().Do();
            }
        }

        public class CreateKeysAndIndexesOfTDtoMigration : MigrationBase
        {
            public CreateKeysAndIndexesOfTDtoMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // creates Node table keys and indexes
                Create.KeysAndIndexes<UserDto>().Do();
            }
        }

        public class CreateKeysAndIndexesMigration : MigrationBase
        {
            public CreateKeysAndIndexesMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // creates *all* tables keys and indexes
                foreach (var x in DatabaseSchemaCreator.OrderedTables)
                {
                    // ok - for tests, restrict to Node
                    if (x != typeof(UserDto)) continue;

                    Create.KeysAndIndexes(x).Do();
                }
            }
        }

        public class CreateColumnMigration : MigrationBase
        {
            public CreateColumnMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // cannot delete the column without this, of course
                Delete.KeysAndIndexes().Do();

                Delete.Column("id").FromTable("umbracoUser").Do();

                var table = DefinitionFactory.GetTableDefinition(typeof(UserDto), SqlSyntax);
                var column = table.Columns.First(x => x.Name == "id");
                var create = SqlSyntax.Format(column); // returns [id] INTEGER NOT NULL IDENTITY(1060,1)
                Database.Execute($"ALTER TABLE {SqlSyntax.GetQuotedTableName("umbracoUser")} ADD " + create);
            }
        }
    }
}
