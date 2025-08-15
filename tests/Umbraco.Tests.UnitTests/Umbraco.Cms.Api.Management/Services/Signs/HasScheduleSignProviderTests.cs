using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Signs;

[TestFixture]
internal class HasScheduleSignProviderTests
{
    [Test]
    public async Task HasScheduleSignProvider_Can_Provide_Tree_Signs()
    {
        var contentServiceMock = new Mock<IContentService>();

        var sut = new HasScheduleSignProvider(contentServiceMock.Object);
        Assert.IsTrue(sut.CanProvideSigns<DocumentTreeItemResponseModel>());
    }

    [Test]
    public async Task HasScheduleSignProvider_Can_Provide_Collection_Signs()
    {
        var contentServiceMock = new Mock<IContentService>();

        var sut = new HasScheduleSignProvider(contentServiceMock.Object);
        Assert.IsTrue(sut.CanProvideSigns<DocumentCollectionResponseModel>());
    }

    [Test]
    public async Task HasScheduleSignProvider_Can_Provide_Plain_Signs()
    {
        var contentServiceMock = new Mock<IContentService>();

        var sut = new HasScheduleSignProvider(contentServiceMock.Object);
        Assert.IsTrue(sut.CanProvideSigns<DocumentItemResponseModel>());
    }

    [Test]
    public async Task HasScheduleSignProvider_Should_Populate_Tree_Signs()
    {
        var entities = new List<EntitySlim>
        {
            new() { Key = Guid.NewGuid(), Name = "Item 1" }, new() { Key = Guid.NewGuid(), Name = "Item 2" },
        };

        var contentServiceMock = new Mock<IContentService>();
        contentServiceMock
            .Setup(x => x.GetScheduledContentKeys(It.IsAny<IEnumerable<Guid>>()))
            .Returns([entities[1].Key]);
        var sut = new HasScheduleSignProvider(contentServiceMock.Object);

        var viewModels = new List<DocumentTreeItemResponseModel>
        {
            new() { Id = entities[0].Key }, new() { Id = entities[1].Key },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Signs.Count(), 1);

        var signModel = viewModels[1].Signs.First();
        Assert.AreEqual("Umb.ScheduledForPublish", signModel.Alias);
    }

    [Test]
    public async Task HasScheduleSignProvider_Should_Populate_Collection_Signs()
    {
        var entities = new List<EntitySlim>
        {
            new() { Key = Guid.NewGuid(), Name = "Item 1" }, new() { Key = Guid.NewGuid(), Name = "Item 2" },
        };

        var contentServiceMock = new Mock<IContentService>();
        contentServiceMock
            .Setup(x => x.GetScheduledContentKeys(It.IsAny<IEnumerable<Guid>>()))
            .Returns([entities[1].Key]);
        var sut = new HasScheduleSignProvider(contentServiceMock.Object);

        var viewModels = new List<DocumentCollectionResponseModel>
        {
            new() { Id = entities[0].Key }, new() { Id = entities[1].Key },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Signs.Count(), 1);

        var signModel = viewModels[1].Signs.First();
        Assert.AreEqual("Umb.ScheduledForPublish", signModel.Alias);
    }

    [Test]
    public async Task HasScheduleSignProvider_Should_Populate_Plain_Signs()
    {
        var entities = new List<EntitySlim>
        {
            new() { Key = Guid.NewGuid(), Name = "Item 1" }, new() { Key = Guid.NewGuid(), Name = "Item 2" },
        };

        var contentServiceMock = new Mock<IContentService>();
        contentServiceMock
            .Setup(x => x.GetScheduledContentKeys(It.IsAny<IEnumerable<Guid>>()))
            .Returns([entities[1].Key]);
        var sut = new HasScheduleSignProvider(contentServiceMock.Object);

        var viewModels = new List<DocumentItemResponseModel>
        {
            new() { Id = entities[0].Key }, new() { Id = entities[1].Key },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Signs.Count(), 1);

        var signModel = viewModels[1].Signs.First();
        Assert.AreEqual("Umb.ScheduledForPublish", signModel.Alias);
    }
}
