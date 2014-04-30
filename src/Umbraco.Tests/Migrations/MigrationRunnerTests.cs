using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class MigrationRunnerTests
    {
        [Test]
        public void Executes_Only_One_Migration_For_Spanning_Multiple_Targets()
        {
            var runner = new MigrationRunner(new Version(4, 0, 0), new Version(6, 0, 0), "Test");

            var migrations = runner.OrderedUpgradeMigrations(new List<IMigration> { new MultiMigration() });

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
            var runner = new MigrationRunner(new Version(4, 0, 0), new Version(5, 0, 0), "Test");

            var migrations = runner.OrderedUpgradeMigrations(new List<IMigration> { new MultiMigration() });

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
            var runner = new MigrationRunner(new Version(5, 0, 1), new Version(6, 0, 0), "Test");

            var migrations = runner.OrderedUpgradeMigrations(new List<IMigration> { new MultiMigration() });

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
            public override void Up()
            {
                Context.Expressions.Add(new AlterColumnExpression());
            }

            public override void Down()
            {
                Context.Expressions.Add(new AlterColumnExpression());
            }
        }
    }
}