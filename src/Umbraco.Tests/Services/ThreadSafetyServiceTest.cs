using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.DI;

namespace Umbraco.Tests.Services
{
	[TestFixture, RequiresSTA]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ThreadSafetyServiceTest : TestWithDatabaseBase
	{
		//private IDatabaseUnitOfWorkProvider _uowProvider;
		//private PerThreadSqlCeDatabaseFactory _dbFactory;
	    private IUmbracoDatabaseAccessor _accessor;
	 //   private DatabaseContext _dbContext;
	 //   private ServiceContext _services;

		public override void SetUp()
		{
			base.SetUp();
            
   //         //we need to use our own custom IDatabaseFactory for the DatabaseContext because we MUST ensure that
   //         //a Database instance is created per thread, whereas the default implementation which will work in an HttpContext
   //         //threading environment, or a single apartment threading environment will not work for this test because
   //         //it is multi-threaded.
   //         _dbFactory = new PerThreadSqlCeDatabaseFactory(Logger, Mock.Of<IMapperCollection>());
   //         var repositoryFactory = new RepositoryFactory(Container);
   //         _uowProvider = new NPocoUnitOfWorkProvider(_dbFactory, repositoryFactory);

   //         // overwrite the local object
   //         _dbContext = new DatabaseContext(_dbFactory, Logger, Mock.Of<IRuntimeState>(), Mock.Of<IMigrationEntryService>());

   //         //disable cache
		 //   var cacheHelper = CacheHelper.CreateDisabledCacheHelper();

			////here we are going to override the ServiceContext because normally with our test cases we use a
			////global Database object but this is NOT how it should work in the web world or in any multi threaded scenario.
			////we need a new Database object for each thread.
		 //   var evtMsgs = new TransientEventMessagesFactory();
		 //   _services = TestObjects.GetServiceContext(
   //             repositoryFactory,
   //             _uowProvider,
   //             new FileUnitOfWorkProvider(),
   //             cacheHelper,
   //             Logger,
   //             evtMsgs,
   //             Enumerable.Empty<IUrlSegmentProvider>());

			CreateTestData();
		}

        protected override void Compose()
        {
            base.Compose();

            //replace some services
            //Container.Register<IDatabaseFactory>(factory => _dbFactory);
            //Container.Register<IDatabaseFactory>(f => _dbFactory = new PerThreadSqlCeDatabaseFactory(f.GetInstance<ILogger>(), f.GetInstance<IMapperCollection>()));
            Container.RegisterSingleton(f => _accessor = new ThreadStaticUmbracoDatabaseAccessor()); // per-thread database

            //Container.Register<DatabaseContext>(factory => _dbContext);
            //Container.Register<ServiceContext>(factory => _services);
        }

		public override void TearDown()
		{
			// dispose!
           // _dbFactory?.Dispose();
		    _accessor.UmbracoDatabase?.Dispose();

            base.TearDown();
		}

        private const int MaxThreadCount = 20;

		[Test]
		public void Ensure_All_Threads_Execute_Successfully_Content_Service()
		{
			var contentService = (ContentService) ServiceContext.ContentService;

			var threads = new List<Thread>();
            var exceptions = new List<Exception>();

            Debug.WriteLine("Starting test");

            for (var i = 0; i < MaxThreadCount; i++)
			{
				var t = new Thread(() =>
				{
					try
					{
						//Debug.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] ({DateTime.Now.ToString("HH:mm:ss,FFF")}) Create 1st content.");
						var content1 = contentService.CreateContent("test" + Guid.NewGuid(), -1, "umbTextpage", 0);

						//Debug.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] ({DateTime.Now.ToString("HH:mm:ss,FFF")}) Save 1st content.");
						contentService.Save(content1);
                        //Debug.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] ({DateTime.Now.ToString("HH:mm:ss,FFF")}) Saved 1st content.");

						Thread.Sleep(100); //quick pause for maximum overlap!

                        //Debug.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] ({DateTime.Now.ToString("HH:mm:ss,FFF")}) Create 2nd content.");
                        var content2 = contentService.CreateContent("test" + Guid.NewGuid(), -1, "umbTextpage", 0);

                        //Debug.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] ({DateTime.Now.ToString("HH:mm:ss,FFF")}) Save 2nd content.");
                        contentService.Save(content2);
                        //Debug.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] ({DateTime.Now.ToString("HH:mm:ss,FFF")}) Saved 2nd content.");
                    }
                    catch (Exception e)
					{
                        //Debug.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] ({DateTime.Now.ToString("HH:mm:ss,FFF")}) Exception!");
						lock (exceptions) { exceptions.Add(e); }
					}
				});
				threads.Add(t);
			}

            // start all threads
            Debug.WriteLine("Starting threads");
            threads.ForEach(x => x.Start());

            // wait for all to complete
            Debug.WriteLine("Joining threads");
            threads.ForEach(x => x.Join());

            // kill them all
            // uh? no!
            //threads.ForEach(x => x.Abort());

            Debug.WriteLine("Checking exceptions");
            if (exceptions.Count == 0)
			{
				//now look up all items, there should be 40!
				var items = contentService.GetRootContent();
				Assert.AreEqual(40, items.Count());
			}
			else
			{
			    throw new Exception("Exceptions!", exceptions.First()); // rethrow the first one...
			}
		}

		[Test]
		public void Ensure_All_Threads_Execute_Successfully_Media_Service()
		{
			//we will mimick the ServiceContext in that each repository in a service (i.e. ContentService) is a singleton
			var mediaService = (MediaService)ServiceContext.MediaService;

			var threads = new List<Thread>();
            var exceptions = new List<Exception>();

            Debug.WriteLine("Starting test...");

			for (var i = 0; i < MaxThreadCount; i++)
			{
				var t = new Thread(() =>
				{
					try
					{
						//Debug.WriteLine("Created content on thread: " + Thread.CurrentThread.ManagedThreadId);

						//create 2 content items

					    var folder1 = mediaService.CreateMedia("test" + Guid.NewGuid(), -1, Constants.Conventions.MediaTypes.Folder, 0);
						//Debug.WriteLine("Saving folder1 on thread: " + Thread.CurrentThread.ManagedThreadId);
						mediaService.Save(folder1, 0);

						Thread.Sleep(100); //quick pause for maximum overlap!

                        var folder2 = mediaService.CreateMedia("test" + Guid.NewGuid(), -1, Constants.Conventions.MediaTypes.Folder, 0);
						//Debug.WriteLine("Saving folder2 on thread: " + Thread.CurrentThread.ManagedThreadId);
						mediaService.Save(folder2, 0);
					}
					catch (Exception e)
					{
                        lock (exceptions) { exceptions.Add(e); }
                    }
				});
				threads.Add(t);
			}

			//start all threads
			threads.ForEach(x => x.Start());

			//wait for all to complete
			threads.ForEach(x => x.Join());

			//kill them all
            // uh? no!
			//threads.ForEach(x => x.Abort());

			if (exceptions.Count == 0)
			{
				//now look up all items, there should be 40!
				var items = mediaService.GetRootMedia();
				Assert.AreEqual(40, items.Count());
			}
			else
			{
                throw new Exception("Exceptions!", exceptions.First()); // rethrow the first one...
            }

		}

		public void CreateTestData()
		{
			//Create and Save ContentType "umbTextpage" -> 1045
			ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
			contentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
			ServiceContext.ContentTypeService.Save(contentType);
		}

		/// <summary>
		/// A special implementation of <see cref="IDatabaseFactory"/> that mimics the DefaultDatabaseFactory
		/// (one db per HttpContext) by providing one db per thread, as required for multi-threaded
		/// tests.
		/// </summary>
		internal class PerThreadSqlCeDatabaseFactory : DisposableObject, IDatabaseFactory
		{
            // the DefaultDatabaseFactory uses thread-static databases where there is no http context,
            // so it would need to be disposed in each thread in order for each database to be disposed,
            // instead we use this factory which also maintains one database per thread but can dispose
            // them all in one call

		    private readonly ILogger _logger;
		    private readonly IMapperCollection _mappers;
            private IQueryFactory _queryFactory;

            private readonly DbProviderFactory _dbProviderFactory =
		        DbProviderFactories.GetFactory(Constants.DbProviderNames.SqlCe);

		    public bool Configured => true;
            public bool CanConnect => true;

            public void Configure(string connectionString, string providerName)
		    {
		        throw new NotImplementedException();
		    }

		    public ISqlSyntaxProvider SqlSyntax { get; } = new SqlCeSyntaxProvider();

		    public IQueryFactory QueryFactory => _queryFactory ?? (_queryFactory = new QueryFactory(SqlSyntax, _mappers));

		    public DatabaseType DatabaseType => DatabaseType.SQLCe;

            public PerThreadSqlCeDatabaseFactory(ILogger logger, IMapperCollection mappers)
            {
                _logger = logger;
                _mappers = mappers;
            }

		    private readonly ConcurrentDictionary<int, UmbracoDatabase> _databases = new ConcurrentDictionary<int, UmbracoDatabase>();

			public UmbracoDatabase GetDatabase()
			{
			    var settings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];
                return _databases.GetOrAdd(Thread.CurrentThread.ManagedThreadId,
                    i => new UmbracoDatabase(settings.ConnectionString, SqlSyntax, DatabaseType, _dbProviderFactory, _logger));
			}

			protected override void DisposeResources()
			{
				// dispose the databases
			    foreach (var database in _databases.Values) database.Dispose();
			    _databases.Clear();
			}
		}
	}
}