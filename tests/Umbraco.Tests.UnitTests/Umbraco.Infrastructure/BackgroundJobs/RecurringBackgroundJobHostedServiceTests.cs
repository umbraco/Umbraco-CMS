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

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, serverRole: serverRole);
        await sut.PerformExecuteAsync(null);

        mockJob.Verify(job => job.RunJobAsync(), Times.Once);
    }

    [TestCase(ServerRole.Subscriber)]
    public async Task Does_Execute_When_Server_Role_Is_Subscriber_And_Specified()
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();
        mockJob.Setup(x => x.ServerRoles).Returns(new ServerRole[] { ServerRole.Subscriber });

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, serverRole: ServerRole.Subscriber);
        await sut.PerformExecuteAsync(null);

        mockJob.Verify(job => job.RunJobAsync(), Times.Once);
    }

    [Test]
    public async Task Does_Not_Execute_When_Not_Main_Dom()
    {
        var mockJob = new Mock<IRecurringBackgroundJob>();

        var sut = CreateRecurringBackgroundJobHostedService(mockJob, isMainDom: false);
        await sut.PerformExecuteAsync(null);

        mockJob.Verify(job => job.RunJobAsync(), Times.Never);
    }


    private RecurringHostedServiceBase CreateRecurringBackgroundJobHostedService(
        Mock<IRecurringBackgroundJob> mockJob,
        RuntimeLevel runtimeLevel = RuntimeLevel.Run,
        ServerRole serverRole = ServerRole.Single,
        bool isMainDom = true)
    {
        var mockRunTimeState = new Mock<IRuntimeState>();
        mockRunTimeState.SetupGet(x => x.Level).Returns(runtimeLevel);

        var mockServerRegistrar = new Mock<IServerRoleAccessor>();
        mockServerRegistrar.Setup(x => x.CurrentServerRole).Returns(serverRole);

        var mockMainDom = new Mock<IMainDom>();
        mockMainDom.SetupGet(x => x.IsMainDom).Returns(isMainDom);

        var mockLogger = new Mock<ILogger<RecurringBackgroundJobHostedService<IRecurringBackgroundJob>>>();
        var mockEventAggregator = new Mock<IEventAggregator>();

        return new RecurringBackgroundJobHostedService<IRecurringBackgroundJob>(
            mockRunTimeState.Object,
            mockLogger.Object,
            mockMainDom.Object,
            mockServerRegistrar.Object,
            mockEventAggregator.Object,
            mockJob.Object);
    }
}
