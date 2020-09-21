using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class ModelsBuilderSettingsBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const string modelsMode = "PureLive";

            var builder = new ModelsBuilderSettingsBuilder();

            // Act
            var modelsBuilderSettings = builder
                .WithModelsMode(modelsMode)
                .Build();

            // Assert
            Assert.AreEqual(ModelsMode.PureLive, modelsBuilderSettings.ModelsModeValue);
        }
    }
}
