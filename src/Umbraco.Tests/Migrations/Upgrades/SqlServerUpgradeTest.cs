using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations.Upgrades
{
    [TestFixture, NUnit.Framework.Ignore]
    public class SqlServerUpgradeTest : BaseUpgradeTest
    {
        public override void DatabaseSpecificSetUp()
        {
        }

        public override void DatabaseSpecificTearDown()
        {
        }

        public override ISqlSyntaxProvider GetSyntaxProvider()
        {
            return SqlServerSyntax.Provider;
        }

        public override UmbracoDatabase GetConfiguredDatabase()
        {
            return new UmbracoDatabase(@"server=.\SQLEXPRESS;database=EmptyForTest;user id=umbraco;password=umbraco", "System.Data.SqlClient");
        }

        public override DatabaseProviders GetDatabaseProvider()
        {
            return DatabaseProviders.SqlServer;
        }

        public override string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.SqlServerTotal_480;
        }
    }
}