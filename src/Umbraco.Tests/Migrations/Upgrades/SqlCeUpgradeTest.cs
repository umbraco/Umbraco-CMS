using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Migrations.Upgrades
{
    [TestFixture]
    public class SqlCeUpgradeTest : BaseUpgradeTest
    {
        public override void DatabaseSpecificSetUp()
        {
            string filePath = string.Concat(Path, "\\UmbracoNPocoTests.sdf");

            // no more "clear database" just recreate it
            if (File.Exists(filePath)) File.Delete(filePath);

            if (!File.Exists(filePath))
            {
                try
                {
                    //Delete database file before continueing
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception)
                {
                    //if this doesn't work we have to make sure everything is reset! otherwise
                    // well run into issues because we've already set some things up
                    TearDown();
                    throw;
                }

                //Create the Sql CE database
                //Get the connectionstring settings from config
                var settings = ConfigurationManager.ConnectionStrings[Constants.System.UmbracoConnectionName];
                using (var engine = new SqlCeEngine(settings.ConnectionString))
                {
                    engine.CreateDatabase();
                }
            }
            else
            {
                //TestHelper.ClearDatabase();
            }

        }

        public override void DatabaseSpecificTearDown()
        {
            //legacy API database connection close
            //SqlCeContextGuardian.CloseBackgroundConnection();

            //TestHelper.ClearDatabase();
        }

        public override IUmbracoDatabase GetConfiguredDatabase()
        {
            var dbProviderFactory = DbProviderFactories.GetFactory(Constants.DbProviderNames.SqlCe);
            var sqlContext = new SqlContext(new SqlCeSyntaxProvider(), DatabaseType.SQLCe, Mock.Of<IPocoDataFactory>());
            return new UmbracoDatabase("Datasource=|DataDirectory|UmbracoNPocoTests.sdf;Flush Interval=1;", sqlContext, dbProviderFactory, Mock.Of<ILogger>());
        }

        public override string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.SqlCeTotal_480;
        }
    }
}
