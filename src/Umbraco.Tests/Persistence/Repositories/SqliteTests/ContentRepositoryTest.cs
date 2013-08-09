using System.Configuration;
using System.IO;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Persistence.Repositories.SqliteTests
{
    [TestFixture]
    public class ContentRepositoryTest : Repositories.ContentRepositoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            CreateTestData();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        protected override string GetDbProviderName()
        {
            return "System.Data.SQLite";
        }

        protected override string GetDbConnectionString()
        {
            return @"Data Source=|DataDirectory|"+ GetDbFileName() +";Version=3;New=True;Compress=True;";
        }

        protected override string GetDbFileName()
        {
            return "UmbracoDb.sqlite";
        }

        protected override void CreateSqlCeDatabase()
        {
            if (DatabaseTestBehavior == DatabaseBehavior.NoDatabasePerFixture)
                return;

            var path = TestHelper.CurrentAssemblyDirectory;

            //Get the connectionstring settings from config
            ConfigurationManager.AppSettings.Set(
                Core.Configuration.GlobalSettings.UmbracoConnectionName,
                GetDbConnectionString());

            string dbFilePath = string.Concat(path, "\\" + GetDbFileName());

            //create a new database file if
            // - is the first test in the session
            // - the database file doesn't exist
            // - NewDbFileAndSchemaPerTest
            // - _isFirstTestInFixture + DbInitBehavior.NewDbFileAndSchemaPerFixture

            //if this is the first test in the session, always ensure a new db file is created
            if (IsFirstRunInTestSession || !File.Exists(dbFilePath)
                || DatabaseTestBehavior == DatabaseBehavior.NewDbFileAndSchemaPerTest
                || (IsFirstTestInFixture && DatabaseTestBehavior == DatabaseBehavior.NewDbFileAndSchemaPerFixture))
            {

                RemoveDatabaseFile(ex =>
                {
                    //if this doesn't work we have to make sure everything is reset! otherwise
                    // well run into issues because we've already set some things up
                    TearDown();
                    throw ex;
                });
            }

            //clear the database if
            // - NewSchemaPerTest
            // - _isFirstTestInFixture + DbInitBehavior.NewSchemaPerFixture

            else if (DatabaseTestBehavior == DatabaseBehavior.NewSchemaPerTest
                || (IsFirstTestInFixture && DatabaseTestBehavior == DatabaseBehavior.NewSchemaPerFixture))
            {
                DatabaseContext.Database.UninstallDatabaseSchema();
            }
        }
    }
}