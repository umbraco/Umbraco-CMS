using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Strings;

[TestFixture]
public class Utf8ToAsciiConverterGoldenTests
{
    private IUtf8ToAsciiConverter _newConverter = null!;
    private static readonly Dictionary<string, string> GoldenMappings;

    static Utf8ToAsciiConverterGoldenTests()
    {
        var testDataPath = Path.Combine(
            AppContext.BaseDirectory,
            "Umbraco.Core",
            "Strings",
            "TestData",
            "golden-mappings.json");

        if (!File.Exists(testDataPath))
        {
            throw new InvalidOperationException(
                $"Golden mappings file not found at: {testDataPath}. " +
                "Ensure the test data is configured to copy to output directory.");
        }

        var json = File.ReadAllText(testDataPath);
        var doc = JsonDocument.Parse(json);
        GoldenMappings = doc.RootElement
            .GetProperty("mappings")
            .EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.GetString() ?? "");

        if (GoldenMappings.Count == 0)
        {
            throw new InvalidOperationException(
                "Golden mappings file is empty. Test data may be corrupted.");
        }
    }

    [SetUp]
    public void SetUp()
    {
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            NullLogger<CharacterMappingLoader>.Instance);

        _newConverter = new Utf8ToAsciiConverterNew(loader);
    }

    public static IEnumerable<TestCaseData> GetGoldenMappings()
    {
        foreach (var (input, expected) in GoldenMappings)
        {
            yield return new TestCaseData(input, expected);
        }
    }

    [TestCaseSource(nameof(GetGoldenMappings))]
    public void NewConverter_MatchesGoldenMapping(string input, string expected)
    {
        var result = _newConverter.Convert(input);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(GetGoldenMappings))]
    public void NewConverter_MatchesOriginalBehavior(string input, string expected)
    {
        // Compare new implementation against original
        // Note: Original has buffer overflow bugs for chars that expand to 4+ chars (e.g., ⑽→(10))
        string? originalResult;
        try
        {
            originalResult = Utf8ToAsciiConverter.ToAsciiString(input);
        }
        catch (IndexOutOfRangeException)
        {
            // Original converter has known buffer bugs for high-expansion characters
            // New converter fixes these - verify it produces the expected golden mapping
            var newResult = _newConverter.Convert(input);
            Assert.That(newResult, Is.EqualTo(expected),
                $"Original throws IndexOutOfRangeException, but new converter should match golden mapping");
            return;
        }

        var result = _newConverter.Convert(input);
        Assert.That(result, Is.EqualTo(originalResult));
    }
}
