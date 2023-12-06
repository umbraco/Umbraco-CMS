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
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;

[TestFixture]
public class InstructionProcessJobTests
{
    private Mock<IServerMessenger> _mockDatabaseServerMessenger;


    [Test]
    public async Task Executes_And_Touches_Server()
    {
        var sut = CreateInstructionProcessJob();
        await sut.RunJobAsync();
        VerifyMessengerSynced();
    }

    private InstructionProcessJob CreateInstructionProcessJob()
    {
       
        var mockLogger = new Mock<ILogger<InstructionProcessJob>>();

        _mockDatabaseServerMessenger = new Mock<IServerMessenger>();

        var settings = new GlobalSettings();

        return new InstructionProcessJob(
            _mockDatabaseServerMessenger.Object,
            mockLogger.Object,
            Options.Create(settings));
    }

    private void VerifyMessengerNotSynced() => VerifyMessengerSyncedTimes(Times.Never());

    private void VerifyMessengerSynced() => VerifyMessengerSyncedTimes(Times.Once());

    private void VerifyMessengerSyncedTimes(Times times) => _mockDatabaseServerMessenger.Verify(x => x.Sync(), times);
}
