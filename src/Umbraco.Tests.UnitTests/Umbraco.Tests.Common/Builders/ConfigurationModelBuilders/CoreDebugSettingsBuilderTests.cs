using NUnit.Framework;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class CoreDebugSettingsBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const bool dumpOnTimeoutThreadAbort = true;
            const bool logUncompletedScopes = true;

            var builder = new CoreDebugSettingsBuilder();

            // Act
            var coreDebugSettings = builder
                .WithDumpOnTimeoutThreadAbort(dumpOnTimeoutThreadAbort)
                .WithLogUncompletedScopes(logUncompletedScopes)
                .Build();

            // Assert
            Assert.AreEqual(dumpOnTimeoutThreadAbort, coreDebugSettings.DumpOnTimeoutThreadAbort);
            Assert.AreEqual(logUncompletedScopes, coreDebugSettings.LogUncompletedScopes);
        }
    }
}
