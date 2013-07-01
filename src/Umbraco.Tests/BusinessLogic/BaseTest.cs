using System;
using System.Collections.Generic;
using System.Configuration;
using AutoMapper;
using NUnit.Framework;
using SqlCE4Umbraco;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Sections;
using Umbraco.Tests.TestHelpers;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
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
            InitializeMappers();
            InitializeDatabase();
            InitializeApps();
            InitializeAppConfigFile();
            InitializeTreeConfigFile();
        }

        private void InitializeMappers()
        {
            Mapper.Initialize(configuration =>
                {
                    var mappers = PluginManager.Current.FindAndCreateInstances<IMapperConfiguration>();
                    foreach (var mapper in mappers)
                    {
                        mapper.ConfigureMappings(configuration);
                    }
                });
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
            ConfigurationManager.AppSettings.Set(Core.Configuration.GlobalSettings.UmbracoConnectionName, @"datalayer=SQLCE4Umbraco.SqlCEHelper,SQLCE4Umbraco;data source=|DataDirectory|\UmbracoPetaPocoTests.sdf");

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
            SectionCollection.MakeNew("content", "content", "file", 0);
            //Application.SetTestApps(new List<Application>()
            //    {
            //        new Application(Constants.Applications.Content, "content", "content", 0)
            //    });
        }

        private void InitializeAppConfigFile()
        {
            SectionCollection.AppConfigFilePath = IOHelper.MapPath(SystemDirectories.Config + "/" + SectionCollection.AppConfigFileName, false);
        }

        private void InitializeTreeConfigFile()
        {
            Core.Trees.ApplicationTreeCollection.TreeConfigFilePath = IOHelper.MapPath(SystemDirectories.Config + "/" + Core.Trees.ApplicationTreeCollection.TreeConfigFileName, false);
        }

    }
}