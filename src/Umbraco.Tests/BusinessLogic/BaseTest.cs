using System;
using System.Collections.Generic;
using System.Configuration;
using NUnit.Framework;
using SqlCE4Umbraco;
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
			ConfigurationManager.AppSettings.Set("umbracoDbDSN", "");
        }

        /// <summary>
        /// Ensures everything is setup to allow for unit tests to execute for each test
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            InitializeDatabase();
            InitializeApps();
            InitializeAppConfigFile();
            InitializeTreeConfigFile();
        }

        private void ClearDatabase()
        {
            var dataHelper = DataLayerHelper.CreateSqlHelper(GlobalSettings.DbDSN) as SqlCEHelper;
            if (dataHelper == null)
                throw new InvalidOperationException("The sql helper for unit tests must be of type SqlCEHelper, check the ensure the connection string used for this test is set to use SQLCE");
            dataHelper.ClearDatabase();
        }

        private void InitializeDatabase()
        {
            ConfigurationManager.AppSettings.Set("umbracoDbDSN", @"datalayer=SQLCE4Umbraco.SqlCEHelper,SQLCE4Umbraco;data source=|DataDirectory|\Umbraco.sdf");

			ClearDatabase();

            var dataHelper = DataLayerHelper.CreateSqlHelper(GlobalSettings.DbDSN);
            var installer = dataHelper.Utility.CreateInstaller();
            if (installer.CanConnect)
            {
                installer.Install();
            }
        }

        private void InitializeApps()
        {
            Application.Apps = new List<Application>()
                {
                    new Application("content", "content", "content", 0)
                };
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