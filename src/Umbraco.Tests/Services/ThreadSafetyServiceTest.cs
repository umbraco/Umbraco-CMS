using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
	[TestFixture, RequiresSTA]
	public class ThreadSafetyServiceTest : BaseDatabaseFactoryTest
	{
		[SetUp]
		public override void Initialize()
		{
			base.Initialize();

			CreateTestData();
		}

		[TearDown]
		public override void TearDown()
		{
			_error = null;

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

			Debug.WriteLine("Starting...");

			for (var i = 0; i < MaxThreadCount; i++)
			{
				var t = new Thread(() =>
					{
						try
						{
                            Debug.WriteLine("[{0}] Running...", Thread.CurrentThread.ManagedThreadId);

                            using (var scope = ApplicationContext.Current.ScopeProvider.CreateScope())
						    {
						        var database = scope.Database;
                                Debug.WriteLine("[{0}] Database {1}.", Thread.CurrentThread.ManagedThreadId, database.InstanceSid);
                            }

                            //create 2 content items

                            string name1 = "test" + Guid.NewGuid();
							var content1 = contentService.CreateContent(name1, -1, "umbTextpage", 0);

						    Debug.WriteLine("[{0}] Saving content #1.", Thread.CurrentThread.ManagedThreadId);
							contentService.Save(content1);

							Thread.Sleep(100); //quick pause for maximum overlap!

                            string name2 = "test" + Guid.NewGuid();
							var content2 = contentService.CreateContent(name2, -1, "umbTextpage", 0);
                            Debug.WriteLine("[{0}] Saving content #2.", Thread.CurrentThread.ManagedThreadId);
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

			Debug.WriteLine("Starting...");

			for (var i = 0; i < MaxThreadCount; i++)
			{
				var t = new Thread(() =>
				{
					try
					{
                        Debug.WriteLine("[{0}] Running...", Thread.CurrentThread.ManagedThreadId);

                        using (var scope = ApplicationContext.Current.ScopeProvider.CreateScope())
                        {
                            var database = scope.Database;
                            Debug.WriteLine("[{0}] Database {1}.", Thread.CurrentThread.ManagedThreadId, database.InstanceSid);
                        }

                        //create 2 content items

                        string name1 = "test" + Guid.NewGuid();
					    var folder1 = mediaService.CreateMedia(name1, -1, Constants.Conventions.MediaTypes.Folder, 0);
                        Debug.WriteLine("[{0}] Saving content #1.", Thread.CurrentThread.ManagedThreadId);
                        mediaService.Save(folder1, 0);

						Thread.Sleep(100); //quick pause for maximum overlap!

                        string name = "test" + Guid.NewGuid();
                        var folder2 = mediaService.CreateMedia(name, -1, Constants.Conventions.MediaTypes.Folder, 0);
                        Debug.WriteLine("[{0}] Saving content #2.", Thread.CurrentThread.ManagedThreadId);
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
	}
}