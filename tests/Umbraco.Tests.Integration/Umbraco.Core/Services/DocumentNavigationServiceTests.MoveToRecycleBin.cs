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
        Assert.That(beforeMoveRecycleBinSiblingsCount, Is.EqualTo(0));
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
            Assert.That(nodeExists, Is.False);
            Assert.That(nodeExistsInRecycleBin, Is.True);
            Assert.That(updatedParentKeyInRecycleBin, Is.Not.EqualTo(originalParentKey));

            Assert.That(updatedParentKeyInRecycleBin, Is.Null); // Verify the node's parent is now located at the root of the recycle bin (null)
            Assert.That(afterMoveDescendants, Is.EqualTo(beforeMoveDescendants));
            Assert.That(afterMoveParentChildrenCount, Is.EqualTo(beforeMoveParentChildrenCount - 1));
            Assert.That(afterMoveRecycleBinSiblingsCount, Is.EqualTo(beforeMoveRecycleBinSiblingsCount + 1));
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
            Assert.That(siblingsKeysAfterDeletionList, Has.Count.EqualTo(1));
            Assert.That(siblingsKeysAfterDeletionList[0], Is.EqualTo(Child2.Key));
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
            Assert.That(siblingsKeysAfterCreationList, Has.Count.EqualTo(2));
            Assert.That(siblingsKeysAfterCreationList[0], Is.EqualTo(Child2.Key));
            Assert.That(siblingsKeysAfterCreationList[1], Is.EqualTo(key));
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
            Assert.That(childrenKeysAfterDeletionList, Has.Count.EqualTo(3));
            Assert.That(childrenKeysAfterDeletionList[0], Is.EqualTo(Grandchild1.Key));
            Assert.That(childrenKeysAfterDeletionList[1], Is.EqualTo(Grandchild2.Key));
            Assert.That(childrenKeysAfterDeletionList[2], Is.EqualTo(key));
            Assert.That(childrenKeysBeforeDeletionList.SequenceEqual(childrenKeysAfterDeletionList), Is.True);
        });
    }
}
