using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class DatabaseContextTests
    {
	    private DatabaseContext _dbContext;

		[SetUp]
		public void Setup()
		{
			_dbContext = new DatabaseContext(new DefaultDatabaseFactory());

			//unfortunately we have to set this up because the PetaPocoExtensions require singleton access
			ApplicationContext.Current = new ApplicationContext
				{
					DatabaseContext = _dbContext,
					IsReady = true
				};			
		}

		[TearDown]
		public void TearDown()
		{
			_dbContext = null;
			ApplicationContext.Current = null;
		}

        [Test]
        public void Can_Verify_Single_Database_Instance()
        {
			var db1 = _dbContext.Database;
			var db2 = _dbContext.Database;

            Assert.AreSame(db1, db2);
        }

        [Test]
        public void Can_Assert_DatabaseProvider()
        {
			var provider = _dbContext.DatabaseProvider;

            Assert.AreEqual(DatabaseProviders.SqlServerCE, provider);
        }

        [Test]
        public void Can_Assert_Created_Database()
        {
            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            //Delete database file before continueing
            string filePath = string.Concat(path, "\\UmbracoPetaPocoTests.sdf");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            //Get the connectionstring settings from config
            var settings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];

            //Create the Sql CE database
            var engine = new SqlCeEngine(settings.ConnectionString);
            engine.CreateDatabase();

            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntaxProvider.Instance;

            //Create the umbraco database
			_dbContext.Database.CreateDatabaseSchema();

			bool umbracoNodeTable = _dbContext.Database.TableExist("umbracoNode");
			bool umbracoUserTable = _dbContext.Database.TableExist("umbracoUser");
			bool cmsTagsTable = _dbContext.Database.TableExist("cmsTags");

            Assert.That(umbracoNodeTable, Is.True);
            Assert.That(umbracoUserTable, Is.True);
            Assert.That(cmsTagsTable, Is.True);
        }
    }
}