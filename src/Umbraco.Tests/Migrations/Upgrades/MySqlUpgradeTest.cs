using System.Data.Common;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations.Upgrades
{
    [TestFixture, NUnit.Framework.Ignore("fixme - ignored test")]
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

        public override IUmbracoDatabase GetConfiguredDatabase()
        {
            var dbProviderFactory = DbProviderFactories.GetFactory(Constants.DbProviderNames.MySql);
            var sqlContext = new SqlContext(new MySqlSyntaxProvider(Mock.Of<ILogger>()), DatabaseType.MySQL, Mock.Of<IPocoDataFactory>());
            return new UmbracoDatabase("Server = 169.254.120.3; Database = upgradetest; Uid = umbraco; Pwd = umbraco", sqlContext, dbProviderFactory, Mock.Of<ILogger>());
        }

        public override string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.MySqlTotal_480;
        }
    }
}
