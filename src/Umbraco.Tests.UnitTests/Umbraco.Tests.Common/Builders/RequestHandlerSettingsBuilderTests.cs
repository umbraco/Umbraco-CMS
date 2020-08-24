using NUnit.Framework;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class RequestHandlerSettingsBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const bool addTrailingSlash = true;

            var builder = new RequestHandlerSettingsBuilder();

            // Act
            var requestHandlerSettings = builder
                .WithAddTrailingSlash(addTrailingSlash)
                .Build();

            // Assert
            Assert.AreEqual(addTrailingSlash, requestHandlerSettings.AddTrailingSlash);
        }
    }
}
