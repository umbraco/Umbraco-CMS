using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class FindingMigrationsTest
    {
        [SetUp]
        public void Initialize()
        {
            MigrationResolver.Current = new MigrationResolver(
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

            //This is needed because the Migration resolver is creating migratoni instances with their full ctors
            ApplicationContext.EnsureContext(
                new ApplicationContext(
                    new DatabaseContext(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), "test"),
                    new ServiceContext(), 
                    CacheHelper.CreateDisabledCacheHelper(),
                    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())),  
                true);

            //This is needed because the Migration resolver is creating the migration instances with their full ctors
            LoggerResolver.Current = new LoggerResolver(Mock.Of<ILogger>())
            {
                CanResolveBeforeFrozen = true
            };
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

			Resolution.Freeze();

            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();
        }

        [Test]
        public void Can_Find_Migrations_With_Target_Version_Six()
        {
	        var foundMigrations = MigrationResolver.Current.Migrations;
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

            var context = new MigrationContext(DatabaseProviders.SqlServerCE, null, Mock.Of<ILogger>());
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

        [TearDown]
        public void TearDown()
        {	        
            LoggerResolver.Reset();
            MigrationResolver.Reset();
        }
    }
}