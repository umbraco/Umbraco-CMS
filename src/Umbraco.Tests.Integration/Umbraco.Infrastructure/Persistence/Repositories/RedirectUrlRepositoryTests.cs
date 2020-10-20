﻿using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class RedirectUrlRepositoryTests : UmbracoIntegrationTest
    {
        [SetUp]
        public void SetUp()
        {
            CreateTestData();
        }

        [Test]
        public void CanSaveAndGet()
        {
            var provider = ScopeProvider;

            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository(provider);
                var rurl = new RedirectUrl
                {
                    ContentKey = _textpage.Key,
                    Url = "blah"
                };
                repo.Save(rurl);
                scope.Complete();

                Assert.AreNotEqual(0, rurl.Id);
            }

            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository(provider);
                var rurl = repo.GetMostRecentUrl("blah");
                scope.Complete();

                Assert.IsNotNull(rurl);
                Assert.AreEqual(_textpage.Id, rurl.ContentId);
            }
        }

        [Test]
        public void CanSaveAndGetMostRecent()
        {
            var provider = ScopeProvider;

            Assert.AreNotEqual(_textpage.Id, _otherpage.Id);

            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository(provider);
                var rurl = new RedirectUrl
                {
                    ContentKey = _textpage.Key,
                    Url = "blah"
                };
                repo.Save(rurl);
                scope.Complete();

                Assert.AreNotEqual(0, rurl.Id);

                // FIXME: too fast = same date = key violation?
                // and... can that happen in real life?
                // we don't really *care* about the IX, only supposed to make things faster...
                // BUT in realife we AddOrUpdate in a trx so it should be safe, always

                rurl = new RedirectUrl
                {
                    ContentKey = _otherpage.Key,
                    Url = "blah",
                    CreateDateUtc = rurl.CreateDateUtc.AddSeconds(1) // ensure time difference
                };
                repo.Save(rurl);
                scope.Complete();

                Assert.AreNotEqual(0, rurl.Id);
            }

            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository(provider);
                var rurl = repo.GetMostRecentUrl("blah");
                scope.Complete();

                Assert.IsNotNull(rurl);
                Assert.AreEqual(_otherpage.Id, rurl.ContentId);
            }
        }

        [Test]
        public void CanSaveAndGetByContent()
        {
            var provider = ScopeProvider;

            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository(provider);
                var rurl = new RedirectUrl
                {
                    ContentKey = _textpage.Key,
                    Url = "blah"
                };
                repo.Save(rurl);
                scope.Complete();

                Assert.AreNotEqual(0, rurl.Id);

                // FIXME: goes too fast and bam, errors, first is blah

                rurl = new RedirectUrl
                {
                    ContentKey = _textpage.Key,
                    Url = "durg",
                    CreateDateUtc = rurl.CreateDateUtc.AddSeconds(1) // ensure time difference
                };
                repo.Save(rurl);
                scope.Complete();

                Assert.AreNotEqual(0, rurl.Id);
            }

            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository(provider);
                var rurls = repo.GetContentUrls(_textpage.Key).ToArray();
                scope.Complete();

                Assert.AreEqual(2, rurls.Length);
                Assert.AreEqual("durg", rurls[0].Url);
                Assert.AreEqual("blah", rurls[1].Url);
            }
        }

        [Test]
        public void CanSaveAndDelete()
        {
            var provider = ScopeProvider;

            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository(provider);
                var rurl = new RedirectUrl
                {
                    ContentKey = _textpage.Key,
                    Url = "blah"
                };
                repo.Save(rurl);
                scope.Complete();

                Assert.AreNotEqual(0, rurl.Id);

                rurl = new RedirectUrl
                {
                    ContentKey = _otherpage.Key,
                    Url = "durg"
                };
                repo.Save(rurl);
                scope.Complete();

                Assert.AreNotEqual(0, rurl.Id);
            }

            using (var scope = provider.CreateScope())
            {
                var repo = CreateRepository(provider);
                repo.DeleteContentUrls(_textpage.Key);
                scope.Complete();

                var rurls = repo.GetContentUrls(_textpage.Key);

                Assert.AreEqual(0, rurls.Count());
            }
        }

        private IRedirectUrlRepository CreateRepository(IScopeProvider provider)
        {
            return new RedirectUrlRepository((IScopeAccessor) provider, AppCaches, LoggerFactory.CreateLogger<RedirectUrlRepository>());
        }

        private IContent _textpage, _subpage, _otherpage, _trashed;

        public void CreateTestData()
        {
            var fileService = GetRequiredService<IFileService>();
            var template = TemplateBuilder.CreateTextPageTemplate();
            fileService.SaveTemplate(template); // else, FK violation on contentType!

            var contentService = GetRequiredService<IContentService>();
            var contentTypeService = GetRequiredService<IContentTypeService>();
            //Create and Save ContentType "umbTextpage" -> (NodeDto.NodeIdSeed)
            var contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
            contentType.Key = Guid.NewGuid();
            contentTypeService.Save(contentType);

            //Create and Save Content "Homepage" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 1)
            _textpage = ContentBuilder.CreateSimpleContent(contentType);
            _textpage.Key = Guid.NewGuid();
            contentService.Save(_textpage);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 2)
            _subpage = ContentBuilder.CreateSimpleContent(contentType, "Text Page 1", _textpage.Id);
            _subpage.Key = Guid.NewGuid();
            contentService.Save(_subpage);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 3)
            _otherpage = ContentBuilder.CreateSimpleContent(contentType, "Text Page 2", _textpage.Id);
            _otherpage.Key = Guid.NewGuid();
            contentService.Save(_otherpage);

            //Create and Save Content "Text Page Deleted" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 4)
            _trashed = ContentBuilder.CreateSimpleContent(contentType, "Text Page Deleted", -20);
            _trashed.Key = Guid.NewGuid();
            ((Content) _trashed).Trashed = true;
            contentService.Save(_trashed);
        }
    }
}
