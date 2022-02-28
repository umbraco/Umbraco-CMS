// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.TestHelpers.Stubs;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ContentServicePerformanceTest : UmbracoIntegrationTest
    {
        protected DocumentRepository DocumentRepository => (DocumentRepository)GetRequiredService<IDocumentRepository>();

        protected IFileService FileService => GetRequiredService<IFileService>();

        protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

        protected IContentService ContentService => GetRequiredService<IContentService>();

        protected IContentType ContentType { get; set; }

        [SetUp]
        public void SetUpData() => CreateTestData();

        [Test]
        public void Profiler() => Assert.IsInstanceOf<TestProfiler>(GetRequiredService<IProfiler>());

        private static IProfilingLogger GetTestProfilingLogger()
        {
            var profiler = new TestProfiler();
            return new ProfilingLogger(new NullLogger<ProfilingLogger>(), profiler);
        }

        [Test]
        public void Retrieving_All_Content_In_Site()
        {
            // NOTE: Doing this the old 1 by 1 way and based on the results of the ContentServicePerformanceTest.Retrieving_All_Content_In_Site
            // the old way takes 143795ms, the new above way takes:
            // 14249ms
            //
            // ... NOPE, made some new changes, it is now....
            // 5290ms  !!!!!!
            //
            // that is a 96% savings of processing and sql calls!
            //
            // ... NOPE, made even more nice changes, it is now...
            // 4452ms !!!!!!!
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType1 = ContentTypeBuilder.CreateTextPageContentType("test1", "test1", defaultTemplateId: template.Id);
            ContentType contentType2 = ContentTypeBuilder.CreateTextPageContentType("test2", "test2", defaultTemplateId: template.Id);
            ContentType contentType3 = ContentTypeBuilder.CreateTextPageContentType("test3", "test3", defaultTemplateId: template.Id);
            ContentTypeService.Save(new[] { contentType1, contentType2, contentType3 });
            contentType1.AllowedContentTypes = new[]
            {
                new ContentTypeSort(new Lazy<int>(() => contentType2.Id), 0, contentType2.Alias),
                new ContentTypeSort(new Lazy<int>(() => contentType3.Id), 1, contentType3.Alias)
            };
            contentType2.AllowedContentTypes = new[]
            {
                new ContentTypeSort(new Lazy<int>(() => contentType1.Id), 0, contentType1.Alias),
                new ContentTypeSort(new Lazy<int>(() => contentType3.Id), 1, contentType3.Alias)
            };
            contentType3.AllowedContentTypes = new[]
            {
                new ContentTypeSort(new Lazy<int>(() => contentType1.Id), 0, contentType1.Alias),
                new ContentTypeSort(new Lazy<int>(() => contentType2.Id), 1, contentType2.Alias)
            };
            ContentTypeService.Save(new[] { contentType1, contentType2, contentType3 });

            IEnumerable<Content> roots = ContentBuilder.CreateTextpageContent(contentType1, -1, 10);
            ContentService.Save(roots);
            foreach (Content root in roots)
            {
                IEnumerable<Content> item1 = ContentBuilder.CreateTextpageContent(contentType1, root.Id, 10);
                IEnumerable<Content> item2 = ContentBuilder.CreateTextpageContent(contentType2, root.Id, 10);
                IEnumerable<Content> item3 = ContentBuilder.CreateTextpageContent(contentType3, root.Id, 10);

                ContentService.Save(item1.Concat(item2).Concat(item3));
            }

            var total = new List<IContent>();

            using (GetTestProfilingLogger().TraceDuration<ContentServicePerformanceTest>("Getting all content in site"))
            {
                TestProfiler.Enable();
                total.AddRange(ContentService.GetRootContent());
                foreach (IContent content in total.ToArray())
                {
                    total.AddRange(ContentService.GetPagedDescendants(content.Id, 0, int.MaxValue, out long _));
                }

                TestProfiler.Disable();
                StaticApplicationLogging.Logger.LogInformation("Returned {Total} items", total.Count);
            }
        }

        [Test]
        public void Creating_100_Items()
        {
            // Arrange
            IContentType contentType = ContentTypeService.Get(ContentType.Id);
            IEnumerable<Content> pages = ContentBuilder.CreateTextpageContent(contentType, -1, 100);

            // Act
            var watch = Stopwatch.StartNew();
            ContentService.Save(pages, 0);
            watch.Stop();
            long elapsed = watch.ElapsedMilliseconds;

            Debug.Print("100 content items saved in {0} ms", elapsed);

            // Assert
            Assert.That(pages.Any(x => x.HasIdentity == false), Is.False);
        }

        [Test]
        public void Creating_1000_Items()
        {
            // Arrange
            IContentType contentType = ContentTypeService.Get(ContentType.Id);
            IEnumerable<Content> pages = ContentBuilder.CreateTextpageContent(contentType, -1, 1000);

            // Act
            var watch = Stopwatch.StartNew();
            ContentService.Save(pages, 0);
            watch.Stop();
            long elapsed = watch.ElapsedMilliseconds;

            Debug.Print("100 content items saved in {0} ms", elapsed);

            // Assert
            Assert.That(pages.Any(x => x.HasIdentity == false), Is.False);
        }

        [Test]
        public void Getting_100_Uncached_Items()
        {
            // Arrange
            IContentType contentType = ContentTypeService.Get(ContentType.Id);
            IEnumerable<Content> pages = ContentBuilder.CreateTextpageContent(contentType, -1, 100);
            ContentService.Save(pages, 0);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DocumentRepository repository = DocumentRepository;

                // Act
                var watch = Stopwatch.StartNew();
                IEnumerable<IContent> contents = repository.GetMany();
                watch.Stop();
                long elapsed = watch.ElapsedMilliseconds;

                Debug.Print("100 content items retrieved in {0} ms without caching", elapsed);

                // Assert
                Assert.That(contents.Any(x => x.HasIdentity == false), Is.False);
                Assert.That(contents.Any(x => x == null), Is.False);
            }
        }

        [Test]
        public void Getting_1000_Uncached_Items()
        {
            // Arrange
            IContentType contentType = ContentTypeService.Get(ContentType.Id);
            IEnumerable<Content> pages = ContentBuilder.CreateTextpageContent(contentType, -1, 1000);
            ContentService.Save(pages, 0);

            using (IScope scope = ScopeProvider.CreateScope())
            {
                DocumentRepository repository = DocumentRepository;

                // Act
                var watch = Stopwatch.StartNew();
                IEnumerable<IContent> contents = repository.GetMany();
                watch.Stop();
                long elapsed = watch.ElapsedMilliseconds;

                Debug.Print("1000 content items retrieved in {0} ms without caching", elapsed);

                // Assert
                // Assert.That(contents.Any(x => x.HasIdentity == false), Is.False);
                // Assert.That(contents.Any(x => x == null), Is.False);
            }
        }

        [Test]
        public void Getting_100_Cached_Items()
        {
            // Arrange
            IContentType contentType = ContentTypeService.Get(ContentType.Id);
            IEnumerable<Content> pages = ContentBuilder.CreateTextpageContent(contentType, -1, 100);
            ContentService.Save(pages, 0);

            using (IScope scope = ScopeProvider.CreateScope())
            {
                DocumentRepository repository = DocumentRepository;

                // Act
                IEnumerable<IContent> contents = repository.GetMany();

                var watch = Stopwatch.StartNew();
                IEnumerable<IContent> contentsCached = repository.GetMany();
                watch.Stop();
                long elapsed = watch.ElapsedMilliseconds;

                Debug.Print("100 content items retrieved in {0} ms with caching", elapsed);

                // Assert
                Assert.That(contentsCached.Any(x => x.HasIdentity == false), Is.False);
                Assert.That(contentsCached.Any(x => x == null), Is.False);
                Assert.That(contentsCached.Count(), Is.EqualTo(contents.Count()));
            }
        }

        [Test]
        public void Getting_1000_Cached_Items()
        {
            // Arrange
            IContentType contentType = ContentTypeService.Get(ContentType.Id);
            IEnumerable<Content> pages = ContentBuilder.CreateTextpageContent(contentType, -1, 1000);
            ContentService.Save(pages, 0);

            using (IScope scope = ScopeProvider.CreateScope())
            {
                DocumentRepository repository = DocumentRepository;

                // Act
                IEnumerable<IContent> contents = repository.GetMany();

                var watch = Stopwatch.StartNew();
                IEnumerable<IContent> contentsCached = repository.GetMany();
                watch.Stop();
                long elapsed = watch.ElapsedMilliseconds;

                Debug.Print("1000 content items retrieved in {0} ms with caching", elapsed);

                // Assert
                // Assert.That(contentsCached.Any(x => x.HasIdentity == false), Is.False);
                // Assert.That(contentsCached.Any(x => x == null), Is.False);
                // Assert.That(contentsCached.Count(), Is.EqualTo(contents.Count()));
            }
        }

        public void CreateTestData()
        {
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            // Create and Save ContentType "textpage" -> ContentType.Id
            ContentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
            ContentTypeService.Save(ContentType);
        }
    }
}
