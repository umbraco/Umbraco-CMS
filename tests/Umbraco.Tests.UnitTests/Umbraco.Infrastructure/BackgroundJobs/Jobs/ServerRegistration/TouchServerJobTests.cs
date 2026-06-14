// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;

[TestFixture]
public class TouchServerJobTests
{
    private Mock<IServerRegistrationService> _mockServerRegistrationService;
    private Mock<ILogger<TouchServerJob>> _mockLogger;

    private const string ApplicationUrl = "https://mysite.com/";
    private readonly TimeSpan _staleServerTimeout = TimeSpan.FromMinutes(2);


    [Test]
    public async Task Touches_Server_With_Placeholder_When_Application_Url_Is_Not_Available()
    {
        var sut = CreateTouchServerTask(applicationUrl: string.Empty);
        await sut.RunJobAsync(CancellationToken.None);
        VerifyServerTouchedWithPlaceholder();
    }

    [Test]
    public async Task Executes_And_Touches_Server()
    {
        var sut = CreateTouchServerTask();
        await sut.RunJobAsync(CancellationToken.None);
        VerifyServerTouched();
    }

    [Test]
    public async Task Does_Not_Execute_When_Role_Accessor_Is_Not_Elected()
    {
        var sut = CreateTouchServerTask(useElection: false);
        await sut.RunJobAsync(CancellationToken.None);
        VerifyServerNotTouched();
    }

    [Test]
    public void Does_Not_Throw_When_Touching_The_Server_Fails()
    {
        var sut = CreateTouchServerTask();
        _mockServerRegistrationService
            .Setup(x => x.TouchServer(It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .Throws(new InvalidOperationException("database unavailable"));

        // A failure to touch the server must not bubble out and kill the recurring job loop.
        Assert.DoesNotThrowAsync(() => sut.RunJobAsync(CancellationToken.None));
    }

    [Test]
    public void Runs_On_All_Server_Roles()
    {
        var sut = CreateTouchServerTask();

        // The touch job must run on every role (including Unknown/Subscriber); otherwise a server whose role
        // has not yet been elected would never register itself and the role could never be determined.
        CollectionAssert.AreEquivalent(Enum.GetValues<ServerRole>(), sut.ServerRoles);
    }

    [Test]
    public void Period_Is_Configured_From_WaitTimeBetweenCalls()
    {
        var waitTimeBetweenCalls = TimeSpan.FromSeconds(42);
        var sut = CreateTouchServerTask(waitTimeBetweenCalls: waitTimeBetweenCalls);
        Assert.AreEqual(waitTimeBetweenCalls, sut.Period);
    }

    [Test]
    public async Task Returns_And_Logs_When_Touch_Hangs()
    {
        using var gate = new ManualResetEventSlim(false);
        var sut = CreateTouchServerTask(touchTimeout: TimeSpan.FromMilliseconds(50), onTouch: () => gate.Wait());

        try
        {
            // A hung TouchServer() must not block the recurring loop: RunJobAsync should return after the timeout
            // rather than awaiting the (synchronous, un-cancellable) call forever.
            Task runTask = sut.RunJobAsync(CancellationToken.None);
            Task winner = await Task.WhenAny(runTask, Task.Delay(TimeSpan.FromSeconds(10)));

            Assert.AreSame(runTask, winner, "RunJobAsync did not return after the touch timed out.");
            await runTask; // Must complete without throwing so the loop continues.

            VerifyErrorLogged();
        }
        finally
        {
            // Release the abandoned touch so it does not leave a blocked thread-pool thread behind.
            gate.Set();
        }
    }

    [Test]
    public async Task Skips_Touch_While_Previous_Is_Still_Running()
    {
        using var gate = new ManualResetEventSlim(false);
        var sut = CreateTouchServerTask(touchTimeout: TimeSpan.FromMilliseconds(50), onTouch: () => gate.Wait());

        try
        {
            // First run starts a touch that hangs and times out, leaving it in-flight.
            await sut.RunJobAsync(CancellationToken.None);

            // Second run must skip rather than start (and block on) another touch while the first is still running.
            await sut.RunJobAsync(CancellationToken.None);

            VerifyServerTouchedTimes(Times.Once());
        }
        finally
        {
            gate.Set();
        }
    }

    [Test]
    public async Task Falls_Back_And_Warns_When_TouchTimeout_Invalid()
    {
        // A non-positive timeout is misconfiguration; the job should warn and fall back to a sane default
        // rather than treating every touch as immediately timed out.
        var sut = CreateTouchServerTask(touchTimeout: TimeSpan.Zero);

        VerifyWarningLogged();

        // The fallback timeout is generous, so a normal (fast) touch still completes successfully.
        await sut.RunJobAsync(CancellationToken.None);
        VerifyServerTouched();
    }

    private TouchServerJob CreateTouchServerTask(
        RuntimeLevel runtimeLevel = RuntimeLevel.Run,
        string applicationUrl = ApplicationUrl,
        bool useElection = true,
        TimeSpan? waitTimeBetweenCalls = null,
        TimeSpan? touchTimeout = null,
        Action? onTouch = null)
    {
        var mockRequestAccessor = new Mock<IHostingEnvironment>();
        mockRequestAccessor.SetupGet(x => x.ApplicationMainUrl)
            .Returns(!string.IsNullOrEmpty(applicationUrl) ? new Uri(ApplicationUrl) : null);

        var mockRunTimeState = new Mock<IRuntimeState>();
        mockRunTimeState.SetupGet(x => x.Level).Returns(runtimeLevel);

        _mockLogger = new Mock<ILogger<TouchServerJob>>();

        _mockServerRegistrationService = new Mock<IServerRegistrationService>();
        if (onTouch is not null)
        {
            _mockServerRegistrationService
                .Setup(x => x.TouchServer(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Callback(onTouch);
        }

        var registrarSettings = new DatabaseServerRegistrarSettings
        {
            StaleServerTimeout = _staleServerTimeout,
            WaitTimeBetweenCalls = waitTimeBetweenCalls ?? TimeSpan.FromMinutes(1),
        };
        if (touchTimeout.HasValue)
        {
            registrarSettings.TouchTimeout = touchTimeout.Value;
        }

        var settings = new GlobalSettings
        {
            DatabaseServerRegistrar = registrarSettings,
        };

        IServerRoleAccessor roleAccessor = useElection
            ? new ElectedServerRoleAccessor(_mockServerRegistrationService.Object)
            : new SingleServerRoleAccessor();

        return new TouchServerJob(
            _mockServerRegistrationService.Object,
            mockRequestAccessor.Object,
            _mockLogger.Object,
            new TestOptionsMonitor<GlobalSettings>(settings),
            roleAccessor);
    }

    private void VerifyServerNotTouched() => VerifyServerTouchedTimes(Times.Never());

    private void VerifyServerTouched() => VerifyServerTouchedTimes(Times.Once());

    private void VerifyServerTouchedTimes(Times times) => _mockServerRegistrationService
        .Verify(
            x => x.TouchServer(
                It.Is<string>(y => y == ApplicationUrl),
                It.Is<TimeSpan>(y => y == _staleServerTimeout)),
            times);

    private void VerifyServerTouchedWithPlaceholder() => _mockServerRegistrationService
        .Verify(
            x => x.TouchServer(
                It.Is<string>(y => y == Environment.MachineName),
                It.Is<TimeSpan>(y => y == _staleServerTimeout)),
            Times.Once());

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
