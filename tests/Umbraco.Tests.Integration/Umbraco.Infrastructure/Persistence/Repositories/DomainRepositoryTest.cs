// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class DomainRepositoryTest : UmbracoIntegrationTest
    {
        private ILanguageRepository LanguageRepository => GetRequiredService<ILanguageRepository>();

        private IDocumentRepository DocumentRepository => GetRequiredService<IDocumentRepository>();

        private IContentTypeRepository ContentTypeRepository => GetRequiredService<IContentTypeRepository>();

        private DomainRepository CreateRepository(IScopeProvider provider)
        {
            var accessor = (IScopeAccessor)provider;
            var domainRepository = new DomainRepository(accessor, AppCaches.NoCache, LoggerFactory.CreateLogger<DomainRepository>());
            return domainRepository;
        }

        private int CreateTestData(string isoName, out ContentType ct)
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var globalSettings = new GlobalSettings();
                var lang = new Language(globalSettings, isoName);
                LanguageRepository.Save(lang);

                ct = ContentTypeBuilder.CreateBasicContentType("test", "Test");
                ContentTypeRepository.Save(ct);
                var content = new Content("test", -1, ct) { CreatorId = 0, WriterId = 0 };
                DocumentRepository.Save(content);
                scope.Complete();
                return content.Id;
            }
        }

        [Test]
        public void Can_Create_And_Get_By_Id()
        {
            int contentId = CreateTestData("en-AU", out ContentType ct);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DomainRepository repo = CreateRepository(provider);

                ILanguage lang = LanguageRepository.GetByIsoCode("en-AU");
                IContent content = DocumentRepository.Get(contentId);

                var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.Save(domain);

                // re-get
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
            int contentId = CreateTestData("en-AU", out ContentType ct);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DomainRepository repo = CreateRepository(provider);

                IContent content = DocumentRepository.Get(contentId);

                var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id };
                repo.Save(domain);

                // re-get
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
            int contentId = CreateTestData("en-AU", out ContentType ct);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DomainRepository repo = CreateRepository(provider);

                ILanguage lang = LanguageRepository.GetByIsoCode("en-AU");
                IContent content = DocumentRepository.Get(contentId);

                var domain1 = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.Save(domain1);

                var domain2 = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };

                Assert.Throws<DuplicateNameException>(() => repo.Save(domain2));
            }
        }

        [Test]
        public void Can_Delete()
        {
            int contentId = CreateTestData("en-AU", out ContentType ct);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DomainRepository repo = CreateRepository(provider);

                ILanguage lang = LanguageRepository.GetByIsoCode("en-AU");
                IContent content = DocumentRepository.Get(contentId);

                var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.Save(domain);

                repo.Delete(domain);

                // re-get
                domain = repo.Get(domain.Id);

                Assert.IsNull(domain);
            }
        }

        [Test]
        public void Can_Update()
        {
            int contentId1 = CreateTestData("en-AU", out ContentType ct);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DomainRepository repo = CreateRepository(provider);

                IContent content1 = DocumentRepository.Get(contentId1);

                // more test data
                ILanguage lang1 = LanguageRepository.GetByIsoCode("en-AU");
                var globalSettings = new GlobalSettings();
                var lang2 = new Language(globalSettings, "es");
                LanguageRepository.Save(lang2);
                var content2 = new Content("test", -1, ct) { CreatorId = 0, WriterId = 0 };
                DocumentRepository.Save(content2);

                var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content1.Id, LanguageId = lang1.Id };
                repo.Save(domain);

                // re-get
                domain = repo.Get(domain.Id);

                domain.DomainName = "blah.com";
                domain.RootContentId = content2.Id;
                domain.LanguageId = lang2.Id;
                repo.Save(domain);

                // re-get
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
            int contentId = CreateTestData("en-AU", out ContentType ct);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DomainRepository repo = CreateRepository(provider);

                ILanguage lang = LanguageRepository.GetByIsoCode("en-AU");
                IContent content = DocumentRepository.Get(contentId);

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain("test" + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                    repo.Save(domain);
                }

                bool found = repo.Exists("test1.com");

                Assert.IsTrue(found);
            }
        }

        [Test]
        public void Get_By_Name()
        {
            int contentId = CreateTestData("en-AU", out ContentType ct);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DomainRepository repo = CreateRepository(provider);

                ILanguage lang = LanguageRepository.GetByIsoCode("en-AU");
                IContent content = DocumentRepository.Get(contentId);

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain("test" + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                    repo.Save(domain);
                }

                IDomain found = repo.GetByName("test1.com");

                Assert.IsNotNull(found);
            }
        }

        [Test]
        public void Get_All()
        {
            int contentId = CreateTestData("en-AU", out ContentType ct);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DomainRepository repo = CreateRepository(provider);

                ILanguage lang = LanguageRepository.GetByIsoCode("en-AU");
                IContent content = DocumentRepository.Get(contentId);

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain("test " + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                    repo.Save(domain);
                }

                IEnumerable<IDomain> all = repo.GetMany();

                Assert.AreEqual(10, all.Count());
            }
        }

        [Test]
        public void Get_All_Ids()
        {
            int contentId = CreateTestData("en-AU", out ContentType ct);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DomainRepository repo = CreateRepository(provider);

                ILanguage lang = LanguageRepository.GetByIsoCode("en-AU");
                IContent content = DocumentRepository.Get(contentId);

                var ids = new List<int>();
                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain("test " + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                    repo.Save(domain);
                    ids.Add(domain.Id);
                }

                IEnumerable<IDomain> all = repo.GetMany(ids.Take(8).ToArray());

                Assert.AreEqual(8, all.Count());
            }
        }

        [Test]
        public void Get_All_Without_Wildcards()
        {
            int contentId = CreateTestData("en-AU", out ContentType ct);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DomainRepository repo = CreateRepository(provider);

                ILanguage lang = LanguageRepository.GetByIsoCode("en-AU");
                IContent content = DocumentRepository.Get(contentId);

                for (int i = 0; i < 10; i++)
                {
                    var domain = (IDomain)new UmbracoDomain((i % 2 == 0) ? "test " + i + ".com" : ("*" + i))
                    {
                        RootContentId = content.Id,
                        LanguageId = lang.Id
                    };
                    repo.Save(domain);
                }

                IEnumerable<IDomain> all = repo.GetAll(false);

                Assert.AreEqual(5, all.Count());
            }
        }

        [Test]
        public void Get_All_For_Content()
        {
            int contentId = CreateTestData("en-AU", out ContentType ct);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DomainRepository repo = CreateRepository(provider);

                var contentItems = new List<IContent>();

                ILanguage lang = LanguageRepository.GetByIsoCode("en-AU");
                contentItems.Add(DocumentRepository.Get(contentId));

                // more test data (3 content items total)
                for (int i = 0; i < 2; i++)
                {
                    var c = new Content("test" + i, -1, ct) { CreatorId = 0, WriterId = 0 };
                    DocumentRepository.Save(c);
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

                IEnumerable<IDomain> all1 = repo.GetAssignedDomains(contentItems[0].Id, true);
                Assert.AreEqual(5, all1.Count());

                IEnumerable<IDomain> all2 = repo.GetAssignedDomains(contentItems[1].Id, true);
                Assert.AreEqual(5, all2.Count());

                IEnumerable<IDomain> all3 = repo.GetAssignedDomains(contentItems[2].Id, true);
                Assert.AreEqual(0, all3.Count());
            }
        }

        [Test]
        public void Get_All_For_Content_Without_Wildcards()
        {
            int contentId = CreateTestData("en-AU", out ContentType ct);

            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                DomainRepository repo = CreateRepository(provider);

                var contentItems = new List<IContent>();

                ILanguage lang = LanguageRepository.GetByIsoCode("en-AU");
                contentItems.Add(DocumentRepository.Get(contentId));

                // more test data (3 content items total)
                for (int i = 0; i < 2; i++)
                {
                    var c = new Content("test" + i, -1, ct) { CreatorId = 0, WriterId = 0 };
                    DocumentRepository.Save(c);
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

                IEnumerable<IDomain> all1 = repo.GetAssignedDomains(contentItems[0].Id, false);
                Assert.AreEqual(5, all1.Count());

                IEnumerable<IDomain> all2 = repo.GetAssignedDomains(contentItems[1].Id, false);
                Assert.AreEqual(0, all2.Count());
            }
        }
    }
}
