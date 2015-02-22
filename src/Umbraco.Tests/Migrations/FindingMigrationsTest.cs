using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.LightInject;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.Migrations.Stubs;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class FindingMigrationsTest
    {

        private MigrationResolver _migrationResolver;

        [SetUp]
        public void Initialize()
        {
            var container = new ServiceContainer();
            container.Register<ILogger>(factory => Mock.Of<ILogger>(), new PerContainerLifetime());
            container.Register<ISqlSyntaxProvider>(factory => Mock.Of<ISqlSyntaxProvider>(), new PerContainerLifetime());

            _migrationResolver = new MigrationResolver(
                container, 
                Mock.Of<ILogger>(),
                () => new List<Type>
				{
					typeof (AlterUserTableMigrationStub),
					typeof(Dummy),
					typeof (SixZeroMigration1),					
					typeof (SixZeroMigration2),
					typeof (FourElevenMigration),
                    typeof (FiveZeroMigration)                   
				});

        }

        [Test]
        public void Can_Find_Migrations_With_Target_Version_Six()
        {
            var foundMigrations = _migrationResolver.Migrations;
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

            var context = new MigrationContext(
                DatabaseProviders.SqlServerCE,
                new Database("test", "System.Data.SqlClient"), 
                Mock.Of<ILogger>(), 
                new SqlCeSyntaxProvider());
            foreach (var migration1 in list)
            {
                var migration = (MigrationBase) migration1;
                migration.GetUpExpressions(context);
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