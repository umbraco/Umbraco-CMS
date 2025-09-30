using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class TreeEntityExtensionsTests
{
    [TestCase("-1,1234", new int[] { })]
    [TestCase("-1,1234,5678", new int[] { 1234 })]
    [TestCase("-1,1234,5678,9012", new int[] { 5678, 1234 })]
    [TestCase("-1,1234,5678,9012,2345", new int[] { 9012, 5678, 1234 })]
    public void Parse_Ancestor_Ids_Excludes_Root_And_Self(string path, int[] expectedIds)
    {
        var entityMock = new Mock<ITreeEntity>();
        entityMock.SetupGet(m => m.Path).Returns(path);

        var result = entityMock.Object.AncestorIds();
        Assert.AreEqual(expectedIds.Length, result.Length);
        Assert.That(expectedIds, Is.EquivalentTo(result));
    }
}
