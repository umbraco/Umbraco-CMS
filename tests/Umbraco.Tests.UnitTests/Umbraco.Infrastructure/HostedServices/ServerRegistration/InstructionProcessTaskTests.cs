// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices.ServerRegistration;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HostedServices.ServerRegistration;

[TestFixture]
public class InstructionProcessTaskTests
{
    private Mock<IServerMessenger> _mockDatabaseServerMessenger;

    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Unknown)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.BootFailed)]
    public async Task Does_Not_Execute_When_Runtime_State_Is_Not_Run(RuntimeLevel runtimeLevel)
    {
        var sut = CreateInstructionProcessTask(runtimeLevel);
        await sut.PerformExecuteAsync(null);
        VerifyMessengerNotSynced();
    }

    [Test]
    public async Task Executes_And_Touches_Server()
    {
        var sut = CreateInstructionProcessTask();
        await sut.PerformExecuteAsync(null);
        VerifyMessengerSynced();
    }

    private InstructionProcessTask CreateInstructionProcessTask(RuntimeLevel runtimeLevel = RuntimeLevel.Run)
    {
        var mockRunTimeState = new Mock<IRuntimeState>();
        mockRunTimeState.SetupGet(x => x.Level).Returns(runtimeLevel);

        var mockLogger = new Mock<ILogger<InstructionProcessTask>>();

        _mockDatabaseServerMessenger = new Mock<IServerMessenger>();

        var settings = new GlobalSettings();

        return new InstructionProcessTask(
            mockRunTimeState.Object,
            _mockDatabaseServerMessenger.Object,
            mockLogger.Object,
            Options.Create(settings));
    }

    private void VerifyMessengerNotSynced() => VerifyMessengerSyncedTimes(Times.Never());

    private void VerifyMessengerSynced() => VerifyMessengerSyncedTimes(Times.Once());

    private void VerifyMessengerSyncedTimes(Times times) => _mockDatabaseServerMessenger.Verify(x => x.Sync(), times);
}
