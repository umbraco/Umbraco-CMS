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
        Assert.That(beforeMoveRecycleBinSiblingsCount, Is.EqualTo(0));
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
            Assert.That(nodeExists, Is.False);
            Assert.That(nodeExistsInRecycleBin, Is.True);
            Assert.That(updatedParentKeyInRecycleBin, Is.Not.EqualTo(originalParentKey));
            Assert.That(updatedParentKeyInRecycleBin, Is.Null); // Verify the node's parent is now located at the root of the recycle bin (null)
            Assert.That(afterMoveDescendants, Is.EqualTo(beforeMoveDescendants));
            Assert.That(afterMoveParentSiblingsCount, Is.EqualTo(beforeMoveParentSiblingsCount - 1));
            Assert.That(afterMoveRecycleBinSiblingsCount, Is.EqualTo(beforeMoveRecycleBinSiblingsCount + 1));
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
            Assert.That(siblingsKeysAfterDeletionList, Has.Count.EqualTo(1));
            Assert.That(siblingsKeysAfterDeletionList[0], Is.EqualTo(SubAlbum1.Key));
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
            Assert.That(siblingsKeysAfterCreationList, Has.Count.EqualTo(2));
            Assert.That(siblingsKeysAfterDeletionList[0], Is.EqualTo(SubAlbum1.Key));
            Assert.That(siblingsKeysAfterCreationList[1], Is.EqualTo(key));
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
            Assert.That(childrenKeysAfterDeletionList, Has.Count.EqualTo(3));
            Assert.That(childrenKeysAfterDeletionList[0], Is.EqualTo(Image2.Key));
            Assert.That(childrenKeysAfterDeletionList[1], Is.EqualTo(Image3.Key));
            Assert.That(childrenKeysAfterDeletionList[2], Is.EqualTo(key));
            Assert.That(childrenKeysBeforeDeletionList.SequenceEqual(childrenKeysAfterDeletionList), Is.True);
        });
    }
}
