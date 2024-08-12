using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DocumentNavigationServiceTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    // Testing with IContentEditingService as it calls IContentService underneath
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IDocumentNavigationService DocumentNavigationService => GetRequiredService<IDocumentNavigationService>();

    private ContentType ContentType { get; set; }

    private IContent Root { get; set; }

    private IContent Child1 { get; set; }

    private IContent Grandchild1 { get; set; }

    private IContent Grandchild2 { get; set; }

    private IContent Child2 { get; set; }

    private IContent Grandchild3 { get; set; }

    private IContent GreatGrandchild1 { get; set; }

    private IContent Child3 { get; set; }

    private IContent Grandchild4 { get; set; }

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
        ContentType.AllowedTemplates = null;
        ContentType.AllowedContentTypes = new[] { new ContentTypeSort(ContentType.Key, 0, ContentType.Alias) };
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);

        // Content
        var rootModel = CreateContentCreateModel("Root", new Guid("E48DD82A-7059-418E-9B82-CDD5205796CF"));
        var rootCreateAttempt = await ContentEditingService.CreateAsync(rootModel, Constants.Security.SuperUserKey);
        Root = rootCreateAttempt.Result.Content!;

        var child1Model = CreateContentCreateModel("Child 1", new Guid("C6173927-0C59-4778-825D-D7B9F45D8DDE"), Root.Key);
        var child1CreateAttempt = await ContentEditingService.CreateAsync(child1Model, Constants.Security.SuperUserKey);
        Child1 = child1CreateAttempt.Result.Content!;

        var grandchild1Model = CreateContentCreateModel("Grandchild 1", new Guid("E856AC03-C23E-4F63-9AA9-681B42A58573"), Child1.Key);
        var grandchild1CreateAttempt = await ContentEditingService.CreateAsync(grandchild1Model, Constants.Security.SuperUserKey);
        Grandchild1 = grandchild1CreateAttempt.Result.Content!;

        var grandchild2Model = CreateContentCreateModel("Grandchild 2", new Guid("A1B1B217-B02F-4307-862C-A5E22DB729EB"), Child1.Key);
        var grandchild2CreateAttempt = await ContentEditingService.CreateAsync(grandchild2Model, Constants.Security.SuperUserKey);
        Grandchild2 = grandchild2CreateAttempt.Result.Content!;

        var child2Model = CreateContentCreateModel("Child 2", new Guid("60E0E5C4-084E-4144-A560-7393BEAD2E96"), Root.Key);
        var child2CreateAttempt = await ContentEditingService.CreateAsync(child2Model, Constants.Security.SuperUserKey);
        Child2 = child2CreateAttempt.Result.Content!;

        var grandchild3Model = CreateContentCreateModel("Grandchild 3", new Guid("D63C1621-C74A-4106-8587-817DEE5FB732"), Child2.Key);
        var grandchild3CreateAttempt = await ContentEditingService.CreateAsync(grandchild3Model, Constants.Security.SuperUserKey);
        Grandchild3 = grandchild3CreateAttempt.Result.Content!;

        var greatGrandchild1Model = CreateContentCreateModel("Great-grandchild 1", new Guid("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7"), Grandchild3.Key);
        var greatGrandchild1CreateAttempt = await ContentEditingService.CreateAsync(greatGrandchild1Model, Constants.Security.SuperUserKey);
        GreatGrandchild1 = greatGrandchild1CreateAttempt.Result.Content!;

        var child3Model = CreateContentCreateModel("Child 3", new Guid("B606E3FF-E070-4D46-8CB9-D31352029FDF"), Root.Key);
        var child3CreateAttempt = await ContentEditingService.CreateAsync(child3Model, Constants.Security.SuperUserKey);
        Child3 = child3CreateAttempt.Result.Content!;

        var grandchild4Model = CreateContentCreateModel("Grandchild 4", new Guid("F381906C-223C-4466-80F7-B63B4EE073F8"), Child3.Key);
        var grandchild4CreateAttempt = await ContentEditingService.CreateAsync(grandchild4Model, Constants.Security.SuperUserKey);
        Grandchild4 = grandchild4CreateAttempt.Result.Content!;
    }

    // protected override void CustomTestSetup(IUmbracoBuilder builder)
    // {
    //     builder.Services.AddHostedService<NavigationInitializationService>();
    // }

    [Test]
    // TODO: Test that you can rebuild bin structure as well
    public async Task Structure_Can_Rebuild()
    {
        // Arrange
        Guid nodeKey = Root.Key;

        // Capture original built state of DocumentNavigationService
        DocumentNavigationService.TryGetParentKey(nodeKey, out Guid? originalParentKey);
        DocumentNavigationService.TryGetChildrenKeys(nodeKey, out IEnumerable<Guid> originalChildrenKeys);
        DocumentNavigationService.TryGetDescendantsKeys(nodeKey, out IEnumerable<Guid> originalDescendantsKeys);
        DocumentNavigationService.TryGetAncestorsKeys(nodeKey, out IEnumerable<Guid> originalAncestorsKeys);
        DocumentNavigationService.TryGetSiblingsKeys(nodeKey, out IEnumerable<Guid> originalSiblingsKeys);

        // Im-memory navigation structure is empty here
        var newDocumentNavigationService = new DocumentNavigationService(GetRequiredService<ICoreScopeProvider>(), GetRequiredService<INavigationRepository>());
        var initialNodeExists = newDocumentNavigationService.TryGetParentKey(nodeKey, out _);

        // Act
        await newDocumentNavigationService.RebuildAsync();

        // Capture rebuilt state
        var nodeExists = newDocumentNavigationService.TryGetParentKey(nodeKey, out Guid? parentKeyFromRebuild);
        newDocumentNavigationService.TryGetChildrenKeys(nodeKey, out IEnumerable<Guid> childrenKeysFromRebuild);
        newDocumentNavigationService.TryGetDescendantsKeys(nodeKey, out IEnumerable<Guid> descendantsKeysFromRebuild);
        newDocumentNavigationService.TryGetAncestorsKeys(nodeKey, out IEnumerable<Guid> ancestorsKeysFromRebuild);
        newDocumentNavigationService.TryGetSiblingsKeys(nodeKey, out IEnumerable<Guid> siblingsKeysFromRebuild);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(initialNodeExists);

            // Verify that the item is present in the navigation structure after a rebuild
            Assert.IsTrue(nodeExists);

            // Verify that we have the same items as in the original built state of DocumentNavigationService
            Assert.AreEqual(originalParentKey, parentKeyFromRebuild);
            CollectionAssert.AreEquivalent(originalChildrenKeys, childrenKeysFromRebuild);
            CollectionAssert.AreEquivalent(originalDescendantsKeys, descendantsKeysFromRebuild);
            CollectionAssert.AreEquivalent(originalAncestorsKeys, ancestorsKeysFromRebuild);
            CollectionAssert.AreEquivalent(originalSiblingsKeys, siblingsKeysFromRebuild);
        });
    }

    [Test]
    public async Task Structure_Does_Not_Update_When_Scope_Is_Not_Completed()
    {
        // Arrange
        Guid notCreatedRootKey = new Guid("516927E5-8574-497B-B45B-E27EFAB47DE4");

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = ContentType.Key,
            ParentKey = Constants.System.RootKey, // Create node at content root
            InvariantName = "Root 2",
            Key = notCreatedRootKey,
        };

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        }

        // Act
        var nodeExists = DocumentNavigationService.TryGetParentKey(notCreatedRootKey, out _);

        // Assert
        Assert.IsFalse(nodeExists);
    }

    [Test]
    public async Task Structure_Updates_When_Creating_Content()
    {
        // Arrange
        DocumentNavigationService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> initialSiblingsKeys);
        var initialRootNodeSiblingsCount = initialSiblingsKeys.Count();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = ContentType.Key,
            ParentKey = Constants.System.RootKey, // Create node at content root
            InvariantName = "Root 2",
        };

        // Act
        var createAttempt = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Guid createdItemKey = createAttempt.Result.Content!.Key;

        // Verify that the structure has updated by checking the siblings list of the Root once again
        DocumentNavigationService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> updatedSiblingsKeys);
        List<Guid> siblingsList = updatedSiblingsKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsNotEmpty(siblingsList);
            Assert.AreEqual(initialRootNodeSiblingsCount + 1, siblingsList.Count);
            Assert.AreEqual(createdItemKey, siblingsList.First());
        });
    }

    [Test]
    public async Task Structure_Does_Not_Update_When_Updating_Content()
    {
        // Arrange
        Guid nodeToUpdate = Root.Key;

        // Capture initial state
        DocumentNavigationService.TryGetParentKey(nodeToUpdate, out Guid? initialParentKey);
        DocumentNavigationService.TryGetChildrenKeys(nodeToUpdate, out IEnumerable<Guid> initialChildrenKeys);
        DocumentNavigationService.TryGetDescendantsKeys(nodeToUpdate, out IEnumerable<Guid> initialDescendantsKeys);
        DocumentNavigationService.TryGetAncestorsKeys(nodeToUpdate, out IEnumerable<Guid> initialAncestorsKeys);
        DocumentNavigationService.TryGetSiblingsKeys(nodeToUpdate, out IEnumerable<Guid> initialSiblingsKeys);

        var updateModel = new ContentUpdateModel
        {
            InvariantName = "Updated Root",
        };

        // Act
        var updateAttempt = await ContentEditingService.UpdateAsync(nodeToUpdate, updateModel, Constants.Security.SuperUserKey);
        Guid updatedItemKey = updateAttempt.Result.Content!.Key;

        // Capture updated state
        var nodeExists = DocumentNavigationService.TryGetParentKey(updatedItemKey, out Guid? updatedParentKey);
        DocumentNavigationService.TryGetChildrenKeys(updatedItemKey, out IEnumerable<Guid> childrenKeysAfterUpdate);
        DocumentNavigationService.TryGetDescendantsKeys(updatedItemKey, out IEnumerable<Guid> descendantsKeysAfterUpdate);
        DocumentNavigationService.TryGetAncestorsKeys(updatedItemKey, out IEnumerable<Guid> ancestorsKeysAfterUpdate);
        DocumentNavigationService.TryGetSiblingsKeys(updatedItemKey, out IEnumerable<Guid> siblingsKeysAfterUpdate);

        // Assert
        Assert.Multiple(() =>
        {
            // Verify that the item is still present in the navigation structure
            Assert.IsTrue(nodeExists);

            Assert.AreEqual(nodeToUpdate, updatedItemKey);

            // Verify that nothing's changed
            Assert.AreEqual(initialParentKey, updatedParentKey);
            CollectionAssert.AreEquivalent(initialChildrenKeys, childrenKeysAfterUpdate);
            CollectionAssert.AreEquivalent(initialDescendantsKeys, descendantsKeysAfterUpdate);
            CollectionAssert.AreEquivalent(initialAncestorsKeys, ancestorsKeysAfterUpdate);
            CollectionAssert.AreEquivalent(initialSiblingsKeys, siblingsKeysAfterUpdate);
        });
    }

    // TODO: test that item exists in recycle bin str. and it is removed from content str;
    // TODO: also check that initial siblings count's decreased,
    // TODO: and that descendants are still the same (i.e. they've also been moved to recycle bin)
    [Test]
    public async Task Structure_Updates_When_Moving_Content_To_Recycle_Bin()
    {
        // Arrange
        Guid nodeToMoveToRecycleBin = Child3.Key;
        DocumentNavigationService.TryGetParentKey(nodeToMoveToRecycleBin, out Guid? originalParentKey);

        // Act
        await ContentEditingService.MoveToRecycleBinAsync(nodeToMoveToRecycleBin, Constants.Security.SuperUserKey);

        // Assert
        var nodeExists = DocumentNavigationService.TryGetParentKey(nodeToMoveToRecycleBin, out _); // Verify that the item is no longer in the document structure
        var nodeExistsInRecycleBin = DocumentNavigationService.TryGetParentKeyInBin(nodeToMoveToRecycleBin, out Guid? updatedParentKeyInRecycleBin);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(nodeExists);
            Assert.IsTrue(nodeExistsInRecycleBin);
            Assert.AreNotEqual(originalParentKey, updatedParentKeyInRecycleBin);
            Assert.IsNull(updatedParentKeyInRecycleBin); // Verify the node's parent is now located at the root of the recycle bin (null)
        });
    }

    // TODO: check that the descendants have also been removed from both structures - navigation and trash
    [Test]
    public async Task Structure_Updates_When_Deleting_From_Recycle_Bin()
    {
        // Arrange
        Guid nodeToDelete = Child1.Key;
        Guid nodeInRecycleBin = Grandchild4.Key;

        // Move nodes to recycle bin
        await ContentEditingService.MoveToRecycleBinAsync(nodeInRecycleBin, Constants.Security.SuperUserKey); // Make sure we have an item already in the recycle bin to act as a sibling
        await ContentEditingService.MoveToRecycleBinAsync(nodeToDelete, Constants.Security.SuperUserKey); // Make sure the item is in the recycle bin
        DocumentNavigationService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> initialSiblingsKeys);

        // Act
        await ContentEditingService.DeleteFromRecycleBinAsync(nodeToDelete, Constants.Security.SuperUserKey);

        // Assert
        DocumentNavigationService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> updatedSiblingsKeys);

        // Verify siblings count has decreased by one
        Assert.AreEqual(initialSiblingsKeys.Count() - 1, updatedSiblingsKeys.Count());
    }

    [Test]
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", "60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Grandchild 1 to Child 2
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", null)] // Child 3 to content root
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 2 to Child 1
    public async Task Structure_Updates_When_Moving_Content(Guid nodeToMove, Guid? targetParentKey)
    {
        // Arrange
        DocumentNavigationService.TryGetParentKey(nodeToMove, out Guid? originalParentKey);

        // Act
        var moveAttempt = await ContentEditingService.MoveAsync(nodeToMove, targetParentKey, Constants.Security.SuperUserKey);

        // Verify the node's new parent is updated
        DocumentNavigationService.TryGetParentKey(moveAttempt.Result!.Key, out Guid? updatedParentKey);

        // Assert
        Assert.Multiple(() =>
        {
            if (targetParentKey is null)
            {
                Assert.IsNull(updatedParentKey);
            }
            else
            {
                Assert.IsNotNull(updatedParentKey);
            }

            Assert.AreNotEqual(originalParentKey, updatedParentKey);
            Assert.AreEqual(targetParentKey, updatedParentKey);
        });
    }

    // TODO: test that it is added to its new parent - check parent's children
    // TODO: test that it has the same amount of descendants - depending on value of includeDescendants param
    // TODO: test that the number of target parent descendants updates when copying node with descendants
    // TODO: test that copied node descendants have different keys than source node descendants
    [Test]
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", "A1B1B217-B02F-4307-862C-A5E22DB729EB")] // Grandchild 2 to itself
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", null)] // Child 2 to content root
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 3 to Child 1
    public async Task Structure_Updates_When_Copying_Content(Guid nodeToCopy, Guid? targetParentKey)
    {
        // Arrange
        DocumentNavigationService.TryGetParentKey(nodeToCopy, out Guid? sourceParentKey);

        // Act
        var copyAttempt = await ContentEditingService.CopyAsync(nodeToCopy, targetParentKey, false, false, Constants.Security.SuperUserKey);
        Guid copiedItemKey = copyAttempt.Result.Key;

        // Assert
        Assert.AreNotEqual(nodeToCopy, copiedItemKey);

        DocumentNavigationService.TryGetParentKey(copiedItemKey, out Guid? copiedItemParentKey);

        Assert.Multiple(() =>
        {
            if (targetParentKey is null)
            {
                // Verify the copied node's parent is null (it's been copied to content root)
                Assert.IsNull(copiedItemParentKey);
            }
            else
            {
                Assert.IsNotNull(copiedItemParentKey);
            }

            Assert.AreEqual(targetParentKey, copiedItemParentKey);
            Assert.AreNotEqual(sourceParentKey, copiedItemParentKey);
        });
    }

    [Test]
    // TODO: Test that the descendants of the node have also been removed from both structures
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Child 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    public async Task Structure_Updates_When_Deleting_Content(Guid nodeToDelete)
    {
        // Arrange
        DocumentNavigationService.TryGetDescendantsKeys(nodeToDelete, out IEnumerable<Guid> initialDescendantsKeys);

        // Act
        // Deletes the item whether it is in the recycle bin or not
        var deleteAttempt = await ContentEditingService.DeleteAsync(nodeToDelete, Constants.Security.SuperUserKey);
        Guid deletedItemKey = deleteAttempt.Result.Key;

        // Assert
        var nodeExists = DocumentNavigationService.TryGetDescendantsKeys(deletedItemKey, out _);
        var nodeExistsInRecycleBin = DocumentNavigationService.TryGetDescendantsKeysInBin(nodeToDelete, out _);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(nodeToDelete, deletedItemKey);
            Assert.IsFalse(nodeExists);
            Assert.IsFalse(nodeExistsInRecycleBin);

            foreach (Guid descendant in initialDescendantsKeys)
            {
                var descendantExists = DocumentNavigationService.TryGetParentKey(descendant, out _);
                Assert.IsFalse(descendantExists);

                var descendantExistsInRecycleBin = DocumentNavigationService.TryGetParentKeyInBin(descendant, out _);
                Assert.IsFalse(descendantExistsInRecycleBin);
            }
        });
    }

    [Test]
    // TODO: test that descendants are also restored in the right place
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Grandchild 1 to Child 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", "D63C1621-C74A-4106-8587-817DEE5FB732")] // Grandchild 2 to Grandchild 3
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", null)] // Child 3 to content root
    public async Task Structure_Updates_When_Restoring_Content(Guid nodeToRestore, Guid? targetParentKey)
    {
        // Arrange
        Guid nodeInRecycleBin = GreatGrandchild1.Key;

        // Move nodes to recycle bin
        await ContentEditingService.MoveToRecycleBinAsync(nodeInRecycleBin, Constants.Security.SuperUserKey); // Make sure we have an item already in the recycle bin to act as a sibling
        await ContentEditingService.MoveToRecycleBinAsync(nodeToRestore, Constants.Security.SuperUserKey); // Make sure the item is in the recycle bin
        DocumentNavigationService.TryGetParentKeyInBin(nodeToRestore, out Guid? initialParentKey);
        DocumentNavigationService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> initialSiblingsKeys);

        // Act
        var restoreAttempt = await ContentEditingService.RestoreAsync(nodeToRestore, targetParentKey, Constants.Security.SuperUserKey);
        Guid restoredItemKey = restoreAttempt.Result.Key;

        // Assert
        DocumentNavigationService.TryGetParentKey(restoredItemKey, out Guid? restoredItemParentKey);
        DocumentNavigationService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> updatedSiblingsKeys);

        Assert.Multiple(() =>
        {
            // Verify siblings count has decreased by one
            Assert.AreEqual(initialSiblingsKeys.Count() - 1, updatedSiblingsKeys.Count());

            if (targetParentKey is null)
            {
                Assert.IsNull(restoredItemParentKey);
            }
            else
            {
                Assert.IsNotNull(restoredItemParentKey);
                Assert.AreNotEqual(initialParentKey, restoredItemParentKey);
            }

            Assert.AreEqual(targetParentKey, restoredItemParentKey);
        });
    }

    private ContentCreateModel CreateContentCreateModel(string name, Guid key, Guid? parentKey = null)
        => new()
        {
            ContentTypeKey = ContentType.Key,
            ParentKey = parentKey ?? Constants.System.RootKey,
            InvariantName = name,
            Key = key,
        };
}
