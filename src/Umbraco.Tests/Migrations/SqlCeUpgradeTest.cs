using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SQLCE4Umbraco;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class SqlCeUpgradeTest
    {
        /// <summary>Regular expression that finds multiline block comments.</summary>
        private static readonly Regex m_findComments = new Regex(@"\/\*.*?\*\/", RegexOptions.Singleline | RegexOptions.Compiled);

        [SetUp]
        public void Initialize()
        {
            TestHelper.SetupLog4NetForTests();
            TestHelper.InitializeContentDirectories();

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            UmbracoSettings.UseLegacyXmlSchema = false;

            //this ensures its reset
            PluginManager.Current = new PluginManager(false);

            //for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
            PluginManager.Current.AssembliesToScan = new[]
                                                         {
                                                             typeof (MigrationRunner).Assembly
                                                         };

            RepositoryResolver.Current = new RepositoryResolver(
                new RepositoryFactory());

            //Delete database file before continueing
            string filePath = string.Concat(path, "\\UmbracoPetaPocoTests.sdf");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            //Get the connectionstring settings from config
            var settings = ConfigurationManager.ConnectionStrings["umbracoDbDsn"];
            ConfigurationManager.AppSettings.Set("umbracoDbDSN", @"datalayer=SQLCE4Umbraco.SqlCEHelper,SQLCE4Umbraco;data source=|DataDirectory|\UmbracoPetaPocoTests.sdf");

            //Create the Sql CE database
            var engine = new SqlCeEngine(settings.ConnectionString);
            engine.CreateDatabase();

            Resolution.Freeze();
            ApplicationContext.Current = new ApplicationContext(
                //assign the db context
                new DatabaseContext(new DefaultDatabaseFactory()),
                //assign the service context
                new ServiceContext(new PetaPocoUnitOfWorkProvider(), new FileUnitOfWorkProvider(), new PublishingStrategy())) { IsReady = true };

            ApplicationContext.Current.DatabaseContext.Initialize();
        }

        [Test]
        public void Can_Upgrade_From_470_To_600()
        {
            var configuredVersion = new Version("4.7.0");
            var targetVersion = new Version("6.0.0");
            var db = ApplicationContext.Current.DatabaseContext.Database;

            //Create db schema and data from old Total.sql file for Sql Ce
            string statements = SqlScripts.SqlResources.SqlCeTotal_480;
            // replace block comments by whitespace
            statements = m_findComments.Replace(statements, " ");
            // execute all non-empty statements
            foreach (string statement in statements.Split(";".ToCharArray()))
            {
                string rawStatement = statement.Trim();
                if (rawStatement.Length > 0)
                    db.Execute(rawStatement);
            }

            //Setup the MigrationRunner
            var migrationRunner = new MigrationRunner(configuredVersion, targetVersion);
            bool upgraded = migrationRunner.Execute(db, true);

            Assert.That(upgraded, Is.True);

            bool hasTabTable = db.TableExist("cmsTab");
            bool hasPropertyTypeGroupTable = db.TableExist("cmsPropertyTypeGroup");
            bool hasAppTreeTable = db.TableExist("umbracoAppTree");

            Assert.That(hasTabTable, Is.False);
            Assert.That(hasPropertyTypeGroupTable, Is.True);
            Assert.That(hasAppTreeTable, Is.False);
        }

        [TearDown]
        public void TearDown()
        {
            ApplicationContext.Current.DatabaseContext.Database.Dispose();
            //reset the app context            
            ApplicationContext.Current.ApplicationCache.ClearAllCache();

            //legacy API database connection close
            SqlCeContextGuardian.CloseBackgroundConnection();

            PluginManager.Current = null;

            ApplicationContext.Current = null;
            Resolution.IsFrozen = false;
            RepositoryResolver.Reset();

            TestHelper.CleanContentDirectories();

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            string filePath = string.Concat(path, "\\UmbracoPetaPocoTests.sdf");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}