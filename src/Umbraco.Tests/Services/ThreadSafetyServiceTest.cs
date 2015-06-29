using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using umbraco.editorControls.tinyMCE3;
using umbraco.interfaces;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
	[TestFixture, RequiresSTA]
	public class ThreadSafetyServiceTest : BaseDatabaseFactoryTest
	{
		private PerThreadPetaPocoUnitOfWorkProvider _uowProvider;
		private PerThreadDatabaseFactory _dbFactory;

		[SetUp]
		public override void Initialize()
		{            
			base.Initialize();
			
			//we need to use our own custom IDatabaseFactory for the DatabaseContext because we MUST ensure that 
			//a Database instance is created per thread, whereas the default implementation which will work in an HttpContext
			//threading environment, or a single apartment threading environment will not work for this test because 
			//it is multi-threaded.
			_dbFactory = new PerThreadDatabaseFactory(Logger);
			//overwrite the local object
            ApplicationContext.DatabaseContext = new DatabaseContext(_dbFactory, Logger, new SqlCeSyntaxProvider(), "System.Data.SqlServerCe.4.0");

            //disable cache
		    var cacheHelper = CacheHelper.CreateDisabledCacheHelper();

			//here we are going to override the ServiceContext because normally with our test cases we use a 
			//global Database object but this is NOT how it should work in the web world or in any multi threaded scenario.
			//we need a new Database object for each thread.
            var repositoryFactory = new RepositoryFactory(cacheHelper, Logger, SqlSyntax, SettingsForTests.GenerateMockSettings());
			_uowProvider = new PerThreadPetaPocoUnitOfWorkProvider(_dbFactory);
            ApplicationContext.Services = new ServiceContext(
                repositoryFactory,
                _uowProvider, 
                new FileUnitOfWorkProvider(), 
                new PublishingStrategy(), 
                cacheHelper, 
                Logger);

			CreateTestData();
		}

		[TearDown]
		public override void TearDown()
		{
			_error = null;

			//dispose!
			_dbFactory.Dispose();
			_uowProvider.Dispose();

			base.TearDown();
		}

		/// <summary>
		/// Used to track exceptions during multi-threaded tests, volatile so that it is not locked in CPU registers.
		/// </summary>
		private volatile Exception _error = null;

		private const int MaxThreadCount = 20;

		[Test]
		public void Ensure_All_Threads_Execute_Successfully_Content_Service()
		{
			//we will mimick the ServiceContext in that each repository in a service (i.e. ContentService) is a singleton
			var contentService = (ContentService)ServiceContext.ContentService;
			
			var threads = new List<Thread>();
			
			Debug.WriteLine("Starting test...");

			for (var i = 0; i < MaxThreadCount; i++)
			{
				var t = new Thread(() =>
					{
						try
						{
							Debug.WriteLine("Created content on thread: " + Thread.CurrentThread.ManagedThreadId);							
							
							//create 2 content items

                            string name1 = "test" + Guid.NewGuid();
							var content1 = contentService.CreateContent(name1, -1, "umbTextpage", 0);
							
							Debug.WriteLine("Saving content1 on thread: " + Thread.CurrentThread.ManagedThreadId);
							contentService.Save(content1);

							Thread.Sleep(100); //quick pause for maximum overlap!

                            string name2 = "test" + Guid.NewGuid();
							var content2 = contentService.CreateContent(name2, -1, "umbTextpage", 0);
							Debug.WriteLine("Saving content2 on thread: " + Thread.CurrentThread.ManagedThreadId);
							contentService.Save(content2);	
						}
						catch(Exception e)
						{														
							_error = e;
						}						
					});
				threads.Add(t);
			}

			//start all threads
			threads.ForEach(x => x.Start());

			//wait for all to complete
			threads.ForEach(x => x.Join());

			//kill them all
			threads.ForEach(x => x.Abort());

			if (_error == null)
			{
				//now look up all items, there should be 40!
				var items = contentService.GetRootContent();
				Assert.AreEqual(40, items.Count());
			}
			else
			{
			    throw new Exception("Error!", _error);
			}
			
		}

		[Test]
		public void Ensure_All_Threads_Execute_Successfully_Media_Service()
		{
			//we will mimick the ServiceContext in that each repository in a service (i.e. ContentService) is a singleton
			var mediaService = (MediaService)ServiceContext.MediaService;

			var threads = new List<Thread>();

			Debug.WriteLine("Starting test...");

			for (var i = 0; i < MaxThreadCount; i++)
			{
				var t = new Thread(() =>
				{
					try
					{
						Debug.WriteLine("Created content on thread: " + Thread.CurrentThread.ManagedThreadId);

						//create 2 content items

                        string name1 = "test" + Guid.NewGuid();
					    var folder1 = mediaService.CreateMedia(name1, -1, Constants.Conventions.MediaTypes.Folder, 0);
						Debug.WriteLine("Saving folder1 on thread: " + Thread.CurrentThread.ManagedThreadId);
						mediaService.Save(folder1, 0);

						Thread.Sleep(100); //quick pause for maximum overlap!

                        string name = "test" + Guid.NewGuid();
                        var folder2 = mediaService.CreateMedia(name, -1, Constants.Conventions.MediaTypes.Folder, 0);
						Debug.WriteLine("Saving folder2 on thread: " + Thread.CurrentThread.ManagedThreadId);
						mediaService.Save(folder2, 0);
					}
					catch (Exception e)
					{
						_error = e;
					}
				});
				threads.Add(t);
			}

			//start all threads
			threads.ForEach(x => x.Start());

			//wait for all to complete
			threads.ForEach(x => x.Join());

			//kill them all
			threads.ForEach(x => x.Abort());

			if (_error == null)
			{
				//now look up all items, there should be 40!
				var items = mediaService.GetRootMedia();
				Assert.AreEqual(40, items.Count());
			}
			else
			{
				Assert.Fail("ERROR! " + _error);
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
		/// Creates a Database object per thread, this mimics the web context which is per HttpContext and is required for the multi-threaded test
		/// </summary>
		internal class PerThreadDatabaseFactory : DisposableObject, IDatabaseFactory
		{
		    private readonly ILogger _logger;

		    public PerThreadDatabaseFactory(ILogger logger)
		    {
		        _logger = logger;
		    }

		    private readonly ConcurrentDictionary<int, UmbracoDatabase> _databases = new ConcurrentDictionary<int, UmbracoDatabase>(); 

			public UmbracoDatabase CreateDatabase()
			{
				var db = _databases.GetOrAdd(
                    Thread.CurrentThread.ManagedThreadId,
                    i => new UmbracoDatabase(Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName, _logger));
				return db;
			}

			protected override void DisposeResources()
			{
				//dispose the databases
				_databases.ForEach(x => x.Value.Dispose());
			}
		}

		/// <summary>
		/// Creates a UOW with a Database object per thread
		/// </summary>
		internal class PerThreadPetaPocoUnitOfWorkProvider : DisposableObject, IDatabaseUnitOfWorkProvider
		{
			private readonly PerThreadDatabaseFactory _dbFactory;

			public PerThreadPetaPocoUnitOfWorkProvider(PerThreadDatabaseFactory dbFactory)
			{
				_dbFactory = dbFactory;
			}

			public IDatabaseUnitOfWork GetUnitOfWork()
			{
				//Create or get a database instance for this thread.
				var db = _dbFactory.CreateDatabase();
				return new PetaPocoUnitOfWork(db);
			}

			protected override void DisposeResources()
			{
				//dispose the databases
				_dbFactory.Dispose();
			}
		}

	}
}