// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PublicAccessRepositoryTest : UmbracoIntegrationTest
{
    private IContentTypeRepository ContentTypeRepository => GetRequiredService<IContentTypeRepository>();

    private DocumentRepository DocumentRepository => (DocumentRepository)GetRequiredService<IDocumentRepository>();

    private PublicAccessRepository CreateRepository() =>
        new(
            StaticServiceProvider.Instance.GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(),
            AppCaches,
            LoggerFactory.CreateLogger<PublicAccessRepository>(),
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>());

    [Test]
    public async Task Can_Delete()
    {
        var content = CreateTestData(3).ToArray();

        using var scope = NewScopeProvider.CreateScope();
        var repo = CreateRepository();

        PublicAccessRule[] rules = { new() { RuleValue = "test", RuleType = "RoleName" } };
        var entry = new PublicAccessEntry(content[0], content[1], content[2], rules);
        await repo.SaveAsync(entry, CancellationToken.None);

        await repo.DeleteAsync(entry, CancellationToken.None);

        var found = await repo.GetAsync(entry.Key, CancellationToken.None);
        Assert.IsNull(found);

        scope.Complete();
    }

    [Test]
    public async Task Can_Add()
    {
        var content = CreateTestData(3).ToArray();

        using var scope = NewScopeProvider.CreateScope();
        var repo = CreateRepository();

        PublicAccessRule[] rules = { new() { RuleValue = "test", RuleType = "RoleName" } };
        var entry = new PublicAccessEntry(content[0], content[1], content[2], rules);
        await repo.SaveAsync(entry, CancellationToken.None);

        var found = (await repo.GetAllAsync(CancellationToken.None)).ToArray();

        Assert.AreEqual(1, found.Length);
        Assert.AreEqual(content[0].Id, found[0].ProtectedNodeId);
        Assert.AreEqual(content[1].Id, found[0].LoginNodeId);
        Assert.AreEqual(content[2].Id, found[0].NoAccessNodeId);
        Assert.IsTrue(found[0].HasIdentity);
        Assert.AreNotEqual(default(DateTime), found[0].CreateDate);
        Assert.AreNotEqual(default(DateTime), found[0].UpdateDate);
        Assert.AreEqual(1, found[0].Rules.Count());
        Assert.AreEqual("test", found[0].Rules.First().RuleValue);
        Assert.AreEqual("RoleName", found[0].Rules.First().RuleType);
        Assert.AreNotEqual(default(DateTime), found[0].Rules.First().CreateDate);
        Assert.AreNotEqual(default(DateTime), found[0].Rules.First().UpdateDate);
        Assert.IsTrue(found[0].Rules.First().HasIdentity);

        scope.Complete();
    }

    [Test]
    [LongRunning]
    public async Task Can_Add2()
    {
        var content = CreateTestData(3).ToArray();

        using var scope = NewScopeProvider.CreateScope();
        var repo = CreateRepository();

        PublicAccessRule[] rules =
        {
            new() { RuleValue = "test", RuleType = "RoleName" },
            new() { RuleValue = "test2", RuleType = "RoleName2" },
        };
        var entry = new PublicAccessEntry(content[0], content[1], content[2], rules);
        await repo.SaveAsync(entry, CancellationToken.None);

        var found = (await repo.GetAllAsync(CancellationToken.None)).ToArray();

        Assert.AreEqual(1, found.Length);
        Assert.AreEqual(content[0].Id, found[0].ProtectedNodeId);
        Assert.AreEqual(content[1].Id, found[0].LoginNodeId);
        Assert.AreEqual(content[2].Id, found[0].NoAccessNodeId);
        Assert.IsTrue(found[0].HasIdentity);
        Assert.AreNotEqual(default(DateTime), found[0].CreateDate);
        Assert.AreNotEqual(default(DateTime), found[0].UpdateDate);
        Assert.That(entry.Rules, Is.EquivalentTo(found[0].Rules));
        Assert.AreNotEqual(default(DateTime), found[0].Rules.First().CreateDate);
        Assert.AreNotEqual(default(DateTime), found[0].Rules.First().UpdateDate);
        Assert.IsTrue(found[0].Rules.First().HasIdentity);

        scope.Complete();
    }

    [Test]
    public async Task Can_Update()
    {
        var content = CreateTestData(3).ToArray();

        using var scope = NewScopeProvider.CreateScope();
        var repo = CreateRepository();

        PublicAccessRule[] rules = { new() { RuleValue = "test", RuleType = "RoleName" } };
        var entry = new PublicAccessEntry(content[0], content[1], content[2], rules);
        await repo.SaveAsync(entry, CancellationToken.None);

        // re-get
        entry = await repo.GetAsync(entry.Key, CancellationToken.None);

        entry.Rules.First().RuleValue = "blah";
        entry.Rules.First().RuleType = "asdf";
        await repo.SaveAsync(entry, CancellationToken.None);

        // re-get
        entry = await repo.GetAsync(entry.Key, CancellationToken.None);

        Assert.AreEqual("blah", entry.Rules.First().RuleValue);
        Assert.AreEqual("asdf", entry.Rules.First().RuleType);

        scope.Complete();
    }

    [Test]
    public async Task Get_By_Id()
    {
        var content = CreateTestData(3).ToArray();

        using var scope = NewScopeProvider.CreateScope();
        var repo = CreateRepository();

        PublicAccessRule[] rules = { new() { RuleValue = "test", RuleType = "RoleName" } };
        var entry = new PublicAccessEntry(content[0], content[1], content[2], rules);
        await repo.SaveAsync(entry, CancellationToken.None);

        // re-get
        entry = await repo.GetAsync(entry.Key, CancellationToken.None);

        Assert.IsNotNull(entry);

        scope.Complete();
    }

    [Test]
    public async Task Get_All()
    {
        var content = CreateTestData(30).ToArray();

        using var scope = NewScopeProvider.CreateScope();
        var repo = CreateRepository();

        var allEntries = new List<PublicAccessEntry>();
        for (var i = 0; i < 10; i++)
        {
            var rules = new List<PublicAccessRule>();
            for (var j = 0; j < 50; j++)
            {
                rules.Add(new PublicAccessRule { RuleValue = "test" + j, RuleType = "RoleName" + j });
            }

            var entry1 = new PublicAccessEntry(content[i], content[i + 1], content[i + 2], rules);
            await repo.SaveAsync(entry1, CancellationToken.None);

            allEntries.Add(entry1);
        }

        // now remove a few rules from a few of the items and then add some more, this will put things 'out of order' which
        // we need to verify our sort order is working for the relator
        // TODO: no "relator" in v8?!
        for (var i = 0; i < allEntries.Count; i++)
        {
            // all the even ones
            if (i % 2 == 0)
            {
                var rules = allEntries[i].Rules.ToArray();
                for (var j = 0; j < rules.Length; j++)
                {
                    // all the even ones
                    if (j % 2 == 0)
                    {
                        allEntries[i].RemoveRule(rules[j]);
                    }
                }

                allEntries[i].AddRule("newrule" + i, "newrule" + i);
                await repo.SaveAsync(allEntries[i], CancellationToken.None);
            }
        }

        var found = (await repo.GetAllAsync(CancellationToken.None)).ToArray();
        Assert.AreEqual(10, found.Length);

        foreach (var publicAccessEntry in found)
        {
            var matched = allEntries.First(x => x.Key == publicAccessEntry.Key);

            Assert.AreEqual(matched.Rules.Count(), publicAccessEntry.Rules.Count());
        }

        scope.Complete();
    }

    [Test]
    public async Task Get_All_With_Id()
    {
        var content = CreateTestData(3).ToArray();

        using var scope = NewScopeProvider.CreateScope();
        var repo = CreateRepository();

        PublicAccessRule[] rules1 = { new() { RuleValue = "test", RuleType = "RoleName" } };
        var entry1 = new PublicAccessEntry(content[0], content[1], content[2], rules1);
        await repo.SaveAsync(entry1, CancellationToken.None);

        PublicAccessRule[] rules2 = { new() { RuleValue = "test", RuleType = "RoleName" } };
        var entry2 = new PublicAccessEntry(content[1], content[0], content[2], rules2);
        await repo.SaveAsync(entry2, CancellationToken.None);

        var found = (await repo.GetManyAsync(new[] { entry1.Key }, CancellationToken.None)).ToArray();
        Assert.AreEqual(1, found.Length);

        scope.Complete();
    }

    private IEnumerable<IContent> CreateTestData(int count)
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var ct = ContentTypeBuilder.CreateBasicContentType("testing");
            ContentTypeRepository.Save(ct);

            var result = new List<IContent>();
            for (var i = 0; i < count; i++)
            {
                var c = new Content("test" + i, -1, ct);
                DocumentRepository.Save(c);
                result.Add(c);
            }

            scope.Complete();

            return result;
        }
    }
}
