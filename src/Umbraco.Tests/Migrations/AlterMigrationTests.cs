using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.Migrations.Stubs;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class AlterMigrationTests
    {
        [SetUp]
        public void SetUp()
        {
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;
        }

        [Test]
        public void Can_Get_Up_Migration_From_MigrationStub()
        {
            // Arrange
            var context = new MigrationContext(DatabaseProviders.SqlServerCE, null);
            var stub = new AlterUserTableMigrationStub();

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