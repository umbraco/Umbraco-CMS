// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
public class AdvancedMigrationTests : UmbracoIntegrationTest
{
    private IUmbracoVersion UmbracoVersion => GetRequiredService<IUmbracoVersion>();
    private IEventAggregator EventAggregator => GetRequiredService<IEventAggregator>();
    private IMigrationPlanExecutor MigrationPlanExecutor => GetRequiredService<IMigrationPlanExecutor>();

    [Test]
    public void CreateTableOfTDto()
    {
        var builder = Mock.Of<IMigrationBuilder>();
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

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            var upgrader = new Upgrader(
                new MigrationPlan("test")
                    .From(string.Empty)
                    .To<CreateTableOfTDtoMigration>("done"));

            upgrader.Execute(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());

            var db = ScopeAccessor.AmbientScope.Database;
            var exists = ScopeAccessor.AmbientScope.SqlContext.SqlSyntax.DoesTableExist(db, "umbracoUser");

            Assert.IsTrue(exists);
        }
    }

    [Test]
    public void DeleteKeysAndIndexesOfTDto()
    {
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
            var upgrader = new Upgrader(
                new MigrationPlan("test")
                    .From(string.Empty)
                    .To<CreateTableOfTDtoMigration>("a")
                    .To<DeleteKeysAndIndexesMigration>("done"));

            upgrader.Execute(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());
            scope.Complete();
        }
    }

    [Test]
    public void CreateKeysAndIndexesOfTDto()
    {
        if (BaseTestDatabase.IsSqlite())
        {
            // TODO: Think about this for future migrations.
            Assert.Ignore("Can't add / drop keys in SQLite.");
            return;
        }

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
            var upgrader = new Upgrader(
                new MigrationPlan("test")
                    .From(string.Empty)
                    .To<CreateTableOfTDtoMigration>("a")
                    .To<DeleteKeysAndIndexesMigration>("b")
                    .To<CreateKeysAndIndexesOfTDtoMigration>("done"));

            upgrader.Execute(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());
            scope.Complete();
        }
    }

    [Test]
    public void CreateKeysAndIndexes()
    {
        if (BaseTestDatabase.IsSqlite())
        {
            // TODO: Think about this for future migrations.
            Assert.Ignore("Can't add / drop keys in SQLite.");
            return;
        }

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
            var upgrader = new Upgrader(
                new MigrationPlan("test")
                    .From(string.Empty)
                    .To<CreateTableOfTDtoMigration>("a")
                    .To<DeleteKeysAndIndexesMigration>("b")
                    .To<CreateKeysAndIndexesMigration>("done"));

            upgrader.Execute(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());
            scope.Complete();
        }
    }

    [Test]
    public void AddColumn()
    {
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
                        return new AddColumnMigration(c);
                    default:
                        throw new NotSupportedException();
                }
            });

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            var upgrader = new Upgrader(
                new MigrationPlan("test")
                    .From(string.Empty)
                    .To<CreateTableOfTDtoMigration>("a")
                    .To<AddColumnMigration>("done"));

            upgrader.Execute(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());

            var db = ScopeAccessor.AmbientScope.Database;

            var columnInfo = ScopeAccessor.AmbientScope.SqlContext.SqlSyntax.GetColumnsInSchema(db)
                .Where(x => x.TableName == "umbracoUser")
                .FirstOrDefault(x => x.ColumnName == "Foo");

            Assert.Multiple(() =>
            {
                Assert.NotNull(columnInfo);
                Assert.IsTrue(columnInfo.DataType.Contains("nvarchar"));
            });
        }
    }

    public class CreateTableOfTDtoMigration : MigrationBase
    {
        public CreateTableOfTDtoMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate() =>

            // Create User table with keys, indexes, etc.
            Create.Table<UserDto>().Do();
    }

    public class DeleteKeysAndIndexesMigration : MigrationBase
    {
        public DeleteKeysAndIndexesMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate()
        {
            // drops User table keys and indexes
            // Execute.DropKeysAndIndexes("umbracoUser");

            // drops *all* tables keys and indexes
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToList();
            foreach (var table in tables)
            {
                Delete.KeysAndIndexes(table, false).Do();
            }

            foreach (var table in tables)
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

        protected override void Migrate() =>

            // Create User table keys and indexes.
            Create.KeysAndIndexes<UserDto>().Do();
    }

    public class CreateKeysAndIndexesMigration : MigrationBase
    {
        public CreateKeysAndIndexesMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate()
        {
            // Creates *all* tables keys and indexes
            foreach (var x in DatabaseSchemaCreator._orderedTables)
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

    public class AddColumnMigration : MigrationBase
    {
        public AddColumnMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate() =>
            Database.Execute($"ALTER TABLE {SqlSyntax.GetQuotedTableName("umbracoUser")} ADD Foo nvarchar(255)");
    }
}
