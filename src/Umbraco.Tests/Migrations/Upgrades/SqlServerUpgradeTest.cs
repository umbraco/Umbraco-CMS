using System;
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
    [TestFixture, NUnit.Framework.Ignore]
    public class SqlServerUpgradeTest : BaseUpgradeTest
    {
        public override void DatabaseSpecificSetUp()
        {
        }

        public override void DatabaseSpecificTearDown()
        {
        }

        public override UmbracoDatabase GetConfiguredDatabase()
        {
            var dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var sqlContext = new SqlContext(new SqlServerSyntaxProvider(new Lazy<IDatabaseFactory>(() => null)), Mock.Of<IPocoDataFactory>(), DatabaseType.SqlServer2008);
            return new UmbracoDatabase(@"server=.\SQLEXPRESS;database=EmptyForTest;user id=umbraco;password=umbraco", sqlContext, dbProviderFactory, Mock.Of<ILogger>());
        }

        public override string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.SqlServerTotal_480;
        }
    }
}