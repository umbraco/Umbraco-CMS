using System;
using System.Data.SqlServerCe;
using System.IO;
using NUnit.Framework;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Use this abstract class for tests that requires direct access to the PetaPoco <see cref="Database"/> object.
    /// This base test class uses Sql Ce populated with the umbraco db schema.
    /// </summary>
    [TestFixture]
    public abstract class BaseDatabaseTest
    {
        private Database _database;

        [SetUp]
        public virtual void Initialize()
        {
            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            //Delete database file before continueing
            string filePath = string.Concat(path, "\\test.sdf");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            //Get the connectionstring settings from config
            var connectionstring = "Datasource=|DataDirectory|test.sdf";
            var providerName = "System.Data.SqlServerCe.4.0";

            //Create the Sql CE database
            var engine = new SqlCeEngine(connectionstring);
            engine.CreateDatabase();

            //Create the umbraco database
            _database = new Database(connectionstring, providerName);
            _database.Initialize();
        }

        protected Database Database
        {
            get { return _database; }
        }

        [TearDown]
        public virtual void TearDown()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", null);
        }
    }
}