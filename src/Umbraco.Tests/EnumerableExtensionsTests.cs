using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Tests;
using umbraco.BusinessLogic;
using Umbraco.Core.Resolving;

namespace Umbraco.Tests
{
	[TestFixture]
    public class EnumerableExtensionsTests
    {
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