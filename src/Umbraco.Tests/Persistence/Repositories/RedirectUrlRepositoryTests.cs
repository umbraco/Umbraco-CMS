using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class RedirectUrlRepositoryTests : BaseDatabaseFactoryTest
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
            base.TearDown();
        }

        [Test]
        public void CanSaveAndGet()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);

            using (var uow = provider.GetUnitOfWork())
            using (var repo = CreateRepository(uow))
            {
                var rurl = new RedirectUrl
                {
                    ContentKey = _textpage.Key,
                    Url = "blah"
                };
                repo.AddOrUpdate(rurl);
                uow.Commit();

                Assert.AreNotEqual(0, rurl.Id);
            }

            using (var uow = provider.GetUnitOfWork())
            using (var repo = CreateRepository(uow))
            {
                var rurl = repo.GetMostRecentUrl("blah");
                uow.Commit();

                Assert.IsNotNull(rurl);
                Assert.AreEqual(_textpage.Id, rurl.ContentId);
            }
        }

        [Test]
        public void CanSaveAndGetMostRecent()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);

            Assert.AreNotEqual(_textpage.Id, _otherpage.Id);

            using (var uow = provider.GetUnitOfWork())
            using (var repo = CreateRepository(uow))
            {
                var rurl = new RedirectUrl
                {
                    ContentKey = _textpage.Key,
                    Url = "blah"
                };
                repo.AddOrUpdate(rurl);
                uow.Commit();

                Assert.AreNotEqual(0, rurl.Id);

                // fixme - too fast = same date = key violation?
                // and... can that happen in real life?
                // we don't really *care* about the IX, only supposed to make things faster...
                // BUT in realife we AddOrUpdate in a trx so it should be safe, always

                rurl = new RedirectUrl
                {
                    ContentKey = _otherpage.Key,
                    Url = "blah",
                    CreateDateUtc = rurl.CreateDateUtc.AddSeconds(1) // ensure time difference
                };
                repo.AddOrUpdate(rurl);
                uow.Commit();

                Assert.AreNotEqual(0, rurl.Id);
            }

            using (var uow = provider.GetUnitOfWork())
            using (var repo = CreateRepository(uow))
            {
                var rurl = repo.GetMostRecentUrl("blah");
                uow.Commit();

                Assert.IsNotNull(rurl);
                Assert.AreEqual(_otherpage.Id, rurl.ContentId);
            }
        }

        [Test]
        public void CanSaveAndGetByContent()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);

            using (var uow = provider.GetUnitOfWork())
            using (var repo = CreateRepository(uow))
            {
                var rurl = new RedirectUrl
                {
                    ContentKey = _textpage.Key,
                    Url = "blah"
                };
                repo.AddOrUpdate(rurl);
                uow.Commit();

                Assert.AreNotEqual(0, rurl.Id);

                // fixme - goes too fast and bam, errors, first is blah

                rurl = new RedirectUrl
                {
                    ContentKey = _textpage.Key,
                    Url = "durg",
                    CreateDateUtc = rurl.CreateDateUtc.AddSeconds(1) // ensure time difference
                };
                repo.AddOrUpdate(rurl);
                uow.Commit();

                Assert.AreNotEqual(0, rurl.Id);
            }

            using (var uow = provider.GetUnitOfWork())
            using (var repo = CreateRepository(uow))
            {
                var rurls = repo.GetContentUrls(_textpage.Key).ToArray();
                uow.Commit();

                Assert.AreEqual(2, rurls.Length);
                Assert.AreEqual("durg", rurls[0].Url);
                Assert.AreEqual("blah", rurls[1].Url);
            }
        }

        [Test]
        public void CanSaveAndDelete()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);

            using (var uow = provider.GetUnitOfWork())
            using (var repo = CreateRepository(uow))
            {
                var rurl = new RedirectUrl
                {
                    ContentKey = _textpage.Key,
                    Url = "blah"
                };
                repo.AddOrUpdate(rurl);
                uow.Commit();

                Assert.AreNotEqual(0, rurl.Id);

                rurl = new RedirectUrl
                {
                    ContentKey = _otherpage.Key,
                    Url = "durg"
                };
                repo.AddOrUpdate(rurl);
                uow.Commit();

                Assert.AreNotEqual(0, rurl.Id);
            }

            using (var uow = provider.GetUnitOfWork())
            using (var repo = CreateRepository(uow))
            {
                repo.DeleteContentUrls(_textpage.Key);
                uow.Commit();

                var rurls = repo.GetContentUrls(_textpage.Key);

                Assert.AreEqual(0, rurls.Count());
            }
        }

        private IRedirectUrlRepository CreateRepository(IScopeUnitOfWork uow)
        {
            return new RedirectUrlRepository(uow, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax);
        }

        private IContent _textpage, _subpage, _otherpage, _trashed;

        public void CreateTestData()
        {
            //Create and Save ContentType "umbTextpage" -> (NodeDto.NodeIdSeed)
            var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            contentType.Key = Guid.NewGuid();
            ServiceContext.ContentTypeService.Save(contentType);

            //Create and Save Content "Homepage" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 1)
            _textpage = MockedContent.CreateSimpleContent(contentType);
            _textpage.Key = Guid.NewGuid();
            ServiceContext.ContentService.Save(_textpage);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 2)
            _subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", _textpage.Id);
            _subpage.Key = Guid.NewGuid();
            ServiceContext.ContentService.Save(_subpage);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 3)
            _otherpage = MockedContent.CreateSimpleContent(contentType, "Text Page 2", _textpage.Id);
            _otherpage.Key = Guid.NewGuid();
            ServiceContext.ContentService.Save(_otherpage);

            //Create and Save Content "Text Page Deleted" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 4)
            _trashed = MockedContent.CreateSimpleContent(contentType, "Text Page Deleted", -20);
            _trashed.Key = Guid.NewGuid();
            ((Content) _trashed).Trashed = true;
            ServiceContext.ContentService.Save(_trashed);
        }
    }
}
