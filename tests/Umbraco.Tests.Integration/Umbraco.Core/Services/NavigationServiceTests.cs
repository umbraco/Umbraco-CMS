using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class NavigationServiceTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private INavigationService NavigationService => GetRequiredService<INavigationService>();

    private ContentType ContentType { get; set; }

    private Content Root { get; set; }

    private Content Child1 { get; set; }

    private Content Grandchild1 { get; set; }

    private Content Grandchild2 { get; set; }

    private Content Child2 { get; set; }

    private Content Grandchild3 { get; set; }

    private Content GreatGrandchild1 { get; set; }

    private Content Child3 { get; set; }

    private Content Grandchild4 { get; set; }

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

        // TODO: DELETE
        // Root - Guid.Parse("E48DD82A-7059-418E-9B82-CDD5205796CF"); // Root's key
        //    - Child 1 - Guid.Parse("C6173927-0C59-4778-825D-D7B9F45D8DDE"); // Child 1's key
        //      - Grandchild 1 - Guid.Parse("E856AC03-C23E-4F63-9AA9-681B42A58573"); // Grandchild 1's key
        //      - Grandchild 2 - Guid.Parse("A1B1B217-B02F-4307-862C-A5E22DB729EB"); // Grandchild 2's key
        //    - Child 2 - Guid.Parse("60E0E5C4-084E-4144-A560-7393BEAD2E96"); // Child 2's key
        //      - Grandchild 3 - Guid.Parse("D63C1621-C74A-4106-8587-817DEE5FB732"); // Grandchild 3's key
        //        - Great-grandchild 1 - Guid.Parse("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7"); // Great-grandchild 1's key
        //    - Child 3 - Guid.Parse("B606E3FF-E070-4D46-8CB9-D31352029FDF"); // Child 3's key
        //      - Grandchild 4 - Guid.Parse("F381906C-223C-4466-80F7-B63B4EE073F8"); // Grandchild 4's key

        // Doc Type
        ContentType = ContentTypeBuilder.CreateSimpleContentType("page", "Page");
        ContentType.Key = new Guid("DD72B8A6-2CE3-47F0-887E-B695A1A5D086");
        ContentType.AllowedAsRoot = true;
        ContentType.AllowedTemplates = null;
        ContentType.AllowedContentTypes = new[] { new ContentTypeSort(ContentType.Key, 0, ContentType.Alias) };
        await ContentTypeService.CreateAsync(ContentType, Constants.Security.SuperUserKey);

        // Content
        Root = ContentBuilder.CreateSimpleContent(ContentType, "Root");
        Root.Key = new Guid("E48DD82A-7059-418E-9B82-CDD5205796CF");
        ContentService.Save(Root, Constants.Security.SuperUserId);

        Child1 = ContentBuilder.CreateSimpleContent(ContentType, "Child 1", Root.Id);
        Child1.Key = new Guid("C6173927-0C59-4778-825D-D7B9F45D8DDE");
        ContentService.Save(Child1, Constants.Security.SuperUserId);

        Grandchild1 = ContentBuilder.CreateSimpleContent(ContentType, "Grandchild 1", Child1.Id);
        Grandchild1.Key = new Guid("E856AC03-C23E-4F63-9AA9-681B42A58573");
        ContentService.Save(Grandchild1, Constants.Security.SuperUserId);

        Grandchild2 = ContentBuilder.CreateSimpleContent(ContentType, "Grandchild 2", Child1.Id);
        Grandchild2.Key = new Guid("A1B1B217-B02F-4307-862C-A5E22DB729EB");
        ContentService.Save(Grandchild2, Constants.Security.SuperUserId);

        Child2 = ContentBuilder.CreateSimpleContent(ContentType, "Child 2", Root.Id);
        Child2.Key = new Guid("60E0E5C4-084E-4144-A560-7393BEAD2E96");
        ContentService.Save(Child2, Constants.Security.SuperUserId);

        Grandchild3 = ContentBuilder.CreateSimpleContent(ContentType, "Grandchild 3", Child2.Id);
        Grandchild3.Key = new Guid("D63C1621-C74A-4106-8587-817DEE5FB732");
        ContentService.Save(Grandchild3, Constants.Security.SuperUserId);

        GreatGrandchild1 = ContentBuilder.CreateSimpleContent(ContentType, "Great-grandchild 1", Grandchild3.Id);
        GreatGrandchild1.Key = new Guid("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7");
        ContentService.Save(GreatGrandchild1, Constants.Security.SuperUserId);

        Child3 = ContentBuilder.CreateSimpleContent(ContentType, "Child 3", Root.Id);
        Child3.Key = new Guid("B606E3FF-E070-4D46-8CB9-D31352029FDF");
        ContentService.Save(Child3, Constants.Security.SuperUserId);

        Grandchild4 = ContentBuilder.CreateSimpleContent(ContentType, "Grandchild 4", Child3.Id);
        Grandchild4.Key = new Guid("F381906C-223C-4466-80F7-B63B4EE073F8");
        ContentService.Save(Grandchild4, Constants.Security.SuperUserId);
    }

    // protected override void CustomTestSetup(IUmbracoBuilder builder)
    // {
    //     builder.Services.AddHostedService<NavigationInitializationService>();
    // }

    [Test]
    public void Structure_Does_Not_Update_When_Scope_Is_Not_Completed()
    {
        // Arrange
        Content notCreatedRoot = ContentBuilder.CreateSimpleContent(ContentType, "Root 2");
        notCreatedRoot.Key = new Guid("516927E5-8574-497B-B45B-E27EFAB47DE4");

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            ContentService.Save(notCreatedRoot, Constants.Security.SuperUserId); // TODO: change to Content editing service
        }

        // Act
        var result = NavigationService.TryGetSiblingsKeys(notCreatedRoot.Key, out IEnumerable<Guid> siblingsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(siblingsKeys);
        });
    }

    [Test]
    public async Task Structure_Updates_When_Creating_Content()
    {
        // Arrange
        NavigationService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> initialSiblingsKeys);
        var initialRootNodeSiblingsCount = initialSiblingsKeys.Count();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = ContentType.Key,
            ParentKey = Constants.System.RootKey, // Create node at content root
            InvariantName = "Test",
        };

        // Act
        var createAttempt = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Guid createdItemKey = createAttempt.Result.Content!.Key;

        // Verify that the structure has updated by checking the siblings list of the Root once again
        NavigationService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> updatedSiblingsKeys);
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
        NavigationService.TryGetParentKey(nodeToUpdate, out Guid? initialParentKey);
        NavigationService.TryGetChildrenKeys(nodeToUpdate, out IEnumerable<Guid> initialChildrenKeys);
        NavigationService.TryGetDescendantsKeys(nodeToUpdate, out IEnumerable<Guid> initialDescendantsKeys);
        NavigationService.TryGetAncestorsKeys(nodeToUpdate, out IEnumerable<Guid> initialAncestorsKeys);
        NavigationService.TryGetSiblingsKeys(nodeToUpdate, out IEnumerable<Guid> initialSiblingsKeys);

        var updateModel = new ContentUpdateModel
        {
            InvariantName = "Updated Root",
        };

        // Act
        var updateAttempt = await ContentEditingService.UpdateAsync(nodeToUpdate, updateModel, Constants.Security.SuperUserKey);
        Guid updatedItemKey = updateAttempt.Result.Content!.Key;

        // Capture updated state
        var parentExists = NavigationService.TryGetParentKey(updatedItemKey, out Guid? updatedParentKey);
        NavigationService.TryGetChildrenKeys(updatedItemKey, out IEnumerable<Guid> childrenKeysAfterUpdate);
        NavigationService.TryGetDescendantsKeys(updatedItemKey, out IEnumerable<Guid> descendantsKeysAfterUpdate);
        NavigationService.TryGetAncestorsKeys(updatedItemKey, out IEnumerable<Guid> ancestorsKeysAfterUpdate);
        NavigationService.TryGetSiblingsKeys(updatedItemKey, out IEnumerable<Guid> siblingsKeysAfterUpdate);

        // Assert
        Assert.Multiple(() =>
        {
            // Verify that the item is still present in the navigation structure
            Assert.IsTrue(parentExists);

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
    // [Test]
    // public async Task Structure_Updates_When_Moving_Content_To_Recycle_Bin() // TODO: Missing implementation
    // {
    //     // Arrange
    //     Guid nodeToMoveToRecycleBin = Child3.Key;
    //     NavigationService.TryGetParentKey(nodeToMoveToRecycleBin, out Guid? originalParentKey);
    //
    //     // Act
    //     await ContentEditingService.MoveToRecycleBinAsync(nodeToMoveToRecycleBin, Constants.Security.SuperUserKey);
    //     var nodeExists = NavigationService.TryGetParentKey(nodeToMoveToRecycleBin, out Guid? updatedParentKey); // Verify that the item is no longer in the content structure
    //     var nodeExistsInRecycleBin = NavigationService.TryGetParentKey(nodeToMoveToRecycleBin, out Guid? updatedParentKey); // TODO: use recycle bin str.
    //
    //     // Assert
    //     Assert.Multiple(() =>
    //     {
    //         Assert.IsFalse(nodeExists);
    //         Assert.IsTrue(nodeExistsInRecycleBin);
    //         Assert.AreNotEqual(originalParentKey, updatedParentKey);
    //         Assert.IsNull(updatedParentKey); // Verify the node's parent is now located at the root of the recycle bin (null)
    //     });
    // }

    // TODO: check that the descendants have also been removed from both structures - navigation and trash
    // [Test]
    // public async Task Structure_Updates_When_Deleting_From_Recycle_Bin() // TODO: Missing implementation
    // {
    //     // Arrange
    //     Guid nodeToDelete = Child1.Key;
    //     Guid nodeInRecycleBin = Grandchild4.Key;
    //
    //     // Move nodes to recycle bin
    //     await ContentEditingService.MoveToRecycleBinAsync(nodeInRecycleBin, Constants.Security.SuperUserKey); // Make sure we have an item already in the recycle bin to act as a sibling
    //     await ContentEditingService.MoveToRecycleBinAsync(nodeToDelete, Constants.Security.SuperUserKey); // Make sure the item is in the recycle bin
    //     NavigationService.TryGetSiblingsKeys(nodeInRecycleBin, out IEnumerable<Guid> initialSiblingsKeys); // TODO: call trashed items str.
    //
    //     // Act
    //     await ContentEditingService.DeleteFromRecycleBinAsync(nodeToDelete, Constants.Security.SuperUserKey);
    //     NavigationService.TryGetSiblingsKeys(nodeInRecycleBin, out IEnumerable<Guid> updatedSiblingsKeys); // TODO: call trashed items str.
    //
    //     // Assert
    //     // Verify siblings count has decreased by one
    //     Assert.AreEqual(initialSiblingsKeys.Count() - 1, updatedSiblingsKeys.Count());
    // }

    [Test]
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", "60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Grandchild 1 to Child 2
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", null)] // Child 3 to content root
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 2 to Child 1
    public async Task Structure_Updates_When_Moving_Content(Guid nodeToMove, Guid? targetParentKey)
    {
        // Arrange
        NavigationService.TryGetParentKey(nodeToMove, out Guid? originalParentKey);

        // Act
        var moveAttempt = await ContentEditingService.MoveAsync(nodeToMove, targetParentKey, Constants.Security.SuperUserKey);

        // Verify the node's new parent is updated
        NavigationService.TryGetParentKey(moveAttempt.Result!.Key, out Guid? updatedParentKey);

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
        NavigationService.TryGetParentKey(nodeToCopy, out Guid? sourceParentKey);

        // Act
        var copyAttempt = await ContentEditingService.CopyAsync(nodeToCopy, targetParentKey, false, false, Constants.Security.SuperUserKey);
        Guid copiedItemKey = copyAttempt.Result.Key;

        // Assert
        Assert.AreNotEqual(nodeToCopy, copiedItemKey);

        NavigationService.TryGetParentKey(copiedItemKey, out Guid? copiedItemParentKey);

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
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Child 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    public async Task Structure_Updates_When_Deleting_Content(Guid nodeToDelete)
    {
        // Arrange
        NavigationService.TryGetDescendantsKeys(nodeToDelete, out IEnumerable<Guid> initialDescendantsKeys);

        // Act
        var deleteAttempt = await ContentEditingService.DeleteAsync(nodeToDelete, Constants.Security.SuperUserKey);
        Guid deletedItemKey = deleteAttempt.Result.Key;

        // Assert
        var nodeExists = NavigationService.TryGetDescendantsKeys(deletedItemKey, out _);
        //var nodeExistsInRecycleBin = NavigationService.TryGetDescendantsKeys(nodeToDelete, out _); // TODO: use recycle bin str.

        Assert.Multiple(() =>
        {
            Assert.AreEqual(nodeToDelete, deletedItemKey);
            Assert.IsFalse(nodeExists);
            //Assert.IsFalse(nodeExistsInRecycleBin);

            foreach (Guid descendant in initialDescendantsKeys)
            {
                var descendantExists = NavigationService.TryGetParentKey(descendant, out _);
                Assert.IsFalse(descendantExists);

                // var descendantExistsInRecycleBin = NavigationService.TryGetParentKey(descendant, out _); // TODO: use recycle bin str.
                // Assert.IsFalse(descendantExistsInRecycleBin);
            }
        });
    }

    // [Test]
    // public async Task Structure_Updates_When_Restoring_Content(Guid nodeToRestore, Guid? targetParentKey) // todo: test with target parent null
    // {
    //     // Arrange
    //     Guid nodeInRecycleBin = GreatGrandchild1.Key;
    //
    //     // Move nodes to recycle bin
    //     await ContentEditingService.MoveToRecycleBinAsync(nodeInRecycleBin, Constants.Security.SuperUserKey); // Make sure we have an item already in the recycle bin to act as a sibling
    //     await ContentEditingService.MoveToRecycleBinAsync(nodeToRestore, Constants.Security.SuperUserKey); // Make sure the item is in the recycle bin
    //     NavigationService.TryGetParentKey(nodeToRestore, out Guid? initialParentKey);
    //     NavigationService.TryGetSiblingsKeys(nodeInRecycleBin, out IEnumerable<Guid> initialSiblingsKeys); // TODO: call trashed items str.
    //
    //     // Act
    //     var restoreAttempt = await ContentEditingService.RestoreAsync(nodeToRestore, targetParentKey, Constants.Security.SuperUserKey);
    //     Guid restoredItemKey = restoreAttempt.Result.Key;
    //
    //     // Assert
    //     NavigationService.TryGetParentKey(restoredItemKey, out Guid? restoredItemParentKey);
    //     NavigationService.TryGetSiblingsKeys(nodeInRecycleBin, out IEnumerable<Guid> updatedSiblingsKeys); // TODO: call trashed items str.
    //
    //     Assert.Multiple(() =>
    //     {
    //         // Verify siblings count has decreased by one
    //         Assert.AreEqual(initialSiblingsKeys.Count() - 1, updatedSiblingsKeys.Count());
    //
    //         if (targetParentKey is null)
    //         {
    //             Assert.IsNull(restoredItemParentKey);
    //         }
    //         else
    //         {
    //             Assert.IsNotNull(restoredItemParentKey);
    //         }
    //
    //         Assert.AreNotEqual(initialParentKey, restoredItemParentKey);
    //         Assert.AreEqual(targetParentKey, restoredItemParentKey);
    //     });
    // }
}
