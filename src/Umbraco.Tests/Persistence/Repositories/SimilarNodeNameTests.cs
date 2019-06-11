using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    public class SimilarNodeNameTests
    {
        [TestCase("Alpha", "Alpha", 0)]
        [TestCase("Alpha", "ALPHA", +1)] // case is important
        [TestCase("Alpha", "Bravo", -1)]
        [TestCase("Bravo", "Alpha", +1)]
        [TestCase("Alpha (1)", "Alpha (1)", 0)]
        [TestCase("Alpha", "Alpha (1)", -1)]
        [TestCase("Alpha (1)", "Alpha", +1)]
        [TestCase("Alpha (1)", "Alpha (2)", -1)]
        [TestCase("Alpha (2)", "Alpha (1)", +1)]
        [TestCase("Alpha (2)", "Alpha (10)", -1)] // this is the real stuff
        [TestCase("Alpha (10)", "Alpha (2)", +1)] // this is the real stuff
        [TestCase("Kilo", "Golf (2)", +1)]
        [TestCase("Kilo (1)", "Golf (2)", +1)]
        [TestCase("", "", 0)]
        [TestCase(null, null, 0)]
        public void ComparerTest(string name1, string name2, int expected)
        {
            var comparer = new SimilarNodeName.Comparer();

            var result = comparer.Compare(new SimilarNodeName { Name = name1 }, new SimilarNodeName { Name = name2 });
            if (expected == 0)
                Assert.AreEqual(0, result);
            else if (expected < 0)
                Assert.IsTrue(result < 0, "Expected <0 but was " + result);
            else if (expected > 0)
                Assert.IsTrue(result > 0, "Expected >0 but was " + result);
        }

        [Test]
        public void OrderByTest()
        {
            var names = new[]
            {
                new SimilarNodeName { Id = 1, Name = "Alpha (2)" },
                new SimilarNodeName { Id = 2, Name = "Alpha" },
                new SimilarNodeName { Id = 3, Name = "Golf" },
                new SimilarNodeName { Id = 4, Name = "Zulu" },
                new SimilarNodeName { Id = 5, Name = "Mike" },
                new SimilarNodeName { Id = 6, Name = "Kilo (1)" },
                new SimilarNodeName { Id = 7, Name = "Yankee" },
                new SimilarNodeName { Id = 8, Name = "Kilo" },
                new SimilarNodeName { Id = 9, Name = "Golf (2)" },
                new SimilarNodeName { Id = 10, Name = "Alpha (1)" },
            };

            var ordered = names.OrderBy(x => x, new SimilarNodeName.Comparer()).ToArray();

            var i = 0;
            Assert.AreEqual(2, ordered[i++].Id);
            Assert.AreEqual(10, ordered[i++].Id);
            Assert.AreEqual(1, ordered[i++].Id);
            Assert.AreEqual(3, ordered[i++].Id);
            Assert.AreEqual(9, ordered[i++].Id);
            Assert.AreEqual(8, ordered[i++].Id);
            Assert.AreEqual(6, ordered[i++].Id);
            Assert.AreEqual(5, ordered[i++].Id);
            Assert.AreEqual(7, ordered[i++].Id);
            Assert.AreEqual(4, ordered[i++].Id);
        }

        [TestCase(0, "Charlie", "Charlie")]
        [TestCase(0, "Zulu", "Zulu (1)")]
        [TestCase(0, "Golf", "Golf (1)")]
        [TestCase(0, "Kilo", "Kilo (2)")]
        [TestCase(0, "Alpha", "Alpha (3)")]
        [TestCase(0, "Kilo (1)", "Kilo (1) (1)")] // though... we might consider "Kilo (2)"
        [TestCase(6, "Kilo (1)", "Kilo (1)")] // because of the id
        [TestCase(0, "alpha", "alpha (3)")]
        [TestCase(0, "", " (1)")]
        [TestCase(0, null, " (1)")]
        public void Test(int nodeId, string nodeName, string expected)
        {
            var names = new[]
            {
                new SimilarNodeName { Id = 1, Name = "Alpha (2)" },
                new SimilarNodeName { Id = 2, Name = "Alpha" },
                new SimilarNodeName { Id = 3, Name = "Golf" },
                new SimilarNodeName { Id = 4, Name = "Zulu" },
                new SimilarNodeName { Id = 5, Name = "Mike" },
                new SimilarNodeName { Id = 6, Name = "Kilo (1)" },
                new SimilarNodeName { Id = 7, Name = "Yankee" },
                new SimilarNodeName { Id = 8, Name = "Kilo" },
                new SimilarNodeName { Id = 9, Name = "Golf (2)" },
                new SimilarNodeName { Id = 10, Name = "Alpha (1)" },
            };

            Assert.AreEqual(expected, SimilarNodeName.GetUniqueName(names, nodeId, nodeName));
        }
    }
}
