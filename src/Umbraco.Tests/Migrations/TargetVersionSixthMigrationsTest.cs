using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class TargetVersionSixthMigrationsTest
    {
        [SetUp]
        public void Initialize()
        {
            TestHelper.SetupLog4NetForTests();

            //this ensures its reset
            PluginManager.Current = new PluginManager(false);

            //for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
            PluginManager.Current.AssembliesToScan = new[]
                                                         {
                                                             typeof (MigrationRunner).Assembly
                                                         };

            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntax.Provider;
        }

        [Test]
        public void Can_Find_Migrations_In_Current_Assembly()
        {
            var foundTypes = PluginManager.Current.ResolveMigrationTypes();

            Assert.That(foundTypes.Any(), Is.True);
            Assert.That(foundTypes.Count(), Is.GreaterThanOrEqualTo(11));
        }

        [Test]
        public void Can_Find_Targetted_Migrations()
        {
            var configuredVersion = new Version("4.11.0");
            var targetVersion = new Version("6.0.0");
            var foundMigrations = PluginManager.Current.FindMigrations();

            var migrationRunner = new MigrationRunner(configuredVersion, targetVersion);
            var migrations = migrationRunner.OrderedUpgradeMigrations(foundMigrations);

            var context = new MigrationContext();
            foreach (MigrationBase migration in migrations)
            {
                migration.GetUpExpressions(context);
            }

            foreach (var expression in context.Expressions)
            {
                Console.WriteLine(expression.ToString());
            }

            Assert.That(migrations.Count(), Is.EqualTo(11));
        }

        [TearDown]
        public void TearDown()
        {
            PluginManager.Current = null;
        }
    }
}