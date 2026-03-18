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

/// <summary>
/// Contains unit tests for the <see cref="TouchServerJob"/> class to verify its behavior and functionality.
/// </summary>
[TestFixture]
public class TouchServerJobTests
{
    private Mock<IServerRegistrationService> _mockServerRegistrationService;

    private const string ApplicationUrl = "https://mysite.com/";
    private readonly TimeSpan _staleServerTimeout = TimeSpan.FromMinutes(2);


    /// <summary>
    /// Tests that the job does not execute when the application URL is not available.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Does_Not_Execute_When_Application_Url_Is_Not_Available()
    {
        var sut = CreateTouchServerTask(applicationUrl: string.Empty);
        await sut.RunJobAsync();
        VerifyServerNotTouched();
    }

    /// <summary>
    /// Tests that the TouchServerJob executes and successfully touches the server.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Executes_And_Touches_Server()
    {
        var sut = CreateTouchServerTask();
        await sut.RunJobAsync();
        VerifyServerTouched();
    }

    /// <summary>
    /// Tests that the job does not execute when the role accessor is not elected.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Does_Not_Execute_When_Role_Accessor_Is_Not_Elected()
    {
        var sut = CreateTouchServerTask(useElection: false);
        await sut.RunJobAsync();
        VerifyServerNotTouched();
    }

    private TouchServerJob CreateTouchServerTask(
        RuntimeLevel runtimeLevel = RuntimeLevel.Run,
        string applicationUrl = ApplicationUrl,
        bool useElection = true)
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
            DatabaseServerRegistrar = new DatabaseServerRegistrarSettings { StaleServerTimeout = _staleServerTimeout },
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
}
