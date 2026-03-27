// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
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

    private IDomainRepository DomainRepository => GetRequiredService<IDomainRepository>();

    private async Task<(IContent Content, ContentType ContentType)> CreateTestDataAsync(string isoName)
    {
        var provider = ScopeProvider;
        using var efCoreScope = NewScopeProvider.CreateScope();
        using (var scope = provider.CreateScope())
        {
            var lang = new Language(isoName, isoName);
            await LanguageRepository.SaveAsync(lang, CancellationToken.None);

            var ct = ContentTypeBuilder.CreateBasicContentType("test", "Test");
            ContentTypeRepository.Save(ct);
            var content = new Content("test", -1, ct) { CreatorId = 0, WriterId = 0 };
            DocumentRepository.Save(content);
            scope.Complete();
            efCoreScope.Complete();
            return (content, ct);
        }
    }

    [Test]
    public async Task Can_Create_And_Get_By_Key()
    {
        var content = (await CreateTestDataAsync("en-AU")).Content;

        using var efCoreScope = NewScopeProvider.CreateScope();

        var lang = await LanguageRepository.GetByIsoCodeAsync("en-AU");

        var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang!.Id };
        await DomainRepository.SaveAsync(domain, CancellationToken.None);

        // re-get by key
        domain = await DomainRepository.GetAsync(domain.Key, CancellationToken.None);

        Assert.NotNull(domain);
        Assert.IsTrue(domain!.HasIdentity);
        Assert.Greater(domain.Id, 0);
        Assert.AreEqual("test.com", domain.DomainName);
        Assert.AreEqual(content.Id, domain.RootContentId);
        Assert.AreEqual(content.Key, domain.RootContentKey);
        Assert.AreEqual(lang.Id, domain.LanguageId);
        Assert.AreEqual(lang.IsoCode, domain.LanguageIsoCode);

        efCoreScope.Complete();
    }

    [Test]
    public async Task Can_Create_And_Get_By_Key_Empty_Lang()
    {
        var content = (await CreateTestDataAsync("en-AU")).Content;

        using var efCoreScope = NewScopeProvider.CreateScope();

        var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id };
        await DomainRepository.SaveAsync(domain, CancellationToken.None);

        // re-get by key
        domain = await DomainRepository.GetAsync(domain.Key, CancellationToken.None);

        Assert.NotNull(domain);
        Assert.IsTrue(domain!.HasIdentity);
        Assert.Greater(domain.Id, 0);
        Assert.AreEqual("test.com", domain.DomainName);
        Assert.AreEqual(content.Id, domain.RootContentId);
        Assert.IsFalse(domain.LanguageId.HasValue);

        efCoreScope.Complete();
    }

    [Test]
    public async Task Cant_Create_Duplicate_Domain_Name()
    {
        var content = (await CreateTestDataAsync("en-AU")).Content;

        using var efCoreScope = NewScopeProvider.CreateScope();

        var lang = await LanguageRepository.GetByIsoCodeAsync("en-AU");

        var domain1 = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang!.Id };
        await DomainRepository.SaveAsync(domain1, CancellationToken.None);

        var domain2 = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang.Id };

        Assert.ThrowsAsync<DuplicateNameException>(() => DomainRepository.SaveAsync(domain2, CancellationToken.None));

        efCoreScope.Complete();
    }

    [Test]
    public async Task Can_Delete()
    {
        var content = (await CreateTestDataAsync("en-AU")).Content;

        using var efCoreScope = NewScopeProvider.CreateScope();

        var lang = await LanguageRepository.GetByIsoCodeAsync("en-AU");

        var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content.Id, LanguageId = lang!.Id };
        await DomainRepository.SaveAsync(domain, CancellationToken.None);

        await DomainRepository.DeleteAsync(domain, CancellationToken.None);

        // re-get
        domain = await DomainRepository.GetAsync(domain.Key, CancellationToken.None);

        Assert.IsNull(domain);

        efCoreScope.Complete();
    }

    [Test]
    public async Task Can_Update()
    {
        var (content1, ct) = await CreateTestDataAsync("en-AU");

        using var efCoreScope = NewScopeProvider.CreateScope();

        // more test data
        var lang1 = await LanguageRepository.GetByIsoCodeAsync("en-AU");
        var lang2 = new Language("es", "Spanish");
        await LanguageRepository.SaveAsync(lang2, CancellationToken.None);

        using (var scope = ScopeProvider.CreateScope())
        {
            var content2 = new Content("test", -1, ct) { CreatorId = 0, WriterId = 0 };
            DocumentRepository.Save(content2);
            scope.Complete();

            var domain = (IDomain)new UmbracoDomain("test.com") { RootContentId = content1.Id, LanguageId = lang1!.Id };
            await DomainRepository.SaveAsync(domain, CancellationToken.None);

            // re-get
            domain = (await DomainRepository.GetAsync(domain.Key, CancellationToken.None))!;

            domain.DomainName = "blah.com";
            domain.RootContentId = content2.Id;
            domain.LanguageId = lang2.Id;
            await DomainRepository.SaveAsync(domain, CancellationToken.None);

            // re-get
            domain = (await DomainRepository.GetAsync(domain.Key, CancellationToken.None))!;

            Assert.AreEqual("blah.com", domain.DomainName);
            Assert.AreEqual(content2.Id, domain.RootContentId);
            Assert.AreEqual(lang2.Id, domain.LanguageId);
            Assert.AreEqual(lang2.IsoCode, domain.LanguageIsoCode);
        }

        efCoreScope.Complete();
    }

    [Test]
    public async Task Exists()
    {
        var content = (await CreateTestDataAsync("en-AU")).Content;

        using var efCoreScope = NewScopeProvider.CreateScope();

        var lang = await LanguageRepository.GetByIsoCodeAsync("en-AU");

        for (var i = 0; i < 10; i++)
        {
            var domain = (IDomain)new UmbracoDomain("test" + i + ".com") { RootContentId = content.Id, LanguageId = lang!.Id };
            await DomainRepository.SaveAsync(domain, CancellationToken.None);
        }

        var found = await DomainRepository.ExistsAsync("test1.com");

        Assert.IsTrue(found);

        efCoreScope.Complete();
    }

    [Test]
    public async Task Get_By_Name()
    {
        var content = (await CreateTestDataAsync("en-AU")).Content;

        using var efCoreScope = NewScopeProvider.CreateScope();

        var lang = await LanguageRepository.GetByIsoCodeAsync("en-AU");

        for (var i = 0; i < 10; i++)
        {
            var domain = (IDomain)new UmbracoDomain("test" + i + ".com") { RootContentId = content.Id, LanguageId = lang!.Id };
            await DomainRepository.SaveAsync(domain, CancellationToken.None);
        }

        var found = await DomainRepository.GetByNameAsync("test1.com");

        Assert.IsNotNull(found);

        efCoreScope.Complete();
    }

    [Test]
    public async Task Get_All()
    {
        var content = (await CreateTestDataAsync("en-AU")).Content;

        using var efCoreScope = NewScopeProvider.CreateScope();

        var lang = await LanguageRepository.GetByIsoCodeAsync("en-AU");

        for (var i = 0; i < 10; i++)
        {
            var domain = (IDomain)new UmbracoDomain("test " + i + ".com") { RootContentId = content.Id, LanguageId = lang!.Id };
            await DomainRepository.SaveAsync(domain, CancellationToken.None);
        }

        var all = await DomainRepository.GetAllAsync(CancellationToken.None);

        Assert.AreEqual(10, all.Count());

        efCoreScope.Complete();
    }

    [Test]
    public async Task Get_Many_By_Keys()
    {
        var content = (await CreateTestDataAsync("en-AU")).Content;

        using var efCoreScope = NewScopeProvider.CreateScope();

        var lang = await LanguageRepository.GetByIsoCodeAsync("en-AU");

        var keys = new List<Guid>();
        for (var i = 0; i < 10; i++)
        {
            var domain = (IDomain)new UmbracoDomain("test " + i + ".com") { RootContentId = content.Id, LanguageId = lang!.Id };
            await DomainRepository.SaveAsync(domain, CancellationToken.None);
            keys.Add(domain.Key);
        }

        var all = await DomainRepository.GetManyAsync(keys.Take(8).ToArray(), CancellationToken.None);

        Assert.AreEqual(8, all.Count());

        efCoreScope.Complete();
    }

    [Test]
    public async Task Get_All_Without_Wildcards()
    {
        var content = (await CreateTestDataAsync("en-AU")).Content;

        using var efCoreScope = NewScopeProvider.CreateScope();

        var lang = await LanguageRepository.GetByIsoCodeAsync("en-AU");

        for (var i = 0; i < 10; i++)
        {
            var domain = (IDomain)new UmbracoDomain(i % 2 == 0 ? "test " + i + ".com" : "*" + i)
            {
                RootContentId = content.Id,
                LanguageId = lang!.Id
            };
            await DomainRepository.SaveAsync(domain, CancellationToken.None);
        }

        var all = await DomainRepository.GetAllAsync(false);

        Assert.AreEqual(5, all.Count());

        efCoreScope.Complete();
    }

    [Test]
    public async Task Get_All_For_Content()
    {
        var (content, ct) = await CreateTestDataAsync("en-AU");

        using var efCoreScope = NewScopeProvider.CreateScope();

        var contentItems = new List<IContent> { content };

        var lang = await LanguageRepository.GetByIsoCodeAsync("en-AU");

        // more test data (3 content items total)
        using (var scope = ScopeProvider.CreateScope())
        {
            for (var i = 0; i < 2; i++)
            {
                var c = new Content("test" + i, -1, ct) { CreatorId = 0, WriterId = 0 };
                DocumentRepository.Save(c);
                contentItems.Add(c);
            }

            scope.Complete();
        }

        for (var i = 0; i < 10; i++)
        {
            var domain = (IDomain)new UmbracoDomain(i % 2 == 0 ? "test " + i + ".com" : "*" + i)
            {
                RootContentId = (i % 2 == 0 ? contentItems[0] : contentItems[1]).Id,
                LanguageId = lang!.Id
            };
            await DomainRepository.SaveAsync(domain, CancellationToken.None);
        }

        var all1 = await DomainRepository.GetAssignedDomainsAsync(contentItems[0].Key, true);
        Assert.AreEqual(5, all1.Count());

        var all2 = await DomainRepository.GetAssignedDomainsAsync(contentItems[1].Key, true);
        Assert.AreEqual(5, all2.Count());

        var all3 = await DomainRepository.GetAssignedDomainsAsync(contentItems[2].Key, true);
        Assert.AreEqual(0, all3.Count());

        efCoreScope.Complete();
    }

    [Test]
    public async Task Get_All_For_Content_Without_Wildcards()
    {
        var (content, ct) = await CreateTestDataAsync("en-AU");

        using var efCoreScope = NewScopeProvider.CreateScope();

        var contentItems = new List<IContent> { content };

        var lang = await LanguageRepository.GetByIsoCodeAsync("en-AU");

        // more test data (3 content items total)
        using (var scope = ScopeProvider.CreateScope())
        {
            for (var i = 0; i < 2; i++)
            {
                var c = new Content("test" + i, -1, ct) { CreatorId = 0, WriterId = 0 };
                DocumentRepository.Save(c);
                contentItems.Add(c);
            }

            scope.Complete();
        }

        for (var i = 0; i < 10; i++)
        {
            var domain = (IDomain)new UmbracoDomain(i % 2 == 0 ? "test " + i + ".com" : "*" + i)
            {
                RootContentId = (i % 2 == 0 ? contentItems[0] : contentItems[1]).Id,
                LanguageId = lang!.Id
            };
            await DomainRepository.SaveAsync(domain, CancellationToken.None);
        }

        var all1 = await DomainRepository.GetAssignedDomainsAsync(contentItems[0].Key, false);
        Assert.AreEqual(5, all1.Count());

        var all2 = await DomainRepository.GetAssignedDomainsAsync(contentItems[1].Key, false);
        Assert.AreEqual(0, all2.Count());

        efCoreScope.Complete();
    }
}
