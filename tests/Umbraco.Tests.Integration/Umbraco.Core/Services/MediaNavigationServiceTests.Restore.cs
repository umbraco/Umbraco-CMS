using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class MediaNavigationServiceTests
{
    [Test]
    [TestCase("62BCE72F-8C18-420E-BCAC-112B5ECC95FD", "139DC977-E50F-4382-9728-B278C4B7AC6A")] // Image 4 to Sub-album 1
    [TestCase("DBCAFF2F-BFA4-4744-A948-C290C432D564", "1CD97C02-8534-4B72-AE9E-AE52EC94CF31")] // Sub-album 2 to Album
    [TestCase("3E489C32-9315-42DA-95CE-823D154B09C8", null)] // Image 2 to media root
    public async Task Structure_Updates_When_Restoring_Media(Guid nodeToRestore, Guid? targetParentKey)
    {
        // Arrange
        Guid nodeInRecycleBin = Image3.Key;

        // Move nodes to recycle bin
        await MediaEditingService.MoveToRecycleBinAsync(nodeInRecycleBin, Constants.Security.SuperUserKey);
        await MediaEditingService.MoveToRecycleBinAsync(nodeToRestore, Constants.Security.SuperUserKey);
        MediaNavigationQueryService.TryGetParentKeyInBin(nodeToRestore, out Guid? initialParentKey);
        MediaNavigationQueryService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> initialSiblingsKeys);
        MediaNavigationQueryService.TryGetDescendantsKeysInBin(nodeToRestore, out IEnumerable<Guid> initialDescendantsKeys);
        var beforeRestoreDescendants = initialDescendantsKeys.ToList();

        // Act
        var restoreAttempt = await MediaEditingService.RestoreAsync(nodeToRestore, targetParentKey, Constants.Security.SuperUserKey);
        Guid restoredItemKey = restoreAttempt.Result.Key;

        // Assert
        MediaNavigationQueryService.TryGetParentKey(restoredItemKey, out Guid? restoredItemParentKey);
        MediaNavigationQueryService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> updatedSiblingsKeys);
        MediaNavigationQueryService.TryGetDescendantsKeys(restoredItemKey, out IEnumerable<Guid> afterRestoreDescendantsKeys);
        var afterRestoreDescendants = afterRestoreDescendantsKeys.ToList();

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

            Assert.AreEqual(beforeRestoreDescendants, afterRestoreDescendants);
            Assert.AreEqual(targetParentKey, restoredItemParentKey);
        });
    }

    [Test]
    [TestCase(null)] // Media root
    [TestCase("139DC977-E50F-4382-9728-B278C4B7AC6A")] // Sub-album 1
    [TestCase("DBCAFF2F-BFA4-4744-A948-C290C432D564")] // Sub-album 2
    [TestCase("E0B23D56-9A0E-4FC4-BD42-834B73B4C7AB")] // Sub-sub-album 1
    public async Task Restoring_Content_Adds_It_Last(Guid? targetParentKey)
    {
        // Arrange
        Guid nodeToRestore = Image1.Key;

        // Move node to recycle bin
        await MediaEditingService.MoveToRecycleBinAsync(nodeToRestore, Constants.Security.SuperUserKey);

        // Act
        await MediaEditingService.RestoreAsync(nodeToRestore, targetParentKey, Constants.Security.SuperUserKey);

        // Assert
        if (targetParentKey is null)
        {
            MediaNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);
            Assert.AreEqual(nodeToRestore, rootKeys.Last());
        }
        else
        {
            MediaNavigationQueryService.TryGetChildrenKeys(targetParentKey.Value, out IEnumerable<Guid> childrenKeys);
            Assert.AreEqual(nodeToRestore, childrenKeys.Last());
        }
    }
}
