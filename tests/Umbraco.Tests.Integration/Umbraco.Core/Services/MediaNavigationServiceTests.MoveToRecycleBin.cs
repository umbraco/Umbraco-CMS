using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class MediaNavigationServiceTests
{
    [Test]
    public async Task Structure_Updates_When_Moving_Media_To_Recycle_Bin()
    {
        // Arrange
        Guid nodeToMoveToRecycleBin = Image3.Key;
        Guid nodeInRecycleBin = SubSubAlbum1.Key;
        await MediaEditingService.MoveToRecycleBinAsync(nodeInRecycleBin, Constants.Security.SuperUserKey);
        MediaNavigationQueryService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> initialSiblingsKeys);
        var beforeMoveRecycleBinSiblingsCount = initialSiblingsKeys.Count();
        Assert.AreEqual(beforeMoveRecycleBinSiblingsCount, 0);
        MediaNavigationQueryService.TryGetParentKey(nodeToMoveToRecycleBin, out Guid? originalParentKey);
        MediaNavigationQueryService.TryGetDescendantsKeys(nodeToMoveToRecycleBin, out IEnumerable<Guid> initialDescendantsKeys);
        var beforeMoveDescendants = initialDescendantsKeys.ToList();
        MediaNavigationQueryService.TryGetChildrenKeys(originalParentKey.Value, out IEnumerable<Guid> initialParentChildrenKeys);
        var beforeMoveParentSiblingsCount = initialParentChildrenKeys.Count();

        // Act
        await MediaEditingService.MoveToRecycleBinAsync(nodeToMoveToRecycleBin, Constants.Security.SuperUserKey);

        // Assert
        var nodeExists = MediaNavigationQueryService.TryGetParentKey(nodeToMoveToRecycleBin, out _); // Verify that the item is no longer in the document structure
        var nodeExistsInRecycleBin = MediaNavigationQueryService.TryGetParentKeyInBin(nodeToMoveToRecycleBin, out Guid? updatedParentKeyInRecycleBin);
        MediaNavigationQueryService.TryGetDescendantsKeysInBin(nodeToMoveToRecycleBin, out IEnumerable<Guid> afterMoveDescendantsKeys);
        var afterMoveDescendants = afterMoveDescendantsKeys.ToList();
        MediaNavigationQueryService.TryGetChildrenKeys(originalParentKey.Value, out IEnumerable<Guid> afterMoveParentChildrenKeys);
        var afterMoveParentSiblingsCount = afterMoveParentChildrenKeys.Count();
        MediaNavigationQueryService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> afterMoveRecycleBinSiblingsKeys);
        var afterMoveRecycleBinSiblingsCount = afterMoveRecycleBinSiblingsKeys.Count();

        Assert.Multiple(() =>
        {
            Assert.IsFalse(nodeExists);
            Assert.IsTrue(nodeExistsInRecycleBin);
            Assert.AreNotEqual(originalParentKey, updatedParentKeyInRecycleBin);
            Assert.IsNull(updatedParentKeyInRecycleBin); // Verify the node's parent is now located at the root of the recycle bin (null)
            Assert.AreEqual(beforeMoveDescendants, afterMoveDescendants);
            Assert.AreEqual(beforeMoveParentSiblingsCount - 1, afterMoveParentSiblingsCount);
            Assert.AreEqual(beforeMoveRecycleBinSiblingsCount + 1, afterMoveRecycleBinSiblingsCount);
        });
    }

    // TODO: Add more test cases
    [Test]
    public async Task Sort_Order_Of_Siblings_Updates_When_Moving_Media_To_Recycle_Bin_And_Adding_New_One()
    {
        // Arrange
        Guid nodeToMoveToRecycleBin = SubAlbum2.Key;
        Guid node = Image1.Key;

        // Act
        await MediaEditingService.MoveToRecycleBinAsync(nodeToMoveToRecycleBin, Constants.Security.SuperUserKey);

        // Assert
        MediaNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> siblingsKeysAfterDeletion);
        var siblingsKeysAfterDeletionList = siblingsKeysAfterDeletion.ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, siblingsKeysAfterDeletionList.Count);
            Assert.AreEqual(SubAlbum1.Key, siblingsKeysAfterDeletionList[0]);
        });

        // Create a new sibling under the same parent
        var key = Guid.NewGuid();
        var createModel = CreateMediaCreateModel("Child Image", key, ImageMediaType.Key, Album.Key);
        await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        MediaNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> siblingsKeysAfterCreation);
        var siblingsKeysAfterCreationList = siblingsKeysAfterCreation.ToList();

        // Verify sibling order after creating the new media
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, siblingsKeysAfterCreationList.Count);
            Assert.AreEqual(SubAlbum1.Key, siblingsKeysAfterDeletionList[0]);
            Assert.AreEqual(key, siblingsKeysAfterCreationList[1]);
        });
    }

    [Test]
    public async Task Sort_Order_Of_Chilldren_Is_Maintained_When_Moving_Media_To_Recycle_Bin()
    {
        // Arrange
        Guid nodeToMoveToRecycleBin = SubAlbum1.Key;

        // Create a new grandchild under Child1
        var key = Guid.NewGuid();
        var createModel = CreateMediaCreateModel("Image 5", key, ImageMediaType.Key, nodeToMoveToRecycleBin);
        await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        MediaNavigationQueryService.TryGetChildrenKeys(nodeToMoveToRecycleBin, out IEnumerable<Guid> childrenKeysBeforeDeletion);
        var childrenKeysBeforeDeletionList = childrenKeysBeforeDeletion.ToList();

        // Act
        await MediaEditingService.MoveToRecycleBinAsync(nodeToMoveToRecycleBin, Constants.Security.SuperUserKey);

        // Assert
        MediaNavigationQueryService.TryGetChildrenKeysInBin(nodeToMoveToRecycleBin, out IEnumerable<Guid> childrenKeysAfterDeletion);
        var childrenKeysAfterDeletionList = childrenKeysAfterDeletion.ToList();

        // Verify children order in the bin
        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, childrenKeysAfterDeletionList.Count);
            Assert.AreEqual(Image2.Key, childrenKeysAfterDeletionList[0]);
            Assert.AreEqual(Image3.Key, childrenKeysAfterDeletionList[1]);
            Assert.AreEqual(key, childrenKeysAfterDeletionList[2]);
            Assert.IsTrue(childrenKeysBeforeDeletionList.SequenceEqual(childrenKeysAfterDeletionList));
        });
    }
}
