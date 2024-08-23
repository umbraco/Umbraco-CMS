using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

// TODO: check that the descendants have also been removed from both structures - navigation and trash
public partial class DocumentNavigationServiceTests
{
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
}
