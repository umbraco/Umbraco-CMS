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

        if (File.Exists(testDataPath))
        {
            var json = File.ReadAllText(testDataPath);
            var doc = JsonDocument.Parse(json);
            GoldenMappings = doc.RootElement
                .GetProperty("mappings")
                .EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.GetString() ?? "");
        }
        else
        {
            GoldenMappings = new Dictionary<string, string>();
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
        var originalResult = Utf8ToAsciiConverter.ToAsciiString(input);
        var newResult = _newConverter.Convert(input);

        Assert.That(newResult, Is.EqualTo(originalResult));
    }
}
