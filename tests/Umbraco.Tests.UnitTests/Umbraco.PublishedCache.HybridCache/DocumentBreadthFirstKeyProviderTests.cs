using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Document;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
public class DocumentBreadthFirstKeyProviderTests
{
    [Test]
    public void ZeroSeedCountReturnsZeroKeys()
    {
        // The structure here doesn't matter greatly, it just matters that there is something.
        var navigationQueryService = new Mock<IDocumentNavigationQueryService>();
        var rootKey = Guid.NewGuid();
        IEnumerable<Guid> rootKeyList = new List<Guid> { rootKey };
        IEnumerable<Guid> rootChildren = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeyList)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(It.IsAny<Guid>(), out rootChildren)).Returns(true);

        var cacheSettings = new CacheSettings { DocumentBreadthFirstSeedCount = 0 };
        var sut = new DocumentBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(cacheSettings));

        var result = sut.GetSeedKeys();

        Assert.Zero(result.Count);
    }

    [Test]
    public void OnlyReturnsKeysUpToSeedCount()
    {
        // Structure
        // Root
        // - Child1
        // - Child2
        // - Child3
        var navigationQueryService = new Mock<IDocumentNavigationQueryService>();
        var rootKey = Guid.NewGuid();
        IEnumerable<Guid> rootKeyList = new List<Guid> { rootKey };
        IEnumerable<Guid> rootChildren = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeyList)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(rootKey, out rootChildren)).Returns(true);

        var expected = 3;
        var cacheSettings = new CacheSettings { DocumentBreadthFirstSeedCount = expected };
        var sut = new DocumentBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(cacheSettings));

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

        var navigationQueryService = new Mock<IDocumentNavigationQueryService>();
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
        var cacheSettings = new CacheSettings { DocumentBreadthFirstSeedCount = 4 };

        var sut = new DocumentBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(cacheSettings));

        var result = sut.GetSeedKeys();

        Assert.That(result.Contains(grandChildKey), Is.False);
    }

    [Test]
    public void CanGetAll()
    {
        var navigationQueryService = new Mock<IDocumentNavigationQueryService>();
        var rootKey = Guid.NewGuid();


        IEnumerable<Guid> rootKeyList = new List<Guid> { rootKey };
        var childrenCount = 300;
        List<Guid> rootChildren = new List<Guid> ();
        for (int i = 0; i < childrenCount; i++)
        {
            rootChildren.Add(Guid.NewGuid());
        }

        IEnumerable<Guid> childrenEnumerable = rootChildren;
        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeyList)).Returns(true);
        navigationQueryService.Setup(x => x.TryGetChildrenKeys(rootKey, out childrenEnumerable)).Returns(true);
        var settings = new CacheSettings { DocumentBreadthFirstSeedCount = int.MaxValue };


        var sut = new DocumentBreadthFirstKeyProvider(navigationQueryService.Object, Options.Create(settings));

        var result = sut.GetSeedKeys();

        var expected = childrenCount + 1; // Root + children
        Assert.That(result.Count, Is.EqualTo(expected));
    }
}
