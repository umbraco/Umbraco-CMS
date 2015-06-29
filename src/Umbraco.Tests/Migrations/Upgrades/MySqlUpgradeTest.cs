using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations.Upgrades
{
    [TestFixture, NUnit.Framework.Ignore]
    public class MySqlUpgradeTest : BaseUpgradeTest
    {
        public override void DatabaseSpecificSetUp()
        {
            //TODO Create new database here
        }

        public override void DatabaseSpecificTearDown()
        {
            //TODO Remove created database here
        }

        public override ISqlSyntaxProvider GetSyntaxProvider()
        {
            return MySqlSyntax.Provider;
        }

        public override UmbracoDatabase GetConfiguredDatabase()
        {
            return new UmbracoDatabase("Server = 169.254.120.3; Database = upgradetest; Uid = umbraco; Pwd = umbraco", "MySql.Data.MySqlClient");
        }

        public override DatabaseProviders GetDatabaseProvider()
        {
            return DatabaseProviders.MySql;
        }

        public override string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.MySqlTotal_480;
        }
    }
}