// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
    public class AdvancedMigrationTests : UmbracoIntegrationTest
    {
        private readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

        private IUmbracoVersion UmbracoVersion => GetRequiredService<IUmbracoVersion>();

        [Test]
        public void CreateTableOfTDto()
        {
            IMigrationBuilder builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    if (t != typeof(CreateTableOfTDtoMigration))
                    {
                        throw new NotSupportedException();
                    }

                    return new CreateTableOfTDtoMigration(c);
                });

            using (IScope scope = ScopeProvider.CreateScope())
            {
                var upgrader = new Upgrader(
                    new MigrationPlan("test")
                        .From(string.Empty)
                        .To<CreateTableOfTDtoMigration>("done"));

                upgrader.Execute(ScopeProvider, builder, Mock.Of<IKeyValueService>(), _loggerFactory.CreateLogger<Upgrader>(), _loggerFactory);

                var helper = new DatabaseSchemaCreator(scope.Database, LoggerFactory.CreateLogger<DatabaseSchemaCreator>(), LoggerFactory, UmbracoVersion);
                bool exists = helper.TableExists("umbracoUser");
                Assert.IsTrue(exists);

                scope.Complete();
            }
        }

        [Test]
        public void DeleteKeysAndIndexesOfTDto()
        {
            IMigrationBuilder builder = Mock.Of<IMigrationBuilder>();
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

            using (IScope scope = ScopeProvider.CreateScope())
            {
                var upgrader = new Upgrader(
                    new MigrationPlan("test")
                        .From(string.Empty)
                        .To<CreateTableOfTDtoMigration>("a")
                        .To<DeleteKeysAndIndexesMigration>("done"));

                upgrader.Execute(ScopeProvider, builder, Mock.Of<IKeyValueService>(), _loggerFactory.CreateLogger<Upgrader>(), _loggerFactory);
                scope.Complete();
            }
        }

        [Test]
        public void CreateKeysAndIndexesOfTDto()
        {
            IMigrationBuilder builder = Mock.Of<IMigrationBuilder>();
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

            using (IScope scope = ScopeProvider.CreateScope())
            {
                var upgrader = new Upgrader(
                    new MigrationPlan("test")
                        .From(string.Empty)
                        .To<CreateTableOfTDtoMigration>("a")
                        .To<DeleteKeysAndIndexesMigration>("b")
                        .To<CreateKeysAndIndexesOfTDtoMigration>("done"));

                upgrader.Execute(ScopeProvider, builder, Mock.Of<IKeyValueService>(), _loggerFactory.CreateLogger<Upgrader>(), _loggerFactory);
                scope.Complete();
            }
        }

        [Test]
        public void CreateKeysAndIndexes()
        {
            IMigrationBuilder builder = Mock.Of<IMigrationBuilder>();
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

            using (IScope scope = ScopeProvider.CreateScope())
            {
                var upgrader = new Upgrader(
                    new MigrationPlan("test")
                        .From(string.Empty)
                        .To<CreateTableOfTDtoMigration>("a")
                        .To<DeleteKeysAndIndexesMigration>("b")
                        .To<CreateKeysAndIndexesMigration>("done"));

                upgrader.Execute(ScopeProvider, builder, Mock.Of<IKeyValueService>(), _loggerFactory.CreateLogger<Upgrader>(), _loggerFactory);
                scope.Complete();
            }
        }

        [Test]
        public void CreateColumn()
        {
            IMigrationBuilder builder = Mock.Of<IMigrationBuilder>();
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

            using (IScope scope = ScopeProvider.CreateScope())
            {
                var upgrader = new Upgrader(
                    new MigrationPlan("test")
                        .From(string.Empty)
                        .To<CreateTableOfTDtoMigration>("a")
                        .To<CreateColumnMigration>("done"));

                upgrader.Execute(ScopeProvider, builder, Mock.Of<IKeyValueService>(), _loggerFactory.CreateLogger<Upgrader>(), _loggerFactory);
                scope.Complete();
            }
        }

        public class CreateTableOfTDtoMigration : MigrationBase
        {
            public CreateTableOfTDtoMigration(IMigrationContext context)
                : base(context)
            {
            }

            public override void Migrate() =>

                // Create User table with keys, indexes, etc.
                Create.Table<UserDto>().Do();
        }

        public class DeleteKeysAndIndexesMigration : MigrationBase
        {
            public DeleteKeysAndIndexesMigration(IMigrationContext context)
                : base(context)
            {
            }

            public override void Migrate()
            {
                // drops User table keys and indexes
                // Execute.DropKeysAndIndexes("umbracoUser");

                // drops *all* tables keys and indexes
                var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToList();
                foreach (string table in tables)
                {
                    Delete.KeysAndIndexes(table, false, true).Do();
                }

                foreach (string table in tables)
                {
                    Delete.KeysAndIndexes(table, true, false).Do();
                }
            }
        }

        public class CreateKeysAndIndexesOfTDtoMigration : MigrationBase
        {
            public CreateKeysAndIndexesOfTDtoMigration(IMigrationContext context)
                : base(context)
            {
            }

            public override void Migrate() =>

                // Create User table keys and indexes.
                Create.KeysAndIndexes<UserDto>().Do();
        }

        public class CreateKeysAndIndexesMigration : MigrationBase
        {
            public CreateKeysAndIndexesMigration(IMigrationContext context)
                : base(context)
            {
            }

            public override void Migrate()
            {
                // Creates *all* tables keys and indexes
                foreach (Type x in DatabaseSchemaCreator.OrderedTables)
                {
                    // ok - for tests, restrict to Node
                    if (x != typeof(UserDto))
                    {
                        continue;
                    }

                    Create.KeysAndIndexes(x).Do();
                }
            }
        }

        public class CreateColumnMigration : MigrationBase
        {
            public CreateColumnMigration(IMigrationContext context)
                : base(context)
            {
            }

            public override void Migrate()
            {
                // cannot delete the column without this, of course
                Delete.KeysAndIndexes("umbracoUser").Do();

                Delete.Column("id").FromTable("umbracoUser").Do();

                TableDefinition table = DefinitionFactory.GetTableDefinition(typeof(UserDto), SqlSyntax);
                ColumnDefinition column = table.Columns.First(x => x.Name == "id");
                string create = SqlSyntax.Format(column); // returns [id] INTEGER NOT NULL IDENTITY(1060,1)
                Database.Execute($"ALTER TABLE {SqlSyntax.GetQuotedTableName("umbracoUser")} ADD " + create);
            }
        }
    }
}
