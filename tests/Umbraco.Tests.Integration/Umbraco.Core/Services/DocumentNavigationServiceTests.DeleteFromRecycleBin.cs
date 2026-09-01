using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class DocumentNavigationServiceTests
{
    [Test]
    public async Task Structure_Updates_When_Deleting_From_Recycle_Bin()
    {
        // Arrange
        Guid nodeToDelete = Child1.Key;
        Guid nodeInRecycleBin = Grandchild4.Key;
        DocumentNavigationQueryService.TryGetDescendantsKeys(nodeToDelete, out IEnumerable<Guid> initialDescendantsKeys);

        // Move nodes to recycle bin
        await ContentEditingService.MoveToRecycleBinAsync(nodeInRecycleBin, Constants.Security.SuperUserKey);
        await ContentEditingService.MoveToRecycleBinAsync(nodeToDelete, Constants.Security.SuperUserKey);
        DocumentNavigationQueryService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> initialSiblingsKeys);
        var initialSiblingsCount = initialSiblingsKeys.Count();
        Assert.That(initialSiblingsCount, Is.EqualTo(1));

        // Act
        await ContentEditingService.DeleteFromRecycleBinAsync(nodeToDelete, Constants.Security.SuperUserKey);

        // Assert
        DocumentNavigationQueryService.TryGetSiblingsKeysInBin(nodeInRecycleBin, out IEnumerable<Guid> updatedSiblingsKeys);

        Assert.Multiple(() =>
        {
            // Verify siblings count has decreased by one
            Assert.That(updatedSiblingsKeys.Count(), Is.EqualTo(initialSiblingsCount - 1));
            foreach (Guid descendant in initialDescendantsKeys)
            {
                var descendantExists = DocumentNavigationQueryService.TryGetParentKey(descendant, out _);
                Assert.That(descendantExists, Is.False);
                var descendantExistsInRecycleBin = DocumentNavigationQueryService.TryGetParentKeyInBin(descendant, out _);
                Assert.That(descendantExistsInRecycleBin, Is.False);
            }
        });
    }
}
