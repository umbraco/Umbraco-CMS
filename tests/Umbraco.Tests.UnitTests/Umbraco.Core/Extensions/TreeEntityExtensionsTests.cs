using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models.Entities;
using Range = System.Range;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class TreeEntityExtensionsTests
{
    [TestCase("-1,1234", 0)]
    [TestCase("-1,1234,5678", 1)]
    [TestCase("-1,1234,5678,9012", 2)]
    [TestCase("-1,1234,5678,9012,2345", 3)]
    public void Parse_Ancestor_Ids_Excludes_Root_And_Self(string path, int expectedLength)
    {
        var entityMock = new Mock<ITreeEntity>();
        entityMock.SetupGet(m => m.Path).Returns(path);

        var expectedIds = path
            .Split(',')
            .Skip(1)
            .Take(Range.EndAt(Index.FromEnd(1)))
            .Select(int.Parse)
            .ToArray();
        var result = entityMock.Object.AncestorIds();
        Assert.AreEqual(expectedLength, result.Length);
        Assert.That(expectedIds, Is.EquivalentTo(result));
    }
}
