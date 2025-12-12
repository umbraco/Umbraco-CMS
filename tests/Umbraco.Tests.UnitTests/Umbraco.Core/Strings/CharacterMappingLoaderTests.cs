using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Strings;

[TestFixture]
public class CharacterMappingLoaderTests
{
    [Test]
    public void LoadMappings_LoadsBuiltInMappings()
    {
        // Arrange
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            NullLogger<CharacterMappingLoader>.Instance);

        // Act
        var mappings = loader.LoadMappings();

        // Assert
        Assert.IsNotNull(mappings);
        Assert.That(mappings.Count, Is.GreaterThan(0), "Should have loaded mappings");
    }

    [Test]
    public void LoadMappings_ContainsLigatures()
    {
        // Arrange
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            NullLogger<CharacterMappingLoader>.Instance);

        // Act
        var mappings = loader.LoadMappings();

        // Assert
        Assert.AreEqual("OE", mappings['Œ']);
        Assert.AreEqual("ae", mappings['æ']);
        Assert.AreEqual("ss", mappings['ß']);
    }

    [Test]
    public void LoadMappings_ContainsCyrillic()
    {
        // Arrange
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            NullLogger<CharacterMappingLoader>.Instance);

        // Act
        var mappings = loader.LoadMappings();

        // Assert
        Assert.AreEqual("Shch", mappings['Щ']);
        Assert.AreEqual("zh", mappings['ж']);
        Assert.AreEqual("Ya", mappings['Я']);
    }

    [Test]
    public void LoadMappings_ContainsSpecialLatin()
    {
        // Arrange
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            NullLogger<CharacterMappingLoader>.Instance);

        // Act
        var mappings = loader.LoadMappings();

        // Assert
        Assert.AreEqual("L", mappings['Ł']);
        Assert.AreEqual("O", mappings['Ø']);
        Assert.AreEqual("TH", mappings['Þ']);
    }
}
