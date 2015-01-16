using System;
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
            Console.WriteLine("Number of expressions in context: {0}", context.Expressions.Count);
            Console.WriteLine("");
            foreach (var expression in context.Expressions)
            {
                Console.WriteLine(expression.ToString());
            }
        }
    }
}