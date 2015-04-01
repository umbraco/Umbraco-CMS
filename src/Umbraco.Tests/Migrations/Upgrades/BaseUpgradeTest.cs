using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Tests.Migrations.Upgrades
{
    [TestFixture]
    public abstract class BaseUpgradeTest
    {
        /// <summary>Regular expression that finds multiline block comments.</summary>
        private static readonly Regex FindComments = new Regex(@"\/\*.*?\*\/", RegexOptions.Singleline | RegexOptions.Compiled);

        [SetUp]
        public virtual void Initialize()
        {
            TestHelper.InitializeContentDirectories();

            Path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", Path);
           
			MigrationResolver.Current = new MigrationResolver(
                Mock.Of<ILogger>(),
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
				});

            LoggerResolver.Current = new LoggerResolver(Mock.Of<ILogger>())
            {
                CanResolveBeforeFrozen = true
            };

			Resolution.Freeze();

            DatabaseSpecificSetUp();

            SqlSyntaxContext.SqlSyntaxProvider = GetSyntaxProvider();
        }

        [Test]
        public virtual void Can_Upgrade_From_470_To_600()
        {
            var configuredVersion = new Version("4.7.0");
            var targetVersion = new Version("6.0.0");
            var provider = GetDatabaseProvider();
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

            //Setup the MigrationRunner
            var migrationRunner = new MigrationRunner(Mock.Of<ILogger>(), configuredVersion, targetVersion, GlobalSettings.UmbracoMigrationName);
            bool upgraded = migrationRunner.Execute(db, provider, true);

            Assert.That(upgraded, Is.True);

            bool hasTabTable = db.TableExist("cmsTab");
            bool hasPropertyTypeGroupTable = db.TableExist("cmsPropertyTypeGroup");
            bool hasAppTreeTable = db.TableExist("umbracoAppTree");

            Assert.That(hasTabTable, Is.False);
            Assert.That(hasPropertyTypeGroupTable, Is.True);
            Assert.That(hasAppTreeTable, Is.False);
        }

        [TearDown]
        public virtual void TearDown()
        {
            PluginManager.Current = null;
            SqlSyntaxContext.SqlSyntaxProvider = null;
			MigrationResolver.Reset();
            LoggerResolver.Reset();

            TestHelper.CleanContentDirectories();

            Path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            DatabaseSpecificTearDown();
        }

        public string Path { get; set; }
        public abstract void DatabaseSpecificSetUp();
        public abstract void DatabaseSpecificTearDown();
        public abstract ISqlSyntaxProvider GetSyntaxProvider();
        public abstract UmbracoDatabase GetConfiguredDatabase();
        public abstract DatabaseProviders GetDatabaseProvider();
        public abstract string GetDatabaseSpecificSqlScript();
    }
}