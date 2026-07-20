using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.BackOffice;

public partial class IndexedEntitySearchServiceTests
{
    [Test]
    public async Task Media_CanFindAll()
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: string.Empty,
            parentId: null,
            contentTypeIds: null,
            trashed: null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(33));
            Assert.That(result.Items.Count(), Is.EqualTo(33));
            Assert.That(result.Items.Select(item => item.Key), Is.Unique);
            Assert.That(result.Items.DistinctBy(item => item.Trashed).Count(), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Media_CanFindAllRootsByQuery()
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: "root",
            parentId: null,
            contentTypeIds: null,
            trashed: null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(3));
            Assert.That(result.Items.Count(), Is.EqualTo(3));
            Assert.That(result.Items.Select(item => item.Key), Is.Unique);
            Assert.That(result.Items.Count(item => item.ParentId == Constants.System.Root), Is.EqualTo(2));
            Assert.That(result.Items.Count(item => item.ParentId == Constants.System.RecycleBinMedia), Is.EqualTo(1));
            Assert.That(result.Items.All(item => item.Name!.Contains("Root")), Is.True);
        });
    }

    [Test]
    public async Task Media_CanFindSpecificRootByQuery()
    {
        IMedia root = MediaService.GetRootMedia().OrderBy(media => media.SortOrder).Skip(1).First();
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: "single1root",
            parentId: null,
            contentTypeIds: null,
            trashed: null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Items.First().Key, Is.EqualTo(root.Key));
        });
    }

    [Test]
    public async Task Media_CanFindAllChildrenByQuery()
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: "child",
            parentId: null,
            contentTypeIds: null,
            trashed: null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(30));
            Assert.That(result.Items.Count(), Is.EqualTo(30));
            Assert.That(result.Items.Select(item => item.Key), Is.Unique);
            Assert.That(result.Items.DistinctBy(item => item.ParentId).Count(), Is.EqualTo(3));
            Assert.That(result.Items.All(item => item.Name!.Contains("Child")), Is.True);
        });
    }

    [Test]
    public async Task Media_CanFindAllChildrenBelowParent()
    {
        IMedia root = MediaService.GetRootMedia().Last();
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: string.Empty,
            parentId: root.Key,
            contentTypeIds: null,
            trashed: null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(10));
            Assert.That(result.Items.Count(), Is.EqualTo(10));
            Assert.That(result.Items.Select(item => item.Key), Is.Unique);
            Assert.That(result.Items.All(item => item.Name!.Contains("Child")), Is.True);
            Assert.That(result.Items.DistinctBy(item => item.ParentId).Single().ParentId, Is.EqualTo(root.Id));
        });
    }

    [Test]
    public async Task Media_CanFindChildrenBelowParentByQuery()
    {
        IMedia root = MediaService.GetRootMedia().Last();
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: "triple2child",
            parentId: root.Key,
            contentTypeIds: null,
            trashed: null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(3));
            IEntitySlim[] items = result.Items.OrderBy(item => item.SortOrder).ToArray();
            Assert.That(items[0].Name, Is.EqualTo("Child 6"));
            Assert.That(items[1].Name, Is.EqualTo("Child 7"));
            Assert.That(items[2].Name, Is.EqualTo("Child 8"));
            Assert.That(result.Items.DistinctBy(item => item.ParentId).Single().ParentId, Is.EqualTo(root.Id));
        });
    }

    [TestCase("rootMediaType", null, 3)]
    [TestCase("rootMediaType", false, 2)]
    [TestCase("rootMediaType", true, 1)]
    [TestCase("childMediaType", null, 30)]
    [TestCase("childMediaType", false, 20)]
    [TestCase("childMediaType", true, 10)]
    public async Task Media_CanFindAllByMediaType(string mediaTypeAlias, bool? trashed, int expectedTotal)
    {
        Guid mediaTypeKey = MediaTypeService.Get(mediaTypeAlias)?.Key
                            ?? throw new InvalidOperationException($"Could not find {mediaTypeAlias}.");

        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: string.Empty,
            parentId: null,
            contentTypeIds: [mediaTypeKey],
            trashed: trashed);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(expectedTotal));
            IMediaEntitySlim[] items = result.Items.OfType<IMediaEntitySlim>().ToArray();
            Assert.That(items.Length, Is.EqualTo(expectedTotal));
            Assert.That(items.All(item => item.ContentTypeAlias == mediaTypeAlias), Is.True);
        });
    }

    [Test]
    public async Task Media_CanCombineParentAndMediaTypeFiltering()
    {
        IMedia root = MediaService.GetRootMedia().Last();
        Guid mediaTypeKey = MediaTypeService.Get("childMediaType")?.Key
                            ?? throw new InvalidOperationException("Could not find childMediaType");

        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: string.Empty,
            parentId: root.Key,
            contentTypeIds: [mediaTypeKey],
            trashed: null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(10));
            IMediaEntitySlim[] items = result.Items.OfType<IMediaEntitySlim>().ToArray();
            Assert.That(items.Length, Is.EqualTo(10));
            Assert.That(items.All(item => item.ContentTypeAlias is "childMediaType"), Is.True);
            Assert.That(items.All(item => item.ParentId == root.Id), Is.True);
        });
    }

    [TestCase(false, 22)]
    [TestCase(true, 11)]
    public async Task Media_CanFilterByTrashedState(bool trashed, int expectedTotal)
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: string.Empty,
            parentId: null,
            contentTypeIds: null,
            trashed: trashed);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(expectedTotal));
            Assert.That(result.Items.Count(), Is.EqualTo(expectedTotal));
            Assert.That(result.Items.All(item => item.Trashed == trashed), Is.True);
        });
    }

    [TestCase("single0root", false, 1)]
    [TestCase("single0root", true, 0)]
    [TestCase("single2root", false, 0)]
    [TestCase("single2root", true, 1)]
    [TestCase("single1child", false, 2)]
    [TestCase("single1child", true, 1)]
    [TestCase("oddeven1child", false, 10)]
    [TestCase("oddeven1child", true, 5)]
    public async Task Media_CanCombineQueryAndTrashedFilteringContent(string query, bool trashed, int expectedTotal)
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: query,
            parentId: null,
            contentTypeIds: null,
            trashed: trashed);

        Assert.That(expectedTotal, Is.EqualTo(result.Total));
    }

    [Test]
    public async Task Media_RespectsTextBoostingTiers_All()
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: "shared",
            parentId: null,
            contentTypeIds: null,
            trashed: false);

        Assert.That(result.Total, Is.EqualTo(22));
        Assert.That(result.Items.Take(2).All(c => c.Name!.Contains("Root")), Is.True);
        Assert.That(result.Items.Skip(2).All(c => c.Name!.Contains("Child")), Is.True);
    }

    [Test]
    public async Task Media_RespectsTextBoostingTiers_Single()
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: "shared1",
            parentId: null,
            contentTypeIds: null,
            trashed: false);

        Assert.That(result.Total, Is.EqualTo(11));
        Assert.That(result.Items.First().Name, Is.EqualTo("Root 1 shared shared1"));
        Assert.That(result.Items.Skip(1).All(c => c.Name!.Contains("Child")), Is.True);
    }
}
