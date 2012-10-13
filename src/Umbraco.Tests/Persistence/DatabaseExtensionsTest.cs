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

            string filePath = string.Concat(path, "\\test.sdf");
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            string connectionString = "Datasource=|DataDirectory|test.sdf";
            var engine = new SqlCeEngine(connectionString);
            engine.CreateDatabase();
        }

        [Test]
        public void Can_Create_umbracoNode_Table()
        {
            var factory = DatabaseFactory.Current;
            //var database = new Database("Datasource=|DataDirectory|test.sdf", "System.Data.SqlServerCe.4.0");
            using(Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();

                transaction.Complete();
            }
        }
    }
}