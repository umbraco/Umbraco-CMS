using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text.RegularExpressions;
using Moq;
using NUnit.Framework;
using SQLCE4Umbraco;
using Umbraco.Core;
using Umbraco.Core.Logging;
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
    public class TargetVersionSixthMigrationsTest : BaseDatabaseFactoryTest
    {
        /// <summary>Regular expression that finds multiline block comments.</summary>
        private static readonly Regex FindComments = new Regex(@"\/\*.*?\*\/", RegexOptions.Singleline | RegexOptions.Compiled);

        [SetUp]
        public override void Initialize()
        {
            TestHelper.InitializeContentDirectories();

            Path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", Path);
           
			MigrationResolver.Current = new MigrationResolver(
                new ActivatorServiceProvider(), ProfilingLogger.Logger,
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

            //This is needed because the PluginManager is creating the migration instances with their default ctors
            LoggerResolver.Current = new LoggerResolver(Logger)
            {
                CanResolveBeforeFrozen = true
            };
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            Resolution.Freeze();

            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            var engine = new SqlCeEngine("Datasource=|DataDirectory|UmbracoPetaPocoTests.sdf;Flush Interval=1;");
            engine.CreateDatabase();   
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

            var configuredVersion = new Version("4.8.0");
            var targetVersion = new Version("6.0.0");
            var foundMigrations = MigrationResolver.Current.Migrations;

            var migrationRunner = new MigrationRunner(Logger, configuredVersion, targetVersion, GlobalSettings.UmbracoMigrationName);
            var migrations = migrationRunner.OrderedUpgradeMigrations(foundMigrations).ToList();

            var context = new MigrationContext(DatabaseProviders.SqlServerCE, db, Logger);
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
        public override void TearDown()
        {
            base.TearDown();

            PluginManager.Current = null;
            SqlSyntaxContext.SqlSyntaxProvider = null;
            MigrationResolver.Reset();

            TestHelper.CleanContentDirectories();

            Path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            SqlCeContextGuardian.CloseBackgroundConnection();
        }

        public string Path { get; set; }

        public UmbracoDatabase GetConfiguredDatabase()
        {
            return new UmbracoDatabase("Datasource=|DataDirectory|UmbracoPetaPocoTests.sdf;Flush Interval=1;", "System.Data.SqlServerCe.4.0", Mock.Of<ILogger>());
        }

        public DatabaseProviders GetDatabaseProvider()
        {
            return DatabaseProviders.SqlServerCE;
        }

        public string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.SqlCeTotal_480;
        } 
    }
}