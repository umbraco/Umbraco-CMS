using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    // these tests tend to fail from time to time esp. on VSTS
    //
    // read
    // Lock Time-out: https://technet.microsoft.com/en-us/library/ms172402.aspx?f=255&MSPPError=-2147217396
    //    http://support.x-tensive.com/question/5242/strange-locking-exceptions-with-sqlserverce
    //    http://debuggingblog.com/wp/2009/05/07/high-cpu-usage-and-windows-forms-application-hang-with-sqlce-database-and-the-sqlcelocktimeoutexception/
    //
    // tried to increase it via connection string or via SET LOCK_TIMEOUT
    // but still, the test fails on VSTS in most cases, so now ignoring it,
    // as I could not figure out _why_ and it does not look like we are
    // causing it, getting into __sysObjects locks, no idea why

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ThreadSafetyServiceTest : TestWithDatabaseBase
    {
        public override void SetUp()
        {
            base.SetUp();
            CreateTestData();
        }

        // not sure this is doing anything really
        protected override string GetDbConnectionString()
        {
            // need a longer timeout for tests?
            return base.GetDbConnectionString() + "default lock timeout=60000;";
        }

        private const int MaxThreadCount = 20;

        private void Save(ContentService service, IContent content)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.Database.Execute("SET LOCK_TIMEOUT 60000");
                service.Save(content);
                scope.Complete();
            }
        }

        private void Save(MediaService service, IMedia media)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.Database.Execute("SET LOCK_TIMEOUT 60000");
                service.Save(media);
                scope.Complete();
            }
        }

        private ManualResetEventSlim TraceLocks()
        {
            var done = new ManualResetEventSlim(false);

            // comment out to trace locks
            return done;

            //new Thread(() =>
            //{
            //    using (var scope = ScopeProvider.CreateScope())
            //    while (done.IsSet == false)
            //    {
            //        var db = scope.Database;
            //        var info = db.Query<dynamic>("SELECT * FROM sys.lock_information;");
            //        Console.WriteLine("LOCKS:");
            //        foreach (var row in info)
            //        {
            //            Console.WriteLine("> " + row.request_spid + " " + row.resource_type + " " + row.resource_description + " " + row.request_mode + " " + row.resource_table + " " + row.resource_table_id + " " + row.request_status);
            //        }
            //        Thread.Sleep(50);
            //    }
            //}).Start();
            //return done;
        }

        [Test]
        public void Ensure_All_Threads_Execute_Successfully_Content_Service()
        {
            if (Environment.GetEnvironmentVariable("UMBRACO_TMP") != null)
                Assert.Ignore("Do not run on VSTS.");

            // the ServiceContext in that each repository in a service (i.e. ContentService) is a singleton
            var contentService = (ContentService)ServiceContext.ContentService;

            var threads = new List<Thread>();
            var exceptions = new List<Exception>();

            Console.WriteLine("Starting...");

            var done = TraceLocks();

            for (var i = 0; i < MaxThreadCount; i++)
            {
                var t = new Thread(() =>
                {
                    try
                    {
                        Console.WriteLine("[{0}] Running...", Thread.CurrentThread.ManagedThreadId);

                        var name1 = "test-" + Guid.NewGuid();
                        var content1 = contentService.Create(name1, -1, "umbTextpage");

                        Console.WriteLine("[{0}] Saving content #1.", Thread.CurrentThread.ManagedThreadId);
                        Save(contentService, content1);

                        Thread.Sleep(100); //quick pause for maximum overlap!

                        var name2 = "test-" + Guid.NewGuid();
                        var content2 = contentService.Create(name2, -1, "umbTextpage");

                        Console.WriteLine("[{0}] Saving content #2.", Thread.CurrentThread.ManagedThreadId);
                        Save(contentService, content2);
                    }
                    catch (Exception e)
                    {
                        lock (exceptions) { exceptions.Add(e); }
                    }
                });
                threads.Add(t);
            }

            // start all threads
            Console.WriteLine("Starting threads");
            threads.ForEach(x => x.Start());

            // wait for all to complete
            Console.WriteLine("Joining threads");
            threads.ForEach(x => x.Join());

            done.Set();

            Console.WriteLine("Checking exceptions");
            if (exceptions.Count == 0)
            {
                //now look up all items, there should be 40!
                var items = contentService.GetRootContent();
                Assert.AreEqual(2 * MaxThreadCount, items.Count());
            }
            else
            {
                throw new Exception("Exceptions!", exceptions.First()); // rethrow the first one...
            }
        }

        [Test]
        public void Ensure_All_Threads_Execute_Successfully_Media_Service()
        {
            if (Environment.GetEnvironmentVariable("UMBRACO_TMP") != null)
                Assert.Ignore("Do not run on VSTS.");
            // mimick the ServiceContext in that each repository in a service (i.e. ContentService) is a singleton
            var mediaService = (MediaService)ServiceContext.MediaService;

            var threads = new List<Thread>();
            var exceptions = new List<Exception>();

            Console.WriteLine("Starting...");

            var done = TraceLocks();

            for (var i = 0; i < MaxThreadCount; i++)
            {
                var t = new Thread(() =>
                {
                    try
                    {
                        Console.WriteLine("[{0}] Running...", Thread.CurrentThread.ManagedThreadId);

                        var name1 = "test-" + Guid.NewGuid();
                        var media1 = mediaService.CreateMedia(name1, -1, Constants.Conventions.MediaTypes.Folder);
                        Console.WriteLine("[{0}] Saving media #1.", Thread.CurrentThread.ManagedThreadId);
                        Save(mediaService, media1);

                        Thread.Sleep(100); //quick pause for maximum overlap!

                        var name2 = "test-" + Guid.NewGuid();
                        var media2 = mediaService.CreateMedia(name2, -1, Constants.Conventions.MediaTypes.Folder);
                        Console.WriteLine("[{0}] Saving media #2.", Thread.CurrentThread.ManagedThreadId);
                        Save(mediaService, media2);
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

            done.Set();

            if (exceptions.Count == 0)
            {
                // now look up all items, there should be 40!
                var items = mediaService.GetRootMedia();
                Assert.AreEqual(2 * MaxThreadCount, items.Count());
            }
            else
            {
                throw new Exception("Exceptions!", exceptions.First()); // rethrow the first one...
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
