using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
	[TestFixture]
	public class ThreadSafetyServiceTest : BaseDatabaseFactoryTest
	{
		private PerThreadPetaPocoUnitOfWorkProvider _uowProvider;

		[SetUp]
		public override void Initialize()
		{
			base.Initialize();
			
			//here we are going to override the ServiceContext because normally with our test cases we use a 
			//global Database object but this is NOT how it should work in the web world or in any multi threaded scenario.
			//we need a new Database object for each thread.
			_uowProvider = new PerThreadPetaPocoUnitOfWorkProvider();			
			ServiceContext = new ServiceContext(_uowProvider, new FileUnitOfWorkProvider(), new PublishingStrategy());

			CreateTestData();
		}

		[TearDown]
		public override void TearDown()
		{
			_error = null;
			_lastUowIdWithThread = null;

			//dispose!
			_uowProvider.Dispose();

			base.TearDown();
			
			ServiceContext = null;
		}

		/// <summary>
		/// Used to track exceptions during multi-threaded tests, volatile so that it is not locked in CPU registers.
		/// </summary>
		private volatile Exception _error = null;

		private int _maxThreadCount = 20;
		private object _locker = new object();
		private Tuple<int, Guid> _lastUowIdWithThread = null;

		[Test]
		public void Ensure_All_Threads_Reference_Different_Units_Of_Work_Content_Service()
		{
			//we will mimick the ServiceContext in that each repository in a service (i.e. ContentService) is a singleton
			var contentService = (ContentService)ServiceContext.ContentService;
			
			var threads = new List<Thread>();
			
			Debug.WriteLine("Starting test...");

			//bind to event to determine what is going on during the saving process
			ContentService.Saving += HandleSaving;

			for (var i = 0; i < _maxThreadCount; i++)
			{
				var t = new Thread(() =>
					{
						try
						{
							Debug.WriteLine("Created content on thread: " + Thread.CurrentThread.ManagedThreadId);							
							
							//create 2 content items
							
							var content1 = contentService.CreateContent(-1, "umbTextpage", 0);
							content1.Name = "test" + Guid.NewGuid();
							Debug.WriteLine("Saving content1 on thread: " + Thread.CurrentThread.ManagedThreadId);
							contentService.Save(content1);	

							Thread.Sleep(100); //quick pause for maximum overlap!

							var content2 = contentService.CreateContent(-1, "umbTextpage", 0);
							content2.Name = "test" + Guid.NewGuid();
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
				Assert.Fail("ERROR! " + _error);
			}
			
		}

		[Test]
		public void Ensure_All_Threads_Reference_Different_Units_Of_Work_Media_Service()
		{
			//we will mimick the ServiceContext in that each repository in a service (i.e. ContentService) is a singleton
			var mediaService = (MediaService)ServiceContext.MediaService;

			var threads = new List<Thread>();

			Debug.WriteLine("Starting test...");

			//bind to event to determine what is going on during the saving process
			ContentService.Saving += HandleSaving;

			for (var i = 0; i < _maxThreadCount; i++)
			{
				var t = new Thread(() =>
				{
					try
					{
						var folderMediaType = ServiceContext.ContentTypeService.GetMediaType(1031);

						Debug.WriteLine("Created content on thread: " + Thread.CurrentThread.ManagedThreadId);

						//create 2 content items
						
						var folder1 = MockedMedia.CreateMediaFolder(folderMediaType, -1);
						folder1.Name = "test" + Guid.NewGuid();
						Debug.WriteLine("Saving folder1 on thread: " + Thread.CurrentThread.ManagedThreadId);
						mediaService.Save(folder1, 0);

						Thread.Sleep(100); //quick pause for maximum overlap!

						var folder2 = MockedMedia.CreateMediaFolder(folderMediaType, -1);
						folder2.Name = "test" + Guid.NewGuid();
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

		private void HandleSaving(object sender, SaveEventArgs args)
		{
			if (_error == null)
			{
				lock (_locker)
				{
					//get the instance id of the unit of work
					var uowId = ((PetaPocoUnitOfWork)args.UnitOfWork).InstanceId;
					if (_lastUowIdWithThread == null)
					{
						Debug.WriteLine("Initial UOW found = " + uowId + " with thread id " + Thread.CurrentThread.ManagedThreadId);

						_lastUowIdWithThread = new Tuple<int, Guid>(
							Thread.CurrentThread.ManagedThreadId, uowId);
					}
					else
					{
						Debug.WriteLine("Next thread running. UOW found = " + uowId + " with thread id " + Thread.CurrentThread.ManagedThreadId);

						var newTuple = new Tuple<int, Guid>(
							Thread.CurrentThread.ManagedThreadId, uowId);

						//check if the uowId is the same as the last and if the thread Id's are different then we have a problem
						if (newTuple.Item2 == _lastUowIdWithThread.Item2
							&& newTuple.Item1 != _lastUowIdWithThread.Item1)
						{
							_error = new Exception("The threads: " + newTuple.Item1 + " and " + _lastUowIdWithThread.Item1 + " are both referencing the Unit of work: " + _lastUowIdWithThread.Item2);
						}
					}
				}
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
		/// Creates a Database object per thread, this mimics the web context which is per HttpContext
		/// </summary>
		internal class PerThreadPetaPocoUnitOfWorkProvider : DisposableObject, IDatabaseUnitOfWorkProvider
		{			
			private readonly ConcurrentDictionary<int, Database> _databases = new ConcurrentDictionary<int, Database>(); 

			public IDatabaseUnitOfWork GetUnitOfWork()
			{
				//Create or get a database instance for this thread.
				var db = _databases.GetOrAdd(Thread.CurrentThread.ManagedThreadId, i => new Database(Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName));
				
				return new PetaPocoUnitOfWork(db);
			}

			protected override void DisposeResources()
			{
				//dispose the databases
				_databases.ForEach(x => x.Value.Dispose());
			}
		}

	}
}