using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Expressions.Create.Expressions
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
    internal sealed class CreateTableExpressionTests : UmbracoIntegrationTest
    {
        [Test]
        public void Do_ForSimpleTableDefinition_TableIsCreated()
        {
            var factory = GetRequiredService<IUmbracoDatabaseFactory>();
            var database = factory.CreateDatabase();

            var builder = GetBuilder(database);
            builder.Table("foo")
                .WithColumn("id").AsInt32()
                .Do();

            var column = database.SqlContext.SqlSyntax.GetColumnsInSchema(database).Single();

            Assert.That(column.TableName, Is.EqualTo("foo"));
        }

        [Test]
        public void Do_ForDefinitionWithPrimaryKey_PrimaryKeyConstraintIsAdded()
        {
            var factory = GetRequiredService<IUmbracoDatabaseFactory>();
            var database = factory.CreateDatabase();

            var builder = GetBuilder(database);
            builder.Table("foo")
                .WithColumn("bar").AsInt32().PrimaryKey("PK_foo")
                .Do();

            // (TableName, ColumnName, ConstraintName)
            var constraint = database.SqlContext.SqlSyntax.GetConstraintsPerColumn(database).Single();

            Assert.Multiple(() =>
            {
                Assert.That(constraint.Item1, Is.EqualTo("foo"));
                Assert.That(constraint.Item2, Is.EqualTo("bar"));
                Assert.That(constraint.Item3, Does.StartWith("PK_"));
            });
        }

        [Test]
        public void Do_ForDefinitionWithIndex_IndexIsCreated()
        {
            var factory = GetRequiredService<IUmbracoDatabaseFactory>();
            var database = factory.CreateDatabase();

            var builder = GetBuilder(database);
            builder.Table("foo")
                .WithColumn("bar").AsString().Unique("MY_SUPER_COOL_INDEX")
                .Do();

            // (TableName, IndexName, ColumnName, IsUnique)
            var index = database.SqlContext.SqlSyntax.GetDefinedIndexes(database).Single();

            Assert.Multiple(() =>
            {
                Assert.That(index.Item1, Is.EqualTo("foo"));
                Assert.That(index.Item2, Is.EqualTo("MY_SUPER_COOL_INDEX"));
                Assert.That(index.Item3, Is.EqualTo("bar"));
                Assert.That(index.Item4, Is.True);
            });
        }


        [Test]
        public void Do_ForDefinitionWithForeignKeys_ForeignKeysAreCreated()
        {
            var factory = GetRequiredService<IUmbracoDatabaseFactory>();
            var database = factory.CreateDatabase();

            var builder = GetBuilder(database);
            builder.Table("foo")
                .WithColumn("bar").AsInt32().PrimaryKey("PK_foo")
                .Do();

            builder = GetBuilder(database);
            builder.Table("baz")
                .WithColumn("qux").AsInt32()
                .ForeignKey("MY_SUPER_COOL_FK", "foo", "bar")
                .Do();

            // (TableName, ColumnName, ConstraintName)
            var constraint = database.SqlContext.SqlSyntax
                .GetConstraintsPerColumn(database)
                .Single(x => x.Item3 == "MY_SUPER_COOL_FK");

            Assert.Multiple(() =>
            {
                Assert.That(constraint.Item1, Is.EqualTo("baz"));
                Assert.That(constraint.Item2, Is.EqualTo("qux"));
            });
        }

        private CreateBuilder GetBuilder(IUmbracoDatabase db)
        {
            var logger = GetRequiredService<ILogger<MigrationContext>>();

            var ctx = new MigrationContext(new TestPlan(), db, logger);

            return new CreateBuilder(ctx);
        }

        private class TestPlan : MigrationPlan
        {
            public TestPlan()
                : base("test-plan")
            {
            }
        }
    }
}
