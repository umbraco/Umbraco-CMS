using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class MediaNavigationServiceTests
{
    [Test]
    public async Task Structure_Updates_When_Reversing_Children_Sort_Order()
    {
        // Arrange
        Guid nodeToSortItsChildren = Album.Key;
        MediaNavigationQueryService.TryGetChildrenKeys(nodeToSortItsChildren, out IEnumerable<Guid> initialChildrenKeys);
        List<Guid> initialChildrenKeysList = initialChildrenKeys.ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, initialChildrenKeysList.Count);

            // Assert initial order
            Assert.AreEqual(Image1.Key, initialChildrenKeysList[0]);
            Assert.AreEqual(SubAlbum1.Key, initialChildrenKeysList[1]);
            Assert.AreEqual(SubAlbum2.Key, initialChildrenKeysList[2]);
        });

        IEnumerable<SortingModel> sortingModels = initialChildrenKeys
            .Reverse()
            .Select((key, index) => new SortingModel { Key = key, SortOrder = index });

        // Act
        await MediaEditingService.SortAsync(nodeToSortItsChildren, sortingModels, Constants.Security.SuperUserKey);

        // Assert
        MediaNavigationQueryService.TryGetChildrenKeys(nodeToSortItsChildren, out IEnumerable<Guid> sortedChildrenKeys);
        List<Guid> sortedChildrenKeysList = sortedChildrenKeys.ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(SubAlbum2.Key, sortedChildrenKeysList[0]);
            Assert.AreEqual(SubAlbum1.Key, sortedChildrenKeysList[1]);
            Assert.AreEqual(Image1.Key, sortedChildrenKeysList[2]);
        });

        var expectedChildrenKeysList = initialChildrenKeys.Reverse().ToList();

        // Check that the order matches what is expected
        Assert.IsTrue(expectedChildrenKeysList.SequenceEqual(sortedChildrenKeysList));
    }

    [Test]
    public async Task Structure_Updates_When_Children_Have_Custom_Sort_Order()
    {
        // Arrange
        Guid node = Album.Key;
        var customSortingModels = new List<SortingModel>
        {
            new() { Key = SubAlbum1.Key, SortOrder = 0 }, // Move Sub-album 1 to the position 1
            new() { Key = SubAlbum2.Key, SortOrder = 1 }, // Move Sub-album 2 to the position 2
            new() { Key = Image1.Key, SortOrder = 2 }, // Move Image 1 to the position 3
        };

        // Act
        await MediaEditingService.SortAsync(node, customSortingModels, Constants.Security.SuperUserKey);

        // Assert
        MediaNavigationQueryService.TryGetChildrenKeys(node, out IEnumerable<Guid> sortedChildrenKeys);
        List<Guid> sortedChildrenKeysList = sortedChildrenKeys.ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(SubAlbum1.Key, sortedChildrenKeysList[0]);
            Assert.AreEqual(SubAlbum2.Key, sortedChildrenKeysList[1]);
            Assert.AreEqual(Image1.Key, sortedChildrenKeysList[2]);
        });

        var expectedChildrenKeysList = customSortingModels
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Key)
            .ToList();

        // Check that the order matches what is expected
        Assert.IsTrue(expectedChildrenKeysList.SequenceEqual(sortedChildrenKeysList));
    }

    [Test]
    public async Task Structure_Updates_When_Sorting_Items_At_Root()
    {
        // Arrange
        var anotherRootCreateModel = CreateMediaCreateModel("Album 2", Guid.NewGuid(), FolderMediaType.Key, Constants.System.RootKey);
        await MediaEditingService.CreateAsync(anotherRootCreateModel, Constants.Security.SuperUserKey);
        MediaNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> initialRootKeys);

        var sortingModels = initialRootKeys
            .Reverse()
            .Select((rootKey, index) => new SortingModel { Key = rootKey, SortOrder = index });

        // Act
        await MediaEditingService.SortAsync(Constants.System.RootKey, sortingModels, Constants.Security.SuperUserKey);

        // Assert
        MediaNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> sortedRootKeys);

        var expectedRootKeysList = initialRootKeys.Reverse().ToList();

        // Check that the order matches what is expected
        Assert.IsTrue(expectedRootKeysList.SequenceEqual(sortedRootKeys));
    }

    [Test]
    public async Task Descendants_Are_Returned_In_Correct_Order_After_Children_Are_Reordered()
    {
        // Arrange
        Guid node = Album.Key;
        MediaNavigationQueryService.TryGetDescendantsKeys(node, out IEnumerable<Guid> initialDescendantsKeys);

        var customSortingModels = new List<SortingModel>
        {
            new() { Key = SubAlbum2.Key, SortOrder = 0 }, // Move Sub-album 2 to the position 1
            new() { Key = Image1.Key, SortOrder = 1 }, // Move Image 1 to the position 2
            new() { Key = SubAlbum1.Key, SortOrder = 2 }, // Move Sub-album 1 to the position 3
        };

        var expectedDescendantsOrder = new List<Guid>
        {
            SubAlbum2.Key, SubSubAlbum1.Key, Image4.Key, // Sub-album 2 and its descendants
            Image1.Key, // Image 1
            SubAlbum1.Key, Image2.Key, Image3.Key, // Sub-album 1 and its descendants
        };

        // Act
        await MediaEditingService.SortAsync(node, customSortingModels, Constants.Security.SuperUserKey);

        // Assert
        MediaNavigationQueryService.TryGetDescendantsKeys(node, out IEnumerable<Guid> updatedDescendantsKeys);
        List<Guid> updatedDescendantsKeysList = updatedDescendantsKeys.ToList();

        Assert.Multiple(() =>
        {
            Assert.IsFalse(initialDescendantsKeys.SequenceEqual(updatedDescendantsKeysList));
            Assert.IsTrue(expectedDescendantsOrder.SequenceEqual(updatedDescendantsKeysList));
        });
    }

    [Test]
    [TestCase(1, 2, 0, new[] { "DBCAFF2F-BFA4-4744-A948-C290C432D564", "03976EBE-A942-4F24-9885-9186E99AEF7C" })] // Custom sort order: Sub-album 2, Image 1, Sub-album 1; Expected order: Sub-album 2, Image 1
    [TestCase(0, 1, 2, new[] { "03976EBE-A942-4F24-9885-9186E99AEF7C", "DBCAFF2F-BFA4-4744-A948-C290C432D564" })] // Custom sort order: Image 1, Sub-album 1, Sub-album 2; Expected order: Image 1, Sub-album 2
    [TestCase(2, 0, 1, new[] { "DBCAFF2F-BFA4-4744-A948-C290C432D564", "03976EBE-A942-4F24-9885-9186E99AEF7C" })] // Custom sort order: Sub-album 1, Sub-album 2, Image 1; Expected order: Sub-album 2, Image 1
    public async Task Siblings_Are_Returned_In_Correct_Order_After_Sorting(int sortOrder1, int sortOrder2, int sortOrder3, string[] expectedSiblings)
    {
        // Arrange
        Guid node = SubAlbum1.Key;

        var customSortingModels = new List<SortingModel>
        {
            new() { Key = Image1.Key, SortOrder = sortOrder1 }, // Move Image 1 to the position sortOrder1
            new() { Key = SubAlbum1.Key, SortOrder = sortOrder2 }, // Move Sub-album 1 to the position sortOrder2
            new() { Key = SubAlbum2.Key, SortOrder = sortOrder3 }, // Move Sub-album 2 to the position sortOrder3
        };

        Guid[] expectedSiblingsOrder = Array.ConvertAll(expectedSiblings, Guid.Parse);

        // Act
        await MediaEditingService.SortAsync(Album.Key, customSortingModels, Constants.Security.SuperUserKey); // Using the parent key here

        // Assert
        MediaNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> sortedSiblingsKeys);
        var sortedSiblingsKeysList = sortedSiblingsKeys.ToList();

        Assert.IsTrue(expectedSiblingsOrder.SequenceEqual(sortedSiblingsKeysList));
    }

    [Test]
    public async Task Siblings_Are_Returned_In_Correct_Order_After_Sorting_At_Root()
    {
        // Arrange
        Guid node = Album.Key;
        var anotherRootCreateModel1 = CreateMediaCreateModel("Album 2", Guid.NewGuid(), FolderMediaType.Key, Constants.System.RootKey);
        await MediaEditingService.CreateAsync(anotherRootCreateModel1, Constants.Security.SuperUserKey);
        var anotherRootCreateModel2 = CreateMediaCreateModel("Album 3", Guid.NewGuid(), FolderMediaType.Key, Constants.System.RootKey);
        await MediaEditingService.CreateAsync(anotherRootCreateModel2, Constants.Security.SuperUserKey);
        MediaNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> initialRootKeys);

        var sortingModels = initialRootKeys
            .Reverse()
            .Select((rootKey, index) => new SortingModel { Key = rootKey, SortOrder = index });

        var expectedSiblingsKeysList = initialRootKeys.Reverse().Where(k => k != node).ToList();

        // Act
        await MediaEditingService.SortAsync(Constants.System.RootKey, sortingModels, Constants.Security.SuperUserKey);

        // Assert
        MediaNavigationQueryService.TryGetSiblingsKeys(node, out IEnumerable<Guid> sortedSiblingsKeys);
        var sortedSiblingsKeysList = sortedSiblingsKeys.ToList();

        Assert.IsTrue(expectedSiblingsKeysList.SequenceEqual(sortedSiblingsKeysList));
    }
}
