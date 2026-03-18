using NUnit.Framework;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Collections;

/// <summary>
/// Contains unit tests for the <see cref="TopoGraph"/> class in the Umbraco.Core.Collections namespace.
/// </summary>
[TestFixture]
public class TopoGraphTests
{
    /// <summary>
    /// Verifies that the <see cref="TopoGraph{TKey, TValue}"/> correctly detects cyclic dependencies
    /// among items and throws an exception with the expected error message when a cycle is present.
    /// </summary>
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

    /// <summary>
    /// Tests that the TopoGraph correctly ignores cycles when throwOnCycle is false.
    /// </summary>
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

    /// <summary>
    /// Verifies that the <see cref="TopoGraph{TKey, TValue}"/> throws an exception when attempting to sort items with a missing dependency.
    /// Ensures that missing dependencies are correctly detected and reported as errors.
    /// </summary>
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

    /// <summary>
    /// Tests that the TopoGraph correctly ignores missing dependencies when throwOnMissing is false.
    /// </summary>
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

    /// <summary>
    /// Tests that the TopoGraph correctly orders items based on their dependencies.
    /// </summary>
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

    /// <summary>
    /// Tests that the TopoGraph returns items in reverse topological order when requested.
    /// </summary>
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

    /// <summary>
    /// Represents a test helper class used in <see cref="TopoGraphTests"/>.
    /// </summary>
    public class Thing
    {
    /// <summary>
    /// Gets or sets the identifier of the Thing.
    /// </summary>
        public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the thing.
    /// </summary>
        public string Name { get; set; }

    /// <summary>
    /// Gets the list of dependencies.
    /// </summary>
        public List<int> Dependencies { get; } = new List<int>();

    /// <summary>
    /// Adds dependencies to this Thing.
    /// </summary>
    /// <param name="dependencies">The dependencies to add.</param>
    /// <returns>The current Thing instance with added dependencies.</returns>
        public Thing Depends(params int[] dependencies)
        {
            Dependencies.AddRange(dependencies);
            return this;
        }
    }

}
