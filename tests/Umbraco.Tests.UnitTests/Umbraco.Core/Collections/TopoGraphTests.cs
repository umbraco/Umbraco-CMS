using NUnit.Framework;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Collections;

[TestFixture]
public class TopoGraphTests
{
    [Test]
    public void CycleTest()
    {
        var graph = new TopoGraph<int, Thing>(x => x.Id, x => x.Dependencies);
        graph.AddItem(new Thing { Id = 1, Name = "One" }.Depends(3));
        graph.AddItem(new Thing { Id = 2, Name = "Two" }.Depends(1));
        graph.AddItem(new Thing { Id = 3, Name = "Three" }.Depends(2));

        try
        {
            var ordered = graph.GetSortedItems().ToArray();
            Assert.Fail("Expected: Exception.");
        }
        catch (Exception e)
        {
            Assert.IsTrue(e.Message.StartsWith(TopoGraph.CycleDependencyError));
        }
    }

    [Test]
    public void IgnoreCycleTest()
    {
        var graph = new TopoGraph<int, Thing>(x => x.Id, x => x.Dependencies);
        graph.AddItem(new Thing { Id = 1, Name = "One" }.Depends(3));
        graph.AddItem(new Thing { Id = 2, Name = "Two" }.Depends(1));
        graph.AddItem(new Thing { Id = 3, Name = "Three" }.Depends(2));

        var ordered = graph.GetSortedItems(throwOnCycle: false).ToArray();

        // default order is dependencies before item
        Assert.AreEqual(2, ordered[0].Id); // ignored cycle
        Assert.AreEqual(3, ordered[1].Id);
        Assert.AreEqual(1, ordered[2].Id);
    }

    [Test]
    public void MissingTest()
    {
        var graph = new TopoGraph<int, Thing>(x => x.Id, x => x.Dependencies);
        graph.AddItem(new Thing { Id = 1, Name = "One" }.Depends(4));
        graph.AddItem(new Thing { Id = 2, Name = "Two" }.Depends(1));
        graph.AddItem(new Thing { Id = 3, Name = "Three" }.Depends(2));

        try
        {
            var ordered = graph.GetSortedItems().ToArray();
            Assert.Fail("Expected: Exception.");
        }
        catch (Exception e)
        {
            Assert.IsTrue(e.Message.StartsWith(TopoGraph.MissingDependencyError));
        }
    }

    [Test]
    public void IgnoreMissingTest()
    {
        var graph = new TopoGraph<int, Thing>(x => x.Id, x => x.Dependencies);
        graph.AddItem(new Thing { Id = 1, Name = "One" }.Depends(4));
        graph.AddItem(new Thing { Id = 2, Name = "Two" }.Depends(1));
        graph.AddItem(new Thing { Id = 3, Name = "Three" }.Depends(2));

        var ordered = graph.GetSortedItems(throwOnMissing: false).ToArray();

        // default order is dependencies before item
        Assert.AreEqual(1, ordered[0].Id); // ignored dependency
        Assert.AreEqual(2, ordered[1].Id);
        Assert.AreEqual(3, ordered[2].Id);
    }

    [Test]
    public void OrderTest()
    {
        var graph = new TopoGraph<int, Thing>(x => x.Id, x => x.Dependencies);
        graph.AddItem(new Thing { Id = 1, Name = "One" });
        graph.AddItem(new Thing { Id = 2, Name = "Two" }.Depends(1));
        graph.AddItem(new Thing { Id = 3, Name = "Three" }.Depends(2));

        var ordered = graph.GetSortedItems().ToArray();

        // default order is dependencies before item
        Assert.AreEqual(1, ordered[0].Id);
        Assert.AreEqual(2, ordered[1].Id);
        Assert.AreEqual(3, ordered[2].Id);
    }

    [Test]
    public void ReverseTest()
    {
        var graph = new TopoGraph<int, Thing>(x => x.Id, x => x.Dependencies);
        graph.AddItem(new Thing { Id = 1, Name = "One" });
        graph.AddItem(new Thing { Id = 2, Name = "Two" }.Depends(1));
        graph.AddItem(new Thing { Id = 3, Name = "Three" }.Depends(2));

        var ordered = graph.GetSortedItems(reverse: true).ToArray();

        // reverse order is item before dependencies
        Assert.AreEqual(3, ordered[0].Id);
        Assert.AreEqual(2, ordered[1].Id);
        Assert.AreEqual(1, ordered[2].Id);
    }

    public class Thing
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<int> Dependencies { get; } = new List<int>();

        public Thing Depends(params int[] dependencies)
        {
            Dependencies.AddRange(dependencies);
            return this;
        }
    }

}
