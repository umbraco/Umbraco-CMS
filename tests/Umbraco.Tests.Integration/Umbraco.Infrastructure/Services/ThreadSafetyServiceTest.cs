// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
{
    // Every thread's saves queue up on the content tree write lock, so under a slow database the last waiters can
    // exceed the default five-second lock timeout while nothing is actually wrong. These tests prove the services
    // survive the contention, not that the queue drains within the default, so the fixture raises the timeout.
    // The lock mechanism issues SET LOCK_TIMEOUT from that setting on every acquisition, which is why setting it on
    // the connection here would not stick.
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
    internal sealed class ThreadSafetyServiceTest : UmbracoIntegrationTest
    {
        private IContentService ContentService => GetRequiredService<IContentService>();

        private IMediaService MediaService => GetRequiredService<IMediaService>();

        private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

        protected override void CustomTestSetup(IUmbracoBuilder builder)
            => builder.Services.Configure<GlobalSettings>(settings =>
                settings.DistributedLockingWriteLockDefaultTimeout = TimeSpan.FromSeconds(60));

        [SetUp]
        public async Task SetUp()
        {
            await CreateTestData();
        }

        private const int MaxThreadCount = 20;

        private void Save(ContentService service, IContent content)
        {
            using (IScope scope = ScopeProvider.CreateScope())
            {
                service.SaveAsync(content, Constants.Security.SuperUserKey, null, CancellationToken.None).GetAwaiter().GetResult();
                scope.Complete();
            }
        }

        private void Save(MediaService service, IMedia media)
        {
            using (IScope scope = ScopeProvider.CreateScope())
            {
                service.Save(media);
                scope.Complete();
            }
        }

        private ManualResetEventSlim TraceLocks()
        {
            var done = new ManualResetEventSlim(false);

            // comment out to trace locks
            return done;

            // new Thread(() =>
            // {
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
            // }).Start();
            // return done;
        }

        [Test]
        [LongRunning]
        public void Ensure_All_Threads_Execute_Successfully_Content_Service()
        {
            if (Environment.GetEnvironmentVariable("UMBRACO_TMP") != null)
            {
                Assert.Ignore("Do not run on VSTS.");
            }

            ILogger<ThreadSafetyServiceTest> log = GetRequiredService<ILogger<ThreadSafetyServiceTest>>();

            // the ServiceContext in that each repository in a service (i.e. ContentService) is a singleton
            var contentService = (ContentService)ContentService;

            var threads = new List<Thread>();
            var exceptions = new List<Exception>();

            log.LogInformation("Starting...");

            ManualResetEventSlim done = TraceLocks();

            for (int i = 0; i < MaxThreadCount; i++)
            {
                var t = new Thread(() =>
                {
                    try
                    {
                        // NOTE: This is NULL because we have supressed the execution context flow.
                        // If we don't do that we will get various exceptions because we're trying to run concurrent threads
                        // against an ambient context which cannot be done due to the rules of scope creation and completion.
                        // But this works in v8 without the supression!? Why?
                        // In v8 the value of the AmbientScope is simply the current CallContext (i.e. AsyncLocal) Value which
                        // is not a mutable Stack like we are maintaining now. This means that for each child thread
                        // in v8, that thread will see it's own CallContext Scope value that it set and not the 'true'
                        // ambient Scope like we do now.
                        // So although the test passes in v8, there's actually some strange things occuring because Scopes
                        // are being created and disposed concurrently and out of order.
                        var currentScope = ScopeAccessor.AmbientScope;
                        log.LogInformation("[{ThreadId}] Current Scope? {CurrentScope}", Thread.CurrentThread.ManagedThreadId, currentScope?.GetDebugInfo());
                        Assert.IsNull(currentScope);

                        string name1 = "test-" + Guid.NewGuid();
                        IContent content1 = contentService.CreateAsync(name1, (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None).GetAwaiter().GetResult();

                        log.LogInformation("[{ThreadId}] Saving content #1.", Thread.CurrentThread.ManagedThreadId);
                        Save(contentService, content1);

                        Thread.Sleep(100); // quick pause for maximum overlap!

                        string name2 = "test-" + Guid.NewGuid();
                        IContent content2 = contentService.CreateAsync(name2, (Guid?)null, "umbTextpage", Constants.Security.SuperUserKey, CancellationToken.None).GetAwaiter().GetResult();

                        log.LogInformation("[{ThreadId}] Saving content #2.", Thread.CurrentThread.ManagedThreadId);
                        Save(contentService, content2);
                    }
                    catch (Exception e)
                    {
                        // throw;
                        lock (exceptions)
                        {
                            exceptions.Add(e);
                        }
                    }
                });
                threads.Add(t);
            }

            // See NOTE above, we must supress flow here to be able to run concurrent threads,
            // else the AsyncLocal value from this current context will flow to the child threads.
            using (ExecutionContext.SuppressFlow())
            {
                // start all threads
                log.LogInformation("Starting threads");
                threads.ForEach(x => x.Start());
            }

            // wait for all to complete
            log.LogInformation("Joining threads");
            threads.ForEach(x => x.Join());

            done.Set();

            log.LogInformation("Checking exceptions");
            if (exceptions.Count == 0)
            {
                // now look up all items, there should be 40!
                IEnumerable<IContent> items = contentService.GetRootContentAsync(CancellationToken.None).GetAwaiter().GetResult();
                Assert.AreEqual(2 * MaxThreadCount, items.Count());
            }
            else
            {
                throw new Exception("Exceptions!", exceptions.First()); // rethrow the first one...
            }
        }

        [Test]
        [LongRunning]
        public void Ensure_All_Threads_Execute_Successfully_Media_Service()
        {
            if (Environment.GetEnvironmentVariable("UMBRACO_TMP") != null)
            {
                Assert.Ignore("Do not run on VSTS.");
            }

            ILogger<ThreadSafetyServiceTest> log = GetRequiredService<ILogger<ThreadSafetyServiceTest>>();

            // mimick the ServiceContext in that each repository in a service (i.e. ContentService) is a singleton
            var mediaService = (MediaService)MediaService;

            var threads = new List<Thread>();
            var exceptions = new List<Exception>();

            log.LogInformation("Starting...");

            ManualResetEventSlim done = TraceLocks();

            for (int i = 0; i < MaxThreadCount; i++)
            {
                var t = new Thread(() =>
                {
                    try
                    {
                        // NOTE: This is NULL because we have supressed the execution context flow.
                        // If we don't do that we will get various exceptions because we're trying to run concurrent threads
                        // against an ambient context which cannot be done due to the rules of scope creation and completion.
                        // But this works in v8 without the supression!? Why?
                        // In v8 the value of the AmbientScope is simply the current CallContext (i.e. AsyncLocal) Value which
                        // is not a mutable Stack like we are maintaining now. This means that for each child thread
                        // in v8, that thread will see it's own CallContext Scope value that it set and not the 'true'
                        // ambient Scope like we do now.
                        // So although the test passes in v8, there's actually some strange things occuring because Scopes
                        // are being created and disposed concurrently and out of order.
                        var currentScope = ScopeAccessor.AmbientScope;
                        log.LogInformation("[{ThreadId}] Current Scope? {CurrentScope}", Thread.CurrentThread.ManagedThreadId, currentScope?.GetDebugInfo());
                        Assert.IsNull(currentScope);

                        string name1 = "test-" + Guid.NewGuid();
                        IMedia media1 = mediaService.CreateMedia(name1, -1, Constants.Conventions.MediaTypes.Folder);
                        log.LogInformation("[{0}] Saving media #1.", Thread.CurrentThread.ManagedThreadId);
                        Save(mediaService, media1);

                        Thread.Sleep(100); // quick pause for maximum overlap!

                        string name2 = "test-" + Guid.NewGuid();
                        IMedia media2 = mediaService.CreateMedia(name2, -1, Constants.Conventions.MediaTypes.Folder);
                        log.LogInformation("[{0}] Saving media #2.", Thread.CurrentThread.ManagedThreadId);
                        Save(mediaService, media2);
                    }
                    catch (Exception e)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(e);
                        }
                    }
                });
                threads.Add(t);
            }

            // See NOTE above, we must supress flow here to be able to run concurrent threads,
            // else the AsyncLocal value from this current context will flow to the child threads.
            using (ExecutionContext.SuppressFlow())
            {
                // start all threads
                threads.ForEach(x => x.Start());
            }

            // wait for all to complete
            threads.ForEach(x => x.Join());

            done.Set();

            if (exceptions.Count == 0)
            {
                // now look up all items, there should be 40!
                IEnumerable<IMedia> items = mediaService.GetRootMedia();
                Assert.AreEqual(2 * MaxThreadCount, items.Count());
            }
            else
            {
                throw new Exception("Exceptions!", exceptions.First()); // rethrow the first one...
            }
        }

        public async Task CreateTestData()
        {
            // Create and Save ContentType "umbTextpage" -> 1045
            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage");
            contentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
            await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        }
    }
}
