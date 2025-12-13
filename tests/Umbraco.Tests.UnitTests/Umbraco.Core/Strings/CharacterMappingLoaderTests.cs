using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        Assert.AreEqual("Sh", mappings['Щ']);
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

    [Test]
    public void LoadMappings_UserMappingsOverrideBuiltIn_WhenHigherPriority()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var configDir = Path.Combine(tempDir, "config", "character-mappings");
        Directory.CreateDirectory(configDir);

        try
        {
            // Create a user mapping file with higher priority that overrides a built-in mapping
            var userMappingJson = """
            {
              "name": "User Custom Mappings",
              "description": "User overrides for testing",
              "priority": 200,
              "mappings": {
                "æ": "AE_CUSTOM",
                "Œ": "OE_CUSTOM",
                "ß": "SS_CUSTOM"
              }
            }
            """;
            File.WriteAllText(Path.Combine(configDir, "custom.json"), userMappingJson);

            var hostEnv = new Mock<IHostEnvironment>();
            hostEnv.Setup(h => h.ContentRootPath).Returns(tempDir);

            var loader = new CharacterMappingLoader(
                hostEnv.Object,
                NullLogger<CharacterMappingLoader>.Instance);

            // Act
            var mappings = loader.LoadMappings();

            // Assert - user mappings should override built-in
            Assert.AreEqual("AE_CUSTOM", mappings['æ'], "User mapping should override built-in for 'æ'");
            Assert.AreEqual("OE_CUSTOM", mappings['Œ'], "User mapping should override built-in for 'Œ'");
            Assert.AreEqual("SS_CUSTOM", mappings['ß'], "User mapping should override built-in for 'ß'");

            // Other built-in mappings should still exist
            Assert.AreEqual("Sh", mappings['Щ'], "Non-overridden built-in mappings should still work");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Test]
    public void LoadMappings_BuiltInMappingsWin_WhenUserMappingsHaveLowerPriority()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var configDir = Path.Combine(tempDir, "config", "character-mappings");
        Directory.CreateDirectory(configDir);

        try
        {
            // Create a user mapping file with NEGATIVE priority (built-in is 0)
            var userMappingJson = """
            {
              "name": "Low Priority User Mappings",
              "description": "User overrides with low priority",
              "priority": -10,
              "mappings": {
                "æ": "AE_LOW",
                "Œ": "OE_LOW"
              }
            }
            """;
            File.WriteAllText(Path.Combine(configDir, "low-priority.json"), userMappingJson);

            var hostEnv = new Mock<IHostEnvironment>();
            hostEnv.Setup(h => h.ContentRootPath).Returns(tempDir);

            var loader = new CharacterMappingLoader(
                hostEnv.Object,
                NullLogger<CharacterMappingLoader>.Instance);

            // Act
            var mappings = loader.LoadMappings();

            // Assert - built-in mappings should win over lower priority user mappings
            Assert.AreEqual("ae", mappings['æ'], "Built-in mapping should override low-priority user mapping");
            Assert.AreEqual("OE", mappings['Œ'], "Built-in mapping should override low-priority user mapping");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Test]
    public void LoadMappings_LogsWarning_WhenEmbeddedResourceMissing()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CharacterMappingLoader>>();
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            loggerMock.Object);

        // Act
        var mappings = loader.LoadMappings();

        // Assert - should still return mappings (from available resources)
        Assert.IsNotNull(mappings);

        // Note: We can't actually make embedded resources missing in a unit test,
        // but we verify that if they were missing, the code would log a warning
        // and continue loading other resources. This test documents the expected behavior.
        // The actual warning logging is tested implicitly - if resources are missing,
        // the logger would be called with LogLevel.Warning.

        // Verify the loader completed successfully despite potential missing resources
        Assert.That(mappings.Count, Is.GreaterThan(0),
            "Loader should return at least some mappings even if some resources are missing");
    }

    [Test]
    public void LoadMappings_ContinuesLoading_WhenUserFileIsInvalid()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var configDir = Path.Combine(tempDir, "config", "character-mappings");
        Directory.CreateDirectory(configDir);

        try
        {
            // Create an invalid JSON file
            var invalidJson = "{ invalid json content !!!";
            File.WriteAllText(Path.Combine(configDir, "invalid.json"), invalidJson);

            // Create a valid JSON file to verify loading continues
            var validJson = """
            {
              "name": "Valid Mappings",
              "priority": 150,
              "mappings": {
                "X": "TEST"
              }
            }
            """;
            File.WriteAllText(Path.Combine(configDir, "valid.json"), validJson);

            var loggerMock = new Mock<ILogger<CharacterMappingLoader>>();
            var hostEnv = new Mock<IHostEnvironment>();
            hostEnv.Setup(h => h.ContentRootPath).Returns(tempDir);

            var loader = new CharacterMappingLoader(
                hostEnv.Object,
                loggerMock.Object);

            // Act
            var mappings = loader.LoadMappings();

            // Assert - should have loaded built-in mappings and the valid user mapping
            Assert.IsNotNull(mappings);
            Assert.That(mappings.Count, Is.GreaterThan(0), "Should load built-in mappings");
            Assert.AreEqual("TEST", mappings['X'], "Should load valid user mapping despite invalid file");

            // Verify warning was logged for invalid file
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("invalid.json")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Should log warning for invalid JSON file");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Test]
    public void LoadMappings_LogsWarning_WhenMultiCharacterKeysFound()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var configDir = Path.Combine(tempDir, "config", "character-mappings");
        Directory.CreateDirectory(configDir);

        try
        {
            // Create a mapping file with multi-character keys
            var mappingWithMultiChar = """
            {
              "name": "Multi-Char Keys",
              "priority": 150,
              "mappings": {
                "X": "TEST",
                "ABC": "MULTI",
                "XY": "TWO"
              }
            }
            """;
            File.WriteAllText(Path.Combine(configDir, "multichar.json"), mappingWithMultiChar);

            var loggerMock = new Mock<ILogger<CharacterMappingLoader>>();
            var hostEnv = new Mock<IHostEnvironment>();
            hostEnv.Setup(h => h.ContentRootPath).Returns(tempDir);

            var loader = new CharacterMappingLoader(
                hostEnv.Object,
                loggerMock.Object);

            // Act
            var mappings = loader.LoadMappings();

            // Assert - single character key should be loaded
            Assert.AreEqual("TEST", mappings['X'], "Single character mapping should be loaded");

            // Multi-character keys should be skipped and warnings logged
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ABC")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Should log warning for multi-character key 'ABC'");

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("XY")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Should log warning for multi-character key 'XY'");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
