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
        using var secondCallerReady = new ManualResetEventSlim(false);
        using var releaseLoad = new ManualResetEventSlim(false);

        var domainService = new Mock<IDomainService>();
        domainService
#pragma warning disable CS0618 // Type or member is obsolete. This test is for the DomainCacheService, which still calls the obsolete GetAll(bool) method.
            .Setup(x => x.GetAll(true))
#pragma warning restore CS0618 // Type or member is obsolete
            .Returns(() =>
            {
                loadStarted.Set();

                // Fail loudly if the test never releases the load: returning anyway would let the test
                // pass while the synchronization it depends on silently broke (and hide any hang).
                Assert.IsTrue(
                    releaseLoad.Wait(TimeSpan.FromSeconds(10)),
                    "Timed out waiting for the gated domain load to be released.");

                return [configuredDomain];
            });

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());

        // Act
        // The first caller triggers initialization and blocks inside the gated load.
        Task<Domain[]> firstCaller = Task.Run(() => sut.GetAll(false).ToArray());

        Assert.IsTrue(loadStarted.Wait(TimeSpan.FromSeconds(10)), "Initialization did not start.");

        // A second caller arrives while initialization is still in progress.
        Task<Domain[]> secondCaller = Task.Run(() =>
        {
            // Confirm the caller has reached the cache before we assert on its blocking behaviour.
            secondCallerReady.Set();
            return sut.GetAll(false).ToArray();
        });

        Assert.IsTrue(secondCallerReady.Wait(TimeSpan.FromSeconds(10)), "Second caller did not start.");

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
            Assert.IsFalse(completedWhileLoadGated, "Second caller returned before initialization completed - it observed an empty domain cache.");
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

    [Test]
    public void Can_Replace_Entire_Cache_On_RefreshAll()
    {
        IDomain first = CreateDomain(1, "https://site-one.example/", 1001, "en-US", isWildcard: false);
        IDomain second = CreateDomain(2, "https://site-two.example/", 1002, "da-DK", isWildcard: false);

        var current = new List<IDomain> { first, second };
        var domainService = new Mock<IDomainService>();
        domainService
#pragma warning disable CS0618 // Type or member is obsolete. This test is for the DomainCacheService, which still calls the obsolete GetAll(bool) method.
            .Setup(x => x.GetAll(true))
#pragma warning restore CS0618 // Type or member is obsolete
            .Returns(() => current.ToArray());

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());

        // The initial lazy load sees both domains.
        Assert.AreEqual(2, sut.GetAll(false).Count());

        // A domain is removed at the source, then a RefreshAll rebuilds the whole cache from scratch.
        current.Remove(second);
        sut.Refresh([new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll)]);

        Domain[] afterRefreshAll = sut.GetAll(false).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, afterRefreshAll.Length, "RefreshAll should fully replace the cache, dropping removed domains.");
            Assert.AreEqual(first.Id, afterRefreshAll[0].Id);
        });
    }

    [Test]
    public void Can_Get_Assigned_Domains_Sorted_By_Sort_Order()
    {
        var domainService = new Mock<IDomainService>();
        domainService
#pragma warning disable CS0618 // Type or member is obsolete. This test is for the DomainCacheService, which still calls the obsolete GetAll(bool) method.
            .Setup(x => x.GetAll(true))
#pragma warning restore CS0618 // Type or member is obsolete
            .Returns(
            [
                CreateDomain(1, "https://third.example/", 1001, "en-US", isWildcard: false, sortOrder: 2),
                CreateDomain(2, "*1001", 1001, "da-DK", isWildcard: true, sortOrder: 0),
                CreateDomain(3, "https://first.example/", 1001, "en-US", isWildcard: false, sortOrder: 1),
                CreateDomain(4, "https://other-node.example/", 1002, "en-US", isWildcard: false, sortOrder: 0),
            ]);

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());

        int[] withoutWildcards = sut.GetAssigned(1001).Select(x => x.Id).ToArray();
        int[] withWildcards = sut.GetAssigned(1001, includeWildcards: true).Select(x => x.Id).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(new[] { 3, 1 }, withoutWildcards);
            Assert.AreEqual(new[] { 2, 3, 1 }, withWildcards);
            Assert.IsEmpty(sut.GetAssigned(9999, includeWildcards: true));
        });
    }

    [Test]
    public void Can_Determine_Whether_A_Node_Has_Assigned_Domains()
    {
        var domainService = new Mock<IDomainService>();
        domainService
#pragma warning disable CS0618 // Type or member is obsolete. This test is for the DomainCacheService, which still calls the obsolete GetAll(bool) method.
            .Setup(x => x.GetAll(true))
#pragma warning restore CS0618 // Type or member is obsolete
            .Returns(
            [
                CreateDomain(1, "https://site.example/", 1001, "en-US", isWildcard: false),
                CreateDomain(2, "*1002", 1002, "en-US", isWildcard: true),
            ]);

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());

        Assert.Multiple(() =>
        {
            Assert.IsTrue(sut.HasAssigned(1001));
            Assert.IsTrue(sut.HasAssigned(1001, includeWildcards: true));
            Assert.IsFalse(sut.HasAssigned(1002), "A node with only wildcard domains has no assigned domains when wildcards are excluded.");
            Assert.IsTrue(sut.HasAssigned(1002, includeWildcards: true));
            Assert.IsFalse(sut.HasAssigned(9999, includeWildcards: true));
            Assert.IsFalse(sut.HasAssigned(0, includeWildcards: true));
        });
    }

    [Test]
    public void Can_Refresh_Assigned_Domains_When_A_Domain_Moves_Or_Is_Removed()
    {
        var domainService = new Mock<IDomainService>();
        domainService
#pragma warning disable CS0618 // Type or member is obsolete. This test is for the DomainCacheService, which still calls the obsolete GetAll(bool) method.
            .Setup(x => x.GetAll(true))
#pragma warning restore CS0618 // Type or member is obsolete
            .Returns(
            [
                CreateDomain(1, "https://moving.example/", 1001, "en-US", isWildcard: false),
                CreateDomain(2, "https://removed.example/", 1001, "en-US", isWildcard: false, sortOrder: 1),
            ]);

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());
        Assert.AreEqual(2, sut.GetAssigned(1001).Count());

        domainService.Setup(x => x.GetById(1)).Returns(CreateDomain(1, "https://moving.example/", 1002, "en-US", isWildcard: false));
        sut.Refresh(
        [
            new DomainCacheRefresher.JsonPayload(1, DomainChangeTypes.Refresh),
            new DomainCacheRefresher.JsonPayload(2, DomainChangeTypes.Remove),
        ]);

        Assert.Multiple(() =>
        {
            Assert.IsEmpty(sut.GetAssigned(1001), "The moved and removed domains should no longer be assigned to their original node.");
            Assert.IsFalse(sut.HasAssigned(1001));
            Assert.AreEqual(new[] { 1 }, sut.GetAssigned(1002).Select(x => x.Id).ToArray(), "The moved domain should be assigned to its new node.");
            Assert.AreEqual(new[] { 1 }, sut.GetAll(true).Select(x => x.Id).ToArray(), "All domains should reflect the same refresh as the assigned domains.");
            Assert.AreEqual(1002, sut.GetAll(true).Single().ContentId);
        });
    }

    [Test]
    public void Can_Refresh_Assigned_Domains_On_RefreshAll()
    {
        var current = new List<IDomain> { CreateDomain(1, "https://site.example/", 1001, "en-US", isWildcard: false) };
        var domainService = new Mock<IDomainService>();
        domainService
#pragma warning disable CS0618 // Type or member is obsolete. This test is for the DomainCacheService, which still calls the obsolete GetAll(bool) method.
            .Setup(x => x.GetAll(true))
#pragma warning restore CS0618 // Type or member is obsolete
            .Returns(() => current.ToArray());

        var sut = new DomainCacheService(domainService.Object, CreateScopeProvider());
        Assert.IsTrue(sut.HasAssigned(1001));

        current = [CreateDomain(2, "https://other.example/", 1002, "en-US", isWildcard: false)];
        sut.Refresh([new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll)]);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(sut.HasAssigned(1001));
            Assert.AreEqual(new[] { 2 }, sut.GetAssigned(1002).Select(x => x.Id).ToArray());
        });
    }

    private static IDomain CreateDomain(int id, string name, int rootContentId, string culture, bool isWildcard, int sortOrder = 0)
    {
        var domain = new Mock<IDomain>();
        domain.SetupGet(x => x.Id).Returns(id);
        domain.SetupGet(x => x.DomainName).Returns(name);
        domain.SetupGet(x => x.RootContentId).Returns(rootContentId);
        domain.SetupGet(x => x.LanguageIsoCode).Returns(culture);
        domain.SetupGet(x => x.IsWildcard).Returns(isWildcard);
        domain.SetupGet(x => x.SortOrder).Returns(sortOrder);
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
