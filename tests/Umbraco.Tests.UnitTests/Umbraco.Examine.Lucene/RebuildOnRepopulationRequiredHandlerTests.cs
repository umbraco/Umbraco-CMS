using Examine;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Examine.Lucene;

[TestFixture]
public class RebuildOnRepopulationRequiredHandlerTests
{
    private const string IndexName = "TestIndex";

    [Test]
    public async Task Rebuild_Is_Requested_Immediately_When_No_Rebuild_Is_In_Progress()
    {
        var indexRebuilder = CreateIndexRebuilder(isRebuilding: () => false);
        RebuildOnRepopulationRequiredHandler sut = CreateHandler(indexRebuilder.Object, new FakeTimeProvider());

        await sut.RebuildAsync(IndexName);

        indexRebuilder.Verify(x => x.RebuildIndexAsync(IndexName, It.IsAny<TimeSpan?>(), It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task Rebuild_Is_Requested_Only_After_The_Rebuild_In_Progress_Has_Finished()
    {
        var isRebuilding = true;
        var indexRebuilder = CreateIndexRebuilder(isRebuilding: () => isRebuilding);
        var timeProvider = new FakeTimeProvider();
        RebuildOnRepopulationRequiredHandler sut = CreateHandler(indexRebuilder.Object, timeProvider);

        Task rebuild = sut.RebuildAsync(IndexName);
        timeProvider.Advance(TimeSpan.FromMinutes(1));

        Assert.IsFalse(rebuild.IsCompleted, "The rebuild was requested while another rebuild was in progress.");
        indexRebuilder.Verify(x => x.RebuildIndexAsync(It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()), Times.Never);

        isRebuilding = false;
        await AdvanceUntilCompletedAsync(timeProvider, rebuild);

        indexRebuilder.Verify(x => x.RebuildIndexAsync(IndexName, It.IsAny<TimeSpan?>(), It.IsAny<bool>()), Times.Once);
    }

    private static async Task AdvanceUntilCompletedAsync(FakeTimeProvider timeProvider, Task task)
    {
        for (var attempt = 0; attempt < 100 && task.IsCompleted is false; attempt++)
        {
            timeProvider.Advance(TimeSpan.FromSeconds(5));
            await Task.Delay(10);
        }

        await task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private static Mock<IIndexRebuilder> CreateIndexRebuilder(Func<bool> isRebuilding)
    {
        var indexRebuilder = new Mock<IIndexRebuilder>();
        indexRebuilder
            .Setup(x => x.IsRebuildingAsync(IndexName))
            .ReturnsAsync(() => isRebuilding());
        indexRebuilder
            .Setup(x => x.RebuildIndexAsync(IndexName, It.IsAny<TimeSpan?>(), It.IsAny<bool>()))
            .ReturnsAsync(Attempt.Succeed(IndexRebuildResult.Success));
        return indexRebuilder;
    }

    private static RebuildOnRepopulationRequiredHandler CreateHandler(IIndexRebuilder indexRebuilder, TimeProvider timeProvider)
        => new(
            Mock.Of<IExamineManager>(),
            indexRebuilder,
            NullLogger<RebuildOnRepopulationRequiredHandler>.Instance,
            timeProvider);
}
