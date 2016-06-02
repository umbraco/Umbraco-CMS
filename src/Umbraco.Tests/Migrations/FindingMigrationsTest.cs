using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using Umbraco.Tests.Migrations.Stubs;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class FindingMigrationsTest : BaseDatabaseFactoryTest
    {

        [Test]
        public void Can_Find_Migrations_With_Target_Version_Six()
        {
            var migrationResolver = new MigrationResolver(
               Container,
               Logger,
               () => new List<Type>
               {
                    typeof (AlterUserTableMigrationStub),
                    typeof(Dummy),
                    typeof (SixZeroMigration1),
                    typeof (SixZeroMigration2),
                    typeof (FourElevenMigration),
                    typeof (FiveZeroMigration)
               });

            var database = TestObjects.GetUmbracoSqlServerDatabase(Mock.Of<ILogger>());

            var context = new MigrationContext(database, Logger);

            var foundMigrations = migrationResolver.GetMigrations(context);
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

            
            foreach (var migration1 in list)
            {
                var migration = (MigrationBase) migration1;
                migration.Up();
            }

            Assert.That(context.Expressions.Any(), Is.True);

            //Console output
            foreach (var expression in context.Expressions)
            {
                Console.WriteLine(expression.ToString());
            }
        }
    }
}