using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors.JsonPath;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors.JsonPath;

[TestFixture]
public class JsonPathEvaluatorTests
{
    private JsonPathEvaluator _evaluator = null!;

    [SetUp]
    public void SetUp()
    {
        _evaluator = new JsonPathEvaluator();
    }

    [Test]
    public void Select_SimpleProperty_ReturnsMatchingElement()
    {
        // Arrange
        var json = """
        {
            "name": "Test Document",
            "title": "Test Title"
        }
        """;
        var doc = JsonDocument.Parse(json);

        // Act
        var results = _evaluator.Select(doc, "$.name");

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].GetString(), Is.EqualTo("Test Document"));
    }

    [Test]
    public void Select_ArrayFilterByProperty_ReturnsMatchingElements()
    {
        // Arrange
        var json = """
        {
            "values": [
                { "alias": "title", "value": "Title Value" },
                { "alias": "description", "value": "Description Value" }
            ]
        }
        """;
        var doc = JsonDocument.Parse(json);

        // Act
        var results = _evaluator.Select(doc, "$.values[?(@.alias == 'title')]");

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].GetProperty("value").GetString(), Is.EqualTo("Title Value"));
    }

    [Test]
    public void Select_MultipleFilters_ReturnsMatchingElements()
    {
        // Arrange
        var json = """
        {
            "values": [
                { "alias": "title", "culture": "en-US", "value": "English Title" },
                { "alias": "title", "culture": "da-DK", "value": "Danish Title" },
                { "alias": "description", "culture": "en-US", "value": "English Description" }
            ]
        }
        """;
        var doc = JsonDocument.Parse(json);

        // Act
        var results = _evaluator.Select(doc, "$.values[?(@.alias == 'title' && @.culture == 'en-US')]");

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].GetProperty("value").GetString(), Is.EqualTo("English Title"));
    }

    [Test]
    public void Select_NestedPath_ReturnsMatchingElements()
    {
        // Arrange
        var json = """
        {
            "values": [
                {
                    "alias": "contentBlocks",
                    "value": {
                        "contentData": [
                            { "key": "12345678-1234-1234-1234-123456789012", "values": [{ "alias": "headline", "value": "Block Headline" }] }
                        ]
                    }
                }
            ]
        }
        """;
        var doc = JsonDocument.Parse(json);

        // Act
        var results = _evaluator.Select(doc, "$.values[?(@.alias == 'contentBlocks')].value.contentData[?(@.key == '12345678-1234-1234-1234-123456789012')].values[?(@.alias == 'headline')]");

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].GetProperty("value").GetString(), Is.EqualTo("Block Headline"));
    }

    [Test]
    public void Select_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        var json = """
        {
            "values": [
                { "alias": "title", "value": "Title Value" }
            ]
        }
        """;
        var doc = JsonDocument.Parse(json);

        // Act
        var results = _evaluator.Select(doc, "$.values[?(@.alias == 'nonexistent')]");

        // Assert
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void SelectSingle_ReturnsFirstMatch()
    {
        // Arrange
        var json = """
        {
            "values": [
                { "alias": "title", "value": "First" },
                { "alias": "title", "value": "Second" }
            ]
        }
        """;
        var doc = JsonDocument.Parse(json);

        // Act
        var result = _evaluator.SelectSingle(doc, "$.values[?(@.alias == 'title')]");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.GetProperty("value").GetString(), Is.EqualTo("First"));
    }

    [Test]
    public void SelectSingle_NoMatches_ReturnsNull()
    {
        // Arrange
        var json = """
        {
            "values": []
        }
        """;
        var doc = JsonDocument.Parse(json);

        // Act
        var result = _evaluator.SelectSingle(doc, "$.values[?(@.alias == 'title')]");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Exists_MatchFound_ReturnsTrue()
    {
        // Arrange
        var json = """
        {
            "values": [
                { "alias": "title", "value": "Title Value" }
            ]
        }
        """;
        var doc = JsonDocument.Parse(json);

        // Act
        var exists = _evaluator.Exists(doc, "$.values[?(@.alias == 'title')]");

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public void Exists_NoMatch_ReturnsFalse()
    {
        // Arrange
        var json = """
        {
            "values": []
        }
        """;
        var doc = JsonDocument.Parse(json);

        // Act
        var exists = _evaluator.Exists(doc, "$.values[?(@.alias == 'title')]");

        // Assert
        Assert.That(exists, Is.False);
    }

    [Test]
    public void IsValidExpression_ValidExpression_ReturnsTrue()
    {
        // Act
        var isValid = _evaluator.IsValidExpression("$.values[?(@.alias == 'title')]");

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValidExpression_InvalidExpression_ReturnsFalse()
    {
        // Act
        var isValid = _evaluator.IsValidExpression("$.values[?(@.alias == ");

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidExpression_EmptyString_ReturnsFalse()
    {
        // Act
        var isValid = _evaluator.IsValidExpression(string.Empty);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void Select_NullDocument_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _evaluator.Select(null!, "$.name"));
    }

    [Test]
    public void Select_EmptyExpression_ThrowsArgumentException()
    {
        // Arrange
        var json = "{}";
        var doc = JsonDocument.Parse(json);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _evaluator.Select(doc, string.Empty));
    }
}
