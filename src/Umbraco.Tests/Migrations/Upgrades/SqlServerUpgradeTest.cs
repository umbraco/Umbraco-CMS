using System;
using System.Data.Common;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;

namespace Umbraco.Tests.Migrations.Upgrades
{
    [TestFixture, NUnit.Framework.Ignore("fixme - ignored test")]
    public class SqlServerUpgradeTest : BaseUpgradeTest
    {
        public override void DatabaseSpecificSetUp()
        { }

        public override void DatabaseSpecificTearDown()
        { }

        public override IUmbracoDatabase GetConfiguredDatabase()
        {
            var dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var sqlContext = new SqlContext(new SqlServerSyntaxProvider(new Lazy<IScopeProvider>(() => null)), Mock.Of<IPocoDataFactory>(), DatabaseType.SqlServer2008);
            return new UmbracoDatabase(@"server=.\SQLEXPRESS;database=EmptyForTest;user id=umbraco;password=umbraco", sqlContext, dbProviderFactory, Mock.Of<ILogger>());
        }

        public override string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.SqlServerTotal_480;
        }
    }
}
