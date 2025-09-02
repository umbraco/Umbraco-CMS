using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
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
    public void HasScheduleSignProvider_Can_Provide_Document_Tree_Signs()
    {
        var contentServiceMock = new Mock<IContentService>();

        var sut = new HasScheduleSignProvider(contentServiceMock.Object);
        Assert.IsTrue(sut.CanProvideSigns<DocumentTreeItemResponseModel>());
    }

    [Test]
    public void HasScheduleSignProvider_Can_Provide_Document_Collection_Signs()
    {
        var contentServiceMock = new Mock<IContentService>();

        var sut = new HasScheduleSignProvider(contentServiceMock.Object);
        Assert.IsTrue(sut.CanProvideSigns<DocumentCollectionResponseModel>());
    }

    [Test]
    public void HasScheduleSignProvider_Can_Provide_Document_Item_Signs()
    {
        var contentServiceMock = new Mock<IContentService>();

        var sut = new HasScheduleSignProvider(contentServiceMock.Object);
        Assert.IsTrue(sut.CanProvideSigns<DocumentItemResponseModel>());
    }

    [Test]
    public async Task HasScheduleSignProvider_Should_Populate_Document_Tree_Signs()
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

        var variant1 = new DocumentVariantItemResponseModel() { State = DocumentVariantState.Published, Name = "Test" };
        var variant2 = new DocumentVariantItemResponseModel() { State = DocumentVariantState.PublishedPendingChanges, Name = "Test" };

        var viewModels = new List<DocumentTreeItemResponseModel>
        {
            new() { Id = entities[0].Key, Variants = [variant1] }, new() { Id = entities[1].Key, Variants = [variant2] },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Variants.First().Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Variants.First().Signs.Count(), 1);

        var signModel = viewModels[1].Variants.First().Signs.First();
        Assert.AreEqual("Umb.ScheduledForPublish", signModel.Alias);
    }

    [Test]
    public async Task HasScheduleSignProvider_Should_Populate_Document_Collection_Signs()
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

        var variant1 = new DocumentVariantResponseModel() { State = DocumentVariantState.Published, Name = "Test" };
        var variant2 = new DocumentVariantResponseModel() { State = DocumentVariantState.PublishedPendingChanges, Name = "Test" };

        var viewModels = new List<DocumentCollectionResponseModel>
        {
            new() { Id = entities[0].Key, Variants = [variant1] }, new() { Id = entities[1].Key, Variants = [variant2] },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Variants.First().Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Variants.First().Signs.Count(), 1);

        var signModel = viewModels[1].Variants.First().Signs.First();
        Assert.AreEqual("Umb.ScheduledForPublish", signModel.Alias);
    }

    [Test]
    public async Task HasScheduleSignProvider_Should_Populate_Document_Item_Signs()
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

        var variant1 = new DocumentVariantItemResponseModel() { State = DocumentVariantState.Published, Name = "Test" };
        var variant2 = new DocumentVariantItemResponseModel() { State = DocumentVariantState.PublishedPendingChanges, Name = "Test" };

        var viewModels = new List<DocumentItemResponseModel>
        {
            new() { Id = entities[0].Key, Variants = [variant1] }, new() { Id = entities[1].Key, Variants = [variant2] },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Variants.First().Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Variants.First().Signs.Count(), 1);

        var signModel = viewModels[1].Variants.First().Signs.First();
        Assert.AreEqual("Umb.ScheduledForPublish", signModel.Alias);
    }
}
