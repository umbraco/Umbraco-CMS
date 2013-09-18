using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;

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

        [SetUp]
        public virtual void Initialize()
        {
            TestHelper.SetupLog4NetForTests();
            TestHelper.InitializeContentDirectories();

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
            

            RepositoryResolver.Current = new RepositoryResolver(
                new RepositoryFactory(true));  //disable all repo caches for tests!

            SqlSyntaxProvidersResolver.Current = new SqlSyntaxProvidersResolver(
                new List<Type> { typeof(MySqlSyntaxProvider), typeof(SqlCeSyntaxProvider), typeof(SqlServerSyntaxProvider) }) { CanResolveBeforeFrozen = true };

            Resolution.Freeze();

            //disable cache
            var cacheHelper = CacheHelper.CreateDisabledCacheHelper();

            ApplicationContext.Current = new ApplicationContext(
                //assign the db context
                new DatabaseContext(new DefaultDatabaseFactory()),
                //assign the service context
                new ServiceContext(new PetaPocoUnitOfWorkProvider(), new FileUnitOfWorkProvider(), new PublishingStrategy(), cacheHelper),
                cacheHelper)
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

            RepositoryResolver.Reset();
        }
    }
}