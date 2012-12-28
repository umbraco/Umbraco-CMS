using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations.Upgrades
{
    [TestFixture]
    public class SqlCeUpgradeTest : BaseUpgradeTest
    {
        public override void DatabaseSpecificSetUp()
        {
            //Delete database file before continueing
            string filePath = string.Concat(Path, "\\UmbracoPetaPocoTests.sdf");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            //Get the connectionstring settings from config
            var settings = ConfigurationManager.ConnectionStrings["umbracoDbDsn"];

            //Create the Sql CE database
            var engine = new SqlCeEngine(settings.ConnectionString);
            engine.CreateDatabase();
        }

        public override void DatabaseSpecificTearDown()
        {
            string filePath = string.Concat(Path, "\\UmbracoPetaPocoTests.sdf");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public override ISqlSyntaxProvider GetSyntaxProvider()
        {
            return SqlCeSyntax.Provider;
        }

        public override UmbracoDatabase GetConfiguredDatabase()
        {
            return new UmbracoDatabase("Datasource=|DataDirectory|UmbracoPetaPocoTests.sdf", "System.Data.SqlServerCe.4.0");
        }

        public override DatabaseProviders GetDatabaseProvider()
        {
            return DatabaseProviders.SqlServerCE;
        }

        public override string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.SqlCeTotal_480;
        }
    }
}