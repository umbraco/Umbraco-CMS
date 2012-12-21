using System;
using System.Data.SqlServerCe;
using System.IO;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class SqlCeTableByTableTest : BaseTableByTableTest
    {
        private Database _database;

        #region Overrides of BaseTableByTableTest

        [SetUp]
        public override void Initialize()
        {
            TestHelper.SetupLog4NetForTests();
            TestHelper.InitializeContentDirectories();

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            //Delete database file before continueing
            string filePath = string.Concat(path, "\\test.sdf");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            //Create the Sql CE database
            var engine = new SqlCeEngine("Datasource=|DataDirectory|test.sdf");
            engine.CreateDatabase();

            UmbracoSettings.UseLegacyXmlSchema = false;

            RepositoryResolver.Current = new RepositoryResolver(
                new RepositoryFactory());

            Resolution.Freeze();
            ApplicationContext.Current = new ApplicationContext(
                //assign the db context
                new DatabaseContext(new DefaultDatabaseFactory()),
                //assign the service context
                new ServiceContext(new PetaPocoUnitOfWorkProvider(), new FileUnitOfWorkProvider(), new PublishingStrategy())) { IsReady = true };

            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntaxProvider.Instance;

            _database = new Database("Datasource=|DataDirectory|test.sdf",
                                     "System.Data.SqlServerCe.4.0");
        }

        [TearDown]
        public override void TearDown()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            SyntaxConfig.SqlSyntaxProvider = null;

            //reset the app context
            ApplicationContext.Current = null;
            Resolution.IsFrozen = false;

            RepositoryResolver.Reset();
        }

        public override Database Database
        {
            get { return _database; }
        }

        #endregion
    }
}