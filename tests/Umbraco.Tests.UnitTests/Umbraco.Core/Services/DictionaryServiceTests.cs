// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class DictionaryServiceTests
{
    [Test]
    public async Task CalculatePathAsync_NullParentId_ReturnsRootPath()
    {
        var dictionaryItemService = new Mock<IDictionaryItemService>();
        var sut = new DictionaryService(dictionaryItemService.Object);

        var result = await sut.CalculatePathAsync(null, 42);

        Assert.That(result, Is.EqualTo("-1,42"));
        dictionaryItemService.Verify(x => x.GetAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task CalculatePathAsync_RootParent_ReturnsTwoLevelPath()
    {
        var parentKey = Guid.NewGuid();
        var dictionaryItemService = new Mock<IDictionaryItemService>();
        dictionaryItemService
            .Setup(x => x.GetAsync(parentKey))
            .ReturnsAsync(Mock.Of<IDictionaryItem>(d => d.Id == 100 && d.ParentId == null));

        var sut = new DictionaryService(dictionaryItemService.Object);

        var result = await sut.CalculatePathAsync(parentKey, 42);

        Assert.That(result, Is.EqualTo("-1,100,42"));
    }

    [Test]
    public async Task CalculatePathAsync_NestedParents_ReturnsFullHierarchy()
    {
        var grandparentKey = Guid.NewGuid();
        var parentKey = Guid.NewGuid();

        var dictionaryItemService = new Mock<IDictionaryItemService>();
        dictionaryItemService
            .Setup(x => x.GetAsync(parentKey))
            .ReturnsAsync(Mock.Of<IDictionaryItem>(d => d.Id == 100 && d.ParentId == grandparentKey));
        dictionaryItemService
            .Setup(x => x.GetAsync(grandparentKey))
            .ReturnsAsync(Mock.Of<IDictionaryItem>(d => d.Id == 50 && d.ParentId == null));

        var sut = new DictionaryService(dictionaryItemService.Object);

        var result = await sut.CalculatePathAsync(parentKey, 42);

        Assert.That(result, Is.EqualTo("-1,50,100,42"));
    }

    [Test]
    public async Task CalculatePathAsync_ParentNotFound_StopsAtMissingNode()
    {
        var parentKey = Guid.NewGuid();
        var dictionaryItemService = new Mock<IDictionaryItemService>();
        dictionaryItemService
            .Setup(x => x.GetAsync(parentKey))
            .ReturnsAsync((IDictionaryItem?)null);

        var sut = new DictionaryService(dictionaryItemService.Object);

        var result = await sut.CalculatePathAsync(parentKey, 42);

        Assert.That(result, Is.EqualTo("-1,42"));
    }
}
