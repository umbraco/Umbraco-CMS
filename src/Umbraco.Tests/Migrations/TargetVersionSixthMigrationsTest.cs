using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text.RegularExpressions;
using Moq;
using NUnit.Framework;
using Semver;
using SQLCE4Umbraco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Tests.Migrations
{
    [DatabaseTestBehavior(DatabaseBehavior.EmptyDbFilePerTest)]
    [TestFixture]
    public class TargetVersionSixthMigrationsTest : BaseDatabaseFactoryTest
    {
        /// <summary>Regular expression that finds multiline block comments.</summary>
        private static readonly Regex FindComments = new Regex(@"\/\*.*?\*\/", RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// sets up resolvers before resolution is frozen
        /// </summary>
        protected override void FreezeResolution()
        {
            MigrationResolver.Current = new MigrationResolver(
                ProfilingLogger.Logger,
                () => new List<Type>
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

            base.FreezeResolution();
        }

        [Test]
        public void Can_Find_Targetted_Migrations()
        {
            var db = GetConfiguredDatabase();

            //Create db schema and data from old Total.sql file for Sql Ce
            string statements = GetDatabaseSpecificSqlScript();
            // replace block comments by whitespace
            statements = FindComments.Replace(statements, " ");
            // execute all non-empty statements
            foreach (string statement in statements.Split(";".ToCharArray()))
            {
                string rawStatement = statement.Replace("GO", "").Trim();
                if (rawStatement.Length > 0)
                    db.Execute(new Sql(rawStatement));
            }

            var configuredVersion = new SemVersion(4, 8, 0);
            var targetVersion = new SemVersion(6, 0, 0);
            var foundMigrations = MigrationResolver.Current.Migrations;

            var migrationRunner = new MigrationRunner(
                Mock.Of<IMigrationEntryService>(),
                Logger, configuredVersion, targetVersion, GlobalSettings.UmbracoMigrationName);

            var migrations = migrationRunner.OrderedUpgradeMigrations(foundMigrations).ToList();

            var context = new MigrationContext(DatabaseProviders.SqlServerCE, db, Logger);
            foreach (var migration1 in migrations)
            {
                var migration = (MigrationBase) migration1;
                migration.GetUpExpressions(context);
            }

            foreach (var expression in context.Expressions)
            {
                Console.WriteLine(expression.ToString());
            }

            Assert.That(migrations.Count(), Is.EqualTo(12));
        }

        public string Path { get; set; }

        public UmbracoDatabase GetConfiguredDatabase()
        {
            return new UmbracoDatabase("Datasource=|DataDirectory|UmbracoPetaPocoTests.sdf;Flush Interval=1;", "System.Data.SqlServerCe.4.0", Mock.Of<ILogger>());
        }


        public string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.SqlCeTotal_480;
        } 
    }
}