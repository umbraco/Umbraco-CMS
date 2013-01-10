using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
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

			MigrationResolver.Current = new MigrationResolver(new List<Type>
				{
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionFourNineZero.RemoveUmbracoAppConstraints),
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionSixth.DeleteAppTables),
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionSixth.EnsureAppsTreesUpdated),
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionSixth.MoveMasterContentTypeData),
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionSixth.NewCmsContentType2ContentTypeTable),
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionSixth.RemoveMasterContentTypeColumn),
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionSixth.RenameCmsTabTable),
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionSixth.RenameTabIdColumn),
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionSixth.UpdateCmsContentTypeAllowedContentTypeTable),
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionSixth.UpdateCmsContentTypeTable),
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionSixth.UpdateCmsContentVersionTable),
					typeof (Core.Persistence.Migrations.Upgrades.TargetVersionSixth.UpdateCmsPropertyTypeGroupTable)
				});

			Resolution.Freeze();

            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntax.Provider;
        }
		
        [Test]
        public void Can_Find_Targetted_Migrations()
        {
            var configuredVersion = new Version("4.11.0");
            var targetVersion = new Version("6.0.0");
	        var foundMigrations = MigrationResolver.Current.Migrations;

            var migrationRunner = new MigrationRunner(configuredVersion, targetVersion, GlobalSettings.UmbracoMigrationName);
            var migrations = migrationRunner.OrderedUpgradeMigrations(foundMigrations);

            var context = new MigrationContext(DatabaseProviders.SqlServerCE);
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
            MigrationResolver.Reset();
			Resolution.IsFrozen = false;
        }
    }
}