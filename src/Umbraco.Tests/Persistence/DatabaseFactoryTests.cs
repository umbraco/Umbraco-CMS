using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class DatabaseFactoryTests
    {
        [Test]
        public void Can_Verify_Single_Database_Instance()
        {
            var db1 = DatabaseFactory.Current.Database;
            var db2 = DatabaseFactory.Current.Database;

            Assert.AreSame(db1, db2);
        }

        [Test]
        public void Can_Assert_DatabaseProvider()
        {
            var provider = DatabaseContext.Current.DatabaseProvider;

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
            var settings = ConfigurationManager.ConnectionStrings["umbracoDbDsn"];

            //Create the Sql CE database
            var engine = new SqlCeEngine(settings.ConnectionString);
            engine.CreateDatabase();

            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntaxProvider.Instance;

            //Create the umbraco database
            DatabaseFactory.Current.Database.Initialize();

            bool umbracoNodeTable = DatabaseFactory.Current.Database.TableExist("umbracoNode");
            bool umbracoUserTable = DatabaseFactory.Current.Database.TableExist("umbracoUser");
            bool cmsTagsTable = DatabaseFactory.Current.Database.TableExist("cmsTags");

            Assert.That(umbracoNodeTable, Is.True);
            Assert.That(umbracoUserTable, Is.True);
            Assert.That(cmsTagsTable, Is.True);
        }
    }
}