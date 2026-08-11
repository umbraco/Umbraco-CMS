using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.References;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.References;

[TestFixture]
public class ReferencedElementsServiceTests
{
    private Mock<IEntityService> _entityServiceMock = null!;
    private Mock<IRelationService> _relationServiceMock = null!;
    private Mock<IElementService> _elementServiceMock = null!;
    private static readonly Guid ParentKey = Guid.NewGuid();
    private const int ParentId = 1;

    [SetUp]
    public void SetUp()
    {
        _entityServiceMock = new Mock<IEntityService>();
        _relationServiceMock = new Mock<IRelationService>();
        _elementServiceMock = new Mock<IElementService>();

        _entityServiceMock
            .Setup(x => x.Get(ParentKey, UmbracoObjectTypes.Document))
            .Returns(Mock.Of<IEntitySlim>(x => x.Id == ParentId));

        _elementServiceMock
            .Setup(x => x.GetContentSchedulesByKeys(It.IsAny<Guid[]>()))
            .Returns(new Dictionary<Guid, IEnumerable<ContentSchedule>>());
    }

    private ReferencedElementsService CreateSut() =>
        new(_entityServiceMock.Object, _relationServiceMock.Object, _elementServiceMock.Object, TimeProvider.System);

    private static IElementEntitySlim CreateElement(string name, bool published, bool edited, bool trashed = false) =>
        Mock.Of<IElementEntitySlim>(x =>
            x.Id == Random.Shared.Next(1, int.MaxValue)
            && x.Key == Guid.NewGuid()
            && x.Name == name
            && x.Published == published
            && x.Edited == edited
            && x.Trashed == trashed
            && x.Variations == ContentVariation.Nothing
            && x.CultureNames == new Dictionary<string, string>());

