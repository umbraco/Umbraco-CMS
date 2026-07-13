using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.BackOffice;

public partial class ContentSearchServiceTests
{
    [Test]
    public async Task Content_CanFindAllRootsWithoutQuery()
    {
        IContent[] contentAtRoot = ContentService.GetRootContent().OrderBy(content => content.SortOrder).ToArray();
        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync(null, null, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(3));
            Assert.That(result.Items.Count(), Is.EqualTo(3));

            IContent[] items = result.Items.OrderBy(item => item.SortOrder).ToArray();
            Assert.That(items[0].Key, Is.EqualTo(contentAtRoot[0].Key));
            Assert.That(items[1].Key, Is.EqualTo(contentAtRoot[1].Key));
            Assert.That(items[2].Key, Is.EqualTo(contentAtRoot[2].Key));
        });
    }

    [Test]
    public async Task Content_CanFindAllChildrenWithoutQuery()
    {
        IContent root = ContentService.GetRootContent().Last();
        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync(null, root.Key, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(10));
            Assert.That(result.Items.Count(), Is.EqualTo(10));
            Assert.That(result.Items.Select(item => item.Key), Is.Unique);
            Assert.That(result.Items.DistinctBy(item => item.ParentId).Single().ParentId, Is.EqualTo(root.Id));
        });
    }

    [Test]
    public async Task Content_CanFindAllRootsByNonDistinctQuery()
    {
        IContent[] contentAtRoot = ContentService.GetRootContent().OrderBy(content => content.SortOrder).ToArray();
        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync("title", null, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(3));
            Assert.That(result.Items.Count(), Is.EqualTo(3));

            IContent[] items = result.Items.OrderBy(item => item.SortOrder).ToArray();
            Assert.That(items[0].Key, Is.EqualTo(contentAtRoot[0].Key));
            Assert.That(items[1].Key, Is.EqualTo(contentAtRoot[1].Key));
            Assert.That(items[2].Key, Is.EqualTo(contentAtRoot[2].Key));
        });
    }

    [Test]
    public async Task Content_CanFindAllChildrenByQuery()
    {
        IContent root = ContentService.GetRootContent().Last();
        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync("title", root.Key, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(10));
            Assert.That(result.Items.Count(), Is.EqualTo(10));
            Assert.That(result.Items.Select(item => item.Key), Is.Unique);
            Assert.That(result.Items.DistinctBy(item => item.ParentId).Single().ParentId, Is.EqualTo(root.Id));
        });
    }

    [Test]
    public async Task Content_CanFindAllRootsByDistinctQuery()
    {
        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync("root", null, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(3));
            IContent[] items = result.Items.OrderBy(item => item.SortOrder).ToArray();
            Assert.That(items[0].Name, Is.EqualTo("Root 0 shared shared0"));
            Assert.That(items[1].Name, Is.EqualTo("Root 1 shared shared1"));
            Assert.That(items[2].Name, Is.EqualTo("Root 2 shared shared2"));
        });
    }

    [Test]
    public async Task Content_CanFindSingleRootByQuery()
    {
        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync("single1root", null, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Items.First().Name, Is.EqualTo("Root 1 shared shared1"));
        });
    }

    [Test]
    public async Task Content_CanFindSingleChildByQuery()
    {
        IContent root = ContentService.GetRootContent().Last();
        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync("single3child", root.Key, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Items.Single().Name, Is.EqualTo("Child 3"));
            Assert.That(result.Items.Single().ParentId, Is.EqualTo(root.Id));
        });
    }

    [Test]
    public async Task Content_CanFindMultipleChildrenByQuery()
    {
        IContent root = ContentService.GetRootContent().Last();
        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync("triple2child", root.Key, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(3));
            IContent[] items = result.Items.OrderBy(item => item.SortOrder).ToArray();
            Assert.That(items[0].Name, Is.EqualTo("Child 6"));
            Assert.That(items[1].Name, Is.EqualTo("Child 7"));
            Assert.That(items[2].Name, Is.EqualTo("Child 8"));
            Assert.That(result.Items.DistinctBy(item => item.ParentId).Single().ParentId, Is.EqualTo(root.Id));
        });
    }

    [Test]
    public async Task Content_CanFindRootByIdQuery()
    {
        IContent root = ContentService.GetRootContent().First();
        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync(root.Key.AsKeyword(), null, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Items.First().Key, Is.EqualTo(root.Key));
        });
    }

    [Test]
    public async Task Content_CanFindChildByIdQuery()
    {
        IContent root = ContentService.GetRootContent().First();
        IContent child = ContentService.GetPagedChildren(root.Id, 0, 10, out _).First();
        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync(child.Key.AsKeyword(), root.Key, null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Items.First().Key, Is.EqualTo(child.Key));
        });
    }

    [TestCase(Direction.Ascending)]
    [TestCase(Direction.Descending)]
    public async Task Content_CanSortAllChildrenByNameWithoutQuery(Direction direction)
    {
        IContent root = ContentService.GetRootContent().Last();
        IEnumerable<IContent> children = ContentService.GetPagedChildren(root.Id, 0, 10, out _);
        Guid[] expectedChildrenKeys = (direction is Direction.Ascending
                ? children.OrderBy(child => child.Name)
                : children.OrderByDescending(child => child.Name)
            ).Select(child => child.Key).ToArray();

        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync(null, root.Key, Ordering.By("name", direction));

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(10));
            Assert.That(result.Items.Count(), Is.EqualTo(10));
            CollectionAssert.AreEqual(result.Items.Select(item => item.Key), expectedChildrenKeys);
        });
    }

    [TestCase(Direction.Ascending)]
    [TestCase(Direction.Descending)]
    public async Task Content_CanSortAllChildrenByUpdateDateWithoutQuery(Direction direction)
    {
        IContent root = ContentService.GetRootContent().Last();
        IEnumerable<IContent> children = ContentService.GetPagedChildren(root.Id, 0, 10, out _);
        Guid[] expectedChildrenKeys = (direction is Direction.Ascending
                ? children.OrderBy(child => child.UpdateDate)
                : children.OrderByDescending(child => child.UpdateDate)
            ).Select(child => child.Key).ToArray();

        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync(null, root.Key, Ordering.By("updateDate", direction));

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(10));
            Assert.That(result.Items.Count(), Is.EqualTo(10));
            CollectionAssert.AreEqual(result.Items.Select(item => item.Key), expectedChildrenKeys);
        });
    }

    [TestCase(Direction.Ascending)]
    [TestCase(Direction.Descending)]
    public async Task Content_CanSortChildrenByNameWithQuery(Direction direction)
    {
        IContent root = ContentService.GetRootContent().Last();
        var expectedChildrenOrder = new[] { "Child 1", "Child 3", "Child 5", "Child 7", "Child 9" };
        if (direction is Direction.Descending)
        {
            expectedChildrenOrder = expectedChildrenOrder.Reverse().ToArray();
        }

        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync("oddeven1child", root.Key, Ordering.By("name", direction));

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(expectedChildrenOrder.Length));
            Assert.That(result.Items.Count(), Is.EqualTo(expectedChildrenOrder.Length));
            CollectionAssert.AreEqual(result.Items.Select(item => item.Name), expectedChildrenOrder);
        });
    }

    [TestCase(Direction.Ascending)]
    [TestCase(Direction.Descending)]
    public async Task Content_CanSortChildrenByUpdateDateWithQuery(Direction direction)
    {
        IContent root = ContentService.GetRootContent().Last();
        var expectedChildrenOrder = new[] { "Child 0", "Child 2", "Child 4", "Child 6", "Child 8" };
        if (direction is Direction.Descending)
        {
            expectedChildrenOrder = expectedChildrenOrder.Reverse().ToArray();
        }

        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync("oddeven0child", root.Key, Ordering.By("updateDate", direction));

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(expectedChildrenOrder.Length));
            Assert.That(result.Items.Count(), Is.EqualTo(expectedChildrenOrder.Length));
            CollectionAssert.AreEqual(result.Items.Select(item => item.Name), expectedChildrenOrder);
        });
    }

    [Test]
    public async Task Content_CannotFindChildrenWithoutParent()
    {
        PagedModel<IContent> result = await ContentSearchService.SearchChildrenAsync("triple2child", null, null);
        Assert.That(result.Total, Is.EqualTo(0));
    }
}
