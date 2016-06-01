using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class DomainRepositoryTest : BaseDatabaseFactoryTest
    {
        private DomainRepository CreateRepository(IDatabaseUnitOfWork unitOfWork, out ContentTypeRepository contentTypeRepository, out ContentRepository contentRepository, out LanguageRepository languageRepository)
        {
            var templateRepository = new TemplateRepository(unitOfWork, DisabledCache, Logger, Mock.Of<IFileSystem>(), Mock.Of<IFileSystem>(), Mock.Of<ITemplatesSection>(), MappingResolver);
            var tagRepository = new TagRepository(unitOfWork, DisabledCache, Logger, MappingResolver);
            contentTypeRepository = new ContentTypeRepository(unitOfWork, DisabledCache, Logger, templateRepository, MappingResolver);
            contentRepository = new ContentRepository(unitOfWork, DisabledCache, Logger, contentTypeRepository, templateRepository, tagRepository, Mock.Of<IContentSection>(), MappingResolver);
            languageRepository = new LanguageRepository(unitOfWork, DisabledCache, Logger, MappingResolver);
            var domainRepository = new DomainRepository(unitOfWork, DisabledCache, Logger, MappingResolver);
            return domainRepository;
        }

        private int CreateTestData(string isoName, out ContentType ct)
        {
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var lang = new Language(isoName);
                langRepo.AddOrUpdate(lang);

                ct = MockedContentTypes.CreateBasicContentType("test", "Test");
                contentTypeRepo.AddOrUpdate(ct);
                var content = new Content("test", -1, ct) { CreatorId = 0, WriterId = 0 };
                contentRepo.AddOrUpdate(content);
                unitOfWork.Complete();
                return content.Id;
            }
        }

        [Test]
        public void Can_Create_And_Get_By_Id()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = contentRepo.Get(contentId);

                var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.AddOrUpdate(domain);
                unitOfWork.Flush();

                //re-get
                domain = repo.Get(domain.Id);

                Assert.NotNull(domain);
                Assert.IsTrue(domain.HasIdentity);
                Assert.Greater(domain.Id, 0);
                Assert.AreEqual("test.com", domain.DomainName);
                Assert.AreEqual(content.Id, domain.RootContentId);
                Assert.AreEqual(lang.Id, domain.LanguageId);
                Assert.AreEqual(lang.IsoCode, domain.LanguageIsoCode);
            }
        }

        [Test]
        public void Can_Create_And_Get_By_Id_Empty_lang()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var content = contentRepo.Get(contentId);

                var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id };
                repo.AddOrUpdate(domain);
                unitOfWork.Flush();

                //re-get
                domain = repo.Get(domain.Id);

                Assert.NotNull(domain);
                Assert.IsTrue(domain.HasIdentity);
                Assert.Greater(domain.Id, 0);
                Assert.AreEqual("test.com", domain.DomainName);
                Assert.AreEqual(content.Id, domain.RootContentId);
                Assert.IsFalse(domain.LanguageId.HasValue);
            }
        }

        [Test]
        public void Cant_Create_Duplicate_Domain_Name()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = contentRepo.Get(contentId);

                var domain1 = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.AddOrUpdate(domain1);
                unitOfWork.Flush();

                var domain2 = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.AddOrUpdate(domain2);

                Assert.Throws<DuplicateNameException>(unitOfWork.Flush);
            }
        }

        [Test]
        public void Can_Delete()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = contentRepo.Get(contentId);

                var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.AddOrUpdate(domain);
                unitOfWork.Flush();

                repo.Delete(domain);
                unitOfWork.Flush();

                //re-get
                domain = repo.Get(domain.Id);


                Assert.IsNull(domain);
            }
        }

        [Test]
        public void Can_Update()
        {
            ContentType ct;
            var contentId1 = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var content1 = contentRepo.Get(contentId1);

                //more test data
                var lang1 = langRepo.GetByIsoCode("en-AU");
                var lang2 = new Language("es");
                langRepo.AddOrUpdate(lang2);
                var content2 = new Content("test", -1, ct) { CreatorId = 0, WriterId = 0 };
                contentRepo.AddOrUpdate(content2);
                unitOfWork.Flush();


                var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content1.Id, LanguageId = lang1.Id };
                repo.AddOrUpdate(domain);
                unitOfWork.Flush();

                //re-get
                domain = repo.Get(domain.Id);

                domain.DomainName = "blah.com";
                domain.RootContentId = content2.Id;
                domain.LanguageId = lang2.Id;
                repo.AddOrUpdate(domain);
                unitOfWork.Flush();

                //re-get
                domain = repo.Get(domain.Id);

                Assert.AreEqual("blah.com", domain.DomainName);
                Assert.AreEqual(content2.Id, domain.RootContentId);
                Assert.AreEqual(lang2.Id, domain.LanguageId);
                Assert.AreEqual(lang2.IsoCode, domain.LanguageIsoCode);
            }
        }


        [Test]
        public void Exists()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = contentRepo.Get(contentId);

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain("test" + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                    repo.AddOrUpdate(domain);
                    unitOfWork.Flush();
                }

                var found = repo.Exists("test1.com");

                Assert.IsTrue(found);
            }
        }

        [Test]
        public void Get_By_Name()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = contentRepo.Get(contentId);

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain("test" + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                    repo.AddOrUpdate(domain);
                    unitOfWork.Flush();
                }

                var found = repo.GetByName("test1.com");

                Assert.IsNotNull(found);
            }
        }

        [Test]
        public void Get_All()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = contentRepo.Get(contentId);

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain("test " + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                    repo.AddOrUpdate(domain);
                    unitOfWork.Flush();
                }

                var all = repo.GetAll();

                Assert.AreEqual(10, all.Count());
            }
        }

        [Test]
        public void Get_All_Ids()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = contentRepo.Get(contentId);

                var ids = new List<int>();
                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain("test " + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                    repo.AddOrUpdate(domain);
                    unitOfWork.Flush();
                    ids.Add(domain.Id);
                }

                var all = repo.GetAll(ids.Take(8).ToArray());

                Assert.AreEqual(8, all.Count());
            }
        }

        [Test]
        public void Get_All_Without_Wildcards()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = contentRepo.Get(contentId);

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain((i % 2 == 0) ? "test " + i + ".com" : ("*" + i))
                    {
                        RootContentId = content.Id,
                        LanguageId = lang.Id
                    };
                    repo.AddOrUpdate(domain);
                    unitOfWork.Flush();
                }

                var all = repo.GetAll(false);

                Assert.AreEqual(5, all.Count());
            }
        }

        [Test]
        public void Get_All_For_Content()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var contentItems = new List<IContent>();

                var lang = langRepo.GetByIsoCode("en-AU");
                contentItems.Add(contentRepo.Get(contentId));

                //more test data (3 content items total)
                for (int i = 0; i < 2; i++)
                {
                    var c = new Content("test" + i, -1, ct) { CreatorId = 0, WriterId = 0 };
                    contentRepo.AddOrUpdate(c);
                    unitOfWork.Flush();
                    contentItems.Add(c);
                }

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain((i % 2 == 0) ? "test " + i + ".com" : ("*" + i))
                    {
                        RootContentId = ((i % 2 == 0) ? contentItems[0] : contentItems[1]).Id,
                        LanguageId = lang.Id
                    };
                    repo.AddOrUpdate(domain);
                    unitOfWork.Flush();
                }

                var all1 = repo.GetAssignedDomains(contentItems[0].Id, true);
                Assert.AreEqual(5, all1.Count());

                var all2 = repo.GetAssignedDomains(contentItems[1].Id, true);
                Assert.AreEqual(5, all2.Count());

                var all3 = repo.GetAssignedDomains(contentItems[2].Id, true);
                Assert.AreEqual(0, all3.Count());
            }
        }

        [Test]
        public void Get_All_For_Content_Without_Wildcards()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                ContentRepository contentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(unitOfWork, out contentTypeRepo, out contentRepo, out langRepo);

                var contentItems = new List<IContent>();

                var lang = langRepo.GetByIsoCode("en-AU");
                contentItems.Add(contentRepo.Get(contentId));

                //more test data (3 content items total)
                for (int i = 0; i < 2; i++)
                {
                    var c = new Content("test" + i, -1, ct) { CreatorId = 0, WriterId = 0 };
                    contentRepo.AddOrUpdate(c);
                    unitOfWork.Flush();
                    contentItems.Add(c);
                }

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain((i % 2 == 0) ? "test " + i + ".com" : ("*" + i))
                    {
                        RootContentId = ((i % 2 == 0) ? contentItems[0] : contentItems[1]).Id,
                        LanguageId = lang.Id
                    };
                    repo.AddOrUpdate(domain);
                    unitOfWork.Flush();
                }

                var all1 = repo.GetAssignedDomains(contentItems[0].Id, false);
                Assert.AreEqual(5, all1.Count());

                var all2 = repo.GetAssignedDomains(contentItems[1].Id, false);
                Assert.AreEqual(0, all2.Count());
            }
        }
    }
}