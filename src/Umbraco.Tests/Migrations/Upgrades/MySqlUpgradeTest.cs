using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
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
            return new MySqlSyntaxProvider(Mock.Of<ILogger>());
        }

        public override UmbracoDatabase GetConfiguredDatabase()
        {
            return new UmbracoDatabase("Server = 169.254.120.3; Database = upgradetest; Uid = umbraco; Pwd = umbraco", "MySql.Data.MySqlClient", Mock.Of<ILogger>());
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