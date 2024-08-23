using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

// TODO: test that descendants are also restored in the right place
public partial class DocumentNavigationServiceTests
{
    [Test]
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
}
