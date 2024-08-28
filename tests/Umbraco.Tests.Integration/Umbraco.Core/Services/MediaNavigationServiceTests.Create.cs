using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class MediaNavigationServiceTests
{
    // TODO: I am unsure if there are issues with the CreatedMediaCreateModel, when I try to create a ImageMediaType at root, I get validation errors.
    [Test]
    public async Task Structure_Updates_When_Creating_Media_At_Root()
    {
        // Arrange
        MediaNavigationQueryService.TryGetSiblingsKeys(Album.Key, out IEnumerable<Guid> initialSiblingsKeys);
        var initialRootNodeSiblingsCount = initialSiblingsKeys.Count();
        var createModel = CreateMediaCreateModel("Root Image", Guid.NewGuid(), ImageMediaType.Key);

        // Act
        // UNSURE,the createAttempt has a propertyValidationError, but it is still able to create the media
        var createAttempt = await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Guid createdItemKey = createAttempt.Result.Content!.Key;
        // Verify that the structure has updated by checking the siblings list of the Root once again
        MediaNavigationQueryService.TryGetSiblingsKeys(Album.Key, out IEnumerable<Guid> updatedSiblingsKeys);
        List<Guid> siblingsList = updatedSiblingsKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsNotEmpty(siblingsList);
            Assert.AreEqual(initialRootNodeSiblingsCount + 1, siblingsList.Count);
            Assert.AreEqual(createdItemKey, siblingsList.First());
        });
    }

    [Test]
    public async Task Structure_Updates_When_Creating_Child_Media()
    {
        // Arrange
        MediaNavigationQueryService.TryGetChildrenKeys(Album.Key, out IEnumerable<Guid> initialChildrenKeys);
        var initialChild1ChildrenCount = initialChildrenKeys.Count();
        var createModel = CreateMediaCreateModel("Child Image", Guid.NewGuid(), ImageMediaType.Key, Album.Key);

        // Act
        var createAttempt = await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Guid createdItemKey = createAttempt.Result.Content!.Key;
        // Verify that the structure has updated by checking the children of the Child1 node once again
        MediaNavigationQueryService.TryGetChildrenKeys(Album.Key, out IEnumerable<Guid> updatedChildrenKeys);
        List<Guid> childrenList = updatedChildrenKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsNotEmpty(childrenList);
            Assert.AreEqual(initialChild1ChildrenCount + 1, childrenList.Count);
            Assert.IsTrue(childrenList.Contains(createdItemKey));
        });
    }
}
