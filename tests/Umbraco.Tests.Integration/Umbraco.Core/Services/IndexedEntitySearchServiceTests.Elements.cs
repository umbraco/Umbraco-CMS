using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class IndexedEntitySearchServiceTests
{
    [Test]
    public async Task Elements_CanFindAll()
    {
        PagedModel<IEntitySlim> result = await SearchElementsAsync(string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(33));
            Assert.That(result.Items.Count(), Is.EqualTo(33));
            Assert.That(result.Items.Select(item => item.Key), Is.Unique);
            Assert.That(result.Items.All(item => item is IElementEntitySlim), Is.True);
            Assert.That(result.Items.DistinctBy(item => item.Trashed).Count(), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Elements_CanFindByName()
    {
        PagedModel<IEntitySlim> result = await SearchElementsAsync("Root element 1");

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Items.Single().Name, Is.EqualTo("Root element 1"));
            Assert.That(result.Items.Single().ParentId, Is.EqualTo(Constants.System.Root));
        });
    }

    [Test]
    public async Task Elements_CanFindByPropertyValue()
    {
        PagedModel<IEntitySlim> result = await SearchElementsAsync("single3libchild");

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(3));
            Assert.That(result.Items.All(item => item.Name == "Element 3"), Is.True);
            Assert.That(result.Items.Select(item => item.ParentId), Is.EquivalentTo(ElementContainers.Select(container => container.Id)));
        });
    }

    [TestCase(null, 3)]
    [TestCase(false, 2)]
    [TestCase(true, 1)]
    public async Task Elements_CanFilterByTrashed(bool? trashed, int expectedTotal)
    {
        PagedModel<IEntitySlim> result = await SearchElementsAsync("single3libchild", trashed: trashed);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(expectedTotal));
            if (trashed.HasValue)
            {
                Assert.That(result.Items.All(item => item.Trashed == trashed.Value), Is.True);
            }
        });
    }

    [Test]
    public async Task Elements_CanFilterByParentContainer()
    {
        EntityContainer container = ElementContainers.First();

        PagedModel<IEntitySlim> result = await SearchElementsAsync(string.Empty, parentId: container.Key);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(10));
            Assert.That(result.Items.All(item => item.ParentId == container.Id), Is.True);
        });
    }

    [Test]
    public async Task Elements_CanFilterByElementType()
    {
        PagedModel<IEntitySlim> result = await SearchElementsAsync(string.Empty, contentTypeIds: [RootLibraryElementType.Key]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(3));
            Assert.That(result.Items.OfType<IContentEntitySlim>().All(item => item.ContentTypeKey == RootLibraryElementType.Key), Is.True);
        });
    }

    [Test]
    public async Task Elements_CanPage()
    {
        PagedModel<IEntitySlim> firstPage = await SearchElementsAsync("libchild", trashed: false, skip: 0, take: 15);
        PagedModel<IEntitySlim> secondPage = await SearchElementsAsync("libchild", trashed: false, skip: 15, take: 15);

        Assert.Multiple(() =>
        {
            Assert.That(firstPage.Total, Is.EqualTo(20));
            Assert.That(secondPage.Total, Is.EqualTo(20));
            Assert.That(firstPage.Items.Count(), Is.EqualTo(15));
            Assert.That(secondPage.Items.Count(), Is.EqualTo(5));
            Assert.That(firstPage.Items.Select(item => item.Key).Intersect(secondPage.Items.Select(item => item.Key)), Is.Empty);
        });
    }

    [Test]
    public async Task Elements_AreNotFoundByDocumentOrMediaSearch()
    {
        foreach (UmbracoObjectTypes objectType in new[] { UmbracoObjectTypes.Document, UmbracoObjectTypes.Media })
        {
            PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
                objectType,
                query: "libchild",
                parentId: null,
                contentTypeIds: null,
                trashed: null);

            Assert.That(result.Total, Is.Zero, $"Library elements were found by a {objectType} search.");
        }
    }

    [Test]
    public async Task Elements_SearchDoesNotFindDocumentsOrMedia()
    {
        // "single1root" is a document and media property value, and no element property value
        PagedModel<IEntitySlim> result = await SearchElementsAsync("single1root");

        Assert.That(result.Total, Is.Zero);
    }

    private async Task<PagedModel<IEntitySlim>> SearchElementsAsync(
        string query,
        Guid? parentId = null,
        IEnumerable<Guid>? contentTypeIds = null,
        bool? trashed = null,
        int skip = 0,
        int take = 100)
        => await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Element,
            query,
            parentId,
            contentTypeIds,
            trashed,
            skip: skip,
            take: take);
}
