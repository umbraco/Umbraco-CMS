using System;
using System.Collections.Generic;
using System.Configuration;
using NUnit.Framework;
using SqlCE4Umbraco;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.IO;
using GlobalSettings = umbraco.GlobalSettings;

namespace Umbraco.Tests.BusinessLogic
{
	[TestFixture, RequiresSTA]
    public abstract class BaseTest
    {
        /// <summary>
        /// Removes any resources that were used for the test
        /// </summary>
        [TearDown]
        public void Dispose()
        {
            ClearDatabase();
            ConfigurationManager.AppSettings.Set(Core.Configuration.GlobalSettings.UmbracoConnectionName, "");
            ApplicationContext.Current.DisposeIfDisposable();
        }

        /// <summary>
        /// Ensures everything is setup to allow for unit tests to execute for each test
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            ApplicationContext.Current = new ApplicationContext(false){IsReady = true};
            InitializeDatabase();
            InitializeApps();
            InitializeAppConfigFile();
            InitializeTreeConfigFile();
        }

        private void ClearDatabase()
        {
            var databaseSettings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];
            var dataHelper = DataLayerHelper.CreateSqlHelper(databaseSettings.ConnectionString, false) as SqlCEHelper;
			
            if (dataHelper == null)
                throw new InvalidOperationException("The sql helper for unit tests must be of type SqlCEHelper, check the ensure the connection string used for this test is set to use SQLCE");
            dataHelper.ClearDatabase();

            AppDomain.CurrentDomain.SetData("DataDirectory", null);
        }

        private void InitializeDatabase()
        {
            ConfigurationManager.AppSettings.Set(Core.Configuration.GlobalSettings.UmbracoConnectionName, @"datalayer=SQLCE4Umbraco.SqlCEHelper,SQLCE4Umbraco;data source=|DataDirectory|\UmbracoPetaPocoTests.sdf;Flush Interval=1;File Access Retry Timeout=10");

			ClearDatabase();

            AppDomain.CurrentDomain.SetData("DataDirectory", TestHelper.CurrentAssemblyDirectory);

            var databaseSettings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];
            var dataHelper = DataLayerHelper.CreateSqlHelper(databaseSettings.ConnectionString, false) as SqlCEHelper;
			
            var installer = dataHelper.Utility.CreateInstaller();
            if (installer.CanConnect)
            {
                installer.Install();
            }
        }

        private void InitializeApps()
        {
            Application.MakeNew("content", "content", "file", 0);
            //Application.SetTestApps(new List<Application>()
            //    {
            //        new Application(Constants.Applications.Content, "content", "content", 0)
            //    });
        }

        private void InitializeAppConfigFile()
        {
            Application.AppConfigFilePath = IOHelper.MapPath(SystemDirectories.Config + "/" + Application.AppConfigFileName, false);
        }

        private void InitializeTreeConfigFile()
        {
            ApplicationTree.TreeConfigFilePath = IOHelper.MapPath(SystemDirectories.Config + "/" + ApplicationTree.TreeConfigFileName, false);
        }

    }
}