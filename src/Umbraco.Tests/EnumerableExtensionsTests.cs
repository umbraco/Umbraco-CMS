using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Tests;
using umbraco.BusinessLogic;

namespace Umbraco.Tests
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {

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

        [Test]
        public void Contains_All()
        {
            var list1 = new[] {1, 2, 3, 4, 5, 6};
            var list2 = new[] {6, 5, 3, 2, 1, 4};
            var list3 = new[] {6, 5, 4, 3};

            Assert.IsTrue(list1.ContainsAll(list2));
            Assert.IsTrue(list2.ContainsAll(list1));
            Assert.IsTrue(list1.ContainsAll(list3));
            Assert.IsFalse(list3.ContainsAll(list1));
        }

        [Test]
        public void Flatten_List_2()
        {
            var hierarchy = new TestItem()
                {
                    Children = new List<TestItem>()
                        {
                            new TestItem(),
                            new TestItem(),
                            new TestItem()
                        }
                };

            var flattened = hierarchy.Children.FlattenList(x => x.Children);

            Assert.AreEqual(3, flattened.Count());
        }

        [Test]
        public void Flatten_List()
        {
            var hierarchy = new TestItem()
                {
                    Children = new List<TestItem>()
	                    {
	                        new TestItem()
	                            {
	                                Children = new List<TestItem>()
	                                    {
	                                        new TestItem()
	                                            {
	                                                Children = new List<TestItem>()
	                                                    {
	                                                        new TestItem()
	                                                            {
	                                                                Children = new List<TestItem>()
	                                                                    {
	                                                                        new TestItem(),
                                                                            new TestItem()
	                                                                    }
	                                                            }
	                                                    }
	                                            }
	                                    }
	                            },
	                        new TestItem()
	                            {
	                                Children = new List<TestItem>()
	                                    {
	                                        new TestItem()
	                                            {
	                                                Children = new List<TestItem>()
	                                                    {
	                                                        new TestItem()
	                                                            {
	                                                                Children = new List<TestItem>()
	                                                            }
	                                                    }
	                                            },
	                                        new TestItem()
	                                            {
	                                                Children = new List<TestItem>()
	                                                    {
	                                                        new TestItem()
	                                                            {
	                                                                Children = new List<TestItem>()
	                                                            }
	                                                    }
	                                            }
	                                    }
	                            },
	                    }
                };

            var flattened = hierarchy.Children.FlattenList(x => x.Children);

            Assert.AreEqual(10, flattened.Count());
        }

        private class TestItem
        {
            public TestItem()
            {
                Children = Enumerable.Empty<TestItem>();
            }
            public IEnumerable<TestItem> Children { get; set; }
        }

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

        [TestCase]
        public void DistinctBy_ReturnsDistinctElements_AndResetsIteratorCorrectly()
        {
            // Arrange
            var tuple1 = new System.Tuple<string, string>("fruit", "apple");
            var tuple2 = new System.Tuple<string, string>("fruit", "orange");
            var tuple3 = new System.Tuple<string, string>("fruit", "banana");
            var tuple4 = new System.Tuple<string, string>("fruit", "banana"); // Should be filtered out
            var list = new List<System.Tuple<string, string>>()
                {
                    tuple1,
                    tuple2,
                    tuple3,
                    tuple4
                };

            // Act
            var iteratorSource = list.DistinctBy(x => x.Item2);

            // Assert
            // First check distinction
            Assert.AreEqual(3, iteratorSource.Count());

            // Check for iterator block mistakes - reset to original query first
            iteratorSource = list.DistinctBy(x => x.Item2);
            Assert.AreEqual(iteratorSource.Count(), iteratorSource.ToList().Count());
        }
    }
}