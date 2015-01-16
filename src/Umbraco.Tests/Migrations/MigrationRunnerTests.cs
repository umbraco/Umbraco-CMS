using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class MigrationRunnerTests
    {
        [Test]
        public void Executes_Only_One_Migration_For_Spanning_Multiple_Targets()
        {
            var runner = new MigrationRunner(Mock.Of<ILogger>(), new Version(4, 0, 0), new Version(6, 0, 0), "Test");

            var migrations = runner.OrderedUpgradeMigrations(new List<IMigration> { new MultiMigration(new SqlCeSyntaxProvider(), Mock.Of<ILogger>()) });

            var ctx = runner.InitializeMigrations(
                //new List<IMigration> {new DoRunMigration(), new DoNotRunMigration()},
                migrations.ToList(),
                new Database("umbracoDbDSN")
                , DatabaseProviders.SqlServerCE, true);

            Assert.AreEqual(1, ctx.Expressions.Count());
        }

        [Test]
        public void Executes_Migration_For_Spanning_One_Target_1()
        {
            var runner = new MigrationRunner(Mock.Of<ILogger>(), new Version(4, 0, 0), new Version(5, 0, 0), "Test");

            var migrations = runner.OrderedUpgradeMigrations(new List<IMigration> { new MultiMigration(new SqlCeSyntaxProvider(), Mock.Of<ILogger>()) });

            var ctx = runner.InitializeMigrations(
                //new List<IMigration> {new DoRunMigration(), new DoNotRunMigration()},
                migrations.ToList(),
                new Database("umbracoDbDSN")
                , DatabaseProviders.SqlServerCE, true);

            Assert.AreEqual(1, ctx.Expressions.Count());
        }

        [Test]
        public void Executes_Migration_For_Spanning_One_Target_2()
        {
            var runner = new MigrationRunner(Mock.Of<ILogger>(), new Version(5, 0, 1), new Version(6, 0, 0), "Test");

            var migrations = runner.OrderedUpgradeMigrations(new List<IMigration> { new MultiMigration(new SqlCeSyntaxProvider(), Mock.Of<ILogger>()) });

            var ctx = runner.InitializeMigrations(
                //new List<IMigration> {new DoRunMigration(), new DoNotRunMigration()},
                migrations.ToList(),
                new Database("umbracoDbDSN")
                , DatabaseProviders.SqlServerCE, true);

            Assert.AreEqual(1, ctx.Expressions.Count());
        }

        [Migration("6.0.0", 1, "Test")]
        [Migration("5.0.0", 1, "Test")]
        private class MultiMigration : MigrationBase
        {
            public MultiMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
            {
            }

            public override void Up()
            {
                Context.Expressions.Add(new AlterColumnExpression(SqlSyntax));
            }

            public override void Down()
            {
                Context.Expressions.Add(new AlterColumnExpression(SqlSyntax));
            }
        }
    }
}