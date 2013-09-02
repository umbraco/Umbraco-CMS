using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture, NUnit.Framework.Ignore]
    public abstract class MemberRepositoryTest : MemberRepositoryBaseTest
    {
        private Database _database;

        #region Overrides of MemberRepositoryBaseTest

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            SqlSyntaxContext.SqlSyntaxProvider = SqlServerSyntax.Provider;

            _database = new Database(@"server=.\SQLEXPRESS;database=EmptyForTest;user id=umbraco;password=umbraco",
                                     "System.Data.SqlClient");
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public override Database Database
        {
            get { return _database; }
        }

        #endregion


    }

    [TestFixture]
    public abstract class MemberRepositoryBaseTest
    {
        [SetUp]
        public virtual void Initialize()
        {
            TestHelper.SetupLog4NetForTests();
            TestHelper.InitializeContentDirectories();

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            RepositoryResolver.Current = new RepositoryResolver(
                new RepositoryFactory());

            //disable cache
            var cacheHelper = CacheHelper.CreateDisabledCacheHelper();

            var dbFactory = new DefaultDatabaseFactory();

            ApplicationContext.Current = new ApplicationContext(
                //assign the db context
                new DatabaseContext(dbFactory),
                //assign the service context
                new ServiceContext(new PetaPocoUnitOfWorkProvider(dbFactory), new FileUnitOfWorkProvider(), new PublishingStrategy(), cacheHelper),
                //disable cache
                cacheHelper)
            {
                IsReady = true
            };

            Resolution.Freeze();
        }

        [TearDown]
        public virtual void TearDown()
        {
            SqlSyntaxContext.SqlSyntaxProvider = null;
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            //reset the app context
            ApplicationContext.Current = null;

            RepositoryResolver.Reset();
        }

        public abstract Database Database { get; }
    }
}