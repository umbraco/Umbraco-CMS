using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

[TestFixture]
internal class ItemAncestorServiceTests
{
    private Mock<IEntityService> _entityServiceMock = null!;
    private ItemAncestorService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var mapper = new Mock<IUmbracoMapper>();
        mapper
            .Setup(m => m.MapEnumerable<IEntitySlim, NamedItemResponseModel>(It.IsAny<IEnumerable<IEntitySlim>>()))
            .Returns((IEnumerable<IEntitySlim> entities) => entities
                .Select(entity => new NamedItemResponseModel { Id = entity.Key, Name = entity.Name ?? string.Empty })
                .ToList());

        _entityServiceMock = new Mock<IEntityService>();
        _sut = new ItemAncestorService(_entityServiceMock.Object, mapper.Object);
    }

    [Test]
    public async Task Empty_Ids_Returns_Empty_Result()
    {
        IEnumerable<ItemAncestorsResponseModel<TestItemResponseModel>> result = await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.Document,
            null,
            new HashSet<Guid>(),
            TestMapper);

        Assert.IsEmpty(result);
    }

    [Test]
    public async Task Entity_Not_Found_Returns_Empty_Result()
    {
        var unknownKey = Guid.NewGuid();

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.IsAny<Guid[]>()))
            .Returns([]);

        IEnumerable<ItemAncestorsResponseModel<TestItemResponseModel>> result = await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.Document,
            null,
            new HashSet<Guid> { unknownKey },
            TestMapper);

        Assert.IsEmpty(result);
    }

    [Test]
    public async Task Root_Level_Entity_Has_Empty_Ancestors()
    {
        var entityKey = Guid.NewGuid();
        var entity = new EntitySlim { Id = 100, Key = entityKey, Path = "-1,100" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, new[] { entityKey }))
            .Returns([entity]);

        List<ItemAncestorsResponseModel<TestItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.Document,
            null,
            new HashSet<Guid> { entityKey },
            TestMapper)).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(entityKey, result[0].Id);
        Assert.IsEmpty(result[0].Ancestors);
    }

    [Test]
    public async Task Single_Entity_With_Ancestors_Returns_Correct_Ordered_Chain()
    {
        var entityKey = Guid.NewGuid();
        var parentKey = Guid.NewGuid();
        var grandparentKey = Guid.NewGuid();

        var entity = new EntitySlim { Id = 300, Key = entityKey, Path = "-1,100,200,300" };
        var grandparent = new EntitySlim { Id = 100, Key = grandparentKey, Path = "-1,100" };
        var parent = new EntitySlim { Id = 200, Key = parentKey, Path = "-1,100,200" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, new[] { entityKey }))
            .Returns([entity]);

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.Is<int[]>(ids => ids.SequenceEqual(new[] { 100, 200 }))))
            .Returns([grandparent, parent]);

        List<ItemAncestorsResponseModel<TestItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.Document,
            null,
            new HashSet<Guid> { entityKey },
            TestMapper)).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(entityKey, result[0].Id);

        var ancestors = result[0].Ancestors.ToList();
        Assert.AreEqual(2, ancestors.Count);

        // Root-first ordering (grandparent before parent).
        Assert.AreEqual(grandparentKey, ancestors[0].Id);
        Assert.AreEqual(parentKey, ancestors[1].Id);
    }

    [Test]
    public async Task Multiple_Entities_Sharing_Ancestors_Returns_Per_Entity_Chains()
    {
        var entity1Key = Guid.NewGuid();
        var entity2Key = Guid.NewGuid();
        var sharedParentKey = Guid.NewGuid();

        // Both entities share the same parent (ID 100).
        var entity1 = new EntitySlim { Id = 200, Key = entity1Key, Path = "-1,100,200" };
        var entity2 = new EntitySlim { Id = 300, Key = entity2Key, Path = "-1,100,300" };
        var sharedParent = new EntitySlim { Id = 100, Key = sharedParentKey, Path = "-1,100" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.Is<Guid[]>(ids => ids.Length == 2)))
            .Returns([entity1, entity2]);

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.Is<int[]>(ids => ids.Contains(100))))
            .Returns([sharedParent]);

        List<ItemAncestorsResponseModel<TestItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.Document,
            null,
            new HashSet<Guid> { entity1Key, entity2Key },
            TestMapper)).ToList();

        Assert.AreEqual(2, result.Count);

        ItemAncestorsResponseModel<TestItemResponseModel> result1 = result.First(r => r.Id == entity1Key);
        ItemAncestorsResponseModel<TestItemResponseModel> result2 = result.First(r => r.Id == entity2Key);

        Assert.AreEqual(1, result1.Ancestors.Count());
        Assert.AreEqual(sharedParentKey, result1.Ancestors.First().Id);

        Assert.AreEqual(1, result2.Ancestors.Count());
        Assert.AreEqual(sharedParentKey, result2.Ancestors.First().Id);
    }

    [Test]
    public async Task Folder_Based_Entity_Type_Resolves_Both_Item_And_Container_Ancestors()
    {
        var entityKey = Guid.NewGuid();
        var folderKey = Guid.NewGuid();
        var itemAncestorKey = Guid.NewGuid();

        // Entity with path containing both a folder (ID 100) and an item ancestor (ID 200).
        var entity = new EntitySlim { Id = 300, Key = entityKey, Path = "-1,100,200,300" };
        var itemAncestor = new EntitySlim { Id = 200, Key = itemAncestorKey, Path = "-1,100,200" };
        var folderAncestor = new EntitySlim { Id = 100, Key = folderKey, Path = "-1,100" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.DataType, new[] { entityKey }))
            .Returns([entity]);

        // Item type GetAll returns only the item ancestor (not the folder).
        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.DataType, It.Is<int[]>(ids => ids.SequenceEqual(new[] { 100, 200 }))))
            .Returns([itemAncestor]);

        // Folder type Get (one-by-one) resolves the folder ancestor.
        _entityServiceMock
            .Setup(x => x.Get(100, UmbracoObjectTypes.DataTypeContainer))
            .Returns(folderAncestor);

        List<ItemAncestorsResponseModel<NamedItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.DataType,
            UmbracoObjectTypes.DataTypeContainer,
            new HashSet<Guid> { entityKey })).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(entityKey, result[0].Id);

        var ancestors = result[0].Ancestors.ToList();
        Assert.AreEqual(2, ancestors.Count);

        // Root-first: folder (100) before item ancestor (200).
        Assert.AreEqual(folderKey, ancestors[0].Id);
        Assert.AreEqual(itemAncestorKey, ancestors[1].Id);
    }

    [Test]
    public async Task Folder_Entity_Found_Via_Folder_Object_Type()
    {
        var folderKey = Guid.NewGuid();
        var parentFolderKey = Guid.NewGuid();

        var folder = new EntitySlim { Id = 200, Key = folderKey, Path = "-1,100,200" };
        var parentFolder = new EntitySlim { Id = 100, Key = parentFolderKey, Path = "-1,100" };

        // Entity not found via item type.
        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.DataType, new[] { folderKey }))
            .Returns([]);

        // Entity found via folder type (one-by-one lookup).
        _entityServiceMock
            .Setup(x => x.Get(folderKey, UmbracoObjectTypes.DataTypeContainer))
            .Returns(folder);

        // Item type GetAll for ancestor IDs returns nothing (it's a folder).
        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.DataType, It.Is<int[]>(ids => ids.Contains(100))))
            .Returns([]);

        // Folder type Get resolves the parent folder.
        _entityServiceMock
            .Setup(x => x.Get(100, UmbracoObjectTypes.DataTypeContainer))
            .Returns(parentFolder);

        List<ItemAncestorsResponseModel<NamedItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.DataType,
            UmbracoObjectTypes.DataTypeContainer,
            new HashSet<Guid> { folderKey })).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(folderKey, result[0].Id);

        var ancestors = result[0].Ancestors.ToList();
        Assert.AreEqual(1, ancestors.Count);
        Assert.AreEqual(parentFolderKey, ancestors[0].Id);
    }

    [Test]
    public async Task Mixed_Found_And_Not_Found_Entities_Omits_Not_Found()
    {
        var foundKey = Guid.NewGuid();
        var notFoundKey = Guid.NewGuid();

        var foundEntity = new EntitySlim { Id = 100, Key = foundKey, Path = "-1,100" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.IsAny<Guid[]>()))
            .Returns([foundEntity]);

        List<ItemAncestorsResponseModel<TestItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.Document,
            null,
            new HashSet<Guid> { foundKey, notFoundKey },
            TestMapper)).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(foundKey, result[0].Id);
    }

    [Test]
    public async Task Self_Is_Not_Included_In_Ancestors()
    {
        var entityKey = Guid.NewGuid();
        var parentKey = Guid.NewGuid();

        var entity = new EntitySlim { Id = 200, Key = entityKey, Path = "-1,100,200" };
        var parent = new EntitySlim { Id = 100, Key = parentKey, Path = "-1,100" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, new[] { entityKey }))
            .Returns([entity]);

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.Is<int[]>(ids => ids.SequenceEqual(new[] { 100 }))))
            .Returns([parent]);

        List<ItemAncestorsResponseModel<TestItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.Document,
            null,
            new HashSet<Guid> { entityKey },
            TestMapper)).ToList();

        var ancestors = result[0].Ancestors.ToList();
        Assert.AreEqual(1, ancestors.Count);
        Assert.That(ancestors.Select(a => a.Id), Does.Not.Contain(entityKey));
        Assert.AreEqual(parentKey, ancestors[0].Id);
    }

    [Test]
    public async Task Deep_Ancestor_Chain_Returns_All_Levels_In_Order()
    {
        var entityKey = Guid.NewGuid();
        var level1Key = Guid.NewGuid();
        var level2Key = Guid.NewGuid();
        var level3Key = Guid.NewGuid();

        var entity = new EntitySlim { Id = 400, Key = entityKey, Path = "-1,100,200,300,400" };
        var level1 = new EntitySlim { Id = 100, Key = level1Key, Path = "-1,100" };
        var level2 = new EntitySlim { Id = 200, Key = level2Key, Path = "-1,100,200" };
        var level3 = new EntitySlim { Id = 300, Key = level3Key, Path = "-1,100,200,300" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, new[] { entityKey }))
            .Returns([entity]);

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.Is<int[]>(ids => ids.Length == 3)))
            .Returns([level1, level2, level3]);

        List<ItemAncestorsResponseModel<TestItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.Document,
            null,
            new HashSet<Guid> { entityKey },
            TestMapper)).ToList();

        var ancestors = result[0].Ancestors.ToList();
        Assert.AreEqual(3, ancestors.Count);
        Assert.AreEqual(level1Key, ancestors[0].Id);
        Assert.AreEqual(level2Key, ancestors[1].Id);
        Assert.AreEqual(level3Key, ancestors[2].Id);
    }

    [Test]
    public async Task Multiple_Root_Level_Entities_All_Have_Empty_Ancestors()
    {
        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();
        var key3 = Guid.NewGuid();

        var entity1 = new EntitySlim { Id = 100, Key = key1, Path = "-1,100" };
        var entity2 = new EntitySlim { Id = 200, Key = key2, Path = "-1,200" };
        var entity3 = new EntitySlim { Id = 300, Key = key3, Path = "-1,300" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Media, It.IsAny<Guid[]>()))
            .Returns([entity1, entity2, entity3]);

        List<ItemAncestorsResponseModel<TestItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.Media,
            null,
            new HashSet<Guid> { key1, key2, key3 },
            TestMapper)).ToList();

        Assert.AreEqual(3, result.Count);
        Assert.That(result, Has.All.Property(nameof(ItemAncestorsResponseModel<TestItemResponseModel>.Ancestors)).Empty);
    }

    [Test]
    public async Task Multiple_Entities_At_Different_Depths()
    {
        var rootEntityKey = Guid.NewGuid();
        var childEntityKey = Guid.NewGuid();
        var parentKey = Guid.NewGuid();

        var rootEntity = new EntitySlim { Id = 100, Key = rootEntityKey, Path = "-1,100" };
        var childEntity = new EntitySlim { Id = 300, Key = childEntityKey, Path = "-1,200,300" };
        var parent = new EntitySlim { Id = 200, Key = parentKey, Path = "-1,200" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.IsAny<Guid[]>()))
            .Returns([rootEntity, childEntity]);

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.Is<int[]>(ids => ids.SequenceEqual(new[] { 200 }))))
            .Returns([parent]);

        List<ItemAncestorsResponseModel<TestItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.Document,
            null,
            new HashSet<Guid> { rootEntityKey, childEntityKey },
            TestMapper)).ToList();

        Assert.AreEqual(2, result.Count);

        ItemAncestorsResponseModel<TestItemResponseModel> rootResult = result.First(r => r.Id == rootEntityKey);
        ItemAncestorsResponseModel<TestItemResponseModel> childResult = result.First(r => r.Id == childEntityKey);

        Assert.IsEmpty(rootResult.Ancestors);
        Assert.AreEqual(1, childResult.Ancestors.Count());
        Assert.AreEqual(parentKey, childResult.Ancestors.First().Id);
    }

    [Test]
    public async Task Folder_Type_Provided_But_All_Ancestors_Are_Items()
    {
        var entityKey = Guid.NewGuid();
        var parentKey = Guid.NewGuid();

        var entity = new EntitySlim { Id = 200, Key = entityKey, Path = "-1,100,200" };
        var parent = new EntitySlim { Id = 100, Key = parentKey, Path = "-1,100" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.DataType, new[] { entityKey }))
            .Returns([entity]);

        // Entity found via item type, so folder Get is never called for entity lookup.

        // All ancestors resolved as items.
        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.DataType, It.Is<int[]>(ids => ids.SequenceEqual(new[] { 100 }))))
            .Returns([parent]);

        List<ItemAncestorsResponseModel<NamedItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.DataType,
            UmbracoObjectTypes.DataTypeContainer,
            new HashSet<Guid> { entityKey })).ToList();

        Assert.AreEqual(1, result.Count);
        var ancestors = result[0].Ancestors.ToList();
        Assert.AreEqual(1, ancestors.Count);
        Assert.AreEqual(parentKey, ancestors[0].Id);

        // Container Get(Guid) should not be called since entity was found as item type.
        _entityServiceMock.Verify(
            x => x.Get(It.IsAny<Guid>(), UmbracoObjectTypes.DataTypeContainer),
            Times.Never);

        // Container Get(int) should not be called since all ancestors were found as items.
        _entityServiceMock.Verify(
            x => x.Get(It.IsAny<int>(), UmbracoObjectTypes.DataTypeContainer),
            Times.Never);
    }

    [Test]
    public async Task Container_Ancestor_Not_Found_Is_Omitted_From_Chain()
    {
        var entityKey = Guid.NewGuid();

        // Entity with two ancestors in its path (IDs 100 and 200).
        var entity = new EntitySlim { Id = 300, Key = entityKey, Path = "-1,100,200,300" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.DataType, new[] { entityKey }))
            .Returns([entity]);

        // Entity found via item type, so no folder lookup for the entity itself.

        // Item lookup finds nothing.
        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.DataType, It.IsAny<int[]>()))
            .Returns([]);

        // Container lookup also finds nothing.
        _entityServiceMock
            .Setup(x => x.Get(It.IsAny<int>(), UmbracoObjectTypes.DataTypeContainer))
            .Returns((IEntitySlim?)null);

        List<ItemAncestorsResponseModel<NamedItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.DataType,
            UmbracoObjectTypes.DataTypeContainer,
            new HashSet<Guid> { entityKey })).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(entityKey, result[0].Id);
        Assert.IsEmpty(result[0].Ancestors);
    }

    [Test]
    public async Task Ancestor_Ids_Are_Deduplicated_Before_Fetching()
    {
        var entity1Key = Guid.NewGuid();
        var entity2Key = Guid.NewGuid();
        var sharedParentKey = Guid.NewGuid();
        var sharedGrandparentKey = Guid.NewGuid();

        // Both entities share the same ancestor chain: 100 -> 200.
        var entity1 = new EntitySlim { Id = 300, Key = entity1Key, Path = "-1,100,200,300" };
        var entity2 = new EntitySlim { Id = 400, Key = entity2Key, Path = "-1,100,200,400" };
        var grandparent = new EntitySlim { Id = 100, Key = sharedGrandparentKey, Path = "-1,100" };
        var parent = new EntitySlim { Id = 200, Key = sharedParentKey, Path = "-1,100,200" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.IsAny<Guid[]>()))
            .Returns([entity1, entity2]);

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.IsAny<int[]>()))
            .Returns([grandparent, parent]);

        (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.Document,
            null,
            new HashSet<Guid> { entity1Key, entity2Key },
            TestMapper)).ToList();

        // Verify GetAll(int[]) was called with deduplicated IDs (2 unique: 100, 200).
        _entityServiceMock.Verify(
            x => x.GetAll(UmbracoObjectTypes.Document, It.Is<int[]>(ids => ids.Length == 2 && ids.Distinct().Count() == 2)),
            Times.Once);
    }

    [Test]
    public async Task Entity_Not_Found_Via_Item_Type_Or_Folder_Type_Returns_Empty()
    {
        var unknownKey = Guid.NewGuid();

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.DataType, It.IsAny<Guid[]>()))
            .Returns([]);

        // Folder lookup (one-by-one) also finds nothing.
        _entityServiceMock
            .Setup(x => x.Get(unknownKey, UmbracoObjectTypes.DataTypeContainer))
            .Returns((IEntitySlim?)null);

        IEnumerable<ItemAncestorsResponseModel<NamedItemResponseModel>> result = await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.DataType,
            UmbracoObjectTypes.DataTypeContainer,
            new HashSet<Guid> { unknownKey });

        Assert.IsEmpty(result);
    }

    [Test]
    public async Task Folder_Lookup_Only_Queries_Keys_Not_Found_As_Items()
    {
        var itemKey = Guid.NewGuid();
        var folderKey = Guid.NewGuid();

        var item = new EntitySlim { Id = 100, Key = itemKey, Path = "-1,100" };
        var folder = new EntitySlim { Id = 200, Key = folderKey, Path = "-1,200" };

        // Item lookup finds only the item.
        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.DataType, It.IsAny<Guid[]>()))
            .Returns([item]);

        // Folder lookup (one-by-one) should only be called for the key not found as an item.
        _entityServiceMock
            .Setup(x => x.Get(folderKey, UmbracoObjectTypes.DataTypeContainer))
            .Returns(folder);

        List<ItemAncestorsResponseModel<NamedItemResponseModel>> result = (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.DataType,
            UmbracoObjectTypes.DataTypeContainer,
            new HashSet<Guid> { itemKey, folderKey })).ToList();

        Assert.AreEqual(2, result.Count);

        // Verify folder Get was called only for the folder key, not the item key.
        _entityServiceMock.Verify(
            x => x.Get(folderKey, UmbracoObjectTypes.DataTypeContainer),
            Times.Once);
        _entityServiceMock.Verify(
            x => x.Get(itemKey, UmbracoObjectTypes.DataTypeContainer),
            Times.Never);
    }

    [Test]
    public async Task Mapper_Receives_All_Ancestor_Entities()
    {
        var entityKey = Guid.NewGuid();
        var parentKey = Guid.NewGuid();
        var grandparentKey = Guid.NewGuid();

        var entity = new EntitySlim { Id = 300, Key = entityKey, Path = "-1,100,200,300" };
        var grandparent = new EntitySlim { Id = 100, Key = grandparentKey, Path = "-1,100" };
        var parent = new EntitySlim { Id = 200, Key = parentKey, Path = "-1,100,200" };

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, new[] { entityKey }))
            .Returns([entity]);

        _entityServiceMock
            .Setup(x => x.GetAll(UmbracoObjectTypes.Document, It.Is<int[]>(ids => ids.SequenceEqual(new[] { 100, 200 }))))
            .Returns([grandparent, parent]);

        IEnumerable<IEntitySlim>? receivedAncestors = null;

        (await _sut.GetAncestorsAsync(
            UmbracoObjectTypes.Document,
            null,
            new HashSet<Guid> { entityKey },
            ancestors =>
            {
                receivedAncestors = ancestors.ToList();
                return TestMapper(receivedAncestors);
            })).ToList();

        Assert.IsNotNull(receivedAncestors);
        var ancestorList = receivedAncestors!.ToList();
        Assert.AreEqual(2, ancestorList.Count);
        Assert.That(ancestorList.Select(a => a.Key), Is.EquivalentTo(new[] { grandparentKey, parentKey }));
    }

    /// <summary>
    /// Simple test mapper that creates a <see cref="TestItemResponseModel"/> from each ancestor entity.
    /// </summary>
    private static Task<IEnumerable<TestItemResponseModel>> TestMapper(IEnumerable<IEntitySlim> ancestors)
        => Task.FromResult(ancestors.Select(a => new TestItemResponseModel { Id = a.Key }));

    /// <summary>
    /// Concrete test implementation of <see cref="ItemResponseModelBase"/> for use in tests.
    /// </summary>
    private class TestItemResponseModel : ItemResponseModelBase
    {
    }
}