    [Test]
    public async Task Returns_ContentNotFound_When_Parent_Does_Not_Exist()
    {
        _entityServiceMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<UmbracoObjectTypes>())).Returns((IEntitySlim?)null);

        Attempt<PagedModel<ReferencedElementWithPendingChanges>, GetReferencesOperationStatus> attempt =
            await CreateSut().GetPagedReferencedElementsWithPendingChangesAsync(ParentKey, UmbracoObjectTypes.Document, 0, 20);

        Assert.IsFalse(attempt.Success);
        Assert.AreEqual(GetReferencesOperationStatus.ContentNotFound, attempt.Status);
    }

    [Test]
    public async Task Filters_Out_Fully_Published_And_Trashed_Elements()
    {
        IElementEntitySlim published = CreateElement("Published", published: true, edited: false);
        IElementEntitySlim draft = CreateElement("Draft", published: false, edited: false);
        IElementEntitySlim pendingChanges = CreateElement("PendingChanges", published: true, edited: true);
        IElementEntitySlim trashed = CreateElement("Trashed", published: false, edited: false, trashed: true);

        SetupChildren(published, draft, pendingChanges, trashed);

        Attempt<PagedModel<ReferencedElementWithPendingChanges>, GetReferencesOperationStatus> attempt =
            await CreateSut().GetPagedReferencedElementsWithPendingChangesAsync(ParentKey, UmbracoObjectTypes.Document, 0, 20);

        Assert.IsTrue(attempt.Success);
        Assert.AreEqual(2, attempt.Result.Total);
        CollectionAssert.AreEquivalent(
            new[] { "Draft", "PendingChanges" },
            attempt.Result.Items.Select(x => x.Element.Name));
    }

    [Test]
    public async Task Take_Zero_Returns_Correct_Total_With_No_Items()
    {
        SetupChildren(
            CreateElement("Draft1", published: false, edited: false),
            CreateElement("Draft2", published: false, edited: false));

        Attempt<PagedModel<ReferencedElementWithPendingChanges>, GetReferencesOperationStatus> attempt =
            await CreateSut().GetPagedReferencedElementsWithPendingChangesAsync(ParentKey, UmbracoObjectTypes.Document, 0, 0);

        Assert.IsTrue(attempt.Success);
        Assert.AreEqual(2, attempt.Result.Total);
        Assert.IsEmpty(attempt.Result.Items);
    }

    [Test]
    public async Task Paging_Skip_And_Take_Are_Applied_In_Name_Order()
    {
        SetupChildren(
            CreateElement("Charlie", published: false, edited: false),
            CreateElement("Alpha", published: false, edited: false),
            CreateElement("Bravo", published: false, edited: false));

        Attempt<PagedModel<ReferencedElementWithPendingChanges>, GetReferencesOperationStatus> attempt =
            await CreateSut().GetPagedReferencedElementsWithPendingChangesAsync(ParentKey, UmbracoObjectTypes.Document, 1, 1);

        Assert.AreEqual(3, attempt.Result.Total);
        Assert.AreEqual(1, attempt.Result.Items.Count());
        Assert.AreEqual("Bravo", attempt.Result.Items.Single().Element.Name);
    }

    [Test]
    public async Task IsScheduled_True_Only_For_Future_Release_Schedule()
    {
        IElementEntitySlim futureRelease = CreateElement("FutureRelease", published: false, edited: false);
        IElementEntitySlim pastRelease = CreateElement("PastRelease", published: false, edited: false);
        IElementEntitySlim futureExpire = CreateElement("FutureExpire", published: false, edited: false);

        SetupChildren(futureRelease, pastRelease, futureExpire);

        _elementServiceMock
            .Setup(x => x.GetContentSchedulesByKeys(It.IsAny<Guid[]>()))
            .Returns(new Dictionary<Guid, IEnumerable<ContentSchedule>>
            {
                [futureRelease.Key] = [new ContentSchedule(string.Empty, DateTime.UtcNow.AddDays(1), ContentScheduleAction.Release)],
                [pastRelease.Key] = [new ContentSchedule(string.Empty, DateTime.UtcNow.AddDays(-1), ContentScheduleAction.Release)],
                [futureExpire.Key] = [new ContentSchedule(string.Empty, DateTime.UtcNow.AddDays(1), ContentScheduleAction.Expire)],
            });

        Attempt<PagedModel<ReferencedElementWithPendingChanges>, GetReferencesOperationStatus> attempt =
            await CreateSut().GetPagedReferencedElementsWithPendingChangesAsync(ParentKey, UmbracoObjectTypes.Document, 0, 20);

        Dictionary<string, bool> isScheduledByName = attempt.Result.Items.ToDictionary(x => x.Element.Name, x => x.IsScheduled);
        Assert.IsTrue(isScheduledByName["FutureRelease"]);
        Assert.IsFalse(isScheduledByName["PastRelease"]);
        Assert.IsFalse(isScheduledByName["FutureExpire"]);
    }

    [Test]
    public async Task Queries_Relation_Service_With_Both_Element_Relation_Type_Aliases()
    {
        SetupChildren();

        await CreateSut().GetPagedReferencedElementsWithPendingChangesAsync(ParentKey, UmbracoObjectTypes.Document, 0, 20);

        var expectedAliases = new[]
        {
            Constants.Conventions.RelationTypes.RelatedElementAlias,
            Constants.Conventions.RelationTypes.RelatedExternalBlockElementAlias,
        };

        _relationServiceMock.Verify(
            x => x.GetChildEntitiesByParentId(
                ParentId,
                It.Is<IEnumerable<string>>(aliases => aliases.SequenceEqual(expectedAliases)),
                UmbracoObjectTypes.Element),
            Times.Once);
    }

    private void SetupChildren(params IElementEntitySlim[] elements) =>
        _relationServiceMock
            .Setup(x => x.GetChildEntitiesByParentId(ParentId, It.IsAny<IEnumerable<string>>(), UmbracoObjectTypes.Element))
            .Returns(elements.Cast<IUmbracoEntity>());
}
