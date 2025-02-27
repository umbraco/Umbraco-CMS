using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class MediaNavigationServiceTests
{
    [Test]
    [TestCase("1CD97C02-8534-4B72-AE9E-AE52EC94CF31")] // Album
    [TestCase("DBCAFF2F-BFA4-4744-A948-C290C432D564")] // Sub-album 2
    [TestCase("3E489C32-9315-42DA-95CE-823D154B09C8")] // Image 2
    public async Task Structure_Updates_When_Deleting_Media(Guid nodeToDelete)
    {
        // Arrange
        MediaNavigationQueryService.TryGetDescendantsKeys(nodeToDelete, out IEnumerable<Guid> initialDescendantsKeys);

        // Act
        // Deletes the item whether it is in the recycle bin or not
        var deleteAttempt = await MediaEditingService.DeleteAsync(nodeToDelete, Constants.Security.SuperUserKey);
        Guid deletedItemKey = deleteAttempt.Result.Key;

        // Assert
        var nodeExists = MediaNavigationQueryService.TryGetDescendantsKeys(deletedItemKey, out _);
        var nodeExistsInRecycleBin = MediaNavigationQueryService.TryGetDescendantsKeysInBin(nodeToDelete, out _);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(nodeToDelete, deletedItemKey);
            Assert.IsFalse(nodeExists);
            Assert.IsFalse(nodeExistsInRecycleBin);
            foreach (Guid descendant in initialDescendantsKeys)
            {
                var descendantExists = MediaNavigationQueryService.TryGetParentKey(descendant, out _);
                Assert.IsFalse(descendantExists);
                var descendantExistsInRecycleBin = MediaNavigationQueryService.TryGetParentKeyInBin(descendant, out _);
                Assert.IsFalse(descendantExistsInRecycleBin);
            }
        });
    }

    // TODO: Add more test cases
    [Test]
    public async Task Sort_Order_Of_Siblings_Updates_When_Deleting_Media_And_Adding_New_One()
    {
        // Arrange
        Guid nodeToDelete = SubAlbum2.Key;
        Guid node = Image1.Key;

        // Act
        await MediaEditingService.DeleteAsync(nodeToDelete, Constants.Security.SuperUserKey);

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
            Assert.AreEqual(SubAlbum1.Key, siblingsKeysAfterCreationList[0]);
            Assert.AreEqual(key, siblingsKeysAfterCreationList[1]);
        });
    }
}
