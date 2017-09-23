using System;
using System.Text.RegularExpressions;
using Moq;
using NPoco;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

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

            DatabaseSpecificSetUp();
        }

        [Test]
        [NUnit.Framework.Ignore("remove in v8")] // fixme remove in v8
        public virtual void Can_Upgrade_From_470_To_600()
        {
            var configuredVersion = new SemVersion(4, 7, 0);
            var targetVersion = new SemVersion(6, 0, 0);
            var db = GetConfiguredDatabase();

            //Create db schema and data from old Total.sql file for Sql Ce
            var statements = GetDatabaseSpecificSqlScript();
            // replace block comments by whitespace
            statements = FindComments.Replace(statements, " ");
            // execute all non-empty statements
            foreach (var statement in statements.Split(";".ToCharArray()))
            {
                var rawStatement = statement.Replace("GO", "").Trim();
                if (rawStatement.Length > 0)
                    db.Execute(new Sql(rawStatement));
            }

            var logger = Mock.Of<ILogger>();
            var context = new MigrationContext(db, logger);

            //Setup the MigrationRunner
            var migrationRunner = new MigrationRunner(
                Mock.Of<IMigrationCollectionBuilder>(),
                Mock.Of<IMigrationEntryService>(),
                logger,
                configuredVersion,
                targetVersion,
                Constants.System.UmbracoMigrationName
                //pass in explicit migrations
                // fixme - all gone in v8 - migrations need to be refactored anyways
                //new Core.Persistence.Migrations.Upgrades.TargetVersionFourNineZero.RemoveUmbracoAppConstraints(context),
                //new DeleteAppTables(context),
                //new EnsureAppsTreesUpdated(context),
                //new MoveMasterContentTypeData(context),
                //new NewCmsContentType2ContentTypeTable(context),
                //new RemoveMasterContentTypeColumn(context),
                //new RenameCmsTabTable(context),
                //new RenameTabIdColumn(context),
                //new UpdateCmsContentTypeAllowedContentTypeTable(context),
                //new UpdateCmsContentTypeTable(context),
                //new UpdateCmsContentVersionTable(context),
                //new UpdateCmsPropertyTypeGroupTable(context)
                );

            var upgraded = migrationRunner.Execute(context /*, true*/);

            Assert.That(upgraded, Is.True);

            var schemaHelper = new DatabaseSchemaHelper(db, logger);

            var hasTabTable = schemaHelper.TableExist("cmsTab");
            var hasPropertyTypeGroupTable = schemaHelper.TableExist("cmsPropertyTypeGroup");
            var hasAppTreeTable = schemaHelper.TableExist("umbracoAppTree");

            Assert.That(hasTabTable, Is.False);
            Assert.That(hasPropertyTypeGroupTable, Is.True);
            Assert.That(hasAppTreeTable, Is.False);
        }

        [TearDown]
        public virtual void TearDown()
        {
            TestHelper.CleanContentDirectories();

            Path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            DatabaseSpecificTearDown();
        }

        public string Path { get; set; }
        public abstract void DatabaseSpecificSetUp();
        public abstract void DatabaseSpecificTearDown();
        public abstract IUmbracoDatabase GetConfiguredDatabase();
        public abstract string GetDatabaseSpecificSqlScript();
    }
}
