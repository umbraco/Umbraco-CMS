// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

/// <summary>
/// Unit tests for the EnumerableExtensions class.
/// </summary>
[TestFixture]
public class EnumerableExtensionsTests
{
    /// <summary>
    /// Tests the <c>UnsortedSequenceEqual</c> extension method to verify that it correctly determines whether two sequences contain the same elements, regardless of order.
    /// Scenarios include sequences with the same elements in different orders, sequences with different elements, and null sequence comparisons.
    /// </summary>
    [Test]
    public void Unsorted_Sequence_Equal()
    {
        var list1 = new[] { 1, 2, 3, 4, 5, 6 };
        var list2 = new[] { 6, 5, 3, 2, 1, 4 };
        var list3 = new[] { 6, 5, 4, 3, 2, 2 };

        Assert.IsTrue(list1.UnsortedSequenceEqual(list2));
        Assert.IsTrue(list2.UnsortedSequenceEqual(list1));
        Assert.IsFalse(list1.UnsortedSequenceEqual(list3));

        Assert.IsTrue(((IEnumerable<object>)null).UnsortedSequenceEqual(null));
        Assert.IsFalse(((IEnumerable<int>)null).UnsortedSequenceEqual(list1));
        Assert.IsFalse(list1.UnsortedSequenceEqual(null));
    }

    /// <summary>
    /// Tests the ContainsAll extension method to verify that it correctly determines if one collection contains all elements of another.
    /// </summary>
    [Test]
    public void Contains_All()
    {
        var list1 = new[] { 1, 2, 3, 4, 5, 6 };
        var list2 = new[] { 6, 5, 3, 2, 1, 4 };
        var list3 = new[] { 6, 5, 4, 3 };

        Assert.IsTrue(list1.ContainsAll(list2));
        Assert.IsTrue(list2.ContainsAll(list1));
        Assert.IsTrue(list1.ContainsAll(list3));
        Assert.IsFalse(list3.ContainsAll(list1));
    }

    /// <summary>
    /// Tests the SelectRecursive extension method with a simple hierarchy.
    /// </summary>
    [Test]
    public void SelectRecursive_2()
    {
        var hierarchy = new TestItem("1") { Children = new List<TestItem> { new("1.1"), new("1.2"), new("1.3") } };

        var selectRecursive = hierarchy.Children.SelectRecursive(x => x.Children);

        Assert.AreEqual(3, selectRecursive.Count());
    }

    /// <summary>
    /// Tests the <c>SelectRecursive</c> extension method to ensure it correctly enumerates all nested children in a hierarchical data structure.
    /// This test constructs a multi-level hierarchy and verifies that all descendant nodes are returned by <c>SelectRecursive</c>.
    /// </summary>
    [Test]
    public void SelectRecursive()
    {
        var hierarchy = new TestItem("1")
        {
            Children = new List<TestItem>
            {
                new("1.1")
                {
                    Children = new List<TestItem>
                    {
                        new("1.1.1")
                        {
                            Children = new List<TestItem>
                            {
                                new("1.1.1.1")
                                {
                                    Children = new List<TestItem>
                                    {
                                        new("1.1.1.1.1"), new("1.1.1.1.2"),
                                    },
                                },
                            },
                        },
                    },
                },
                new("1.2")
                {
                    Children = new List<TestItem>
                    {
                        new("1.2.1")
                        {
                            Children = new List<TestItem>
                            {
                                new("1.2.1.1") { Children = new List<TestItem>() },
                            },
                        },
                        new("1.2.2")
                        {
                            Children = new List<TestItem>
                            {
                                new("1.2.2.1") { Children = new List<TestItem>() },
                            },
                        },
                    },
                },
            },
        };

        var selectRecursive = hierarchy.Children.SelectRecursive(x => x.Children);
        Assert.AreEqual(10, selectRecursive.Count());
    }

    private class TestItem
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestItem"/> class.
    /// </summary>
    /// <param name="name">The name of the test item.</param>
        public TestItem(string name)
        {
            Children = Enumerable.Empty<TestItem>();
            Name = name;
        }

    /// <summary>
    /// Gets the name of the test item.
    /// </summary>
        public string Name { get; }

    /// <summary>
    /// Gets or sets the child items of this TestItem.
    /// </summary>
        public IEnumerable<TestItem> Children { get; set; }
    }

    /// <summary>
    /// Tests that the <c>InGroupsOf</c> extension method correctly groups all elements of a collection into groups of a specified size, and verifies that all original elements are present in the resulting groups.
    /// This includes testing with group sizes both smaller and larger than the collection.
    /// </summary>
    [Test]
    public void InGroupsOf_ReturnsAllElements()
    {
        var integers = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

        var groupsOfTwo = integers.InGroupsOf(2).ToArray();

        var flattened = groupsOfTwo.SelectMany(x => x).ToArray();

        Assert.That(groupsOfTwo.Length, Is.EqualTo(5));
        Assert.That(flattened.Length, Is.EqualTo(integers.Length));
        CollectionAssert.AreEquivalent(integers, flattened);

        var groupsOfMassive = integers.InGroupsOf(100).ToArray();
        Assert.That(groupsOfMassive.Length, Is.EqualTo(1));
        flattened = groupsOfMassive.SelectMany(x => x).ToArray();
        Assert.That(flattened.Length, Is.EqualTo(integers.Length));
        CollectionAssert.AreEquivalent(integers, flattened);
    }

    /// <summary>
    /// Tests that the InGroupsOf extension method can be enumerated multiple times and returns consistent results.
    /// </summary>
    [Test]
    public void InGroupsOf_CanRepeat()
    {
        var integers = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        var inGroupsOf = integers.InGroupsOf(2);
        Assert.AreEqual(5, inGroupsOf.Count());
        Assert.AreEqual(5, inGroupsOf.Count()); // again
    }

    /// <summary>
    /// Tests that the DistinctBy extension method returns distinct elements based on a key selector
    /// and that the iterator can be reset correctly to enumerate the sequence multiple times.
    /// </summary>
    [TestCase]
    public void DistinctBy_ReturnsDistinctElements_AndResetsIteratorCorrectly()
    {
        // Arrange
        var tuple1 = new Tuple<string, string>("fruit", "apple");
        var tuple2 = new Tuple<string, string>("fruit", "orange");
        var tuple3 = new Tuple<string, string>("fruit", "banana");
        var tuple4 = new Tuple<string, string>("fruit", "banana"); // Should be filtered out
        var list = new List<Tuple<string, string>> { tuple1, tuple2, tuple3, tuple4 };

        // Act
        var iteratorSource = list.DistinctBy(x => x.Item2).ToArray();

        // Assert
        // First check distinction
        Assert.AreEqual(3, iteratorSource.Count());

        // Check for iterator block mistakes - reset to original query first
        iteratorSource = list.DistinctBy(x => x.Item2).ToArray();
        Assert.AreEqual(iteratorSource.Length, iteratorSource.ToList().Count);
    }
}
