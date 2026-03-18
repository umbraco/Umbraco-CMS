// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Notifications;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs;

/// <summary>
/// Contains unit tests for the <see cref="RecurringBackgroundJobHostedService"/> class, verifying its background job scheduling and execution behavior.
/// </summary>
[TestFixture]
public class RecurringBackgroundJobHostedServiceTests
{

    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Unknown)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.BootFailed)]
    public async Task Does_Not_Execute_When_Runtime_State_Is_Not_Run(RuntimeLevel runtimeLevel)
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, runtimeLevel: runtimeLevel);
        await sut.PerformExecuteAsync(null);

        mockJob.Verify(job => job.RunJobAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that an ignored notification is published when the runtime state is not "Run".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Publishes_Ignored_Notification_When_Runtime_State_Is_Not_Run()
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();
        var mockEventAggregator = new Mock<IEventAggregator>();

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, runtimeLevel: RuntimeLevel.Unknown, mockEventAggregator: mockEventAggregator);
        await sut.PerformExecuteAsync(null);

        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobExecutingNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobIgnoredNotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the recurring background job does not execute when the server role is not set to Default.
    /// </summary>
    /// <param name="serverRole">The server role under test.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestCase(ServerRole.Unknown)]
    [TestCase(ServerRole.Subscriber)]
    public async Task Does_Not_Execute_When_Server_Role_Is_NotDefault(ServerRole serverRole)
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, serverRole: serverRole);
        await sut.PerformExecuteAsync(null);

        mockJob.Verify(job => job.RunJobAsync(), Times.Never);
    }

    /// <summary>
    /// Verifies that the recurring background job executes when the server role is set to one of the default roles.
    /// </summary>
    /// <param name="serverRole">The server role under which to test job execution (e.g., Single or SchedulingPublisher).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [TestCase(ServerRole.Single)]
    [TestCase(ServerRole.SchedulingPublisher)]
    public async Task Does_Executes_When_Server_Role_Is_Default(ServerRole serverRole)
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();
        mockJob.Setup(x => x.ServerRoles).Returns(IRecurringBackgroundJob.DefaultServerRoles);

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, serverRole: serverRole);
        await sut.PerformExecuteAsync(null);

        mockJob.Verify(job => job.RunJobAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that the recurring background job executes when the server role is Subscriber and matches the specified role.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Does_Execute_When_Server_Role_Is_Subscriber_And_Specified()
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();
        mockJob.Setup(x => x.ServerRoles).Returns(new ServerRole[] { ServerRole.Subscriber });

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, serverRole: ServerRole.Subscriber);
        await sut.PerformExecuteAsync(null);

        mockJob.Verify(job => job.RunJobAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that an ignored notification is published when the server role is not allowed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Publishes_Ignored_Notification_When_Server_Role_Is_Not_Allowed()
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();
        var mockEventAggregator = new Mock<IEventAggregator>();

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, serverRole: ServerRole.Unknown, mockEventAggregator: mockEventAggregator);
        await sut.PerformExecuteAsync(null);

        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobExecutingNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobIgnoredNotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that the recurring background job does not execute when the instance is not the main domain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Does_Not_Execute_When_Not_Main_Dom()
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, isMainDom: false);
        await sut.PerformExecuteAsync(null);

        mockJob.Verify(job => job.RunJobAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that an ignored notification is published when the current instance is not the main domain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Publishes_Ignored_Notification_When_Not_Main_Dom()
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();
        var mockEventAggregator = new Mock<IEventAggregator>();

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, isMainDom: false, mockEventAggregator: mockEventAggregator);
        await sut.PerformExecuteAsync(null);

        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobExecutingNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobIgnoredNotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }



    /// <summary>
    /// Tests that the executed notification is published when the recurring background job is run.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Publishes_Executed_Notification_When_Run()
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();
        mockJob.Setup(x => x.ServerRoles).Returns(IRecurringBackgroundJob.DefaultServerRoles);
        var mockEventAggregator = new Mock<IEventAggregator>();

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, mockEventAggregator: mockEventAggregator);
        await sut.PerformExecuteAsync(null);

        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobExecutingNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobExecutedNotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that a failed notification is published when the recurring background job fails.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Publishes_Failed_Notification_When_Fails()
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();
        mockJob.Setup(x => x.ServerRoles).Returns(IRecurringBackgroundJob.DefaultServerRoles);
        mockJob.Setup(x => x.RunJobAsync()).Throws<Exception>();
        var mockEventAggregator = new Mock<IEventAggregator>();

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, mockEventAggregator: mockEventAggregator);
        await sut.PerformExecuteAsync(null);

        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobExecutingNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobFailedNotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the <see cref="RecurringBackgroundJobHostedService"/> publishes the correct start and stop notifications
    /// (<see cref="RecurringBackgroundJobStartingNotification"/>, <see cref="RecurringBackgroundJobStartedNotification"/>,
    /// <see cref="RecurringBackgroundJobStoppingNotification"/>, and <see cref="RecurringBackgroundJobStoppedNotification"/>)
    /// when started and stopped.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Publishes_Start_And_Stop_Notifications()
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();
        var mockEventAggregator = new Mock<IEventAggregator>();

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, isMainDom: false, mockEventAggregator: mockEventAggregator);
        await sut.StartAsync(CancellationToken.None);
        await sut.StopAsync(CancellationToken.None);


        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobStartingNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobStartedNotification>(), It.IsAny<CancellationToken>()), Times.Once);


        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobStoppingNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        mockEventAggregator.Verify(x => x.PublishAsync(It.IsAny<RecurringBackgroundJobStoppedNotification>(), It.IsAny<CancellationToken>()), Times.Once);

    }


    private RecurringHostedServiceBase CreateRecurringBackgroundJobHostedService(
        Mock<IRecurringBackgroundJob> mockJob,
        RuntimeLevel runtimeLevel = RuntimeLevel.Run,
        ServerRole serverRole = ServerRole.Single,
        bool isMainDom = true,
        Mock<IEventAggregator> mockEventAggregator = null)
    {
        var mockRunTimeState = new Mock<IRuntimeState>();
        mockRunTimeState.SetupGet(x => x.Level).Returns(runtimeLevel);

        var mockServerRegistrar = new Mock<IServerRoleAccessor>();
        mockServerRegistrar.Setup(x => x.CurrentServerRole).Returns(serverRole);

        var mockMainDom = new Mock<IMainDom>();
        mockMainDom.SetupGet(x => x.IsMainDom).Returns(isMainDom);

        var mockLogger = new Mock<ILogger<RecurringBackgroundJobHostedService<IRecurringBackgroundJob>>>();
        if (mockEventAggregator == null)
        {
            mockEventAggregator = new Mock<IEventAggregator>();
        }

        return new RecurringBackgroundJobHostedService<IRecurringBackgroundJob>(
            mockRunTimeState.Object,
            mockLogger.Object,
            mockMainDom.Object,
            mockServerRegistrar.Object,
            mockEventAggregator.Object,
            mockJob.Object);
    }
}
