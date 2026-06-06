using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
internal sealed class DomainCacheServiceTests
{
    [Test]
    public void Can_Get_Configured_Domains_Excluding_Wildcards()
    {
        IDomain assigned = CreateDomain(1, "https://site-one.example/", 1001, "en-US", isWildcard: false);
        IDomain wildcard = CreateDomain(2, "*.site-one.example", 1001, "en-US", isWildcard: true);

        var domainService = new Mock<IDomainService>();
        domainService
#pragma warning disable CS0618 // Type or member is obsolete. This test is for the DomainCacheService, which still calls the obsolete GetAll(bool) method.
            .Setup(x => x.GetAll(true))
#pragma warning restore CS0618 // Type or member is obsolete
            .Returns([assigned, wildcard]);

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());

        Domain[] withoutWildcards = sut.GetAll(false).ToArray();
        Domain[] withWildcards = sut.GetAll(true).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, withoutWildcards.Length);
            Assert.AreEqual(assigned.Id, withoutWildcards[0].Id);
            Assert.AreEqual(2, withWildcards.Length);
        });
    }

    [Test]
    public async Task Cannot_Observe_Empty_Domain_Cache_During_Concurrent_First_Access()
    {
        // Arrange - a single configured domain that every caller must be able to see once the cache
        // has initialized. The IDomainService load is gated so we can force a second caller to race in
        // while the very first load is still in progress.
        IDomain configuredDomain = CreateDomain(1, "https://site-one.example/", 1001, "en-US", isWildcard: false);

        using var loadStarted = new ManualResetEventSlim(false);
        using var releaseLoad = new ManualResetEventSlim(false);

        var domainService = new Mock<IDomainService>();
        domainService
#pragma warning disable CS0618 // Type or member is obsolete. This test is for the DomainCacheService, which still calls the obsolete GetAll(bool) method.
            .Setup(x => x.GetAll(true))
#pragma warning restore CS0618 // Type or member is obsolete
            .Returns(() =>
            {
                loadStarted.Set();
                releaseLoad.Wait(TimeSpan.FromSeconds(10));
                return [configuredDomain];
            });

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());

        // Act
        // The first caller triggers initialization and blocks inside the gated load.
        Task<Domain[]> firstCaller = Task.Run(() => sut.GetAll(false).ToArray());

        Assert.IsTrue(loadStarted.Wait(TimeSpan.FromSeconds(10)), "Initialization did not start.");

        // A second caller arrives while initialization is still in progress. With the unfixed code it
        // observes the initialized flag (set before the load completed), skips loading, and reads an
        // empty cache - the failure that makes a multi-site setup fall back to the first root node.
        Task<Domain[]> secondCaller = Task.Run(() => sut.GetAll(false).ToArray());

        // Give the second caller time to reach the cache (and, on the unfixed code, return empty)
        // before we allow the load to complete.
        await Task.Delay(250);

        releaseLoad.Set();

        Domain[] firstResult = await firstCaller;
        Domain[] secondResult = await secondCaller;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, firstResult.Length, "The caller that initialized the cache should see the configured domain.");
            Assert.AreEqual(1, secondResult.Length, "A caller racing with initialization must not observe an empty domain cache.");
        });
    }

    [Test]
    public void Can_Refresh_To_Add_And_Update_A_Domain()
    {
        var domainService = new Mock<IDomainService>();
        domainService
#pragma warning disable CS0618 // Type or member is obsolete. This test is for the DomainCacheService, which still calls the obsolete GetAll(bool) method.
            .Setup(x => x.GetAll(true))
#pragma warning restore CS0618 // Type or member is obsolete
            .Returns([]);

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());

        // Trigger the initial (empty) load.
        Assert.IsEmpty(sut.GetAll(true));

        // A Refresh payload for a previously unknown domain adds it to the cache.
        domainService.Setup(x => x.GetById(5)).Returns(CreateDomain(5, "https://site.example/", 2002, "en-US", isWildcard: false));
        sut.Refresh([new DomainCacheRefresher.JsonPayload(5, DomainChangeTypes.Refresh)]);
        Domain[] afterAdd = sut.GetAll(false).ToArray();

        // A second Refresh for the same id with changed data must update in place, not duplicate the entry.
        domainService.Setup(x => x.GetById(5)).Returns(CreateDomain(5, "https://renamed.example/", 3003, "en-US", isWildcard: false));
        sut.Refresh([new DomainCacheRefresher.JsonPayload(5, DomainChangeTypes.Refresh)]);
        Domain[] afterUpdate = sut.GetAll(false).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, afterAdd.Length, "Refreshing an unknown domain should add it to the cache.");
            Assert.AreEqual("https://site.example/", afterAdd[0].Name);
            Assert.AreEqual(2002, afterAdd[0].ContentId);

            Assert.AreEqual(1, afterUpdate.Length, "Refreshing an existing domain must update in place, not add a duplicate.");
            Assert.AreEqual("https://renamed.example/", afterUpdate[0].Name);
            Assert.AreEqual(3003, afterUpdate[0].ContentId);
        });
    }

    private static IDomain CreateDomain(int id, string name, int rootContentId, string culture, bool isWildcard)
    {
        var domain = new Mock<IDomain>();
        domain.SetupGet(x => x.Id).Returns(id);
        domain.SetupGet(x => x.DomainName).Returns(name);
        domain.SetupGet(x => x.RootContentId).Returns(rootContentId);
        domain.SetupGet(x => x.LanguageIsoCode).Returns(culture);
        domain.SetupGet(x => x.IsWildcard).Returns(isWildcard);
        domain.SetupGet(x => x.SortOrder).Returns(0);
        return domain.Object;
    }

    private static ICoreScopeProvider CreateScopeProvider()
    {
        var scope = new Mock<ICoreScope>();
        var scopeProvider = new Mock<ICoreScopeProvider>();
        scopeProvider.Setup(x => x.CreateCoreScope()).Returns(scope.Object);
        return scopeProvider.Object;
    }
}
