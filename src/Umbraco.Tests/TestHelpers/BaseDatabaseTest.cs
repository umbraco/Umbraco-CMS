using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Profiling;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Use this abstract class for tests that requires direct access to the PetaPoco <see cref="Database"/> object.
    /// This base test class will use the database setup with ConnectionString and ProviderName from the test implementation
    /// populated with the umbraco db schema.
    /// </summary>
    /// <remarks>Can be used to test against an Sql Ce, Sql Server and MySql database</remarks>
    [TestFixture]
    public abstract class BaseDatabaseTest
    {
        private Database _database;
        protected ILogger Logger { get; private set; }

        [SetUp]
        public virtual void Initialize()
        {
            TestHelper.InitializeContentDirectories();

            Logger = new Logger(new FileInfo(TestHelper.MapPathForTest("~/unit-test-log4net.config")));

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

           
                try
                {
                    //Delete database file before continueing
                    string filePath = string.Concat(path, "\\test.sdf");
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
            
            Resolution.Freeze();

            //disable cache
            var cacheHelper = CacheHelper.CreateDisabledCacheHelper();

            var logger = new Logger(new FileInfo(TestHelper.MapPathForTest("~/unit-test-log4net.config")));
            var repositoryFactory = new RepositoryFactory(cacheHelper, Logger, SyntaxProvider, SettingsForTests.GenerateMockSettings());
            var dbFactory = new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, logger);
            ApplicationContext.Current = new ApplicationContext(
                //assign the db context
                new DatabaseContext(dbFactory, logger, SqlSyntaxProviders.CreateDefault(logger)),
                //assign the service context
                new ServiceContext(repositoryFactory, new PetaPocoUnitOfWorkProvider(dbFactory), new FileUnitOfWorkProvider(), new PublishingStrategy(), cacheHelper, logger),
                cacheHelper,
                new ProfilingLogger(logger, Mock.Of<IProfiler>()))
                {
                    IsReady = true
                };

            SqlSyntaxContext.SqlSyntaxProvider = SyntaxProvider;

            //Create the umbraco database
            _database = new Database(ConnectionString, ProviderName);
            _database.CreateDatabaseSchema(false);
        }

        public abstract string ConnectionString { get; }
        public abstract string ProviderName { get; }
        public abstract ISqlSyntaxProvider SyntaxProvider { get; }

        protected Database Database
        {
            get { return _database; }
        }

        [TearDown]
        public virtual void TearDown()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            //reset the app context
            ApplicationContext.Current = null;
        }
    }
}