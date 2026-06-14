// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;

[TestFixture]
public class InstructionProcessJobTests
{
    private Mock<IServerMessenger> _mockDatabaseServerMessenger;
    private Mock<ILogger<InstructionProcessJob>> _mockLogger;


    [Test]
    public async Task Executes_And_Touches_Server()
    {
        var sut = CreateInstructionProcessJob();
        await sut.RunJobAsync(CancellationToken.None);
        VerifyMessengerSynced();
    }

    [Test]
    public async Task Returns_And_Logs_When_Sync_Hangs()
    {
        using var gate = new ManualResetEventSlim(false);
        var sut = CreateInstructionProcessJob(
            TimeSpan.FromMilliseconds(50),
            () => gate.Wait());

        try
        {
            // A hung Sync() must not block the recurring loop: RunJobAsync should return after the timeout
            // rather than awaiting the (synchronous, un-cancellable) Sync() forever.
            Task runTask = sut.RunJobAsync(CancellationToken.None);
            Task winner = await Task.WhenAny(runTask, Task.Delay(TimeSpan.FromSeconds(10)));

            Assert.AreSame(runTask, winner, "RunJobAsync did not return after the sync timed out.");
            await runTask; // Must complete without throwing so the loop continues.

            VerifyErrorLogged();
        }
        finally
        {
            // Release the abandoned sync so it does not leave a blocked thread-pool thread behind.
            gate.Set();
        }
    }

    [Test]
    public async Task Skips_Sync_While_Previous_Is_Still_Running()
    {
        using var gate = new ManualResetEventSlim(false);
        var sut = CreateInstructionProcessJob(TimeSpan.FromMilliseconds(50), () => gate.Wait());

        try
        {
            // First run starts a sync that hangs and times out, leaving it in-flight.
            await sut.RunJobAsync(CancellationToken.None);

            // Second run must skip rather than start (and block on) another sync while the first is still running.
            await sut.RunJobAsync(CancellationToken.None);

            VerifyMessengerSyncedTimes(Times.Once());
        }
        finally
        {
            gate.Set();
        }
    }

    [Test]
    public async Task Falls_Back_And_Warns_When_SyncTimeout_Invalid()
    {
        // A non-positive timeout is misconfiguration; the job should warn and fall back to a sane default
        // rather than treating every sync as immediately timed out.
        var sut = CreateInstructionProcessJob(TimeSpan.Zero);

        VerifyWarningLogged();

        // The fallback timeout is generous, so a normal (fast) sync still completes successfully.
        await sut.RunJobAsync(CancellationToken.None);
        VerifyMessengerSynced();
    }

    private InstructionProcessJob CreateInstructionProcessJob(TimeSpan? syncTimeout = null, Action? onSync = null)
    {
        _mockLogger = new Mock<ILogger<InstructionProcessJob>>();

        _mockDatabaseServerMessenger = new Mock<IServerMessenger>();
        if (onSync is not null)
        {
            _mockDatabaseServerMessenger.Setup(x => x.Sync()).Callback(onSync);
        }

        var settings = new GlobalSettings();
        if (syncTimeout.HasValue)
        {
            settings.DatabaseServerMessenger.SyncTimeout = syncTimeout.Value;
        }

        return new InstructionProcessJob(
            _mockDatabaseServerMessenger.Object,
            _mockLogger.Object,
            Options.Create(settings));
    }

    private void VerifyMessengerSynced() => VerifyMessengerSyncedTimes(Times.Once());

    private void VerifyMessengerSyncedTimes(Times times) => _mockDatabaseServerMessenger.Verify(x => x.Sync(), times);

    private void VerifyErrorLogged() => VerifyLogged(LogLevel.Error);

    private void VerifyWarningLogged() => VerifyLogged(LogLevel.Warning);

    private void VerifyLogged(LogLevel level) => _mockLogger.Verify(
        l => l.Log(
            It.Is<LogLevel>(y => y == level),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
}
