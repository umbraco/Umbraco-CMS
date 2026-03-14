using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

/// <summary>
/// Unit tests for the TreeEntityExtensions class.
/// </summary>
[TestFixture]
public class TreeEntityExtensionsTests
{
    /// <summary>
    /// Tests that parsing ancestor IDs from a path excludes the root and the current entity itself.
    /// </summary>
    /// <param name="path">The path string containing ancestor IDs separated by commas.</param>
    /// <param name="expectedIds">The expected array of ancestor IDs excluding root and self.</param>
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
