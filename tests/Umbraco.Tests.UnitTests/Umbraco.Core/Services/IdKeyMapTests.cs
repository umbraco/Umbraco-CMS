using System.Data;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class IdKeyMapTests
{
    private static IdKeyMap GetIdKeyMap()
        => new IdKeyMap(Mock.Of<ICoreScopeProvider>(), Mock.Of<IIdKeyMapRepository>());

    private static IdKeyMap GetIdKeyMap(out Mock<IIdKeyMapRepository> repository)
    {
        repository = new Mock<IIdKeyMapRepository>();

        var scopeProvider = new Mock<ICoreScopeProvider>();
        scopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());

        return new IdKeyMap(scopeProvider.Object, repository.Object);
    }

    [Test]
    public void Can_Resolve_Content_Recycle_Bin_Id_From_Key()
    {
        var result = GetIdKeyMap().GetIdForKey(Constants.System.RecycleBinContentKey, UmbracoObjectTypes.Document);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinContent, result.Result);
    }

    [Test]
    public void Can_Resolve_Media_Recycle_Bin_Id_From_Key()
    {
        var result = GetIdKeyMap().GetIdForKey(Constants.System.RecycleBinMediaKey, UmbracoObjectTypes.Media);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinMedia, result.Result);
    }

    [TestCase(UmbracoObjectTypes.Element)]
    [TestCase(UmbracoObjectTypes.ElementContainer)]
    public void Can_Resolve_Element_Recycle_Bin_Id_From_Key(UmbracoObjectTypes objectType)
    {
        var result = GetIdKeyMap().GetIdForKey(Constants.System.RecycleBinElementKey, objectType);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinElement, result.Result);
    }

    [Test]
    public void Can_Resolve_Content_Recycle_Bin_Key_From_Id()
    {
        var result = GetIdKeyMap().GetKeyForId(Constants.System.RecycleBinContent, UmbracoObjectTypes.Document);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinContentKey, result.Result);
    }

    [Test]
    public void Can_Resolve_Media_Recycle_Bin_Key_From_Id()
    {
        var result = GetIdKeyMap().GetKeyForId(Constants.System.RecycleBinMedia, UmbracoObjectTypes.Media);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinMediaKey, result.Result);
    }

    [TestCase(UmbracoObjectTypes.Element)]
    [TestCase(UmbracoObjectTypes.ElementContainer)]
    public void Can_Resolve_Element_Recycle_Bin_Key_From_Id(UmbracoObjectTypes objectType)
    {
        var result = GetIdKeyMap().GetKeyForId(Constants.System.RecycleBinElement, objectType);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinElementKey, result.Result);
    }

    [Test]
    public void Can_Resolve_Both_Directions_From_Populated_Pairs_Without_Hitting_The_Repository()
    {
        var key = Guid.NewGuid();
        IdKeyMap idKeyMap = GetIdKeyMap(out Mock<IIdKeyMapRepository> repository);

        idKeyMap.PopulateCache([(1234, key)], UmbracoObjectTypes.Document);

        Assert.Multiple(() =>
        {
            Attempt<int> idAttempt = idKeyMap.GetIdForKey(key, UmbracoObjectTypes.Document);
            Assert.IsTrue(idAttempt.Success);
            Assert.AreEqual(1234, idAttempt.Result);

            Attempt<Guid> keyAttempt = idKeyMap.GetKeyForId(1234, UmbracoObjectTypes.Document);
            Assert.IsTrue(keyAttempt.Success);
            Assert.AreEqual(key, keyAttempt.Result);
        });

        repository.Verify(
            x => x.GetIdForKey(It.IsAny<Guid>(), It.IsAny<UmbracoObjectTypes>()),
            Times.Never);
        repository.Verify(
            x => x.GetIdForKey(It.IsAny<int>(), It.IsAny<UmbracoObjectTypes>()),
            Times.Never);
    }

    [Test]
    public void Can_Resolve_Both_Directions_From_A_Single_Populated_Pair_Without_Hitting_The_Repository()
    {
        var key = Guid.NewGuid();
        IdKeyMap idKeyMap = GetIdKeyMap(out Mock<IIdKeyMapRepository> repository);

        idKeyMap.PopulateCache(4321, key, UmbracoObjectTypes.Media);

        Assert.Multiple(() =>
        {
            Attempt<int> idAttempt = idKeyMap.GetIdForKey(key, UmbracoObjectTypes.Media);
            Assert.IsTrue(idAttempt.Success);
            Assert.AreEqual(4321, idAttempt.Result);

            Attempt<Guid> keyAttempt = idKeyMap.GetKeyForId(4321, UmbracoObjectTypes.Media);
            Assert.IsTrue(keyAttempt.Success);
            Assert.AreEqual(key, keyAttempt.Result);
        });

        repository.Verify(
            x => x.GetIdForKey(It.IsAny<Guid>(), It.IsAny<UmbracoObjectTypes>()),
            Times.Never);
        repository.Verify(
            x => x.GetIdForKey(It.IsAny<int>(), It.IsAny<UmbracoObjectTypes>()),
            Times.Never);
    }

    [Test]
    public void Cannot_Resolve_Populated_Pair_Under_A_Different_Object_Type()
    {
        var key = Guid.NewGuid();
        IdKeyMap idKeyMap = GetIdKeyMap(out Mock<IIdKeyMapRepository> repository);

        idKeyMap.PopulateCache(1234, key, UmbracoObjectTypes.Document);

        // The entries are keyed by identifier alone, so the object type on the value has to be what rejects
        // the mismatch and sends the lookup on to the repository.
        Assert.IsFalse(idKeyMap.GetIdForKey(key, UmbracoObjectTypes.Media).Success);
        Assert.IsFalse(idKeyMap.GetKeyForId(1234, UmbracoObjectTypes.Media).Success);

        repository.Verify(x => x.GetIdForKey(key, UmbracoObjectTypes.Media), Times.Once);
        repository.Verify(x => x.GetIdForKey(1234, UmbracoObjectTypes.Media), Times.Once);
    }

    [Test]
    public void Can_Populate_Cache_Without_Overriding_Recycle_Bin_Identifiers()
    {
        IdKeyMap idKeyMap = GetIdKeyMap(out _);

        idKeyMap.PopulateCache(Constants.System.RecycleBinContent, Guid.NewGuid(), UmbracoObjectTypes.Document);

        Attempt<Guid> keyAttempt = idKeyMap.GetKeyForId(Constants.System.RecycleBinContent, UmbracoObjectTypes.Document);
        Assert.IsTrue(keyAttempt.Success);
        Assert.AreEqual(Constants.System.RecycleBinContentKey, keyAttempt.Result);
    }

    [Test]
    public void Can_Populate_Cache_Concurrently_While_Reading()
    {
        const int Count = 500;
        IdKeyMap idKeyMap = GetIdKeyMap(out _);
        var pairs = Enumerable.Range(1, Count).Select(id => (Id: id, Key: Guid.NewGuid())).ToArray();

        Assert.DoesNotThrow(() => Parallel.ForEach(pairs, pair =>
        {
            idKeyMap.PopulateCache(pair.Id, pair.Key, UmbracoObjectTypes.Document);
            idKeyMap.GetIdForKey(pair.Key, UmbracoObjectTypes.Document);
            idKeyMap.PopulateCache(pair.Id, pair.Key, UmbracoObjectTypes.Document);
        }));

        foreach ((int id, Guid key) in pairs)
        {
            Assert.AreEqual(id, idKeyMap.GetIdForKey(key, UmbracoObjectTypes.Document).Result);
            Assert.AreEqual(key, idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document).Result);
        }
    }
}
