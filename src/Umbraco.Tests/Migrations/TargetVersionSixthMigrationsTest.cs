using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class TargetVersionSixthMigrationsTest
    {
        [SetUp]
        public void Initialize()
        {
            TestHelper.SetupLog4NetForTests();

			MigrationResolver.Current = new MigrationResolver(() => new List<Type>
				{
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionFourNineZero.RemoveUmbracoAppConstraints),
					typeof (DeleteAppTables),
					typeof (EnsureAppsTreesUpdated),
					typeof (MoveMasterContentTypeData),
					typeof (NewCmsContentType2ContentTypeTable),
					typeof (RemoveMasterContentTypeColumn),
					typeof (RenameCmsTabTable),
					typeof (RenameTabIdColumn),
					typeof (UpdateCmsContentTypeAllowedContentTypeTable),
					typeof (UpdateCmsContentTypeTable),
					typeof (UpdateCmsContentVersionTable),
					typeof (UpdateCmsPropertyTypeGroupTable)
				}.OrderByDescending(x => x.Name));

			Resolution.Freeze();

            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;
        }
		
        [Test]
        public void Can_Find_Targetted_Migrations()
        {
            var configuredVersion = new Version("4.8.0");
            var targetVersion = new Version("6.0.0");
	        var foundMigrations = MigrationResolver.Current.Migrations;

            var migrationRunner = new MigrationRunner(configuredVersion, targetVersion, GlobalSettings.UmbracoMigrationName);
            var migrations = migrationRunner.OrderedUpgradeMigrations(foundMigrations).ToList();

            var context = new MigrationContext(DatabaseProviders.SqlServerCE, null);
            foreach (MigrationBase migration in migrations)
            {
                migration.GetUpExpressions(context);
            }

            foreach (var expression in context.Expressions)
            {
                Console.WriteLine(expression.ToString());
            }

            Assert.That(migrations.Count(), Is.EqualTo(12));
        }

        [TearDown]
        public void TearDown()
        {
            MigrationResolver.Reset();
        }
    }
}