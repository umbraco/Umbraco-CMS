using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Flags;

[TestFixture]
internal class HasScheduleFlagProviderTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IElementService> _elementServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        _contentServiceMock = new Mock<IContentService>();
        _elementServiceMock = new Mock<IElementService>();
    }

    [Test]
    public void HasScheduleFlagProvider_Can_Provide_Document_Tree_Flags()
    {
        var sut = CreateSut();
        Assert.IsTrue(sut.CanProvideFlags<DocumentTreeItemResponseModel>());
    }

    [Test]
    public void HasScheduleFlagProvider_Can_Provide_Document_Collection_Flags()
    {
        var sut = CreateSut();
        Assert.IsTrue(sut.CanProvideFlags<DocumentCollectionResponseModel>());
    }

    [Test]
    public void HasScheduleFlagProvider_Can_Provide_Document_Item_Flags()
    {
        var sut = CreateSut();
        Assert.IsTrue(sut.CanProvideFlags<DocumentItemResponseModel>());
    }

    [Test]
    public void HasScheduleFlagProvider_Can_Provide_Element_Tree_Flags()
    {
        var sut = CreateSut();
        Assert.IsTrue(sut.CanProvideFlags<ElementTreeItemResponseModel>());
    }

    [Test]
    public void HasScheduleFlagProvider_Can_Provide_Element_Item_Flags()
    {
        var sut = CreateSut();
        Assert.IsTrue(sut.CanProvideFlags<ElementItemResponseModel>());
    }

    [Test]
    public async Task HasScheduleFlagProvider_Should_Populate_Document_Tree_Flags()
    {
        Guid key1 = Guid.NewGuid();
        Guid key2 = Guid.NewGuid();

        SetupContentScheduleMock(key1, CreateScheduleCollection(
            new ContentSchedule("en-EN", DateTime.Now.AddDays(1), ContentScheduleAction.Release),
            new ContentSchedule("da-DA", DateTime.Now.AddDays(-1), ContentScheduleAction.Release)));
        SetupContentScheduleMock(key2, CreateScheduleCollection(
            new ContentSchedule("*", DateTime.Now.AddDays(1), ContentScheduleAction.Release)));

        var sut = CreateSut();

        var variant1 = new DocumentVariantItemResponseModel
        {
            State = DocumentVariantState.Published,
            Name = "Test1",
            Culture = "en-EN",
        };
        var variant2 = new DocumentVariantItemResponseModel
        {
            State = DocumentVariantState.Published,
            Name = "Test1",
            Culture = "da-DA",
        };
        var variant3 = new DocumentVariantItemResponseModel
        {
            State = DocumentVariantState.PublishedPendingChanges,
            Name = "Test",
        };

        var viewModels = new List<DocumentTreeItemResponseModel>
        {
            new() { Id = key1, Variants = [variant1, variant2] },
            new() { Id = key2, Variants = [variant3] },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Variants.First(x => x.Culture == "da-DA").Flags.Count(), 0);
        Assert.AreEqual(viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.Count(), 1);
        Assert.AreEqual(viewModels[1].Variants.First().Flags.Count(), 1);

        var flagModel = viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.First();
        Assert.AreEqual("Umb.ScheduledForPublish", flagModel.Alias);
    }

    [Test]
    public async Task HasScheduleFlagProvider_Should_Populate_Document_Collection_Flags()
    {
        Guid key1 = Guid.NewGuid();
        Guid key2 = Guid.NewGuid();

        SetupContentScheduleMock(
            key1,
            CreateScheduleCollection(
                new ContentSchedule("en-EN", DateTime.Now.AddDays(1), ContentScheduleAction.Release),
                new ContentSchedule("da-DA", DateTime.Now.AddDays(-1), ContentScheduleAction.Release)));
        SetupContentScheduleMock(
            key2,
            CreateScheduleCollection(
                new ContentSchedule("*", DateTime.Now.AddDays(1), ContentScheduleAction.Release)));

        var sut = CreateSut();

        var variant1 = new DocumentVariantResponseModel
        {
            State = DocumentVariantState.Published,
            Name = "Test1",
            Culture = "en-EN",
        };
        var variant2 = new DocumentVariantResponseModel
        {
            State = DocumentVariantState.Published,
            Name = "Test1",
            Culture = "da-DA",
        };
        var variant3 = new DocumentVariantResponseModel
        {
            State = DocumentVariantState.PublishedPendingChanges,
            Name = "Test",
        };

        var viewModels = new List<DocumentCollectionResponseModel>
        {
            new()
            {
                Id = key1,
                Variants =
                [
                    variant1,
                    variant2
                ],
            }, new()
            {
                Id = key2,
                Variants =
                [
                    variant3
                ],
            },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Variants.First(x => x.Culture == "da-DA").Flags.Count(), 0);
        Assert.AreEqual(viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.Count(), 1);
        Assert.AreEqual(viewModels[1].Variants.First().Flags.Count(), 1);

        var flagModel = viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.First();
        Assert.AreEqual("Umb.ScheduledForPublish", flagModel.Alias);
    }

    [Test]
    public async Task HasScheduleFlagProvider_Should_Populate_Document_Item_Flags()
    {
        Guid key1 = Guid.NewGuid();
        Guid key2 = Guid.NewGuid();

        SetupContentScheduleMock(
            key1,
            CreateScheduleCollection(
                new ContentSchedule("en-EN", DateTime.Now.AddDays(1), ContentScheduleAction.Release),
                new ContentSchedule("da-DA", DateTime.Now.AddDays(-1), ContentScheduleAction.Release)));
        SetupContentScheduleMock(
            key2,
            CreateScheduleCollection(
                new ContentSchedule("*", DateTime.Now.AddDays(1), ContentScheduleAction.Release)));

        var sut = CreateSut();

        var variant1 = new DocumentVariantItemResponseModel
        {
            State = DocumentVariantState.Published,
            Name = "Test1",
            Culture = "en-EN",
        };
        var variant2 = new DocumentVariantItemResponseModel
        {
            State = DocumentVariantState.Published,
            Name = "Test1",
            Culture = "da-DA",
        };
        var variant3 = new DocumentVariantItemResponseModel
        {
            State = DocumentVariantState.PublishedPendingChanges,
            Name = "Test",
        };

        var viewModels = new List<DocumentItemResponseModel>
        {
            new() { Id = key1, Variants = [variant1, variant2] },
            new() { Id = key2, Variants = [variant3] },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Variants.First(x => x.Culture == "da-DA").Flags.Count(), 0);
        Assert.AreEqual(viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.Count(), 1);
        Assert.AreEqual(viewModels[1].Variants.First().Flags.Count(), 1);

        var flagModel = viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.First();
        Assert.AreEqual("Umb.ScheduledForPublish", flagModel.Alias);
    }

    [Test]
    public async Task HasScheduleFlagProvider_Should_Populate_Element_Tree_Flags()
    {
        Guid key1 = Guid.NewGuid();
        Guid key2 = Guid.NewGuid();

        SetupElementScheduleMock(key1, CreateScheduleCollection(
            new ContentSchedule("en-EN", DateTime.Now.AddDays(1), ContentScheduleAction.Release),
            new ContentSchedule("da-DA", DateTime.Now.AddDays(-1), ContentScheduleAction.Release)));
        SetupElementScheduleMock(key2, CreateScheduleCollection(
            new ContentSchedule("*", DateTime.Now.AddDays(1), ContentScheduleAction.Release)));

        var sut = CreateSut();

        var variant1 = new ElementVariantItemResponseModel
        {
            State = DocumentVariantState.Published,
            Name = "Test1",
            Culture = "en-EN",
        };
        var variant2 = new ElementVariantItemResponseModel
        {
            State = DocumentVariantState.Published,
            Name = "Test1",
            Culture = "da-DA",
        };
        var variant3 = new ElementVariantItemResponseModel
        {
            State = DocumentVariantState.PublishedPendingChanges,
            Name = "Test",
        };

        var viewModels = new List<ElementTreeItemResponseModel>
        {
            new() { Id = key1, Variants = [variant1, variant2] },
            new() { Id = key2, Variants = [variant3] },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Variants.First(x => x.Culture == "da-DA").Flags.Count(), 0);
        Assert.AreEqual(viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.Count(), 1);
        Assert.AreEqual(viewModels[1].Variants.First().Flags.Count(), 1);

        var flagModel = viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.First();
        Assert.AreEqual("Umb.ScheduledForPublish", flagModel.Alias);
    }

    [Test]
    public async Task HasScheduleFlagProvider_Should_Populate_Element_Item_Flags()
    {
        Guid key1 = Guid.NewGuid();
        Guid key2 = Guid.NewGuid();

        SetupElementScheduleMock(key1, CreateScheduleCollection(
            new ContentSchedule("en-EN", DateTime.Now.AddDays(1), ContentScheduleAction.Release),
            new ContentSchedule("da-DA", DateTime.Now.AddDays(-1), ContentScheduleAction.Release)));
        SetupElementScheduleMock(key2, CreateScheduleCollection(
            new ContentSchedule("*", DateTime.Now.AddDays(1), ContentScheduleAction.Release)));

        var sut = CreateSut();

        var variant1 = new ElementVariantItemResponseModel
        {
            State = DocumentVariantState.Published,
            Name = "Test1",
            Culture = "en-EN",
        };
        var variant2 = new ElementVariantItemResponseModel
        {
            State = DocumentVariantState.Published,
            Name = "Test1",
            Culture = "da-DA",
        };
        var variant3 = new ElementVariantItemResponseModel
        {
            State = DocumentVariantState.PublishedPendingChanges,
            Name = "Test",
        };

        var viewModels = new List<ElementItemResponseModel>
        {
            new() { Id = key1, Variants = [variant1, variant2] },
            new() { Id = key2, Variants = [variant3] },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Variants.First(x => x.Culture == "da-DA").Flags.Count(), 0);
        Assert.AreEqual(viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.Count(), 1);
        Assert.AreEqual(viewModels[1].Variants.First().Flags.Count(), 1);

        var flagModel = viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.First();
        Assert.AreEqual("Umb.ScheduledForPublish", flagModel.Alias);
    }

    [Test]
    public async Task HasScheduleFlagProvider_Should_Not_Populate_Element_Flags_When_No_Schedules()
    {
        Guid key1 = Guid.NewGuid();

        SetupElementScheduleMock(key1, new ContentScheduleCollection());

        var sut = CreateSut();

        var variant1 = new ElementVariantItemResponseModel
        {
            State = DocumentVariantState.Published,
            Name = "Test1",
            Culture = "en-EN",
        };

        var viewModels = new List<ElementTreeItemResponseModel>
        {
            new() { Id = key1, Variants = [variant1], },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Variants.First().Flags.Count(), 0);
    }

    private HasScheduleFlagProvider CreateSut()
        => new(_contentServiceMock.Object, _elementServiceMock.Object);

    private void SetupContentScheduleMock(Guid key, ContentScheduleCollection scheduleCollection) =>
        _contentServiceMock
            .Setup(x => x.GetContentScheduleByContentId(key))
            .Returns(scheduleCollection);

    private void SetupElementScheduleMock(Guid key, ContentScheduleCollection scheduleCollection) =>
        _elementServiceMock
            .Setup(x => x.GetContentScheduleByContentId(key))
            .Returns(scheduleCollection);

    private static ContentScheduleCollection CreateScheduleCollection(params ContentSchedule[] schedules)
    {
        var collection = new ContentScheduleCollection();
        foreach (ContentSchedule schedule in schedules)
        {
            collection.Add(schedule);
        }

        return collection;
    }
}
