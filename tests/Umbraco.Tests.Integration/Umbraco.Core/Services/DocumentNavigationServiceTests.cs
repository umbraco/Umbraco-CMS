using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentNavigationServiceTests : DocumentNavigationServiceTestsBase
{
    [SetUp]
    public async Task Setup()
    {
        // Root
        //    - Child 1
        //      - Grandchild 1
        //      - Grandchild 2
        //    - Child 2
        //      - Grandchild 3
        //        - Great-grandchild 1
        //    - Child 3
        //      - Grandchild 4

        // Doc Type
        ContentType = ContentTypeBuilder.CreateSimpleContentType("page", "Page");
        ContentType.Key = new Guid("DD72B8A6-2CE3-47F0-887E-B695A1A5D086");
        ContentType.AllowedAsRoot = true;
        ContentType.AllowedContentTypes = new[] { new ContentTypeSort(ContentType.Key, 0, ContentType.Alias) };
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);

        // Content
        var rootModel = CreateContentCreateModel("Root", new Guid("E48DD82A-7059-418E-9B82-CDD5205796CF"));
        var rootCreateAttempt = await ContentEditingService.CreateAsync(rootModel, Constants.Security.SuperUserKey);
        Root = rootCreateAttempt.Result.Content!;

        var child1Model = CreateContentCreateModel("Child 1", new Guid("C6173927-0C59-4778-825D-D7B9F45D8DDE"), parentKey: Root.Key);
        var child1CreateAttempt = await ContentEditingService.CreateAsync(child1Model, Constants.Security.SuperUserKey);
        Child1 = child1CreateAttempt.Result.Content!;

        var grandchild1Model = CreateContentCreateModel("Grandchild 1", new Guid("E856AC03-C23E-4F63-9AA9-681B42A58573"), parentKey: Child1.Key);
        var grandchild1CreateAttempt = await ContentEditingService.CreateAsync(grandchild1Model, Constants.Security.SuperUserKey);
        Grandchild1 = grandchild1CreateAttempt.Result.Content!;

        var grandchild2Model = CreateContentCreateModel("Grandchild 2", new Guid("A1B1B217-B02F-4307-862C-A5E22DB729EB"), parentKey: Child1.Key);
        var grandchild2CreateAttempt = await ContentEditingService.CreateAsync(grandchild2Model, Constants.Security.SuperUserKey);
        Grandchild2 = grandchild2CreateAttempt.Result.Content!;

        var child2Model = CreateContentCreateModel("Child 2", new Guid("60E0E5C4-084E-4144-A560-7393BEAD2E96"), parentKey: Root.Key);
        var child2CreateAttempt = await ContentEditingService.CreateAsync(child2Model, Constants.Security.SuperUserKey);
        Child2 = child2CreateAttempt.Result.Content!;

        var grandchild3Model = CreateContentCreateModel("Grandchild 3", new Guid("D63C1621-C74A-4106-8587-817DEE5FB732"), parentKey: Child2.Key);
        var grandchild3CreateAttempt = await ContentEditingService.CreateAsync(grandchild3Model, Constants.Security.SuperUserKey);
        Grandchild3 = grandchild3CreateAttempt.Result.Content!;

        var greatGrandchild1Model = CreateContentCreateModel("Great-grandchild 1", new Guid("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7"), parentKey: Grandchild3.Key);
        var greatGrandchild1CreateAttempt = await ContentEditingService.CreateAsync(greatGrandchild1Model, Constants.Security.SuperUserKey);
        GreatGrandchild1 = greatGrandchild1CreateAttempt.Result.Content!;

        var child3Model = CreateContentCreateModel("Child 3", new Guid("B606E3FF-E070-4D46-8CB9-D31352029FDF"), parentKey: Root.Key);
        var child3CreateAttempt = await ContentEditingService.CreateAsync(child3Model, Constants.Security.SuperUserKey);
        Child3 = child3CreateAttempt.Result.Content!;

        var grandchild4Model = CreateContentCreateModel("Grandchild 4", new Guid("F381906C-223C-4466-80F7-B63B4EE073F8"), parentKey: Child3.Key);
        var grandchild4CreateAttempt = await ContentEditingService.CreateAsync(grandchild4Model, Constants.Security.SuperUserKey);
        Grandchild4 = grandchild4CreateAttempt.Result.Content!;
    }

    [Test]
    public async Task Structure_Does_Not_Update_When_Scope_Is_Not_Completed()
    {
        // Arrange
        Guid notCreatedRootKey = new Guid("516927E5-8574-497B-B45B-E27EFAB47DE4");

        // Create node at content root
        var createModel = CreateContentCreateModel("Root 2", notCreatedRootKey);

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        }

        // Act
        var nodeExists = DocumentNavigationQueryService.TryGetParentKey(notCreatedRootKey, out _);

        // Assert
        Assert.IsFalse(nodeExists);
    }

    [Test]
    public async Task Can_Filter_Root_Items_By_Type()
    {
        // Arrange
        DocumentNavigationQueryService.TryGetRootKeysOfType(ContentType.Alias, out IEnumerable<Guid> initialRootKeysOfType);
        List<Guid> initialRootOfTypeList = initialRootKeysOfType.ToList();

        // Doc Type
        var anotherContentType = ContentTypeBuilder.CreateSimpleContentType("anotherPage", "Another page");
        anotherContentType.Key = new Guid("58A2958E-B34F-4289-A225-E99EEC2456AB");
        anotherContentType.AllowedContentTypes = new[] { new ContentTypeSort(anotherContentType.Key, 0, anotherContentType.Alias) };
        anotherContentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(anotherContentType, Constants.Security.SuperUserKey);

        // Update old doc type
        ContentType.AllowedContentTypes = new[] { new ContentTypeSort(ContentType.Key, 0, ContentType.Alias), new ContentTypeSort(anotherContentType.Key, 1, anotherContentType.Alias) };
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Content
        var root2Model = CreateContentCreateModel("Root 2", new Guid("11233548-2E87-4D3E-8FC4-4400F9DBEF56"), anotherContentType.Key); // Using new doc type
        await ContentEditingService.CreateAsync(root2Model, Constants.Security.SuperUserKey);
        var root3Model = CreateContentCreateModel("Root 3", new Guid("6E10F212-CE7F-47B5-A796-345861AEE613"), anotherContentType.Key); // Using new doc type
        await ContentEditingService.CreateAsync(root3Model, Constants.Security.SuperUserKey);

        // Act
        DocumentNavigationQueryService.TryGetRootKeysOfType(anotherContentType.Alias, out IEnumerable<Guid> filteredRootKeysOfType);
        List<Guid> filteredRootList = filteredRootKeysOfType.ToList();

        DocumentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> allRootKeys);
        List<Guid> allRootList = allRootKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, initialRootOfTypeList.Count); // Verify that loaded doc types can be used to filter
            Assert.AreEqual(2, filteredRootList.Count); // Verify that new doc type can be used to filter
            Assert.AreEqual(3, allRootList.Count);
        });
    }

    [Test]
    public async Task Can_Filter_Children_By_Type()
    {
        // Arrange
        Guid parentKey = Root.Key;
        DocumentNavigationQueryService.TryGetChildrenKeysOfType(parentKey, ContentType.Alias, out IEnumerable<Guid> initialChildrenKeysOfType);
        List<Guid> initialChildrenOfTypeList = initialChildrenKeysOfType.ToList();

        // Doc Type
        var anotherContentType = ContentTypeBuilder.CreateSimpleContentType("anotherPage", "Another page");
        anotherContentType.Key = new Guid("58A2958E-B34F-4289-A225-E99EEC2456AB");
        anotherContentType.AllowedContentTypes = new[] { new ContentTypeSort(anotherContentType.Key, 0, anotherContentType.Alias) };
        await ContentTypeService.CreateAsync(anotherContentType, Constants.Security.SuperUserKey);

        // Update old doc type
        ContentType.AllowedContentTypes = new[] { new ContentTypeSort(ContentType.Key, 0, ContentType.Alias), new ContentTypeSort(anotherContentType.Key, 1, anotherContentType.Alias) };
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Content
        var child4Model = CreateContentCreateModel("Child 4", new Guid("11233548-2E87-4D3E-8FC4-4400F9DBEF56"), anotherContentType.Key, parentKey); // Using new doc type
        await ContentEditingService.CreateAsync(child4Model, Constants.Security.SuperUserKey);

        // Act
        DocumentNavigationQueryService.TryGetChildrenKeysOfType(parentKey, anotherContentType.Alias, out IEnumerable<Guid> filteredChildrenKeysOfType);
        List<Guid> filteredChildrenList = filteredChildrenKeysOfType.ToList();

        DocumentNavigationQueryService.TryGetChildrenKeys(parentKey, out IEnumerable<Guid> allChildrenKeys);
        List<Guid> allChildrenList = allChildrenKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, initialChildrenOfTypeList.Count); // Verify that loaded doc types can be used to filter
            Assert.AreEqual(1, filteredChildrenList.Count); // Verify that new doc type can be used to filter
            Assert.AreEqual(4, allChildrenList.Count);
        });
    }

    [Test]
    public async Task Can_Filter_Descendants_By_Type()
    {
        // Arrange
        Guid parentKey = Child2.Key;
        DocumentNavigationQueryService.TryGetDescendantsKeysOfType(parentKey, ContentType.Alias, out IEnumerable<Guid> initialDescendantsKeysOfType);
        List<Guid> initialDescendantsOfTypeList = initialDescendantsKeysOfType.ToList();

        // Doc Type
        var anotherContentType = ContentTypeBuilder.CreateSimpleContentType("anotherPage", "Another page");
        anotherContentType.Key = new Guid("58A2958E-B34F-4289-A225-E99EEC2456AB");
        anotherContentType.AllowedContentTypes = new[] { new ContentTypeSort(anotherContentType.Key, 0, anotherContentType.Alias) };
        await ContentTypeService.CreateAsync(anotherContentType, Constants.Security.SuperUserKey);

        // Update old doc type
        ContentType.AllowedContentTypes = new[] { new ContentTypeSort(ContentType.Key, 0, ContentType.Alias), new ContentTypeSort(anotherContentType.Key, 1, anotherContentType.Alias) };
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Content
        var greatGreatGrandchild1Model = CreateContentCreateModel("Great-great-grandchild 1", new Guid("11233548-2E87-4D3E-8FC4-4400F9DBEF56"), anotherContentType.Key, GreatGrandchild1.Key); // Using new doc type
        await ContentEditingService.CreateAsync(greatGreatGrandchild1Model, Constants.Security.SuperUserKey);

        // Act
        DocumentNavigationQueryService.TryGetDescendantsKeysOfType(parentKey, anotherContentType.Alias, out IEnumerable<Guid> filteredDescendantsKeysOfType);
        List<Guid> filteredDescendantsList = filteredDescendantsKeysOfType.ToList();

        DocumentNavigationQueryService.TryGetDescendantsKeys(parentKey, out IEnumerable<Guid> allDescendantsKeys);
        List<Guid> allDescendantsList = allDescendantsKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, initialDescendantsOfTypeList.Count); // Verify that loaded doc types can be used to filter
            Assert.AreEqual(1, filteredDescendantsList.Count); // Verify that new doc type can be used to filter
            Assert.AreEqual(3, allDescendantsList.Count);
        });
    }

    [Test]
    public async Task Can_Filter_Ancestors_By_Type()
    {
        // Arrange
        Guid childKey = new Guid("1802D6B4-4A3C-4EBA-AFA3-1AF82C2D6483");

        // Doc Type
        var anotherContentType = ContentTypeBuilder.CreateSimpleContentType("anotherPage", "Another page");
        anotherContentType.Key = new Guid("58A2958E-B34F-4289-A225-E99EEC2456AB");
        anotherContentType.AllowedContentTypes = new[] { new ContentTypeSort(anotherContentType.Key, 0, anotherContentType.Alias) };
        await ContentTypeService.CreateAsync(anotherContentType, Constants.Security.SuperUserKey);

        // Update old doc type
        ContentType.AllowedContentTypes = new[] { new ContentTypeSort(ContentType.Key, 0, ContentType.Alias), new ContentTypeSort(anotherContentType.Key, 1, anotherContentType.Alias) };
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Content
        var greatGrandchild2Model = CreateContentCreateModel("Great-grandchild 2", new Guid("11233548-2E87-4D3E-8FC4-4400F9DBEF56"), anotherContentType.Key, Grandchild4.Key); // Using new doc type
        await ContentEditingService.CreateAsync(greatGrandchild2Model, Constants.Security.SuperUserKey);
        var greatGreatGrandchild1Model = CreateContentCreateModel("Great-great-grandchild 1", childKey, anotherContentType.Key, greatGrandchild2Model.Key); // Using new doc type
        await ContentEditingService.CreateAsync(greatGreatGrandchild1Model, Constants.Security.SuperUserKey);

        // Act
        DocumentNavigationQueryService.TryGetAncestorsKeysOfType(childKey, ContentType.Alias, out IEnumerable<Guid> ancestorsKeysOfOriginalType);
        List<Guid> ancestorsKeysOfOriginalTypeList = ancestorsKeysOfOriginalType.ToList();

        DocumentNavigationQueryService.TryGetAncestorsKeysOfType(childKey, anotherContentType.Alias, out IEnumerable<Guid> ancestorsKeysOfNewType);
        List<Guid> ancestorsKeysOfNewTypeList = ancestorsKeysOfNewType.ToList();

        DocumentNavigationQueryService.TryGetAncestorsKeys(childKey, out IEnumerable<Guid> allAncestorsKeys);
        List<Guid> allAncestorsList = allAncestorsKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, ancestorsKeysOfOriginalTypeList.Count); // Verify that loaded doc types can be used to filter
            Assert.AreEqual(1, ancestorsKeysOfNewTypeList.Count); // Verify that new doc type can be used to filter
            Assert.AreEqual(4, allAncestorsList.Count);
        });
    }

    [Test]
    public async Task Can_Filter_Siblings_By_Type()
    {
        // Arrange
        Guid parentKey = Child3.Key;
        DocumentNavigationQueryService.TryGetSiblingsKeysOfType(parentKey, ContentType.Alias, out IEnumerable<Guid> initialSiblingsKeysOfType);
        List<Guid> initialSiblingsOfTypeList = initialSiblingsKeysOfType.ToList();

        // Doc Type
        var anotherContentType = ContentTypeBuilder.CreateSimpleContentType("anotherPage", "Another page");
        anotherContentType.Key = new Guid("58A2958E-B34F-4289-A225-E99EEC2456AB");
        anotherContentType.AllowedContentTypes = new[] { new ContentTypeSort(anotherContentType.Key, 0, anotherContentType.Alias) };
        await ContentTypeService.CreateAsync(anotherContentType, Constants.Security.SuperUserKey);

        // Update old doc type
        ContentType.AllowedContentTypes = new[] { new ContentTypeSort(ContentType.Key, 0, ContentType.Alias), new ContentTypeSort(anotherContentType.Key, 1, anotherContentType.Alias) };
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // Content
        var child4Model = CreateContentCreateModel("Child 4", new Guid("11233548-2E87-4D3E-8FC4-4400F9DBEF56"), anotherContentType.Key, Root.Key); // Using new doc type
        await ContentEditingService.CreateAsync(child4Model, Constants.Security.SuperUserKey);

        // Act
        DocumentNavigationQueryService.TryGetSiblingsKeysOfType(parentKey, anotherContentType.Alias, out IEnumerable<Guid> filteredSiblingsKeysOfType);
        List<Guid> filteredSiblingsList = filteredSiblingsKeysOfType.ToList();

        DocumentNavigationQueryService.TryGetSiblingsKeys(parentKey, out IEnumerable<Guid> allSiblingsKeys);
        List<Guid> allSiblingsList = allSiblingsKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, initialSiblingsOfTypeList.Count); // Verify that loaded doc types can be used to filter
            Assert.AreEqual(1, filteredSiblingsList.Count); // Verify that new doc type can be used to filter
            Assert.AreEqual(3, allSiblingsList.Count);
        });
    }
}
