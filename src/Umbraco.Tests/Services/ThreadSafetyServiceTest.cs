using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
	[TestFixture, RequiresSTA]
    [Ignore("Temp. disabled, issues on VSTS?")]
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

	    protected override string GetDbConnectionString()
	    {
            // need a longer timeout for tests?
	        return base.GetDbConnectionString() + "default lock timeout=10000;";
	    }

	    /// <summary>
		/// Used to track exceptions during multi-threaded tests, volatile so that it is not locked in CPU registers.
		/// </summary>
		private volatile Exception _error;

		private const int MaxThreadCount = 20;

		[Test]
		public void Ensure_All_Threads_Execute_Successfully_Content_Service()
		{
			// the ServiceContext in that each repository in a service (i.e. ContentService) is a singleton
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

                            var name1 = "test-" + Guid.NewGuid();
							var content1 = contentService.CreateContent(name1, -1, "umbTextpage");
						    Debug.WriteLine("[{0}] Saving content #1.", Thread.CurrentThread.ManagedThreadId);
							contentService.Save(content1);

							Thread.Sleep(100); // quick pause for maximum overlap

                            var name2 = "test-" + Guid.NewGuid();
							var content2 = contentService.CreateContent(name2, -1, "umbTextpage");
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

			// start all threads
			threads.ForEach(x => x.Start());

			// wait for all to complete
			threads.ForEach(x => x.Join());

			if (_error == null)
			{
				// now look up all items, there should be 40!
				var items = contentService.GetRootContent();
				Assert.AreEqual(2 * MaxThreadCount, items.Count());
			}
			else
			{
			    Assert.Fail("ERROR! " + _error);
            }
		}

		[Test]
		public void Ensure_All_Threads_Execute_Successfully_Media_Service()
		{
			// mimick the ServiceContext in that each repository in a service (i.e. ContentService) is a singleton
			var mediaService = (MediaService) ServiceContext.MediaService;

			var threads = new List<Thread>();

			Debug.WriteLine("Starting...");

			for (var i = 0; i < MaxThreadCount; i++)
			{
				var t = new Thread(() =>
				{
					try
					{
                        Debug.WriteLine("[{0}] Running...", Thread.CurrentThread.ManagedThreadId);

                        var name1 = "test-" + Guid.NewGuid();
					    var media1 = mediaService.CreateMedia(name1, -1, Constants.Conventions.MediaTypes.Folder);
                        Debug.WriteLine("[{0}] Saving content #1.", Thread.CurrentThread.ManagedThreadId);
                        mediaService.Save(media1);

						Thread.Sleep(100); // quick pause for maximum overlap

                        var name2 = "test-" + Guid.NewGuid();
                        var media2 = mediaService.CreateMedia(name2, -1, Constants.Conventions.MediaTypes.Folder);
                        Debug.WriteLine("[{0}] Saving content #2.", Thread.CurrentThread.ManagedThreadId);
                        mediaService.Save(media2);
					}
					catch (Exception e)
					{
						_error = e;
					}
				});
				threads.Add(t);
			}

			// start all threads
			threads.ForEach(x => x.Start());

			// wait for all to complete
			threads.ForEach(x => x.Join());

			if (_error == null)
			{
				// now look up all items, there should be 40
				var items = mediaService.GetRootMedia();
				Assert.AreEqual(2 * MaxThreadCount, items.Count());
			}
			else
			{
				Assert.Fail("ERROR! " + _error);
			}

		}

		public void CreateTestData()
		{
			// Create and Save ContentType "umbTextpage" -> 1045
			var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
			contentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
			ServiceContext.ContentTypeService.Save(contentType);
		}
	}
}