using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.BackOffice;

public partial class IndexedEntitySearchServiceTests
{
    [Test]
    public async Task Content_CanFindAll()
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
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
    public async Task Content_CanFindAllRootsByQuery()
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
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
            Assert.That(result.Items.Count(item => item.ParentId == Constants.System.RecycleBinContent), Is.EqualTo(1));
            Assert.That(result.Items.All(item => item.Name!.Contains("Root")), Is.True);
        });
    }

    [Test]
    public async Task Content_CanFindSpecificRootByQuery()
    {
        IContent root = ContentService.GetRootContent().OrderBy(content => content.SortOrder).Skip(1).First();
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
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
    public async Task Content_CanFindAllChildrenByQuery()
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
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
    public async Task Content_CanFindAllChildrenBelowParent()
    {
        IContent root = ContentService.GetRootContent().Last();
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
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
    public async Task Content_CanFindChildrenBelowParentByQuery()
    {
        IContent root = ContentService.GetRootContent().Last();
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
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

    [TestCase("rootContentType", null, 3)]
    [TestCase("rootContentType", false, 2)]
    [TestCase("rootContentType", true, 1)]
    [TestCase("childContentType", null, 30)]
    [TestCase("childContentType", false, 20)]
    [TestCase("childContentType", true, 10)]
    public async Task Content_CanFindAllByContentType(string contentTypeAlias, bool? trashed, int expectedTotal)
    {
        Guid contentTypeKey = ContentTypeService.Get(contentTypeAlias)?.Key
                              ?? throw new InvalidOperationException($"Could not find {contentTypeAlias}.");

        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
            query: string.Empty,
            parentId: null,
            contentTypeIds: [contentTypeKey],
            trashed: trashed);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(expectedTotal));
            IDocumentEntitySlim[] items = result.Items.OfType<IDocumentEntitySlim>().ToArray();
            Assert.That(items.Length, Is.EqualTo(expectedTotal));
            Assert.That(items.All(item => item.ContentTypeAlias == contentTypeAlias), Is.True);
        });
    }

    [Test]
    public async Task Content_CanCombineParentAndContentTypeFiltering()
    {
        IContent root = ContentService.GetRootContent().Last();
        Guid contentTypeKey = ContentTypeService.Get("childContentType")?.Key
                              ?? throw new InvalidOperationException("Could not find childContentType");

        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
            query: string.Empty,
            parentId: root.Key,
            contentTypeIds: [contentTypeKey],
            trashed: null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(10));
            IDocumentEntitySlim[] items = result.Items.OfType<IDocumentEntitySlim>().ToArray();
            Assert.That(items.Length, Is.EqualTo(10));
            Assert.That(items.All(item => item.ContentTypeAlias is "childContentType"), Is.True);
            Assert.That(items.All(item => item.ParentId == root.Id), Is.True);
        });
    }

    [TestCase(false, 22)]
    [TestCase(true, 11)]
    public async Task Content_CanFilterByTrashedState(bool trashed, int expectedTotal)
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
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
    public async Task Content_CanCombineQueryAndTrashedFilteringContent(string query, bool trashed, int expectedTotal)
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
            query: query,
            parentId: null,
            contentTypeIds: null,
            trashed: trashed);

        Assert.That(result.Total, Is.EqualTo(expectedTotal));
    }

    [Test]
    public async Task Content_RespectsTextBoostingTiers_All()
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
            query: "shared",
            parentId: null,
            contentTypeIds: null,
            trashed: false);

        Assert.That(result.Total, Is.EqualTo(22));
        Assert.That(result.Items.Take(2).All(c => c.Name!.Contains("Root")), Is.True);
        Assert.That(result.Items.Skip(2).All(c => c.Name!.Contains("Child")), Is.True);
    }

    [Test]
    public async Task Content_RespectsTextBoostingTiers_Single()
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
            query: "shared1",
            parentId: null,
            contentTypeIds: null,
            trashed: false);

        Assert.That(result.Total, Is.EqualTo(11));
        Assert.That(result.Items.First().Name, Is.EqualTo("Root 1 shared shared1"));
        Assert.That(result.Items.Skip(1).All(c => c.Name!.Contains("Child")), Is.True);
    }
}
