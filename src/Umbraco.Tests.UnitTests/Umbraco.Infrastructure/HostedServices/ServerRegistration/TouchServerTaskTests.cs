using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Services;
using Umbraco.Infrastructure.HostedServices.ServerRegistration;
using Umbraco.Web;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.HostedServices.ServerRegistration
{
    [TestFixture]
    public class TouchServerTaskTests
    {
        private Mock<IServerRegistrationService> _mockServerRegistrationService;

        private const string _applicationUrl = "https://mysite.com/";
        private const string _serverIdentity = "Test/1";
        private readonly TimeSpan _staleServerTimeout = TimeSpan.FromMinutes(2);

        [TestCase(RuntimeLevel.Boot)]
        [TestCase(RuntimeLevel.Install)]
        [TestCase(RuntimeLevel.Unknown)]
        [TestCase(RuntimeLevel.Upgrade)]
        [TestCase(RuntimeLevel.BootFailed)]
        public async Task Does_Not_Execute_When_Runtime_State_Is_Not_Run(RuntimeLevel runtimeLevel)
        {
            var sut = CreateTouchServerTask(runtimeLevel: runtimeLevel);
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

        private TouchServerTask CreateTouchServerTask(RuntimeLevel runtimeLevel = RuntimeLevel.Run, string applicationUrl = _applicationUrl)
        {
            var mockRequestAccessor = new Mock<IRequestAccessor>();
            mockRequestAccessor.Setup(x => x.GetApplicationUrl()).Returns(!string.IsNullOrEmpty(applicationUrl) ? new Uri(_applicationUrl) : null);

            var mockRunTimeState = new Mock<IRuntimeState>();
            mockRunTimeState.SetupGet(x => x.Level).Returns(runtimeLevel);

            var mockLogger = new Mock<ILogger<TouchServerTask>>();

            _mockServerRegistrationService = new Mock<IServerRegistrationService>();
            _mockServerRegistrationService.SetupGet(x => x.CurrentServerIdentity).Returns(_serverIdentity);

            var settings = new GlobalSettings
            {
                DatabaseServerRegistrar = new DatabaseServerRegistrarSettings
                {
                    StaleServerTimeout = _staleServerTimeout,
                }
            };

            return new TouchServerTask(mockRunTimeState.Object, _mockServerRegistrationService.Object, mockRequestAccessor.Object,
                mockLogger.Object, Options.Create(settings));
        }

        private void VerifyServerNotTouched()
        {
            VerifyServerTouchedTimes(Times.Never());
        }

        private void VerifyServerTouched()
        {
            VerifyServerTouchedTimes(Times.Once());
        }

        private void VerifyServerTouchedTimes(Times times)
        {
            _mockServerRegistrationService
                .Verify(x => x.TouchServer(
                    It.Is<string>(y => y == _applicationUrl),
                    It.Is<string>(y => y == _serverIdentity),
                    It.Is<TimeSpan>(y => y == _staleServerTimeout)),
                times);
        }
    }
}
