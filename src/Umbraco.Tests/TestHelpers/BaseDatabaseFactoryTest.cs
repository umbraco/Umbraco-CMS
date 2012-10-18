using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using NUnit.Framework;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Use this abstract class for tests that requires a Sql Ce database populated with the umbraco db schema.
    /// The PetaPoco Database class should be used through the <see cref="DatabaseFactory"/> singleton.
    /// </summary>
    [TestFixture]
    public abstract class BaseDatabaseFactoryTest
    {
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
            var settings = ConfigurationManager.ConnectionStrings["umbracoDbDsn"];

            //Create the Sql CE database
            var engine = new SqlCeEngine(settings.ConnectionString);
            engine.CreateDatabase();

            //Create the umbraco database
            DatabaseFactory.Current.Database.Initialize();
        }

        [TearDown]
        public virtual void TearDown()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", null);
        }
    }
}