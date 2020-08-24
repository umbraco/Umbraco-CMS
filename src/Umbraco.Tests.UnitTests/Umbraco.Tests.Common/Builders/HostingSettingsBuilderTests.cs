using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class HostingSettingsBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const bool debugMode = true;
            const LocalTempStorage localTempStorageLocation = LocalTempStorage.AspNetTemp;

            var builder = new HostingSettingsBuilder();

            // Act
            var hostingSettings = builder
                .WithDebugMode(debugMode)
                .WithLocalTempStorageLocation(localTempStorageLocation)
                .Build();

            // Assert
            Assert.AreEqual(debugMode, hostingSettings.DebugMode);
            Assert.AreEqual(localTempStorageLocation, hostingSettings.LocalTempStorageLocation);
        }
    }
}
