using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class DocumentNavigationServiceTests
{
    [Test]
    public async Task Parent_And_Descendants_Are_Updated_When_Content_Is_Moved_To_Recycle_Bin()
    {
        // Arrange
        Guid nodeToMoveToRecycleBin = Child3.Key;
        Guid nodeInRecycleBin = Grandchild4.Key;
        await ContentEditingService.MoveToRecycleBinAsync(nodeInRecycleBin, Constants.Security.SuperUserKey);
        DocumentNavigationQueryService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> initialSiblingsKeys);
        var beforeMoveRecycleBinSiblingsCount = initialSiblingsKeys.Count();
        Assert.AreEqual(beforeMoveRecycleBinSiblingsCount, 0);
        DocumentNavigationQueryService.TryGetParentKey(nodeToMoveToRecycleBin, out Guid? originalParentKey);
        DocumentNavigationQueryService.TryGetDescendantsKeys(nodeToMoveToRecycleBin, out IEnumerable<Guid> initialDescendantsKeys);
        var beforeMoveDescendants = initialDescendantsKeys.ToList();
        DocumentNavigationQueryService.TryGetChildrenKeys(originalParentKey.Value, out IEnumerable<Guid> initialParentChildrenKeys);
        var beforeMoveParentChildrenCount = initialParentChildrenKeys.Count();

        // Act
        await ContentEditingService.MoveToRecycleBinAsync(nodeToMoveToRecycleBin, Constants.Security.SuperUserKey);

        // Assert
        var nodeExists = DocumentNavigationQueryService.TryGetParentKey(nodeToMoveToRecycleBin, out _); // Verify that the item is no longer in the document structure
        var nodeExistsInRecycleBin = DocumentNavigationQueryService.TryGetParentKeyInBin(nodeToMoveToRecycleBin, out Guid? updatedParentKeyInRecycleBin);
        DocumentNavigationQueryService.TryGetDescendantsKeysInBin(nodeToMoveToRecycleBin, out IEnumerable<Guid> afterMoveDescendantsKeys);
        var afterMoveDescendants = afterMoveDescendantsKeys.ToList();
        DocumentNavigationQueryService.TryGetChildrenKeys((Guid)originalParentKey, out IEnumerable<Guid> afterMoveParentChildrenKeys);
        var afterMoveParentChildrenCount = afterMoveParentChildrenKeys.Count();
        DocumentNavigationQueryService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> afterMoveRecycleBinSiblingsKeys);
        var afterMoveRecycleBinSiblingsCount = afterMoveRecycleBinSiblingsKeys.Count();

        Assert.Multiple(() =>
        {
            Assert.IsFalse(nodeExists);
            Assert.IsTrue(nodeExistsInRecycleBin);
            Assert.AreNotEqual(originalParentKey, updatedParentKeyInRecycleBin);

            Assert.IsNull(updatedParentKeyInRecycleBin); // Verify the node's parent is now located at the root of the recycle bin (null)
            Assert.AreEqual(beforeMoveDescendants, afterMoveDescendants);
            Assert.AreEqual(beforeMoveParentChildrenCount - 1, afterMoveParentChildrenCount);
            Assert.AreEqual(beforeMoveRecycleBinSiblingsCount + 1, afterMoveRecycleBinSiblingsCount);
        });
    }

    // TODO: Add more test cases
    [Test]
    public async Task Sort_Order_Of_Siblings_Updates_When_Moving_Content_To_Recycle_Bin_And_Adding_New_One()
    {
        // Arrange
        Guid nodeToMoveToRecycleBin = Child3.Key;
        Guid node = Child1.Key;

        // Act
        await ContentEditingService.MoveToRecycleBinAsync(nodeToMoveToRecycleBin, Constants.Security.SuperUserKey);

        // Assert
        DocumentNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> siblingsKeysAfterDeletion);
        var siblingsKeysAfterDeletionList = siblingsKeysAfterDeletion.ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, siblingsKeysAfterDeletionList.Count);
            Assert.AreEqual(Child2.Key, siblingsKeysAfterDeletionList[0]);
        });

        // Create a new sibling under the same parent
        var key = Guid.NewGuid();
        var createModel = CreateContentCreateModel("Child 4", key, parentKey: Root.Key);
        await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        DocumentNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> siblingsKeysAfterCreation);
        var siblingsKeysAfterCreationList = siblingsKeysAfterCreation.ToList();

        // Verify sibling order after creating the new content
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, siblingsKeysAfterCreationList.Count);
            Assert.AreEqual(Child2.Key, siblingsKeysAfterCreationList[0]);
            Assert.AreEqual(key, siblingsKeysAfterCreationList[1]);
        });
    }

    [Test]
    public async Task Sort_Order_Of_Chilldren_Is_Maintained_When_Moving_Content_To_Recycle_Bin()
    {
        // Arrange
        Guid nodeToMoveToRecycleBin = Child1.Key;

        // Create a new grandchild under Child1
        var key = Guid.NewGuid();
        var createModel = CreateContentCreateModel("Grandchild 3", key, parentKey: nodeToMoveToRecycleBin);
        await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        DocumentNavigationQueryService.TryGetChildrenKeys(nodeToMoveToRecycleBin, out IEnumerable<Guid> childrenKeysBeforeDeletion);
        var childrenKeysBeforeDeletionList = childrenKeysBeforeDeletion.ToList();

        // Act
        await ContentEditingService.MoveToRecycleBinAsync(nodeToMoveToRecycleBin, Constants.Security.SuperUserKey);

        // Assert
        DocumentNavigationQueryService.TryGetChildrenKeysInBin(nodeToMoveToRecycleBin, out IEnumerable<Guid> childrenKeysAfterDeletion);
        var childrenKeysAfterDeletionList = childrenKeysAfterDeletion.ToList();

        // Verify children order in the bin
        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, childrenKeysAfterDeletionList.Count);
            Assert.AreEqual(Grandchild1.Key, childrenKeysAfterDeletionList[0]);
            Assert.AreEqual(Grandchild2.Key, childrenKeysAfterDeletionList[1]);
            Assert.AreEqual(key, childrenKeysAfterDeletionList[2]);
            Assert.IsTrue(childrenKeysBeforeDeletionList.SequenceEqual(childrenKeysAfterDeletionList));
        });
    }
}
