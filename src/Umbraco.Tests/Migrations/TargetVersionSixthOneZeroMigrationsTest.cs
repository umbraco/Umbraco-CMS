//NOTE: SD: Not needed currently as there are no db migrations for 6.1 now that the server tables, etc... are not being shipped in 6.1

//using System;
//using System.Linq;
//using NUnit.Framework;
//using Umbraco.Core;
//using Umbraco.Core.Persistence;
//using Umbraco.Core.Persistence.Migrations;
//using Umbraco.Core.Persistence.SqlSyntax;
//using Umbraco.Tests.TestHelpers;
//using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

//namespace Umbraco.Tests.Migrations
//{
//    [TestFixture]
//    public class TargetVersionSixthOneZeroMigrationsTest : BaseDatabaseFactoryTest
//    {
//        [SetUp]
//        public override void Initialize()
//        {
//            PluginManager.Current = new PluginManager(false);

//            MigrationResolver.Current = new MigrationResolver(
//                () => PluginManager.Current.ResolveMigrationTypes());

//            base.Initialize();
//            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntax.Provider;
//        }

//        [Test]
//        public void Can_Find_Targetted_Migrations()
//        {
//            var configuredVersion = new Version("6.0.0");
//            var targetVersion = new Version("6.1.0");
//            var foundMigrations = MigrationResolver.Current.Migrations;

//            var migrationRunner = new MigrationRunner(configuredVersion, targetVersion, GlobalSettings.UmbracoMigrationName);
//            var migrations = migrationRunner.OrderedUpgradeMigrations(foundMigrations);

//            var context = new MigrationContext(DatabaseProviders.SqlServerCE, DatabaseContext.Database);
//            foreach (MigrationBase migration in migrations)
//            {
//                migration.GetUpExpressions(context);
//            }

//            foreach (var expression in context.Expressions)
//            {
//                Console.WriteLine(expression.ToString());
//            }

//            Assert.That(migrations.Count(), Is.EqualTo(1));
//        }

//        [TearDown]
//        public override void TearDown()
//        {
//            MigrationResolver.Reset();
//            PluginManager.Current = null;
//        }
//    }
//}