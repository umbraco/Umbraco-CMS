// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs;

public class CacheInstructionsPruningJobTests
{
    private readonly Mock<IOptions<GlobalSettings>> _globalSettings = new(MockBehavior.Strict);
    private readonly Mock<ICacheInstructionRepository> _cacheInstructionRepository = new(MockBehavior.Strict);
    private readonly Mock<TimeProvider> _timeProvider = new(MockBehavior.Strict);

    [Test]
    public void Run_Period_Is_Retrieved_From_GlobalSettings()
    {
        var timeBetweenPruneOperations = TimeSpan.FromMinutes(2);
        var job = CreateCacheInstructionsPruningJob(timeBetweenPruneOperations);
        Assert.AreEqual(timeBetweenPruneOperations, job.Period, "The run period should be the same as 'TimeBetweenPruneOperations'.");
    }

    [Test]
    public async Task RunJobAsync_Calls_DeleteInstructionsOlderThan_With_Expected_Date()
    {
        var timeToRetainInstructions = TimeSpan.FromMinutes(30);
        var now = DateTime.UtcNow;
        var expectedPruneDate = now - timeToRetainInstructions;

        _timeProvider.Setup(tp => tp.GetUtcNow()).Returns(now);
        _cacheInstructionRepository.Setup(repo => repo
                .DeleteInstructionsOlderThan(expectedPruneDate));

        var job = CreateCacheInstructionsPruningJob(timeToRetainInstructions: timeToRetainInstructions);

        await job.RunJobAsync();

        _cacheInstructionRepository.Verify(repo => repo.DeleteInstructionsOlderThan(expectedPruneDate), Times.Once);
    }

    private CacheInstructionsPruningJob CreateCacheInstructionsPruningJob(
        TimeSpan? timeBetweenPruneOperations = null,
        TimeSpan? timeToRetainInstructions = null)
    {
        timeBetweenPruneOperations ??= TimeSpan.FromMinutes(5);
        timeToRetainInstructions ??= TimeSpan.FromMinutes(20);

        var globalSettings = new GlobalSettings
        {
            DatabaseServerMessenger = new DatabaseServerMessengerSettings
            {
                TimeBetweenPruneOperations = timeBetweenPruneOperations.Value,
                TimeToRetainInstructions = timeToRetainInstructions.Value,
            },
        };

        _globalSettings
            .Setup(g => g.Value)
            .Returns(globalSettings);

        return new CacheInstructionsPruningJob(_globalSettings.Object, _cacheInstructionRepository.Object, _timeProvider.Object);
    }
}
