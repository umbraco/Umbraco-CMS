using System;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.Migrations.Stubs;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class AlterMigrationTests
    {
        [Test]
        public void Drop_Foreign_Key()
        {
            // Arrange
            var context = new MigrationContext(DatabaseProviders.SqlServerCE, null, Mock.Of<ILogger>());
            var stub = new DropForeignKeyMigrationStub(new SqlCeSyntaxProvider(), Mock.Of<ILogger>());

            // Act
            stub.GetUpExpressions(context);

            // Assert
            Assert.That(context.Expressions.Count(), Is.EqualTo(1));
            Assert.That(context.Expressions.Single().ToString(), Is.EqualTo("ALTER TABLE [umbracoUser2app] DROP CONSTRAINT [FK_umbracoUser2app_umbracoUser_id]"));

        }

        [Test]
        public void CreateColumn()
        {
            var context = new MigrationContext(DatabaseProviders.SqlServerCE, null, Mock.Of<ILogger>());
            var migration = new CreateColumnMigration(new SqlCeSyntaxProvider(), Mock.Of<ILogger>());

            migration.GetUpExpressions(context);

            Assert.That(context.Expressions.Count, Is.EqualTo(1));
            Assert.That(context.Expressions.Single().ToString(), Is.EqualTo("ALTER TABLE [bar] ADD [foo] UniqueIdentifier NOT NULL"));
        }

        [Migration("1.0.0", 0, "Test")]
        public class CreateColumnMigration : MigrationBase
        {
            public CreateColumnMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger)
                : base(sqlSyntax, logger)
            { }

            public override void Up()
            {
                Alter.Table("bar").AddColumn("foo").AsGuid();
            }

            public override void Down()
            { }
        }

        [Test]
        public void AlterColumn()
        {
            var context = new MigrationContext(DatabaseProviders.SqlServerCE, null, Mock.Of<ILogger>());
            var migration = new AlterColumnMigration(new SqlCeSyntaxProvider(), Mock.Of<ILogger>());

            migration.GetUpExpressions(context);

            Assert.That(context.Expressions.Count, Is.EqualTo(1));
            Assert.That(context.Expressions.Single().ToString(), Is.EqualTo("ALTER TABLE [bar] ALTER COLUMN [foo] UniqueIdentifier NOT NULL"));
        }

        [Migration("1.0.0", 0, "Test")]
        public class AlterColumnMigration : MigrationBase
        {
            public AlterColumnMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger)
                : base(sqlSyntax, logger)
            { }

            public override void Up()
            {
                // bad/good syntax...
                //Alter.Column("foo").OnTable("bar").AsGuid().NotNullable();
                Alter.Table("bar").AlterColumn("foo").AsGuid().NotNullable();
            }

            public override void Down()
            { }
        }

        [NUnit.Framework.Ignore("this doesn't actually test anything")]
        [Test]
        public void Can_Get_Up_Migration_From_MigrationStub()
        {
            // Arrange
            var context = new MigrationContext(DatabaseProviders.SqlServerCE, null, Mock.Of<ILogger>());
            var stub = new AlterUserTableMigrationStub(new SqlCeSyntaxProvider(), Mock.Of<ILogger>());

            // Act
            stub.GetUpExpressions(context);

            // Assert
            Assert.That(context.Expressions.Any(), Is.True);

            //Console output
            Debug.Print("Number of expressions in context: {0}", context.Expressions.Count);
            Debug.Print("");
            foreach (var expression in context.Expressions)
            {
                Debug.Print(expression.ToString());
            }
        }
    }
}