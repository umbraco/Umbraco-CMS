using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class DomainRepositoryTest : TestWithDatabaseBase
    {
        private DomainRepository CreateRepository(IScopeProvider provider, out ContentTypeRepository contentTypeRepository, out DocumentRepository documentRepository, out LanguageRepository languageRepository)
        {
            var accessor = (IScopeAccessor) provider;
            var templateRepository = new TemplateRepository(accessor, Core.Cache.AppCaches.Disabled, Logger, TestObjects.GetFileSystemsMock());
            var tagRepository = new TagRepository(accessor, Core.Cache.AppCaches.Disabled, Logger);
            var commonRepository = new ContentTypeCommonRepository(accessor, templateRepository, AppCaches);
            contentTypeRepository = new ContentTypeRepository(accessor, Core.Cache.AppCaches.Disabled, Logger, commonRepository);
            languageRepository = new LanguageRepository(accessor, Core.Cache.AppCaches.Disabled, Logger);
            documentRepository = new DocumentRepository(accessor, Core.Cache.AppCaches.Disabled, Logger, contentTypeRepository, templateRepository, tagRepository, languageRepository);
            var domainRepository = new DomainRepository(accessor, Core.Cache.AppCaches.Disabled, Logger);
            return domainRepository;
        }

        private int CreateTestData(string isoName, out ContentType ct)
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var lang = new Language(isoName);
                langRepo.Save(lang);

                ct = MockedContentTypes.CreateBasicContentType("test", "Test");
                contentTypeRepo.Save(ct);
                var content = new Content("test", -1, ct) { CreatorId = 0, WriterId = 0 };
                documentRepo.Save(content);
                scope.Complete();
                return content.Id;
            }
        }

        [Test]
        public void Can_Create_And_Get_By_Id()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = documentRepo.Get(contentId);

                var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.Save(domain);

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

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var content = documentRepo.Get(contentId);

                var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id };
                repo.Save(domain);

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

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = documentRepo.Get(contentId);

                var domain1 = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.Save(domain1);

                var domain2 = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };

                Assert.Throws<DuplicateNameException>(() => repo.Save(domain2));
            }
        }

        [Test]
        public void Can_Delete()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = documentRepo.Get(contentId);

                var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.Save(domain);

                repo.Delete(domain);

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

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var content1 = documentRepo.Get(contentId1);

                //more test data
                var lang1 = langRepo.GetByIsoCode("en-AU");
                var lang2 = new Language("es");
                langRepo.Save(lang2);
                var content2 = new Content("test", -1, ct) { CreatorId = 0, WriterId = 0 };
                documentRepo.Save(content2);


                var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content1.Id, LanguageId = lang1.Id };
                repo.Save(domain);

                //re-get
                domain = repo.Get(domain.Id);

                domain.DomainName = "blah.com";
                domain.RootContentId = content2.Id;
                domain.LanguageId = lang2.Id;
                repo.Save(domain);

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

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = documentRepo.Get(contentId);

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain("test" + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                    repo.Save(domain);
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

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = documentRepo.Get(contentId);

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain("test" + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                    repo.Save(domain);
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

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = documentRepo.Get(contentId);

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain("test " + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                    repo.Save(domain);
                }

                var all = repo.GetMany();

                Assert.AreEqual(10, all.Count());
            }
        }

        [Test]
        public void Get_All_Ids()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = documentRepo.Get(contentId);

                var ids = new List<int>();
                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain("test " + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                    repo.Save(domain);
                    ids.Add(domain.Id);
                }

                var all = repo.GetMany(ids.Take(8).ToArray());

                Assert.AreEqual(8, all.Count());
            }
        }

        [Test]
        public void Get_All_Without_Wildcards()
        {
            ContentType ct;
            var contentId = CreateTestData("en-AU", out ct);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var lang = langRepo.GetByIsoCode("en-AU");
                var content = documentRepo.Get(contentId);

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain((i % 2 == 0) ? "test " + i + ".com" : ("*" + i))
                    {
                        RootContentId = content.Id,
                        LanguageId = lang.Id
                    };
                    repo.Save(domain);
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

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var contentItems = new List<IContent>();

                var lang = langRepo.GetByIsoCode("en-AU");
                contentItems.Add(documentRepo.Get(contentId));

                //more test data (3 content items total)
                for (int i = 0; i < 2; i++)
                {
                    var c = new Content("test" + i, -1, ct) { CreatorId = 0, WriterId = 0 };
                    documentRepo.Save(c);
                    contentItems.Add(c);
                }

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain((i % 2 == 0) ? "test " + i + ".com" : ("*" + i))
                    {
                        RootContentId = ((i % 2 == 0) ? contentItems[0] : contentItems[1]).Id,
                        LanguageId = lang.Id
                    };
                    repo.Save(domain);
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

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                DocumentRepository documentRepo;
                LanguageRepository langRepo;
                ContentTypeRepository contentTypeRepo;

                var repo = CreateRepository(provider, out contentTypeRepo, out documentRepo, out langRepo);

                var contentItems = new List<IContent>();

                var lang = langRepo.GetByIsoCode("en-AU");
                contentItems.Add(documentRepo.Get(contentId));

                //more test data (3 content items total)
                for (int i = 0; i < 2; i++)
                {
                    var c = new Content("test" + i, -1, ct) { CreatorId = 0, WriterId = 0 };
                    documentRepo.Save(c);
                    contentItems.Add(c);
                }

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain((i % 2 == 0) ? "test " + i + ".com" : ("*" + i))
                    {
                        RootContentId = ((i % 2 == 0) ? contentItems[0] : contentItems[1]).Id,
                        LanguageId = lang.Id
                    };
                    repo.Save(domain);
                }

                var all1 = repo.GetAssignedDomains(contentItems[0].Id, false);
                Assert.AreEqual(5, all1.Count());

                var all2 = repo.GetAssignedDomains(contentItems[1].Id, false);
                Assert.AreEqual(0, all2.Count());
            }
        }
    }
}
