using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.Migrations.Stubs;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class FindingMigrationsTest
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
                                                             typeof (FourNineMigration).Assembly
                                                         };

            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntax.Provider;
        }

        [Test]
        public void Can_Find_Migrations_In_Current_Assembly()
        {
            var foundTypes = PluginManager.Current.ResolveMigrationTypes();

            Assert.That(foundTypes.Any(), Is.True);
            Assert.That(foundTypes.Count(), Is.EqualTo(4));
        }

        [Test]
        public void Can_Find_Migrations_With_Targtet_Version_Six()
        {
            var foundMigrations = PluginManager.Current.FindMigrations();
            var targetVersion = new Version("6.0.0");
            var list = new List<IMigration>();

            foreach (var migration in foundMigrations)
            {
                var migrationAttribute = migration.GetType().FirstAttribute<MigrationAttribute>();
                if (migrationAttribute != null)
                {
                    if (migrationAttribute.TargetVersion == targetVersion)
                    {
                        list.Add(migration);
                    }
                }
            }

            Assert.That(list.Count, Is.EqualTo(3));

            var context = new MigrationContext(DatabaseProviders.SqlServerCE);
            foreach (MigrationBase migration in list)
            {
                migration.GetUpExpressions(context);
            }

            Assert.That(context.Expressions.Any(), Is.True);

            //Console output
            foreach (var expression in context.Expressions)
            {
                Console.WriteLine(expression.ToString());
            }
        }

        [TearDown]
        public void TearDown()
        {
            PluginManager.Current = null;
        }
    }
}