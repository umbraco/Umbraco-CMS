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

    [TestCase(ServerRole.Unknown)]
    [TestCase(ServerRole.Subscriber)]
    public async Task Does_Not_Execute_When_Server_Role_Is_NotDefault(ServerRole serverRole)
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, serverRole: serverRole);
        await sut.PerformExecuteAsync(null);

        mockJob.Verify(job => job.RunJobAsync(), Times.Never);
    }

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

    [Test]
    public async Task Does_Execute_When_Server_Role_Is_Subscriber_And_Specified()
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();
        mockJob.Setup(x => x.ServerRoles).Returns(new ServerRole[] { ServerRole.Subscriber });

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, serverRole: ServerRole.Subscriber);
        await sut.PerformExecuteAsync(null);

        mockJob.Verify(job => job.RunJobAsync(), Times.Once);
    }

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

    [Test]
    public async Task Does_Not_Execute_When_Not_Main_Dom()
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, isMainDom: false);
        await sut.PerformExecuteAsync(null);

        mockJob.Verify(job => job.RunJobAsync(), Times.Never);
    }

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
