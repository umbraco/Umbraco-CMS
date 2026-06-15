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

    private TouchServerJob CreateTouchServerTask(
        RuntimeLevel runtimeLevel = RuntimeLevel.Run,
        string applicationUrl = ApplicationUrl,
        bool useElection = true,
        TimeSpan? waitTimeBetweenCalls = null)
    {
        var mockRequestAccessor = new Mock<IHostingEnvironment>();
        mockRequestAccessor.SetupGet(x => x.ApplicationMainUrl)
            .Returns(!string.IsNullOrEmpty(applicationUrl) ? new Uri(ApplicationUrl) : null);

        var mockRunTimeState = new Mock<IRuntimeState>();
        mockRunTimeState.SetupGet(x => x.Level).Returns(runtimeLevel);

        var mockLogger = new Mock<ILogger<TouchServerJob>>();

        _mockServerRegistrationService = new Mock<IServerRegistrationService>();

        var settings = new GlobalSettings
        {
            DatabaseServerRegistrar = new DatabaseServerRegistrarSettings
            {
                StaleServerTimeout = _staleServerTimeout,
                WaitTimeBetweenCalls = waitTimeBetweenCalls ?? TimeSpan.FromMinutes(1),
            },
        };

        IServerRoleAccessor roleAccessor = useElection
            ? new ElectedServerRoleAccessor(_mockServerRegistrationService.Object)
            : new SingleServerRoleAccessor();

        return new TouchServerJob(
            _mockServerRegistrationService.Object,
            mockRequestAccessor.Object,
            mockLogger.Object,
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
}
