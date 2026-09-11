// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DomainRepositoryTest : UmbracoIntegrationTest
{
    private ILanguageRepository LanguageRepository => GetRequiredService<ILanguageRepository>();

    private IDocumentRepository DocumentRepository => GetRequiredService<IDocumentRepository>();

    private IContentTypeRepository ContentTypeRepository => GetRequiredService<IContentTypeRepository>();

    private DomainRepository CreateRepository(IScopeProvider provider)
    {
        var accessor = (IScopeAccessor)provider;
        var domainRepository =
            new DomainRepository(accessor, AppCaches.NoCache, LoggerFactory.CreateLogger<DomainRepository>(), Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());
        return domainRepository;
    }

    private int CreateTestData(string isoName, out ContentType ct)
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var lang = new Language(isoName, isoName);
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
        var contentId = CreateTestData("en-AU", out var ct);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var lang = LanguageRepository.GetByIsoCode("en-AU");
            var content = DocumentRepository.Get(contentId);

            var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
            repo.Save(domain);

            // re-get
            domain = repo.Get(domain.Id);

            Assert.That(domain, Is.Not.Null);
            Assert.That(domain.HasIdentity, Is.True);
            Assert.That(domain.Id, Is.GreaterThan(0));
            Assert.That(domain.DomainName, Is.EqualTo("test.com"));
            Assert.That(domain.RootContentId, Is.EqualTo(content.Id));
            Assert.That(domain.LanguageId, Is.EqualTo(lang.Id));
            Assert.That(domain.LanguageIsoCode, Is.EqualTo(lang.IsoCode));
        }
    }

    [Test]
    public void Can_Create_And_Get_By_Id_Empty_lang()
    {
        var contentId = CreateTestData("en-AU", out var ct);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var content = DocumentRepository.Get(contentId);

            var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id };
            repo.Save(domain);

            // re-get
            domain = repo.Get(domain.Id);

            Assert.That(domain, Is.Not.Null);
            Assert.That(domain.HasIdentity, Is.True);
            Assert.That(domain.Id, Is.GreaterThan(0));
            Assert.That(domain.DomainName, Is.EqualTo("test.com"));
            Assert.That(domain.RootContentId, Is.EqualTo(content.Id));
            Assert.That(domain.LanguageId.HasValue, Is.False);
        }
    }

    [Test]
    public void Cant_Create_Duplicate_Domain_Name()
    {
        var contentId = CreateTestData("en-AU", out var ct);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var lang = LanguageRepository.GetByIsoCode("en-AU");
            var content = DocumentRepository.Get(contentId);

            var domain1 = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
            repo.Save(domain1);

            var domain2 = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };

            Assert.Throws<DuplicateNameException>(() => repo.Save(domain2));
        }
    }

    [Test]
    public void Can_Delete()
    {
        var contentId = CreateTestData("en-AU", out var ct);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var lang = LanguageRepository.GetByIsoCode("en-AU");
            var content = DocumentRepository.Get(contentId);

            var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };
            repo.Save(domain);

            repo.Delete(domain);

            // re-get
            domain = repo.Get(domain.Id);

            Assert.That(domain, Is.Null);
        }
    }

    [Test]
    public void Can_Update()
    {
        var contentId1 = CreateTestData("en-AU", out var ct);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var content1 = DocumentRepository.Get(contentId1);

            // more test data
            var lang1 = LanguageRepository.GetByIsoCode("en-AU");
            var globalSettings = new GlobalSettings();
            var lang2 = new Language("es", "Spanish");
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

            Assert.That(domain.DomainName, Is.EqualTo("blah.com"));
            Assert.That(domain.RootContentId, Is.EqualTo(content2.Id));
            Assert.That(domain.LanguageId, Is.EqualTo(lang2.Id));
            Assert.That(domain.LanguageIsoCode, Is.EqualTo(lang2.IsoCode));
        }
    }

    [Test]
    public void Exists()
    {
        var contentId = CreateTestData("en-AU", out var ct);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var lang = LanguageRepository.GetByIsoCode("en-AU");
            var content = DocumentRepository.Get(contentId);

            for (var i = 0; i < 10; i++)
            {
                var domain =
                    (IDomain)new UmbracoDomain("test" + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.Save(domain);
            }

            var found = repo.Exists("test1.com");

            Assert.That(found, Is.True);
        }
    }

    [Test]
    public void Get_By_Name()
    {
        var contentId = CreateTestData("en-AU", out var ct);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var lang = LanguageRepository.GetByIsoCode("en-AU");
            var content = DocumentRepository.Get(contentId);

            for (var i = 0; i < 10; i++)
            {
                var domain =
                    (IDomain)new UmbracoDomain("test" + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.Save(domain);
            }

            var found = repo.GetByName("test1.com");

            Assert.That(found, Is.Not.Null);
        }
    }

    [Test]
    public void Get_All()
    {
        var contentId = CreateTestData("en-AU", out var ct);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var lang = LanguageRepository.GetByIsoCode("en-AU");
            var content = DocumentRepository.Get(contentId);

            for (var i = 0; i < 10; i++)
            {
                var domain =
                    (IDomain)new UmbracoDomain("test " + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.Save(domain);
            }

            var all = repo.GetMany();

            Assert.That(all.Count(), Is.EqualTo(10));
        }
    }

    [Test]
    public void Get_All_Ids()
    {
        var contentId = CreateTestData("en-AU", out var ct);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var lang = LanguageRepository.GetByIsoCode("en-AU");
            var content = DocumentRepository.Get(contentId);

            var ids = new List<int>();
            for (var i = 0; i < 10; i++)
            {
                var domain =
                    (IDomain)new UmbracoDomain("test " + i + ".com") { RootContentId = content.Id, LanguageId = lang.Id };
                repo.Save(domain);
                ids.Add(domain.Id);
            }

            var all = repo.GetMany(ids.Take(8).ToArray());

            Assert.That(all.Count(), Is.EqualTo(8));
        }
    }

    [Test]
    public void Get_All_Without_Wildcards()
    {
        var contentId = CreateTestData("en-AU", out var ct);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var lang = LanguageRepository.GetByIsoCode("en-AU");
            var content = DocumentRepository.Get(contentId);

            for (var i = 0; i < 10; i++)
            {
                var domain = (IDomain)new UmbracoDomain(i % 2 == 0 ? "test " + i + ".com" : "*" + i)
                {
                    RootContentId = content.Id,
                    LanguageId = lang.Id
                };
                repo.Save(domain);
            }

            var all = repo.GetAll(false);

            Assert.That(all.Count(), Is.EqualTo(5));
        }
    }

    [Test]
    public void Get_All_For_Content()
    {
        var contentId = CreateTestData("en-AU", out var ct);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var contentItems = new List<IContent>();

            var lang = LanguageRepository.GetByIsoCode("en-AU");
            contentItems.Add(DocumentRepository.Get(contentId));

            // more test data (3 content items total)
            for (var i = 0; i < 2; i++)
            {
                var c = new Content("test" + i, -1, ct) { CreatorId = 0, WriterId = 0 };
                DocumentRepository.Save(c);
                contentItems.Add(c);
            }

            for (var i = 0; i < 10; i++)
            {
                var domain = (IDomain)new UmbracoDomain(i % 2 == 0 ? "test " + i + ".com" : "*" + i)
                {
                    RootContentId = (i % 2 == 0 ? contentItems[0] : contentItems[1]).Id,
                    LanguageId = lang.Id
                };
                repo.Save(domain);
            }

            var all1 = repo.GetAssignedDomains(contentItems[0].Id, true);
            Assert.That(all1.Count(), Is.EqualTo(5));

            var all2 = repo.GetAssignedDomains(contentItems[1].Id, true);
            Assert.That(all2.Count(), Is.EqualTo(5));

            var all3 = repo.GetAssignedDomains(contentItems[2].Id, true);
            Assert.That(all3.Count(), Is.EqualTo(0));
        }
    }

    [Test]
    public void Get_All_For_Content_Without_Wildcards()
    {
        var contentId = CreateTestData("en-AU", out var ct);

        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var contentItems = new List<IContent>();

            var lang = LanguageRepository.GetByIsoCode("en-AU");
            contentItems.Add(DocumentRepository.Get(contentId));

            // more test data (3 content items total)
            for (var i = 0; i < 2; i++)
            {
                var c = new Content("test" + i, -1, ct) { CreatorId = 0, WriterId = 0 };
                DocumentRepository.Save(c);
                contentItems.Add(c);
            }

            for (var i = 0; i < 10; i++)
            {
                var domain = (IDomain)new UmbracoDomain(i % 2 == 0 ? "test " + i + ".com" : "*" + i)
                {
                    RootContentId = (i % 2 == 0 ? contentItems[0] : contentItems[1]).Id,
                    LanguageId = lang.Id
                };
                repo.Save(domain);
            }

            var all1 = repo.GetAssignedDomains(contentItems[0].Id, false);
            Assert.That(all1.Count(), Is.EqualTo(5));

            var all2 = repo.GetAssignedDomains(contentItems[1].Id, false);
            Assert.That(all2.Count(), Is.EqualTo(0));
        }
    }

    [Test]
    public void GetByName_Returns_Deep_Clone_Not_Cached_Instance()
    {
        var contentId = CreateTestData("en-AU", out _);
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var content = DocumentRepository.Get(contentId);
            var lang = LanguageRepository.GetByIsoCode("en-AU");
            var domain = (IDomain)new UmbracoDomain("clone-test.com") { RootContentId = content.Id, LanguageId = lang.Id };
            repo.Save(domain);

            var first = repo.GetByName("clone-test.com");
            var second = repo.GetByName("clone-test.com");

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second!.Id, Is.EqualTo(first!.Id));
            Assert.That(second, Is.Not.SameAs(first));
        }
    }

    [Test]
    public void Exists_By_Name_Returns_Correct_Result()
    {
        var contentId = CreateTestData("en-AU", out _);
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var content = DocumentRepository.Get(contentId);
            var lang = LanguageRepository.GetByIsoCode("en-AU");
            var domain = (IDomain)new UmbracoDomain("exists-test.com") { RootContentId = content.Id, LanguageId = lang.Id };
            repo.Save(domain);

            Assert.That(repo.Exists("exists-test.com"), Is.True);
            Assert.That(repo.Exists("nonexistent.com"), Is.False);
        }
    }

    [Test]
    public void GetByName_Mutation_Does_Not_Affect_Subsequent_Get()
    {
        var contentId = CreateTestData("en-AU", out _);
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var content = DocumentRepository.Get(contentId);
            var lang = LanguageRepository.GetByIsoCode("en-AU");
            var domain = (IDomain)new UmbracoDomain("mutation-test.com") { RootContentId = content.Id, LanguageId = lang.Id };
            repo.Save(domain);

            var first = repo.GetByName("mutation-test.com");
            Assert.That(first, Is.Not.Null);
            var originalName = first!.DomainName;
            first.DomainName = "MUTATED_" + Guid.NewGuid();

            var second = repo.GetByName("mutation-test.com");
            Assert.That(second, Is.Not.Null);
            Assert.That(second!.DomainName, Is.EqualTo(originalName), "Mutation of a returned entity should not affect the cached copy");
        }
    }

    [Test]
    public void GetAssignedDomains_Returns_Only_Matching_Domains()
    {
        var contentId = CreateTestData("en-AU", out _);
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var content = DocumentRepository.Get(contentId);
            var lang = LanguageRepository.GetByIsoCode("en-AU");

            repo.Save((IDomain)new UmbracoDomain("assigned1.com") { RootContentId = content.Id, LanguageId = lang.Id });
            repo.Save((IDomain)new UmbracoDomain("assigned2.com") { RootContentId = content.Id, LanguageId = lang.Id });

            var assigned = repo.GetAssignedDomains(content.Id, true).ToArray();
            Assert.That(assigned, Has.Length.EqualTo(2));

            var unassigned = repo.GetAssignedDomains(-999, true).ToArray();
            Assert.That(unassigned, Is.Empty);
        }
    }
}
