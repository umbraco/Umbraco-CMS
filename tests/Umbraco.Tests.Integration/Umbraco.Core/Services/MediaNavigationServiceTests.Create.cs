using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class MediaNavigationServiceTests
{
    [Test]
    public async Task Structure_Updates_When_Creating_Media_At_Root()
    {
        // Arrange
        MediaNavigationQueryService.TryGetSiblingsKeys(Album.Key, out IEnumerable<Guid> initialSiblingsKeys);
        var initialRootNodeSiblingsCount = initialSiblingsKeys.Count();
        var createModel = CreateMediaCreateModel("Album 2", Guid.NewGuid(), FolderMediaType.Key);

        // Act
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

    [Test]
    [TestCase(null)] // Media root
    [TestCase("1CD97C02-8534-4B72-AE9E-AE52EC94CF31")] // Album
    [TestCase("139DC977-E50F-4382-9728-B278C4B7AC6A")] // Sub-album 1
    [TestCase("DBCAFF2F-BFA4-4744-A948-C290C432D564")] // Sub-album 2
    [TestCase("E0B23D56-9A0E-4FC4-BD42-834B73B4C7AB")] // Sub-sub-album 1
    public async Task Creating_Child_Media_Adds_It_As_The_Last_Child(Guid? parentKey)
    {
        // Arrange
        Guid newNodeKey = Guid.NewGuid();
        var createModel = CreateMediaCreateModel("Child Image", newNodeKey, ImageMediaType.Key, parentKey);

        // Act
        await MediaEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        // Assert
        if (parentKey is null)
        {
            MediaNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);
            Assert.AreEqual(newNodeKey, rootKeys.Last());
        }
        else
        {
            MediaNavigationQueryService.TryGetChildrenKeys(parentKey.Value, out IEnumerable<Guid> childrenKeys);
            Assert.AreEqual(newNodeKey, childrenKeys.Last());
        }
    }
}
