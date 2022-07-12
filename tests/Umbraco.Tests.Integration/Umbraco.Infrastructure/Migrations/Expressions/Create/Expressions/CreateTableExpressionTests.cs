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
    public class CreateTableExpressionTests : UmbracoIntegrationTest
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

            Assert.AreEqual("foo", column.TableName);
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
                Assert.AreEqual("foo", constraint.Item1);
                Assert.AreEqual("bar", constraint.Item2);
                Assert.True(constraint.Item3.StartsWith("PK_"));
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
                Assert.AreEqual("foo", index.Item1);
                Assert.AreEqual("MY_SUPER_COOL_INDEX", index.Item2);
                Assert.AreEqual("bar", index.Item3);
                Assert.True(index.Item4);
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
                Assert.AreEqual("baz", constraint.Item1);
                Assert.AreEqual("qux", constraint.Item2);
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
