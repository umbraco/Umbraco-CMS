using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Search.Core.Extensions;

public partial class ContentExtensionsTests
{
    [TestCase("-1,2,3", 2)]
    [TestCase("-1,2,3,4", 2, 3)]
    [TestCase("-1,2,3,4,5,6,7,8", 2, 3, 4, 5, 6, 7)]
    [TestCase("-1,-2,-3,-4")]
    [TestCase("-1,2")]
    [TestCase("-1")]
    public void AncestorIds_ReturnsRelevantIdsInCorrectOrder(string contentPath, params int[] expectedAncestorIds)
    {
        var contentMock = new Mock<IContentBase>();
        contentMock.SetupGet(m => m.Id).Returns(int.Parse(contentPath.Split(',').Last()));
        contentMock.SetupGet(m => m.Path).Returns(contentPath);

        IEnumerable<int> result = contentMock.Object.AncestorIds();
        Assert.That(result.SequenceEqual(expectedAncestorIds ?? []) , Is.True);
    }
}
