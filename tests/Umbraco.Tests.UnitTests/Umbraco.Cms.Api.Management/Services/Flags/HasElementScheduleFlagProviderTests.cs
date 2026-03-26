
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Flags;

[TestFixture]
internal class HasElementScheduleFlagProviderTests
{
    private Mock<IElementService> _elementServiceMock = null!;
    private TimeProvider _timeProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _elementServiceMock = new Mock<IElementService>();
        _timeProvider = TimeProvider.System;
    }

    [Test]
    public void Can_Provide_Element_Tree_Flags()
    {
        var sut = CreateSut();
        Assert.IsTrue(sut.CanProvideFlags<ElementTreeItemResponseModel>());
    }

    [Test]
    public void Can_Provide_Element_Item_Flags()
    {
        var sut = CreateSut();
        Assert.IsTrue(sut.CanProvideFlags<ElementItemResponseModel>());
    }

    [Test]
    public void Cannot_Provide_Document_Flags()
    {
        var sut = CreateSut();
        Assert.IsFalse(sut.CanProvideFlags<DocumentTreeItemResponseModel>());
    }

    [Test]
    public async Task Should_Populate_Element_Tree_Flags()
    {
        Guid key1 = Guid.NewGuid();
        Guid key2 = Guid.NewGuid();

        SetupBatchScheduleMock(new Dictionary<Guid, IEnumerable<ContentSchedule>>
        {
            [key1] =
            [
                new ContentSchedule("en-EN", DateTime.Now.AddDays(1), ContentScheduleAction.Release),
                new ContentSchedule("da-DA", DateTime.Now.AddDays(-1), ContentScheduleAction.Release),
            ],
            [key2] =
            [
                new ContentSchedule("*", DateTime.Now.AddDays(1), ContentScheduleAction.Release),
            ],
        });

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

        Assert.AreEqual(0, viewModels[0].Variants.First(x => x.Culture == "da-DA").Flags.Count());
        Assert.AreEqual(1, viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.Count());
        Assert.AreEqual(1, viewModels[1].Variants.First().Flags.Count());

        var flagModel = viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.First();
        Assert.AreEqual("Umb.ScheduledForPublish", flagModel.Alias);
    }

    [Test]
    public async Task Should_Populate_Element_Item_Flags()
    {
        Guid key1 = Guid.NewGuid();
        Guid key2 = Guid.NewGuid();

        SetupBatchScheduleMock(new Dictionary<Guid, IEnumerable<ContentSchedule>>
        {
            [key1] =
            [
                new ContentSchedule("en-EN", DateTime.Now.AddDays(1), ContentScheduleAction.Release),
                new ContentSchedule("da-DA", DateTime.Now.AddDays(-1), ContentScheduleAction.Release),
            ],
            [key2] =
            [
                new ContentSchedule("*", DateTime.Now.AddDays(1), ContentScheduleAction.Release),
            ],
        });

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

        Assert.AreEqual(0, viewModels[0].Variants.First(x => x.Culture == "da-DA").Flags.Count());
        Assert.AreEqual(1, viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.Count());
        Assert.AreEqual(1, viewModels[1].Variants.First().Flags.Count());

        var flagModel = viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.First();
        Assert.AreEqual("Umb.ScheduledForPublish", flagModel.Alias);
    }

    [Test]
    public async Task Should_Not_Populate_Flags_When_No_Schedules()
    {
        Guid key1 = Guid.NewGuid();

        SetupBatchScheduleMock(new Dictionary<Guid, IEnumerable<ContentSchedule>>
        {
            [key1] = [],
        });

        var sut = CreateSut();

        var variant1 = new ElementVariantItemResponseModel
        {
            State = DocumentVariantState.Published,
            Name = "Test1",
            Culture = "en-EN",
        };

        var viewModels = new List<ElementTreeItemResponseModel>
        {
            new() { Id = key1, Variants = [variant1] },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.AreEqual(0, viewModels[0].Variants.First().Flags.Count());
    }

    [Test]
    public async Task Should_Use_Batch_Schedule_Retrieval()
    {
        Guid key1 = Guid.NewGuid();
        Guid key2 = Guid.NewGuid();

        SetupBatchScheduleMock(new Dictionary<Guid, IEnumerable<ContentSchedule>>());

        var sut = CreateSut();

        var viewModels = new List<ElementTreeItemResponseModel>
        {
            new() { Id = key1, Variants = [new ElementVariantItemResponseModel { State = DocumentVariantState.Published, Name = "Test1" }] },
            new() { Id = key2, Variants = [new ElementVariantItemResponseModel { State = DocumentVariantState.Published, Name = "Test2" }] },
        };

        await sut.PopulateFlagsAsync(viewModels);

        _elementServiceMock.Verify(
            x => x.GetContentSchedulesByKeys(It.Is<Guid[]>(k => k.Length == 2 && k.Contains(key1) && k.Contains(key2))),
            Times.Once);
        _elementServiceMock.Verify(
            x => x.GetContentScheduleByContentId(It.IsAny<Guid>()),
            Times.Never);
    }

    private HasElementScheduleFlagProvider CreateSut()
        => new(_elementServiceMock.Object, _timeProvider);

    private void SetupBatchScheduleMock(IDictionary<Guid, IEnumerable<ContentSchedule>> schedules) =>
        _elementServiceMock
            .Setup(x => x.GetContentSchedulesByKeys(It.IsAny<Guid[]>()))
            .Returns((Guid[] keys) =>
            {
                var result = new Dictionary<Guid, IEnumerable<ContentSchedule>>();
                foreach (Guid key in keys)
                {
                    if (schedules.TryGetValue(key, out IEnumerable<ContentSchedule>? s))
                    {
                        result[key] = s;
                    }
                }

                return result;
            });
}
