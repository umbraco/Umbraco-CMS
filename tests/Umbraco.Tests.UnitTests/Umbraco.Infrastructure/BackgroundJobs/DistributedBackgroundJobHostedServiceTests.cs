// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
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

    [TestCase(ServerRole.Subscriber)]
    [TestCase(ServerRole.Unknown)]
    public async Task Does_Not_Ensure_Or_Take_Jobs_On_Role_That_Does_Not_Run_Jobs(ServerRole serverRole)
    {
        const int TicksToObserve = 5;

        var databaseReadOnlyAccessor = new Mock<IDatabaseReadOnlyAccessor>();
        databaseReadOnlyAccessor.Setup(x => x.IsReadOnly()).Returns(false);
        var distributedJobService = new Mock<IDistributedJobService>();

        var roleChecks = 0;
        var ticksObserved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var serverRoleAccessor = new Mock<IServerRoleAccessor>();
        serverRoleAccessor
            .Setup(x => x.CurrentServerRole)
            .Returns(() =>
            {
                if (Interlocked.Increment(ref roleChecks) >= TicksToObserve)
                {
                    ticksObserved.TrySetResult();
                }

                return serverRole;
            });

        var sut = CreateSut(distributedJobService.Object, databaseReadOnlyAccessor.Object, serverRoleAccessor.Object);

        await sut.StartAsync(CancellationToken.None);
        await Task.WhenAny(ticksObserved.Task, sut.ExecuteTask!, Task.Delay(_testTimeout));
        await sut.StopAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(sut.ExecuteTask!.IsFaulted, Is.False);
            Assert.That(ticksObserved.Task.IsCompleted, Is.True);
            distributedJobService.Verify(x => x.EnsureJobsAsync(), Times.Never);
            distributedJobService.Verify(x => x.TryTakeRunnableAsync(), Times.Never);
        });
    }

    [TestCase(ServerRole.Subscriber)]
    [TestCase(ServerRole.Unknown)]
    public async Task Takes_Jobs_Once_Server_Role_Changes_To_One_That_Runs_Jobs(ServerRole initialServerRole)
    {
        const int InitialRoleTicks = 3;

        var databaseReadOnlyAccessor = new Mock<IDatabaseReadOnlyAccessor>();
        databaseReadOnlyAccessor.Setup(x => x.IsReadOnly()).Returns(false);

        var roleChecks = 0;
        var serverRoleAccessor = new Mock<IServerRoleAccessor>();
        serverRoleAccessor
            .Setup(x => x.CurrentServerRole)
            .Returns(() => Interlocked.Increment(ref roleChecks) <= InitialRoleTicks
                ? initialServerRole
                : ServerRole.SchedulingPublisher);

        var roleChecksWhenJobsTaken = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var distributedJobService = new Mock<IDistributedJobService>();
        distributedJobService.Setup(x => x.EnsureJobsAsync()).Returns(Task.CompletedTask);
        distributedJobService
            .Setup(x => x.TryTakeRunnableAsync())
            .Callback(() => roleChecksWhenJobsTaken.TrySetResult(Volatile.Read(ref roleChecks)))
            .ReturnsAsync((IDistributedBackgroundJob?)null);

        var sut = CreateSut(distributedJobService.Object, databaseReadOnlyAccessor.Object, serverRoleAccessor.Object);

        await sut.StartAsync(CancellationToken.None);
        await Task.WhenAny(roleChecksWhenJobsTaken.Task, sut.ExecuteTask!, Task.Delay(_testTimeout));
        await sut.StopAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(sut.ExecuteTask!.IsFaulted, Is.False);
            Assert.That(roleChecksWhenJobsTaken.Task.IsCompleted, Is.True);
            distributedJobService.Verify(x => x.EnsureJobsAsync(), Times.Once);
        });
        Assert.That(await roleChecksWhenJobsTaken.Task, Is.EqualTo(InitialRoleTicks + 1));
    }

    [TestCase(ServerRole.Single)]
    [TestCase(ServerRole.SchedulingPublisher)]
    public async Task Takes_Jobs_On_Role_That_Runs_Jobs(ServerRole serverRole)
    {
        var databaseReadOnlyAccessor = new Mock<IDatabaseReadOnlyAccessor>();
        databaseReadOnlyAccessor.Setup(x => x.IsReadOnly()).Returns(false);

        var serverRoleAccessor = new Mock<IServerRoleAccessor>();
        serverRoleAccessor.Setup(x => x.CurrentServerRole).Returns(serverRole);

        var jobsTaken = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var distributedJobService = new Mock<IDistributedJobService>();
        distributedJobService.Setup(x => x.EnsureJobsAsync()).Returns(Task.CompletedTask);
        distributedJobService
            .Setup(x => x.TryTakeRunnableAsync())
            .Callback(() => jobsTaken.TrySetResult())
            .ReturnsAsync((IDistributedBackgroundJob?)null);

        var sut = CreateSut(distributedJobService.Object, databaseReadOnlyAccessor.Object, serverRoleAccessor.Object);

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

    private static DistributedBackgroundJobHostedService CreateSut(
        IDistributedJobService distributedJobService,
        IDatabaseReadOnlyAccessor databaseReadOnlyAccessor,
        IServerRoleAccessor? serverRoleAccessor = null)
    {
        if (serverRoleAccessor is null)
        {
            var schedulingPublisherAccessor = new Mock<IServerRoleAccessor>();
            schedulingPublisherAccessor.Setup(x => x.CurrentServerRole).Returns(ServerRole.SchedulingPublisher);
            serverRoleAccessor = schedulingPublisherAccessor.Object;
        }

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
            databaseReadOnlyAccessor,
            serverRoleAccessor);
    }
}
