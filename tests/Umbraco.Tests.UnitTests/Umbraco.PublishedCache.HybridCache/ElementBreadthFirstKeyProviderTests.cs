using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Element;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
public class ElementBreadthFirstKeyProviderTests
{
    private static IElementPublishStatusQueryService CreatePublishStatusService(bool allPublished = true)
    {
        var mock = new Mock<IElementPublishStatusQueryService>();
        mock.Setup(x => x.IsPublishedInAnyCulture(It.IsAny<Guid>()))
            .Returns(allPublished);
        return mock.Object;
    }

    private static IElementPublishStatusQueryService CreatePublishStatusService(HashSet<Guid> publishedKeys)
    {
        var mock = new Mock<IElementPublishStatusQueryService>();
        mock.Setup(x => x.IsPublishedInAnyCulture(It.IsAny<Guid>()))
            .Returns((Guid key) => publishedKeys.Contains(key));
        return mock.Object;
    }

    [Test]
    public void ZeroSeedCountReturnsZeroKeys()
    {
        var navigationQueryService = new Mock<IElementNavigationQueryService>();
        var rootKey = Guid.NewGuid();
        IEnumerable<Guid> rootKeyList = new List<Guid> { rootKey };
        IEnumerable<Guid> rootChildren = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeyList)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(It.IsAny<Guid>(), out rootChildren)).Returns(true);

        var cacheSettings = new CacheSettings { ElementBreadthFirstSeedCount = 0 };
        var sut = new ElementBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(cacheSettings), CreatePublishStatusService());

        var result = sut.GetSeedKeys();

        Assert.Zero(result.Count);
    }

    [Test]
    public void OnlyReturnsKeysUpToSeedCount()
    {
        // Structure: Root + 3 children, all published
        var navigationQueryService = new Mock<IElementNavigationQueryService>();
        var rootKey = Guid.NewGuid();
        IEnumerable<Guid> rootKeyList = new List<Guid> { rootKey };
        IEnumerable<Guid> rootChildren = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeyList)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(rootKey, out rootChildren)).Returns(true);

        var expected = 3;
        var cacheSettings = new CacheSettings { ElementBreadthFirstSeedCount = expected };
        var sut = new ElementBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(cacheSettings), CreatePublishStatusService());

        var result = sut.GetSeedKeys();

        Assert.That(result.Count, Is.EqualTo(expected));
    }

    [Test]
    public void IsBreadthFirst()
    {
        // Structure
        // Root
        // - Child1
        //    - GrandChild
        // - Child2
        // - Child3
        var navigationQueryService = new Mock<IElementNavigationQueryService>();
        var rootKey = Guid.NewGuid();
        var child1Key = Guid.NewGuid();
        var grandChildKey = Guid.NewGuid();
        IEnumerable<Guid> rootKeyList = new List<Guid> { rootKey };
        IEnumerable<Guid> rootChildren = new List<Guid> { child1Key, Guid.NewGuid(), Guid.NewGuid() };
        IEnumerable<Guid> grandChildren = new List<Guid> { grandChildKey };
        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeyList)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(rootKey, out rootChildren)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(child1Key, out grandChildren)).Returns(true);

        // This'll get all children but no grandchildren
        var cacheSettings = new CacheSettings { ElementBreadthFirstSeedCount = 4 };
        var sut = new ElementBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(cacheSettings), CreatePublishStatusService());

        var result = sut.GetSeedKeys();

        Assert.That(result.Contains(grandChildKey), Is.False);
    }

    [Test]
    public void CanGetAll()
    {
        var navigationQueryService = new Mock<IElementNavigationQueryService>();
        var rootKey = Guid.NewGuid();

        IEnumerable<Guid> rootKeyList = new List<Guid> { rootKey };
        var childrenCount = 300;
        List<Guid> rootChildren = Enumerable.Range(0, childrenCount).Select(_ => Guid.NewGuid()).ToList();

        IEnumerable<Guid> childrenEnumerable = rootChildren;
        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeyList)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(rootKey, out childrenEnumerable)).Returns(true);
        var settings = new CacheSettings { ElementBreadthFirstSeedCount = int.MaxValue };

        var sut = new ElementBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(settings), CreatePublishStatusService());

        var result = sut.GetSeedKeys();

        var expected = childrenCount + 1; // Root + children
        Assert.That(result.Count, Is.EqualTo(expected));
    }

    [Test]
    public void FiltersOutUnpublishedElements()
    {
        // Structure: Root (published) with 3 children, 1 unpublished
        var navigationQueryService = new Mock<IElementNavigationQueryService>();
        var rootKey = Guid.NewGuid();
        var child1Key = Guid.NewGuid();
        var child2Key = Guid.NewGuid();
        var child3Key = Guid.NewGuid();
        IEnumerable<Guid> rootKeyList = new List<Guid> { rootKey };
        IEnumerable<Guid> rootChildren = new List<Guid> { child1Key, child2Key, child3Key };
        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeyList)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(rootKey, out rootChildren)).Returns(true);

        var publishedKeys = new HashSet<Guid> { rootKey, child2Key, child3Key }; // child1 is unpublished

        var cacheSettings = new CacheSettings { ElementBreadthFirstSeedCount = 10 };
        var sut = new ElementBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(cacheSettings), CreatePublishStatusService(publishedKeys));

        var result = sut.GetSeedKeys();

        Assert.That(result, Has.Count.EqualTo(3)); // root + child2 + child3
        Assert.That(result.Contains(rootKey), Is.True);
        Assert.That(result.Contains(child1Key), Is.False, "Unpublished element should be filtered out");
        Assert.That(result.Contains(child2Key), Is.True);
        Assert.That(result.Contains(child3Key), Is.True);
    }

    [Test]
    public void SeedCountReachedDespiteFilteredElements()
    {
        // Structure: 5 root elements, 2 unpublished
        // Seed count = 3, should still get 3 published ones
        var navigationQueryService = new Mock<IElementNavigationQueryService>();
        var published1 = Guid.NewGuid();
        var unpublished1 = Guid.NewGuid();
        var published2 = Guid.NewGuid();
        var unpublished2 = Guid.NewGuid();
        var published3 = Guid.NewGuid();
        IEnumerable<Guid> rootKeyList = new List<Guid> { published1, unpublished1, published2, unpublished2, published3 };
        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeyList)).Returns(true);

        var publishedKeys = new HashSet<Guid> { published1, published2, published3 };

        var cacheSettings = new CacheSettings { ElementBreadthFirstSeedCount = 3 };
        var sut = new ElementBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(cacheSettings), CreatePublishStatusService(publishedKeys));

        var result = sut.GetSeedKeys();

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.Contains(published1), Is.True);
        Assert.That(result.Contains(published2), Is.True);
        Assert.That(result.Contains(published3), Is.True);
    }

    [Test]
    public void ContainersAreTraversedButNotSeeded()
    {
        // Structure:
        // Container1 (folder, not published)
        //   - Element1 (published)
        //   - Element2 (published)
        // Container2 (folder, not published)
        //   - Element3 (published)
        var navigationQueryService = new Mock<IElementNavigationQueryService>();
        var container1Key = Guid.NewGuid();
        var container2Key = Guid.NewGuid();
        var element1Key = Guid.NewGuid();
        var element2Key = Guid.NewGuid();
        var element3Key = Guid.NewGuid();

        IEnumerable<Guid> rootKeyList = new List<Guid> { container1Key, container2Key };
        IEnumerable<Guid> container1Children = new List<Guid> { element1Key, element2Key };
        IEnumerable<Guid> container2Children = new List<Guid> { element3Key };

        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeyList)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(container1Key, out container1Children)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(container2Key, out container2Children)).Returns(true);

        // Only elements are published, containers are not
        var publishedKeys = new HashSet<Guid> { element1Key, element2Key, element3Key };

        var cacheSettings = new CacheSettings { ElementBreadthFirstSeedCount = 10 };
        var sut = new ElementBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(cacheSettings), CreatePublishStatusService(publishedKeys));

        var result = sut.GetSeedKeys();

        // Only elements should be seeded, not containers
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.Contains(container1Key), Is.False, "Container should not be seeded");
        Assert.That(result.Contains(container2Key), Is.False, "Container should not be seeded");
        Assert.That(result.Contains(element1Key), Is.True);
        Assert.That(result.Contains(element2Key), Is.True);
        Assert.That(result.Contains(element3Key), Is.True);
    }

    [Test]
    public void ContainersDoNotCountTowardSeedLimit()
    {
        // Structure:
        // Container (folder, not published)
        //   - Element1 (published)
        //   - Element2 (published)
        //   - Element3 (published)
        // Seed count = 2, should get 2 elements even though container is traversed first
        var navigationQueryService = new Mock<IElementNavigationQueryService>();
        var containerKey = Guid.NewGuid();
        var element1Key = Guid.NewGuid();
        var element2Key = Guid.NewGuid();
        var element3Key = Guid.NewGuid();

        IEnumerable<Guid> rootKeyList = new List<Guid> { containerKey };
        IEnumerable<Guid> containerChildren = new List<Guid> { element1Key, element2Key, element3Key };

        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeyList)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(containerKey, out containerChildren)).Returns(true);

        var publishedKeys = new HashSet<Guid> { element1Key, element2Key, element3Key };

        var cacheSettings = new CacheSettings { ElementBreadthFirstSeedCount = 2 };
        var sut = new ElementBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(cacheSettings), CreatePublishStatusService(publishedKeys));

        var result = sut.GetSeedKeys();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Contains(containerKey), Is.False);
    }

    [Test]
    public void NestedContainersAreTraversed()
    {
        // Structure:
        // Container1 (folder)
        //   - Container2 (nested folder)
        //     - Element1 (published)
        var navigationQueryService = new Mock<IElementNavigationQueryService>();
        var container1Key = Guid.NewGuid();
        var container2Key = Guid.NewGuid();
        var element1Key = Guid.NewGuid();

        IEnumerable<Guid> rootKeyList = new List<Guid> { container1Key };
        IEnumerable<Guid> container1Children = new List<Guid> { container2Key };
        IEnumerable<Guid> container2Children = new List<Guid> { element1Key };

        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeyList)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(container1Key, out container1Children)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(container2Key, out container2Children)).Returns(true);

        var publishedKeys = new HashSet<Guid> { element1Key };

        var cacheSettings = new CacheSettings { ElementBreadthFirstSeedCount = 10 };
        var sut = new ElementBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(cacheSettings), CreatePublishStatusService(publishedKeys));

        var result = sut.GetSeedKeys();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.Contains(element1Key), Is.True, "Element inside nested containers should be found");
    }
}
