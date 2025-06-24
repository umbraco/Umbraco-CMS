// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs;

public class CacheInstructionsPruningJobTests
{
    private readonly Mock<IOptions<GlobalSettings>> _globalSettingsMock = new(MockBehavior.Strict);
    private readonly Mock<ICacheInstructionRepository> _cacheInstructionRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<ICoreScopeProvider> _scopeProviderMock = new(MockBehavior.Strict);
    private readonly Mock<TimeProvider> _timeProviderMock = new(MockBehavior.Strict);

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
        SetupScopeProviderMock();

        var timeToRetainInstructions = TimeSpan.FromMinutes(30);
        var now = DateTime.UtcNow;
        var expectedPruneDate = now - timeToRetainInstructions;

        _timeProviderMock.Setup(tp => tp.GetUtcNow()).Returns(now);
        _cacheInstructionRepositoryMock.Setup(repo => repo
                .DeleteInstructionsOlderThan(expectedPruneDate));

        var job = CreateCacheInstructionsPruningJob(timeToRetainInstructions: timeToRetainInstructions);

        await job.RunJobAsync();

        _cacheInstructionRepositoryMock.Verify(repo => repo.DeleteInstructionsOlderThan(expectedPruneDate), Times.Once);
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

        _globalSettingsMock
            .Setup(g => g.Value)
            .Returns(globalSettings);

        return new CacheInstructionsPruningJob(_globalSettingsMock.Object, _cacheInstructionRepositoryMock.Object, _scopeProviderMock.Object, _timeProviderMock.Object);
    }

    private void SetupScopeProviderMock() =>
        _scopeProviderMock
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());
}
