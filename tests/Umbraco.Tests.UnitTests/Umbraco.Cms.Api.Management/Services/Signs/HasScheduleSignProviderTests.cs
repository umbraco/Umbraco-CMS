using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
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
        var idKeyMapMock = new Mock<IIdKeyMap>();

        var sut = new HasScheduleSignProvider(contentServiceMock.Object, idKeyMapMock.Object);
        Assert.IsTrue(sut.CanProvideSigns<DocumentTreeItemResponseModel>());
    }

    [Test]
    public void HasScheduleSignProvider_Can_Provide_Document_Collection_Signs()
    {
        var contentServiceMock = new Mock<IContentService>();
        var idKeyMapMock = new Mock<IIdKeyMap>();

        var sut = new HasScheduleSignProvider(contentServiceMock.Object, idKeyMapMock.Object);
        Assert.IsTrue(sut.CanProvideSigns<DocumentCollectionResponseModel>());
    }

    [Test]
    public void HasScheduleSignProvider_Can_Provide_Document_Item_Signs()
    {
        var contentServiceMock = new Mock<IContentService>();
        var idKeyMapMock = new Mock<IIdKeyMap>();

        var sut = new HasScheduleSignProvider(contentServiceMock.Object, idKeyMapMock.Object);
        Assert.IsTrue(sut.CanProvideSigns<DocumentItemResponseModel>());
    }

    [Test]
    public async Task HasScheduleSignProvider_Should_Populate_Document_Tree_Signs()
    {
        var entities = new List<EntitySlim>
        {
            new() { Key = Guid.NewGuid(), Name = "Item 1" }, new() { Key = Guid.NewGuid(), Name = "Item 2" },
        };

        var idKeyMapMock = new Mock<IIdKeyMap>();
        idKeyMapMock.Setup(x => x.GetIdForKey(entities[0].Key, UmbracoObjectTypes.Document))
            .Returns(Attempt.Succeed(1));
        idKeyMapMock.Setup(x => x.GetIdForKey(entities[1].Key, UmbracoObjectTypes.Document))
            .Returns(Attempt.Succeed(2));

        Guid[] keys = entities.Select(x => x.Key).ToArray();
        var contentServiceMock = new Mock<IContentService>();
        contentServiceMock
            .Setup(x => x.GetContentSchedulesByIds(keys))
            .Returns(CreateContentSchedules());


        var sut = new HasScheduleSignProvider(contentServiceMock.Object, idKeyMapMock.Object);

        var variant1 = new DocumentVariantItemResponseModel() { State = DocumentVariantState.Published, Name = "Test1", Culture = "en-EN" };
        var variant2 = new DocumentVariantItemResponseModel() { State = DocumentVariantState.Published, Name = "Test1", Culture = "da-DA" };
        var variant3 = new DocumentVariantItemResponseModel() { State = DocumentVariantState.PublishedPendingChanges, Name = "Test" };

        var viewModels = new List<DocumentTreeItemResponseModel>
        {
            new() { Id = entities[0].Key, Variants = [variant1, variant2] }, new() { Id = entities[1].Key, Variants = [variant3] },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Variants.FirstOrDefault(x => x.Culture == "da-DA").Signs.Count(), 0);
        Assert.AreEqual(viewModels[0].Variants.FirstOrDefault(x => x.Culture == "en-EN").Signs.Count(), 1);
        Assert.AreEqual(viewModels[1].Variants.First().Signs.Count(), 1);

        var signModel = viewModels[0].Variants.First().Signs.First();
        Assert.AreEqual("Umb.ScheduledForPublish", signModel.Alias);
    }

    [Test]
    public async Task HasScheduleSignProvider_Should_Populate_Document_Collection_Signs()
    {
        var entities = new List<EntitySlim>
        {
            new() { Key = Guid.NewGuid(), Name = "Item 1" }, new() { Key = Guid.NewGuid(), Name = "Item 2" },
        };

        var idKeyMapMock = new Mock<IIdKeyMap>();
        idKeyMapMock.Setup(x => x.GetIdForKey(entities[0].Key, UmbracoObjectTypes.Document))
            .Returns(Attempt.Succeed(1));
        idKeyMapMock.Setup(x => x.GetIdForKey(entities[1].Key, UmbracoObjectTypes.Document))
            .Returns(Attempt.Succeed(2));

        Guid[] keys = entities.Select(x => x.Key).ToArray();
        var contentServiceMock = new Mock<IContentService>();
        contentServiceMock
            .Setup(x => x.GetContentSchedulesByIds(keys))
            .Returns(CreateContentSchedules());

        var sut = new HasScheduleSignProvider(contentServiceMock.Object, idKeyMapMock.Object);

        var variant1 = new DocumentVariantResponseModel() { State = DocumentVariantState.Published, Name = "Test1", Culture = "en-EN" };
        var variant2 = new DocumentVariantResponseModel() { State = DocumentVariantState.Published, Name = "Test1", Culture = "da-DA" };
        var variant3 = new DocumentVariantResponseModel() { State = DocumentVariantState.PublishedPendingChanges, Name = "Test" };

        var viewModels = new List<DocumentCollectionResponseModel>
        {
            new() { Id = entities[0].Key, Variants = [variant1, variant2] }, new() { Id = entities[1].Key, Variants = [variant3] },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Variants.FirstOrDefault(x => x.Culture == "da-DA").Signs.Count(), 0);
        Assert.AreEqual(viewModels[0].Variants.FirstOrDefault(x => x.Culture == "en-EN").Signs.Count(), 1);
        Assert.AreEqual(viewModels[1].Variants.First().Signs.Count(), 1);

        var signModel = viewModels[0].Variants.First().Signs.First();
        Assert.AreEqual("Umb.ScheduledForPublish", signModel.Alias);
    }

    [Test]
    public async Task HasScheduleSignProvider_Should_Populate_Document_Item_Signs()
    {
        var entities = new List<EntitySlim>
        {
            new() { Key = Guid.NewGuid(), Name = "Item 1" }, new() { Key = Guid.NewGuid(), Name = "Item 2" },
        };

        var idKeyMapMock = new Mock<IIdKeyMap>();
        idKeyMapMock.Setup(x => x.GetIdForKey(entities[0].Key, UmbracoObjectTypes.Document))
            .Returns(Attempt.Succeed(1));
        idKeyMapMock.Setup(x => x.GetIdForKey(entities[1].Key, UmbracoObjectTypes.Document))
            .Returns(Attempt.Succeed(2));

        Guid[] keys = entities.Select(x => x.Key).ToArray();
        var contentServiceMock = new Mock<IContentService>();
        contentServiceMock
            .Setup(x => x.GetContentSchedulesByIds(keys))
            .Returns(CreateContentSchedules());

        var sut = new HasScheduleSignProvider(contentServiceMock.Object, idKeyMapMock.Object);

        var variant1 = new DocumentVariantItemResponseModel() { State = DocumentVariantState.Published, Name = "Test1", Culture = "en-EN" };
        var variant2 = new DocumentVariantItemResponseModel() { State = DocumentVariantState.Published, Name = "Test1", Culture = "da-DA" };
        var variant3 = new DocumentVariantItemResponseModel() { State = DocumentVariantState.PublishedPendingChanges, Name = "Test" };

        var viewModels = new List<DocumentItemResponseModel>
        {
            new() { Id = entities[0].Key, Variants = [variant1, variant2] }, new() { Id = entities[1].Key, Variants = [variant3] },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Variants.FirstOrDefault(x => x.Culture == "da-DA").Signs.Count(), 0);
        Assert.AreEqual(viewModels[0].Variants.FirstOrDefault(x => x.Culture == "en-EN").Signs.Count(), 1);
        Assert.AreEqual(viewModels[1].Variants.First().Signs.Count(), 1);

        var signModel = viewModels[0].Variants.First().Signs.First();
        Assert.AreEqual("Umb.ScheduledForPublish", signModel.Alias);
    }

    private Dictionary<int, IEnumerable<ContentSchedule>> CreateContentSchedules()
    {
        Dictionary<int, IEnumerable<ContentSchedule>> contentSchedules = new Dictionary<int, IEnumerable<ContentSchedule>>();

        contentSchedules.Add(1, [
            new ContentSchedule("en-EN", DateTime.Now.AddDays(1), ContentScheduleAction.Release), // Scheduled for release
            new ContentSchedule("da-DA", DateTime.Now.AddDays(-1), ContentScheduleAction.Release) // Not Scheduled for release
        ]);
        contentSchedules.Add(2, [
            new ContentSchedule("*", DateTime.Now.AddDays(1), ContentScheduleAction.Release) // Scheduled for release
        ]);

        return contentSchedules;
    }
}
