using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    // Children are created in index order 0..4. Names, create dates and update dates are deliberately
    // assigned in three different orders so that each field produces a distinct expected ordering -
    // a sort applied against the wrong field cannot coincidentally produce the right result.
    private static readonly (string Name, int CreateOffset, int UpdateOffset)[] _sortByFieldChildren =
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
        (IContent root, Guid[] childKeysByIndex) = await CreateRootWithChildrenForFieldSorting();

        var result = await ContentEditingService.SortByFieldAsync(root.Key, field, direction, culture: null, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result);

        var actualChildKeys = ContentService
            .GetPagedChildren(root.Id, 0, 100, out _, propertyAliases: null, filter: null, ordering: null)
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .ToArray();
        var expectedChildKeys = expectedChildIndexes.Select(i => childKeysByIndex[i]).ToArray();

        Assert.AreEqual(expectedChildKeys, actualChildKeys);
    }

    [Test]
    public async Task Sort_Children_By_Field_With_No_Children_Returns_Success()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType(alias: "rootPage", name: "Root Page");
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var parent = (await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key, Variants = [new() { Name = "Childless" }], ParentKey = Constants.System.RootKey,
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        var result = await ContentEditingService.SortByFieldAsync(parent.Key, ContentSortField.Name, Direction.Ascending, culture: null, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result);
    }

    [Test]
    public async Task Sort_Children_By_Field_Returns_NotFound_For_Unknown_Parent()
    {
        var result = await ContentEditingService.SortByFieldAsync(Guid.NewGuid(), ContentSortField.Name, Direction.Ascending, culture: null, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result);
    }

    [Test]
    public async Task Sort_Children_By_Field_Returns_SortingInvalid_For_Unknown_Field()
    {
        (IContent root, _) = await CreateRootWithChildrenForFieldSorting();

        var result = await ContentEditingService.SortByFieldAsync(root.Key, (ContentSortField)999, Direction.Ascending, culture: null, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.SortingInvalid, result);
    }

    [TestCase("en-US", new[] { 1, 2, 0 })] // English names ascending: A(1), B(2), C(0)
    [TestCase("da-DK", new[] { 0, 2, 1 })] // Danish names ascending: 1(0), 2(2), 3(1)
    public async Task Sort_Children_By_Name_Uses_The_Variant_Name_For_The_Given_Culture(string culture, int[] expectedChildIndexes)
    {
        var contentType = await CreateVariantContentType(variantTitleAsMandatory: false);
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        ContentTypeService.Save(contentType);

        var root = (await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants = [new() { Culture = "en-US", Name = "Root EN" }, new() { Culture = "da-DK", Name = "Root DA" }],
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        // (en-US name, da-DK name) chosen so the two cultures sort into different orders.
        (string English, string Danish)[] childNames = [("C", "1"), ("A", "3"), ("B", "2")];
        var childKeys = new Guid[childNames.Length];
        for (var i = 0; i < childNames.Length; i++)
        {
            (string english, string danish) = childNames[i];
            var child = (await ContentEditingService.CreateAsync(
                new ContentCreateModel
                {
                    ContentTypeKey = contentType.Key,
                    ParentKey = root.Key,
                    Variants = [new() { Culture = "en-US", Name = english }, new() { Culture = "da-DK", Name = danish }],
                },
                Constants.Security.SuperUserKey)).Result.Content!;
            childKeys[i] = child.Key;
        }

        var result = await ContentEditingService.SortByFieldAsync(root.Key, ContentSortField.Name, Direction.Ascending, culture, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result);

        var actualChildKeys = ContentService
            .GetPagedChildren(root.Id, 0, 100, out _, propertyAliases: null, filter: null, ordering: null)
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .ToArray();
        var expectedChildKeys = expectedChildIndexes.Select(i => childKeys[i]).ToArray();

        Assert.AreEqual(expectedChildKeys, actualChildKeys);
    }

    [TestCase(ContentSortField.Name, Direction.Ascending, new[] { 3, 1, 4, 0, 2 })]
    [TestCase(ContentSortField.Name, Direction.Descending, new[] { 2, 0, 4, 1, 3 })]
    [TestCase(ContentSortField.CreateDate, Direction.Ascending, new[] { 0, 1, 2, 3, 4 })]
    [TestCase(ContentSortField.CreateDate, Direction.Descending, new[] { 4, 3, 2, 1, 0 })]
    [TestCase(ContentSortField.UpdateDate, Direction.Ascending, new[] { 0, 2, 4, 1, 3 })]
    [TestCase(ContentSortField.UpdateDate, Direction.Descending, new[] { 3, 1, 4, 2, 0 })]
    public async Task Can_Sort_Root_Content_By_Field(ContentSortField field, Direction direction, int[] expectedRootIndexes)
    {
        Guid[] rootKeysByIndex = await CreateRootContentForFieldSorting();

        var result = await ContentEditingService.SortByFieldAsync(parentKey: null, field, direction, culture: null, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result);

        var actualRootKeys = ContentService
            .GetPagedChildren(Constants.System.Root, 0, 100, out _, propertyAliases: null, filter: null, ordering: null)
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .ToArray();
        var expectedRootKeys = expectedRootIndexes.Select(i => rootKeysByIndex[i]).ToArray();

        Assert.AreEqual(expectedRootKeys, actualRootKeys);
    }

    private async Task<(IContent Root, Guid[] ChildKeysByIndex)> CreateRootWithChildrenForFieldSorting()
    {
        var childContentType = ContentTypeBuilder.CreateBasicContentType(alias: "childPage", name: "Child Page");
        await ContentTypeService.CreateAsync(childContentType, Constants.Security.SuperUserKey);

        var rootContentType = ContentTypeBuilder.CreateBasicContentType(alias: "rootPage", name: "Root Page");
        rootContentType.AllowedAsRoot = true;
        rootContentType.AllowedContentTypes = [new ContentTypeSort(childContentType.Key, 1, childContentType.Alias)];
        await ContentTypeService.CreateAsync(rootContentType, Constants.Security.SuperUserKey);

        var root = (await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = rootContentType.Key, Variants = [new() { Name = "Root" }], ParentKey = Constants.System.RootKey,
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        var baseDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var childKeys = new Guid[_sortByFieldChildren.Length];
        for (var i = 0; i < _sortByFieldChildren.Length; i++)
        {
            (string name, int createOffset, int updateOffset) = _sortByFieldChildren[i];

            var child = (await ContentEditingService.CreateAsync(
                new ContentCreateModel
                {
                    ContentTypeKey = childContentType.Key, ParentKey = root.Key, Variants = [new() { Name = name }],
                },
                Constants.Security.SuperUserKey)).Result.Content!;
            childKeys[i] = child.Key;

            // Setting the dates marks them dirty, so they are persisted as-is rather than being stamped with "now".
            child.CreateDate = baseDate.AddDays(createOffset);
            child.UpdateDate = baseDate.AddDays(100 + updateOffset);
            ContentService.Save(child);
        }

        return (root, childKeys);
    }

    private async Task<Guid[]> CreateRootContentForFieldSorting()
    {
        foreach (var existingRoot in ContentService.GetRootContent())
        {
            ContentService.Delete(existingRoot);
        }

        var rootContentType = ContentTypeBuilder.CreateBasicContentType(alias: "rootPage", name: "Root Page");
        rootContentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(rootContentType, Constants.Security.SuperUserKey);

        var baseDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var rootKeys = new Guid[_sortByFieldChildren.Length];
        for (var i = 0; i < _sortByFieldChildren.Length; i++)
        {
            (string name, int createOffset, int updateOffset) = _sortByFieldChildren[i];

            var root = (await ContentEditingService.CreateAsync(
                new ContentCreateModel
                {
                    ContentTypeKey = rootContentType.Key, ParentKey = Constants.System.RootKey, Variants = [new() { Name = name }],
                },
                Constants.Security.SuperUserKey)).Result.Content!;
            rootKeys[i] = root.Key;

            // Setting the dates marks them dirty, so they are persisted as-is rather than being stamped with "now".
            root.CreateDate = baseDate.AddDays(createOffset);
            root.UpdateDate = baseDate.AddDays(100 + updateOffset);
            ContentService.Save(root);
        }

        return rootKeys;
    }
}
