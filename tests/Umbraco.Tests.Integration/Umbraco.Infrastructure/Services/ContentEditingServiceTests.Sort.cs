using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

// NOTE: all sorting scenarios are covered by TreeEntitySortingServiceTests; these tests only serve to verify the application of TreeEntitySortingService within ContentEditingService
public partial class ContentEditingServiceTests
{
    [Test]
    public async Task Can_Sort_Children()
    {
        var root = await CreateRootContentWithTenChildren();

        var originalChildren = ContentService.GetPagedChildren(root.Id, 0, 100, out _).ToArray();
        Assert.AreEqual(10, originalChildren.Length);

        var sortingModels = originalChildren.Reverse().Select((child, index) => new SortingModel { Key = child.Key, SortOrder = index });

        var result = await ContentEditingService.SortAsync(root.Key, sortingModels, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result);

        var actualChildrenKeys = ContentService
            .GetPagedChildren(root.Id, 0, 100, out _)
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .ToArray();
        var expectedChildrenKeys = originalChildren
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .Reverse()
            .ToArray();
        Assert.IsTrue(expectedChildrenKeys.SequenceEqual(actualChildrenKeys));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Sort_Root_Content(bool useRootKeyForSorting)
    {
        var existingRoots = ContentService.GetRootContent();
        foreach (var existingRoot in existingRoots)
        {
            ContentService.Delete(existingRoot);
        }

        var rootContentType = ContentTypeBuilder.CreateBasicContentType(alias: "rootPage", name: "Root Page");
        rootContentType.AllowedAsRoot = true;
        ContentTypeService.Save(rootContentType);

        for (var i = 1; i < 11; i++)
        {
            await ContentEditingService.CreateAsync(
                new ContentCreateModel
                {
                    ContentTypeKey = rootContentType.Key, Variants = [new () { Name = $"Root {i}" }], ParentKey = Constants.System.RootKey,
                },
                Constants.Security.SuperUserKey);
        }

        var originalRoots = ContentService.GetPagedChildren(Constants.System.Root, 0, 100, out _).ToArray();
        Assert.AreEqual(10, originalRoots.Length);

        var sortingModels = originalRoots.Reverse().Select((root, index) => new SortingModel { Key = root.Key, SortOrder = index });

        var parentId = useRootKeyForSorting
            ? Constants.System.RootKey
            : null;

        var result = await ContentEditingService.SortAsync(parentId, sortingModels, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result);

        var actualRootKeys = ContentService
            .GetPagedChildren(Constants.System.Root, 0, 100, out _)
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .ToArray();
        var expectedRootKeys = originalRoots
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .Reverse()
            .ToArray();
        Assert.IsTrue(expectedRootKeys.SequenceEqual(actualRootKeys));
    }

    [Test]
    public async Task Cannot_Sort_Unknown_Children()
    {
        var root = await CreateRootContentWithTenChildren();

        var originalChildren = ContentService.GetPagedChildren(root.Id, 0, 100, out _).ToArray();
        Assert.AreEqual(10, originalChildren.Length);

        var sortingModels = new[]
        {
            new SortingModel { Key = Guid.NewGuid(), SortOrder = 0 },
            new SortingModel { Key = Guid.NewGuid(), SortOrder = 1 },
        };

        var result = await ContentEditingService.SortAsync(root.Key, sortingModels, Constants.Security.SuperUserKey);
        Assert.AreEqual(ContentEditingOperationStatus.SortingInvalid, result);

        var actualChildrenKeys = ContentService
            .GetPagedChildren(root.Id, 0, 100, out _)
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .ToArray();
        var expectedChildrenKeys = originalChildren
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .ToArray();
        Assert.IsTrue(expectedChildrenKeys.SequenceEqual(actualChildrenKeys));
    }

    private async Task<IContent> CreateRootContentWithTenChildren()
    {
        var childContentType = ContentTypeBuilder.CreateBasicContentType(alias: "childPage", name: "Child Page");
        ContentTypeService.Save(childContentType);

        var rootContentType = ContentTypeBuilder.CreateBasicContentType(alias: "rootPage", name: "Root Page");
        rootContentType.AllowedAsRoot = true;
        rootContentType.AllowedContentTypes = new[]
        {
            new ContentTypeSort(childContentType.Key, 1, childContentType.Alias)
        };
        ContentTypeService.Save(rootContentType);

        var root = (await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = rootContentType.Key, Variants = [new () { Name = "Root" }], ParentKey = Constants.System.RootKey,
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        for (var i = 1; i < 11; i++)
        {
            var createModel = new ContentCreateModel
            {
                ContentTypeKey = childContentType.Key,
                ParentKey = root.Key,
                Variants = [new () { Name = $"Child {i}" }]
            };

            await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        }

        return root;
    }
}
