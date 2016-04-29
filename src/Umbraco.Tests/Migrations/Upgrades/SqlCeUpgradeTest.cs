using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using Moq;
using NPoco;
using NUnit.Framework;
using SQLCE4Umbraco;
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
                var settings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];
                var engine = new SqlCeEngine(settings.ConnectionString);
                engine.CreateDatabase();
            }
            else
            {
                TestHelper.ClearDatabase();
            }

        }

        public override void DatabaseSpecificTearDown()
        {
            //legacy API database connection close
            SqlCeContextGuardian.CloseBackgroundConnection();

            TestHelper.ClearDatabase();
        }

        public override UmbracoDatabase GetConfiguredDatabase()
        {
            var databaseType = DatabaseType.SQLCe;
            var sqlSyntax = new SqlCeSyntaxProvider();
            var dbProviderFactory = DbProviderFactories.GetFactory(Constants.DbProviderNames.SqlCe);
            return new UmbracoDatabase("Datasource=|DataDirectory|UmbracoNPocoTests.sdf;Flush Interval=1;", sqlSyntax, databaseType, dbProviderFactory, Mock.Of<ILogger>());
        }

        public override string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.SqlCeTotal_480;
        }
    }
}