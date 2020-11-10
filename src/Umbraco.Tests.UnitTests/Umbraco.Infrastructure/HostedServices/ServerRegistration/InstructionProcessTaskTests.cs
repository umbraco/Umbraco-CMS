using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Sync;
using Umbraco.Infrastructure.HostedServices.ServerRegistration;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.HostedServices.ServerRegistration
{
    [TestFixture]
    public class InstructionProcessTaskTests
    {
        private Mock<IDatabaseServerMessenger> _mockDatabaseServerMessenger;

        [TestCase(RuntimeLevel.Boot)]
        [TestCase(RuntimeLevel.Install)]
        [TestCase(RuntimeLevel.Unknown)]
        [TestCase(RuntimeLevel.Upgrade)]
        [TestCase(RuntimeLevel.BootFailed)]
        public async Task Does_Not_Execute_When_Runtime_State_Is_Not_Run(RuntimeLevel runtimeLevel)
        {
            var sut = CreateInstructionProcessTask(runtimeLevel: runtimeLevel);
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

            _mockDatabaseServerMessenger = new Mock<IDatabaseServerMessenger>();

            return new InstructionProcessTask(mockRunTimeState.Object, _mockDatabaseServerMessenger.Object, mockLogger.Object);
        }

        private void VerifyMessengerNotSynced()
        {
            VerifyMessengerSyncedTimes(Times.Never());
        }

        private void VerifyMessengerSynced()
        {
            VerifyMessengerSyncedTimes(Times.Once());
        }

        private void VerifyMessengerSyncedTimes(Times times)
        {
            _mockDatabaseServerMessenger.Verify(x => x.Sync(), times);
        }
    }
}
