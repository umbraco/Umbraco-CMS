// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices.ServerRegistration;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HostedServices.ServerRegistration;

[TestFixture]
public class TouchServerTaskTests
{
    private Mock<IServerRegistrationService> _mockServerRegistrationService;

    private const string ApplicationUrl = "https://mysite.com/";
    private readonly TimeSpan _staleServerTimeout = TimeSpan.FromMinutes(2);

    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Unknown)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.BootFailed)]
    public async Task Does_Not_Execute_When_Runtime_State_Is_Not_Run(RuntimeLevel runtimeLevel)
    {
        var sut = CreateTouchServerTask(runtimeLevel);
        await sut.PerformExecuteAsync(null);
        VerifyServerNotTouched();
    }

    [Test]
    public async Task Does_Not_Execute_When_Application_Url_Is_Not_Available()
    {
        var sut = CreateTouchServerTask(applicationUrl: string.Empty);
        await sut.PerformExecuteAsync(null);
        VerifyServerNotTouched();
    }

    [Test]
    public async Task Executes_And_Touches_Server()
    {
        var sut = CreateTouchServerTask();
        await sut.PerformExecuteAsync(null);
        VerifyServerTouched();
    }

    [Test]
    public async Task Does_Not_Execute_When_Role_Accessor_Is_Not_Elected()
    {
        var sut = CreateTouchServerTask(useElection: false);
        await sut.PerformExecuteAsync(null);
        VerifyServerNotTouched();
    }

    private TouchServerTask CreateTouchServerTask(
        RuntimeLevel runtimeLevel = RuntimeLevel.Run,
        string applicationUrl = ApplicationUrl,
        bool useElection = true)
    {
        var mockRequestAccessor = new Mock<IHostingEnvironment>();
        mockRequestAccessor.SetupGet(x => x.ApplicationMainUrl)
            .Returns(!string.IsNullOrEmpty(applicationUrl) ? new Uri(ApplicationUrl) : null);

        var mockRunTimeState = new Mock<IRuntimeState>();
        mockRunTimeState.SetupGet(x => x.Level).Returns(runtimeLevel);

        var mockLogger = new Mock<ILogger<TouchServerTask>>();

        _mockServerRegistrationService = new Mock<IServerRegistrationService>();

        var settings = new GlobalSettings
        {
            DatabaseServerRegistrar = new DatabaseServerRegistrarSettings { StaleServerTimeout = _staleServerTimeout },
        };

        IServerRoleAccessor roleAccessor = useElection
            ? new ElectedServerRoleAccessor(_mockServerRegistrationService.Object)
            : new SingleServerRoleAccessor();

        return new TouchServerTask(
            mockRunTimeState.Object,
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
