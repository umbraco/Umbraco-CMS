
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Flags;

[TestFixture]
internal class HasDocumentScheduleFlagProviderTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private TimeProvider _timeProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _contentServiceMock = new Mock<IContentService>();
        _timeProvider = TimeProvider.System;
    }

    [Test]
    public void Can_Provide_Document_Tree_Flags()
    {
        var sut = CreateSut();
        Assert.IsTrue(sut.CanProvideFlags<DocumentTreeItemResponseModel>());
    }

    [Test]
    public void Can_Provide_Document_Collection_Flags()
    {
        var sut = CreateSut();
        Assert.IsTrue(sut.CanProvideFlags<DocumentCollectionResponseModel>());
    }

    [Test]
    public void Can_Provide_Document_Item_Flags()
    {
        var sut = CreateSut();
        Assert.IsTrue(sut.CanProvideFlags<DocumentItemResponseModel>());
    }

    [Test]
    public void Cannot_Provide_Element_Flags()
    {
        var sut = CreateSut();
        Assert.IsFalse(sut.CanProvideFlags<ElementTreeItemResponseModel>());
    }

    [Test]
    public async Task Should_Populate_Document_Tree_Flags()
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

        Assert.AreEqual(0, viewModels[0].Variants.First(x => x.Culture == "da-DA").Flags.Count());
        Assert.AreEqual(1, viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.Count());
        Assert.AreEqual(1, viewModels[1].Variants.First().Flags.Count());

        var flagModel = viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.First();
        Assert.AreEqual("Umb.ScheduledForPublish", flagModel.Alias);
    }

    [Test]
    public async Task Should_Populate_Document_Collection_Flags()
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
                Variants = [variant1, variant2],
            },
            new()
            {
                Id = key2,
                Variants = [variant3],
            },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.AreEqual(0, viewModels[0].Variants.First(x => x.Culture == "da-DA").Flags.Count());
        Assert.AreEqual(1, viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.Count());
        Assert.AreEqual(1, viewModels[1].Variants.First().Flags.Count());

        var flagModel = viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.First();
        Assert.AreEqual("Umb.ScheduledForPublish", flagModel.Alias);
    }

    [Test]
    public async Task Should_Populate_Document_Item_Flags()
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

        Assert.AreEqual(0, viewModels[0].Variants.First(x => x.Culture == "da-DA").Flags.Count());
        Assert.AreEqual(1, viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.Count());
        Assert.AreEqual(1, viewModels[1].Variants.First().Flags.Count());

        var flagModel = viewModels[0].Variants.First(x => x.Culture == "en-EN").Flags.First();
        Assert.AreEqual("Umb.ScheduledForPublish", flagModel.Alias);
    }

    [Test]
    public async Task Should_Use_Batch_Schedule_Retrieval()
    {
        Guid key1 = Guid.NewGuid();
        Guid key2 = Guid.NewGuid();

        SetupBatchScheduleMock(new Dictionary<Guid, IEnumerable<ContentSchedule>>());

        var sut = CreateSut();

        var viewModels = new List<DocumentTreeItemResponseModel>
        {
            new() { Id = key1, Variants = [new DocumentVariantItemResponseModel { State = DocumentVariantState.Published, Name = "Test1" }] },
            new() { Id = key2, Variants = [new DocumentVariantItemResponseModel { State = DocumentVariantState.Published, Name = "Test2" }] },
        };

        await sut.PopulateFlagsAsync(viewModels);

        _contentServiceMock.Verify(
            x => x.GetContentSchedulesByKeys(It.Is<Guid[]>(k => k.Length == 2 && k.Contains(key1) && k.Contains(key2))),
            Times.Once);
        _contentServiceMock.Verify(
            x => x.GetContentScheduleByContentId(It.IsAny<Guid>()),
            Times.Never);
    }

    private HasDocumentScheduleFlagProvider CreateSut()
        => new(_contentServiceMock.Object, _timeProvider);

    private void SetupBatchScheduleMock(IDictionary<Guid, IEnumerable<ContentSchedule>> schedules) =>
        _contentServiceMock
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
