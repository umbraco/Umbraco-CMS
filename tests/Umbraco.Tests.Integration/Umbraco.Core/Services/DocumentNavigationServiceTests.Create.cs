using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class DocumentNavigationServiceTests
{
    [Test]
    public async Task Structure_Updates_When_Creating_Content()
    {
        // Arrange
        DocumentNavigationService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> initialSiblingsKeys);
        var initialRootNodeSiblingsCount = initialSiblingsKeys.Count();

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = ContentType.Key,
            ParentKey = Constants.System.RootKey, // Create node at content root
            InvariantName = "Root 2",
        };

        // Act
        var createAttempt = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Guid createdItemKey = createAttempt.Result.Content!.Key;

        // Verify that the structure has updated by checking the siblings list of the Root once again
        DocumentNavigationService.TryGetSiblingsKeys(Root.Key, out IEnumerable<Guid> updatedSiblingsKeys);
        List<Guid> siblingsList = updatedSiblingsKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsNotEmpty(siblingsList);
            Assert.AreEqual(initialRootNodeSiblingsCount + 1, siblingsList.Count);
            Assert.AreEqual(createdItemKey, siblingsList.First());
        });
    }
}
