using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
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
            return new SqlServerSyntaxProvider();
        }

        public override UmbracoDatabase GetConfiguredDatabase()
        {
            return new UmbracoDatabase(@"server=.\SQLEXPRESS;database=EmptyForTest;user id=umbraco;password=umbraco", "System.Data.SqlClient", Mock.Of<ILogger>());
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