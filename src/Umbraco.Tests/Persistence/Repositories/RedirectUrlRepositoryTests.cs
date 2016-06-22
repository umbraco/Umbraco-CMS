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
                    ContentId = _textpage.Id,
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

            using (var uow = provider.GetUnitOfWork())
            using (var repo = CreateRepository(uow))
            {
                var rurl = new RedirectUrl
                {
                    ContentId = _textpage.Id,
                    Url = "blah"
                };
                repo.AddOrUpdate(rurl);
                uow.Commit();

                Assert.AreNotEqual(0, rurl.Id);

                rurl = new RedirectUrl
                {
                    ContentId = _otherpage.Id,
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
                    ContentId = _textpage.Id,
                    Url = "blah"
                };
                repo.AddOrUpdate(rurl);
                uow.Commit();

                Assert.AreNotEqual(0, rurl.Id);

                rurl = new RedirectUrl
                {
                    ContentId = _textpage.Id,
                    Url = "durg"
                };
                repo.AddOrUpdate(rurl);
                uow.Commit();

                Assert.AreNotEqual(0, rurl.Id);
            }

            using (var uow = provider.GetUnitOfWork())
            using (var repo = CreateRepository(uow))
            {
                var rurls = repo.GetContentUrls(_textpage.Id).ToArray();
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
                    ContentId = _textpage.Id,
                    Url = "blah"
                };
                repo.AddOrUpdate(rurl);
                uow.Commit();

                Assert.AreNotEqual(0, rurl.Id);

                rurl = new RedirectUrl
                {
                    ContentId = _otherpage.Id,
                    Url = "durg"
                };
                repo.AddOrUpdate(rurl);
                uow.Commit();

                Assert.AreNotEqual(0, rurl.Id);
            }

            using (var uow = provider.GetUnitOfWork())
            using (var repo = CreateRepository(uow))
            {
                repo.DeleteContentUrls(_textpage.Id);
                uow.Commit();

                var rurls = repo.GetContentUrls(_textpage.Id);

                Assert.AreEqual(0, rurls.Count());
            }
        }

        private IRedirectUrlRepository CreateRepository(IDatabaseUnitOfWork uow)
        {
            return new RedirectUrlRepository(uow, CacheHelper.CreateDisabledCacheHelper(), Logger, SqlSyntax);
        }

        private IContent _textpage, _subpage, _otherpage, _trashed;

        public void CreateTestData()
        {
            //Create and Save ContentType "umbTextpage" -> (NodeDto.NodeIdSeed)
            var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            contentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
            ServiceContext.ContentTypeService.Save(contentType);

            //Create and Save Content "Homepage" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 1)
            _textpage = MockedContent.CreateSimpleContent(contentType);
            _textpage.Key = new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0");
            ServiceContext.ContentService.Save(_textpage, 0);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 2)
            _subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", _textpage.Id);
            _subpage.Key = new Guid("FF11402B-7E53-4654-81A7-462AC2108059");
            ServiceContext.ContentService.Save(_subpage, 0);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 3)
            _otherpage = MockedContent.CreateSimpleContent(contentType, "Text Page 2", _textpage.Id);
            ServiceContext.ContentService.Save(_otherpage, 0);

            //Create and Save Content "Text Page Deleted" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 4)
            _trashed = MockedContent.CreateSimpleContent(contentType, "Text Page Deleted", -20);
            ((Content) _trashed).Trashed = true;
            ServiceContext.ContentService.Save(_trashed, 0);
        }
    }
}
