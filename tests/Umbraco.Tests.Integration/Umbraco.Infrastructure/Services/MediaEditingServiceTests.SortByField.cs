using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal sealed partial class MediaEditingServiceTests
{
    private IMediaService MediaService => GetRequiredService<IMediaService>();

    // Names, create dates and update dates are assigned in three different orders so each field
    // produces a distinct expected ordering (see ContentEditingServiceTests.SortByField for the rationale).
    private static readonly (string Name, int CreateOffset, int UpdateOffset)[] SortByFieldFolders =
    {
        ("delta", 0, 0),   // index 0
        ("bravo", 1, 3),   // index 1
        ("echo", 2, 1),    // index 2
        ("alpha", 3, 4),   // index 3
        ("charlie", 4, 2), // index 4
    };

    [TestCase(ContentSortField.Name, Direction.Ascending, new[] { 3, 1, 4, 0, 2 })]
    [TestCase(ContentSortField.Name, Direction.Descending, new[] { 2, 0, 4, 1, 3 })]
    [TestCase(ContentSortField.CreateDate, Direction.Ascending, new[] { 0, 1, 2, 3, 4 })]
    [TestCase(ContentSortField.CreateDate, Direction.Descending, new[] { 4, 3, 2, 1, 0 })]
    [TestCase(ContentSortField.UpdateDate, Direction.Ascending, new[] { 0, 2, 4, 1, 3 })]
    [TestCase(ContentSortField.UpdateDate, Direction.Descending, new[] { 3, 1, 4, 2, 0 })]
    public async Task Can_Sort_Children_By_Field(ContentSortField field, Direction direction, int[] expectedChildIndexes)
    {
        (IMedia root, Guid[] childKeysByIndex) = await CreateRootWithChildrenForFieldSorting();

        var result = await MediaEditingService.SortByFieldAsync(root.Key, field, direction, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result);

        var actualChildKeys = MediaService
            .GetPagedChildren(root.Id, 0, 100, out _)
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .ToArray();
        var expectedChildKeys = expectedChildIndexes.Select(i => childKeysByIndex[i]).ToArray();

        Assert.AreEqual(expectedChildKeys, actualChildKeys);
    }

    [Test]
    public async Task Sort_Children_By_Field_Returns_NotFound_For_Unknown_Parent()
    {
        var result = await MediaEditingService.SortByFieldAsync(Guid.NewGuid(), ContentSortField.Name, Direction.Ascending, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result);
    }

    [Test]
    public async Task Sort_Children_By_Field_Throws_For_Unrecognised_Field()
    {
        (IMedia root, _) = await CreateRootWithChildrenForFieldSorting();

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MediaEditingService.SortByFieldAsync(root.Key, (ContentSortField)999, Direction.Ascending, Constants.Security.SuperUserKey));
    }

    [TestCase(ContentSortField.Name, Direction.Ascending, new[] { 3, 1, 4, 0, 2 })]
    [TestCase(ContentSortField.Name, Direction.Descending, new[] { 2, 0, 4, 1, 3 })]
    [TestCase(ContentSortField.CreateDate, Direction.Ascending, new[] { 0, 1, 2, 3, 4 })]
    [TestCase(ContentSortField.CreateDate, Direction.Descending, new[] { 4, 3, 2, 1, 0 })]
    [TestCase(ContentSortField.UpdateDate, Direction.Ascending, new[] { 0, 2, 4, 1, 3 })]
    [TestCase(ContentSortField.UpdateDate, Direction.Descending, new[] { 3, 1, 4, 2, 0 })]
    public async Task Can_Sort_Root_Media_By_Field(ContentSortField field, Direction direction, int[] expectedRootIndexes)
    {
        Guid[] rootKeysByIndex = await CreateRootMediaForFieldSorting();

        var result = await MediaEditingService.SortByFieldAsync(parentKey: null, field, direction, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result);

        var actualRootKeys = MediaService
            .GetPagedChildren(Constants.System.Root, 0, 100, out _)
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .ToArray();
        var expectedRootKeys = expectedRootIndexes.Select(i => rootKeysByIndex[i]).ToArray();

        Assert.AreEqual(expectedRootKeys, actualRootKeys);
    }

    [Test]
    public async Task Sort_Children_By_Field_With_No_Children_Returns_Success()
    {
        var folder = await CreateFolderMediaAsync("Childless");

        var result = await MediaEditingService.SortByFieldAsync(folder.Key, ContentSortField.Name, Direction.Ascending, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result);
    }

    private async Task<(IMedia Root, Guid[] ChildKeysByIndex)> CreateRootWithChildrenForFieldSorting()
    {
        var root = await CreateFolderMediaAsync("Root");

        var baseDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var childKeys = new Guid[SortByFieldFolders.Length];
        for (var i = 0; i < SortByFieldFolders.Length; i++)
        {
            (string name, int createOffset, int updateOffset) = SortByFieldFolders[i];

            var child = await CreateFolderMediaAsync(name, parentKey: root.Key);
            childKeys[i] = child.Key;

            // Setting the dates marks them dirty, so they are persisted as-is rather than being stamped with "now".
            child.CreateDate = baseDate.AddDays(createOffset);
            child.UpdateDate = baseDate.AddDays(100 + updateOffset);
            MediaService.Save(child);
        }

        return (root, childKeys);
    }

    private async Task<Guid[]> CreateRootMediaForFieldSorting()
    {
        var baseDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var rootKeys = new Guid[SortByFieldFolders.Length];
        for (var i = 0; i < SortByFieldFolders.Length; i++)
        {
            (string name, int createOffset, int updateOffset) = SortByFieldFolders[i];

            var media = await CreateFolderMediaAsync(name);
            rootKeys[i] = media.Key;

            // Setting the dates marks them dirty, so they are persisted as-is rather than being stamped with "now".
            media.CreateDate = baseDate.AddDays(createOffset);
            media.UpdateDate = baseDate.AddDays(100 + updateOffset);
            MediaService.Save(media);
        }

        return rootKeys;
    }
}
