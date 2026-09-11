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
            .Setup(x => x.GetAllAsync(true))
            .ReturnsAsync([assigned, wildcard]);

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());

        Domain[] withoutWildcards = sut.GetAll(false).ToArray();
        Domain[] withWildcards = sut.GetAll(true).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(withoutWildcards, Has.Length.EqualTo(1));
            Assert.That(withoutWildcards[0].Id, Is.EqualTo(assigned.Id));
            Assert.That(withWildcards, Has.Length.EqualTo(2));
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
        using var secondCallerReady = new ManualResetEventSlim(false);
        using var releaseLoad = new ManualResetEventSlim(false);

        var domainService = new Mock<IDomainService>();
        domainService
            .Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(() =>
            {
                loadStarted.Set();

                // Fail loudly if the test never releases the load: returning anyway would let the test
                // pass while the synchronization it depends on silently broke (and hide any hang).
                Assert.That(
                    releaseLoad.Wait(TimeSpan.FromSeconds(10)),
                    Is.True,
                    "Timed out waiting for the gated domain load to be released.");

                return [configuredDomain];
            });

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());

        // Act
        // The first caller triggers initialization and blocks inside the gated load.
        Task<Domain[]> firstCaller = Task.Run(() => sut.GetAll(false).ToArray());

        Assert.That(loadStarted.Wait(TimeSpan.FromSeconds(10)), Is.True, "Initialization did not start.");

        // A second caller arrives while initialization is still in progress.
        Task<Domain[]> secondCaller = Task.Run(() =>
        {
            // Confirm the caller has reached the cache before we assert on its blocking behaviour.
            secondCallerReady.Set();
            return sut.GetAll(false).ToArray();
        });

        Assert.That(secondCallerReady.Wait(TimeSpan.FromSeconds(10)), Is.True, "Second caller did not start.");

        // A correctly initialized cache makes the second caller block until the gated load completes, so it
        // cannot finish while the load is still held. Observing an empty cache would instead let it return
        // immediately. Completing here - before we release the load - therefore signals the empty-cache
        // regression (and the result would be empty). Because this asserts a blocking invariant, it holds
        // regardless of scheduler timing: a blocked caller never completes within any timeout.
        var completedWhileLoadGated = secondCaller.Wait(TimeSpan.FromSeconds(1));

        releaseLoad.Set();

        Domain[] firstResult = await firstCaller;
        Domain[] secondResult = await secondCaller;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(completedWhileLoadGated, Is.False, "Second caller returned before initialization completed - it observed an empty domain cache.");
            Assert.That(firstResult, Has.Length.EqualTo(1), "The caller that initialized the cache should see the configured domain.");
            Assert.That(secondResult, Has.Length.EqualTo(1), "A caller racing with initialization must not observe an empty domain cache.");
        });
    }

    [Test]
    public void Can_Refresh_To_Add_And_Update_A_Domain()
    {
        var domainService = new Mock<IDomainService>();
        domainService
            .Setup(x => x.GetAllAsync(true))
            .ReturnsAsync([]);

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());

        // Trigger the initial (empty) load.
        Assert.That(sut.GetAll(true), Is.Empty);

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
            Assert.That(afterAdd, Has.Length.EqualTo(1), "Refreshing an unknown domain should add it to the cache.");
            Assert.That(afterAdd[0].Name, Is.EqualTo("https://site.example/"));
            Assert.That(afterAdd[0].ContentId, Is.EqualTo(2002));

            Assert.That(afterUpdate, Has.Length.EqualTo(1), "Refreshing an existing domain must update in place, not add a duplicate.");
            Assert.That(afterUpdate[0].Name, Is.EqualTo("https://renamed.example/"));
            Assert.That(afterUpdate[0].ContentId, Is.EqualTo(3003));
        });
    }

    [Test]
    public void Can_Replace_Entire_Cache_On_RefreshAll()
    {
        IDomain first = CreateDomain(1, "https://site-one.example/", 1001, "en-US", isWildcard: false);
        IDomain second = CreateDomain(2, "https://site-two.example/", 1002, "da-DK", isWildcard: false);

        var current = new List<IDomain> { first, second };
        var domainService = new Mock<IDomainService>();
        domainService
            .Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(() => current.ToArray());

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());

        // The initial lazy load sees both domains.
        Assert.That(sut.GetAll(false).Count(), Is.EqualTo(2));

        // A domain is removed at the source, then a RefreshAll rebuilds the whole cache from scratch.
        current.Remove(second);
        sut.Refresh([new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll)]);

        Domain[] afterRefreshAll = sut.GetAll(false).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(afterRefreshAll, Has.Length.EqualTo(1), "RefreshAll should fully replace the cache, dropping removed domains.");
            Assert.That(afterRefreshAll[0].Id, Is.EqualTo(first.Id));
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
