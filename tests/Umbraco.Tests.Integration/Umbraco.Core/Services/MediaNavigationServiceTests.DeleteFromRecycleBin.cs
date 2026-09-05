using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class MediaNavigationServiceTests
{
    [Test]
    public async Task Structure_Updates_When_Deleting_From_Recycle_Bin()
    {
        // Arrange
        Guid nodeToDelete = Image1.Key;
        Guid nodeInRecycleBin = Image2.Key;
        MediaNavigationQueryService.TryGetDescendantsKeys(nodeToDelete, out IEnumerable<Guid> initialDescendantsKeys);

        // Move nodes to recycle bin
        await MediaEditingService.MoveToRecycleBinAsync(nodeInRecycleBin, Constants.Security.SuperUserKey);
        await MediaEditingService.MoveToRecycleBinAsync(nodeToDelete, Constants.Security.SuperUserKey);
        MediaNavigationQueryService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> initialSiblingsKeys);
        var initialSiblingsCount = initialSiblingsKeys.Count();
        Assert.That(initialSiblingsCount, Is.EqualTo(1));

        // Act
        await MediaEditingService.DeleteFromRecycleBinAsync(nodeToDelete, Constants.Security.SuperUserKey);

        // Assert
        MediaNavigationQueryService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> updatedSiblingsKeys);

        Assert.Multiple(() =>
        {
            // Verify siblings count has decreased by one
            Assert.That(updatedSiblingsKeys.Count(), Is.EqualTo(initialSiblingsCount - 1));
            foreach (Guid descendant in initialDescendantsKeys)
            {
                var descendantExists = MediaNavigationQueryService.TryGetParentKey(descendant, out _);
                Assert.That(descendantExists, Is.False);
                var descendantExistsInRecycleBin = MediaNavigationQueryService.TryGetParentKeyInBin(descendant, out _);
                Assert.That(descendantExistsInRecycleBin, Is.False);
            }
        });
    }
}
