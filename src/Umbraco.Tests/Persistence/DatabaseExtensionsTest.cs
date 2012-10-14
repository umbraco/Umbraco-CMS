using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class DatabaseExtensionsTest
    {
        [SetUp]
		public virtual void Initialize()
        {
            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            //Delete database file before continueing
            string filePath = string.Concat(path, "\\test.sdf");
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            //Get the connectionstring settings from config
            var settings = ConfigurationManager.ConnectionStrings["umbracoDbDsn"];

            //Create the Sql CE database
            var engine = new SqlCeEngine(settings.ConnectionString);
            engine.CreateDatabase();

            //Create the umbraco database
            DatabaseFactory.Current.Database.Initialize();
        }

        [Test]
        public void Can_Create_umbracoNode_Table()
        {
            var factory = DatabaseFactory.Current;
            using(Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoApp_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<AppDto>();

                //transaction.Complete();
            }
        }
    }
}