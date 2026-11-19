// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs;

[TestFixture]
public class DistributedBackgroundJobHostedServiceTests
{
    private static readonly TimeSpan _testTimeout = TimeSpan.FromSeconds(10);

    [Test]
    public async Task Retries_Read_Only_Check_When_It_Throws()
    {
        var databaseReadOnlyAccessor = new Mock<IDatabaseReadOnlyAccessor>();
        databaseReadOnlyAccessor
            .SetupSequence(x => x.IsReadOnly())
            .Throws(new InvalidOperationException("The database is unavailable."))
            .Returns(false);

        var jobsEnsured = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var distributedJobService = new Mock<IDistributedJobService>();
        distributedJobService
            .Setup(x => x.EnsureJobsAsync())
            .Callback(jobsEnsured.SetResult)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(distributedJobService.Object, databaseReadOnlyAccessor.Object);

        await sut.StartAsync(CancellationToken.None);
        await Task.WhenAny(jobsEnsured.Task, sut.ExecuteTask!, Task.Delay(_testTimeout));
        await sut.StopAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(sut.ExecuteTask!.IsFaulted, Is.False);
            Assert.That(jobsEnsured.Task.IsCompleted, Is.True);
            databaseReadOnlyAccessor.Verify(x => x.IsReadOnly(), Times.Exactly(2));
        });
    }

    [Test]
    public async Task Runs_Jobs_When_Ensuring_Jobs_Throws()
    {
        var databaseReadOnlyAccessor = new Mock<IDatabaseReadOnlyAccessor>();
        databaseReadOnlyAccessor.Setup(x => x.IsReadOnly()).Returns(false);

        var jobsTaken = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var distributedJobService = new Mock<IDistributedJobService>();
        distributedJobService
            .Setup(x => x.EnsureJobsAsync())
            .ThrowsAsync(new InvalidOperationException("The jobs could not be ensured."));
        distributedJobService
            .Setup(x => x.TryTakeRunnableAsync())
            .Callback(() => jobsTaken.TrySetResult())
            .ReturnsAsync((IDistributedBackgroundJob?)null);

        var sut = CreateSut(distributedJobService.Object, databaseReadOnlyAccessor.Object);

        await sut.StartAsync(CancellationToken.None);
        await Task.WhenAny(jobsTaken.Task, sut.ExecuteTask!, Task.Delay(_testTimeout));
        await sut.StopAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(sut.ExecuteTask!.IsFaulted, Is.False);
            Assert.That(jobsTaken.Task.IsCompleted, Is.True);
            distributedJobService.Verify(x => x.EnsureJobsAsync(), Times.Once);
        });
    }

    [Test]
    public async Task Does_Not_Ensure_Jobs_When_Database_Is_Read_Only()
    {
        var databaseReadOnlyAccessor = new Mock<IDatabaseReadOnlyAccessor>();
        databaseReadOnlyAccessor.Setup(x => x.IsReadOnly()).Returns(true);
        var distributedJobService = new Mock<IDistributedJobService>();

        var sut = CreateSut(distributedJobService.Object, databaseReadOnlyAccessor.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!.WaitAsync(_testTimeout);

        distributedJobService.Verify(x => x.EnsureJobsAsync(), Times.Never);
        distributedJobService.Verify(x => x.TryTakeRunnableAsync(), Times.Never);
    }

    private static DistributedBackgroundJobHostedService CreateSut(
        IDistributedJobService distributedJobService,
        IDatabaseReadOnlyAccessor databaseReadOnlyAccessor)
    {
        var runtimeState = new Mock<IRuntimeState>();
        runtimeState.Setup(x => x.Level).Returns(RuntimeLevel.Run);

        var settings = new DistributedJobSettings
        {
            Delay = TimeSpan.Zero,
            Period = TimeSpan.FromMilliseconds(1),
        };
        var settingsMonitor = new Mock<IOptionsMonitor<DistributedJobSettings>>();
        settingsMonitor.Setup(x => x.CurrentValue).Returns(settings);

        return new DistributedBackgroundJobHostedService(
            NullLogger<DistributedBackgroundJobHostedService>.Instance,
            runtimeState.Object,
            distributedJobService,
            settingsMonitor.Object,
            databaseReadOnlyAccessor);
    }
}
