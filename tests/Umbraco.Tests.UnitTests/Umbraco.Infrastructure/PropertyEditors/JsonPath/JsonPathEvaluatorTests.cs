using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.ContentEditing;
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

    [Test]
    public void Select_ThreeFiltersWithSegment_ReturnsMatchingElements()
    {
        // Arrange
        var json = """
        {
            "values": [
                { "alias": "price", "culture": "en-US", "segment": "standard", "value": "100" },
                { "alias": "price", "culture": "en-US", "segment": "premium", "value": "150" },
                { "alias": "price", "culture": "da-DK", "segment": "premium", "value": "1000" }
            ]
        }
        """;
        var doc = JsonDocument.Parse(json);

        // Act
        var results = _evaluator.Select(doc, "$.values[?(@.alias == 'price' && @.culture == 'en-US' && @.segment == 'premium')]");

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].GetProperty("value").GetString(), Is.EqualTo("150"));
    }

    [Test]
    public void IsValidExpression_ThreeFiltersWithSegment_ReturnsTrue()
    {
        // Act
        var isValid = _evaluator.IsValidExpression("$.values[?(@.alias == 'price' && @.culture == 'en-US' && @.segment == 'premium')].value");

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void ApplyOperation_ReplaceSimpleProperty_UpdatesValue()
    {
        // Arrange
        var json = """
        {
            "name": "Original Name",
            "title": "Original Title"
        }
        """;

        // Act
        var result = _evaluator.ApplyOperation(json, PatchOperationType.Replace, "$.name", "Updated Name");

        // Assert
        var doc = JsonDocument.Parse(result);
        Assert.That(doc.RootElement.GetProperty("name").GetString(), Is.EqualTo("Updated Name"));
        Assert.That(doc.RootElement.GetProperty("title").GetString(), Is.EqualTo("Original Title"));
    }

    [Test]
    public void ApplyOperation_ReplaceArrayElementProperty_UpdatesValue()
    {
        // Arrange
        var json = """
        {
            "values": [
                { "alias": "title", "value": "Original Value" },
                { "alias": "description", "value": "Description Value" }
            ]
        }
        """;

        // Act
        var result = _evaluator.ApplyOperation(json, PatchOperationType.Replace, "$.values[?(@.alias == 'title')].value", "Updated Value");

        // Assert
        var doc = JsonDocument.Parse(result);
        var values = doc.RootElement.GetProperty("values").EnumerateArray().ToList();
        Assert.That(values[0].GetProperty("value").GetString(), Is.EqualTo("Updated Value"));
        Assert.That(values[1].GetProperty("value").GetString(), Is.EqualTo("Description Value"));
    }

    [Test]
    public void ApplyOperation_ReplaceNestedProperty_UpdatesValue()
    {
        // Arrange
        var json = """
        {
            "values": [
                {
                    "alias": "contentBlocks",
                    "value": {
                        "contentData": [
                            { "key": "block-1", "values": [{ "alias": "headline", "value": "Original" }] },
                            { "key": "block-2", "values": [{ "alias": "headline", "value": "Block 2 Headline" }] }
                        ]
                    }
                }
            ]
        }
        """;

        // Act
        var result = _evaluator.ApplyOperation(
            json,
            PatchOperationType.Replace,
            "$.values[?(@.alias == 'contentBlocks')].value.contentData[?(@.key == 'block-2')].values[?(@.alias == 'headline')].value",
            "Updated Block 2 Headline");

        // Assert
        var doc = JsonDocument.Parse(result);
        var contentBlocks = doc.RootElement.GetProperty("values").EnumerateArray().First();
        var contentData = contentBlocks.GetProperty("value").GetProperty("contentData").EnumerateArray().ToList();
        var block2 = contentData.First(b => b.GetProperty("key").GetString() == "block-2");
        var headline = block2.GetProperty("values").EnumerateArray().First(v => v.GetProperty("alias").GetString() == "headline");
        Assert.That(headline.GetProperty("value").GetString(), Is.EqualTo("Updated Block 2 Headline"));

        // Verify block 1 wasn't changed
        var block1 = contentData.First(b => b.GetProperty("key").GetString() == "block-1");
        var block1Headline = block1.GetProperty("values").EnumerateArray().First(v => v.GetProperty("alias").GetString() == "headline");
        Assert.That(block1Headline.GetProperty("value").GetString(), Is.EqualTo("Original"));
    }

    [Test]
    public void ApplyOperation_NoMatches_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = """
        {
            "values": []
        }
        """;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            _evaluator.ApplyOperation(json, PatchOperationType.Replace, "$.values[?(@.alias == 'nonexistent')].value", "New Value"));
    }

    [Test]
    public void Select_NullComparison_ReturnsMatchingElements()
    {
        // Arrange - Test null comparison in JSONPath filter
        var json = """
        {
            "values": [
                { "alias": "title", "culture": null, "segment": null, "value": "Invariant Value" },
                { "alias": "title", "culture": "en-US", "segment": null, "value": "English Value" }
            ]
        }
        """;
        var doc = JsonDocument.Parse(json);

        // Act - Filter by null culture
        var results = _evaluator.Select(doc, "$.values[?(@.alias == 'title' && @.culture == null)]");

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].GetProperty("value").GetString(), Is.EqualTo("Invariant Value"));
    }

    [Test]
    public void Select_MultipleNullComparisons_ReturnsMatchingElements()
    {
        // Arrange - Test multiple null comparisons
        var json = """
        {
            "values": [
                { "alias": "title", "culture": null, "segment": null, "value": "Invariant" },
                { "alias": "title", "culture": null, "segment": "premium", "value": "Invariant Premium" },
                { "alias": "title", "culture": "en-US", "segment": null, "value": "English" }
            ]
        }
        """;
        var doc = JsonDocument.Parse(json);

        // Act - Filter by both null culture and null segment
        var results = _evaluator.Select(doc, "$.values[?(@.alias == 'title' && @.culture == null && @.segment == null)]");

        // Assert
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].GetProperty("value").GetString(), Is.EqualTo("Invariant"));
    }

    [Test]
    public void ApplyOperation_ReplaceWithNullFilters_UpdatesValue()
    {
        // Arrange - Test replacement with null filters (exact scenario from Block List test)
        var json = """
        {
            "values": [
                {
                    "alias": "contentBlocks",
                    "culture": null,
                    "segment": null,
                    "value": {
                        "contentData": [
                            { "key": "block-1", "values": [{ "alias": "headline", "value": "Block 1 Headline" }] },
                            { "key": "block-2", "values": [{ "alias": "headline", "value": "Block 2 Headline" }] }
                        ]
                    }
                }
            ]
        }
        """;

        // Act
        var result = _evaluator.ApplyOperation(
            json,
            PatchOperationType.Replace,
            "$.values[?(@.alias == 'contentBlocks' && @.culture == null && @.segment == null)].value.contentData[?(@.key == 'block-2')].values[?(@.alias == 'headline')].value",
            "Updated Block 2 Headline");

        // Assert
        var doc = JsonDocument.Parse(result);
        var contentBlocks = doc.RootElement.GetProperty("values").EnumerateArray().First();
        var contentData = contentBlocks.GetProperty("value").GetProperty("contentData").EnumerateArray().ToList();

        var block2 = contentData.First(b => b.GetProperty("key").GetString() == "block-2");
        var headline = block2.GetProperty("values").EnumerateArray().First(v => v.GetProperty("alias").GetString() == "headline");
        Assert.That(headline.GetProperty("value").GetString(), Is.EqualTo("Updated Block 2 Headline"));

        // Verify block 1 wasn't changed
        var block1 = contentData.First(b => b.GetProperty("key").GetString() == "block-1");
        var block1Headline = block1.GetProperty("values").EnumerateArray().First(v => v.GetProperty("alias").GetString() == "headline");
        Assert.That(block1Headline.GetProperty("value").GetString(), Is.EqualTo("Block 1 Headline"));
    }

    [Test]
    public void ApplyOperation_RealBlockListStructure_UpdatesValue()
    {
        // Arrange - Use the exact JSON structure from the integration test
        var blockKey = "25acc650-ab47-40ba-b31b-c5dc0a9eb5b1";
        var json = $$"""
        {
          "template" : null,
          "values" : [ {
            "culture" : null,
            "segment" : null,
            "alias" : "contentBlocks",
            "value" : {
              "contentData" : [ {
                "contentTypeKey" : "98b8b6da-d9c1-47af-abd2-f99cf8150356",
                "udi" : null,
                "key" : "629e4129-3128-4367-9dac-d90edbfa68df",
                "values" : [ {
                  "editorAlias" : "Umbraco.TextBox",
                  "culture" : null,
                  "segment" : null,
                  "alias" : "headline",
                  "value" : "Block 1 Headline"
                } ]
              }, {
                "contentTypeKey" : "98b8b6da-d9c1-47af-abd2-f99cf8150356",
                "udi" : null,
                "key" : "{{blockKey}}",
                "values" : [ {
                  "editorAlias" : "Umbraco.TextBox",
                  "culture" : null,
                  "segment" : null,
                  "alias" : "headline",
                  "value" : "Block 2 Headline"
                } ]
              } ],
              "settingsData" : [ ],
              "expose" : [ ],
              "Layout" : {
                "Umbraco.BlockList" : [ ]
              }
            }
          } ],
          "variants" : [ {
            "culture" : null,
            "segment" : null,
            "name" : "My Blocks Document"
          } ]
        }
        """;

        var path = $"$.values[?(@.alias == 'contentBlocks' && @.culture == null && @.segment == null)].value.contentData[?(@.key == '{blockKey}')].values[?(@.alias == 'headline')].value";

        // Act
        var result = _evaluator.ApplyOperation(json, PatchOperationType.Replace, path, "Updated Block 2 Headline");

        // Assert
        var doc = JsonDocument.Parse(result);
        var values = doc.RootElement.GetProperty("values").EnumerateArray().First();
        var contentData = values.GetProperty("value").GetProperty("contentData").EnumerateArray().ToList();

        // Find block 2 by key
        var block2 = contentData.First(b => b.GetProperty("key").GetString() == blockKey);
        var headline = block2.GetProperty("values").EnumerateArray().First(v => v.GetProperty("alias").GetString() == "headline");
        Assert.That(headline.GetProperty("value").GetString(), Is.EqualTo("Updated Block 2 Headline"));

        // Verify block 1 wasn't changed
        var block1 = contentData.First(b => b.GetProperty("key").GetString() == "629e4129-3128-4367-9dac-d90edbfa68df");
        var block1Headline = block1.GetProperty("values").EnumerateArray().First(v => v.GetProperty("alias").GetString() == "headline");
        Assert.That(block1Headline.GetProperty("value").GetString(), Is.EqualTo("Block 1 Headline"));
    }

    [Test]
    public void JsonObject_ToString_ReturnsValidJson()
    {
        // This test verifies that JsonObject.ToString() returns valid JSON
        // which is critical for BlockEditorValues.DeserializeAndClean
        var json = """
        {
            "contentData": [
                { "key": "block-1", "values": [{ "alias": "headline", "value": "Block 1 Headline" }] },
                { "key": "block-2", "values": [{ "alias": "headline", "value": "Block 2 Headline" }] }
            ],
            "settingsData": [],
            "expose": [],
            "Layout": { "Umbraco.BlockList": [] }
        }
        """;

        // Parse to JsonObject (simulates what JsonObjectConverter returns)
        var jsonObject = JsonNode.Parse(json) as JsonObject;
        Assert.That(jsonObject, Is.Not.Null);

        // Verify ToString() returns valid JSON
        var toStringResult = jsonObject!.ToString();
        Assert.That(toStringResult, Is.Not.Null.And.Not.Empty);

        // Verify ToJsonString() also works
        var toJsonStringResult = jsonObject.ToJsonString();
        Assert.That(toJsonStringResult, Is.Not.Null.And.Not.Empty);

        // Verify both can be parsed back to valid JSON
        var reparsedFromToString = JsonDocument.Parse(toStringResult);
        Assert.That(reparsedFromToString.RootElement.GetProperty("contentData").GetArrayLength(), Is.EqualTo(2));

        var reparsedFromToJsonString = JsonDocument.Parse(toJsonStringResult);
        Assert.That(reparsedFromToJsonString.RootElement.GetProperty("contentData").GetArrayLength(), Is.EqualTo(2));

        // Verify both produce equivalent output
        Console.WriteLine($"ToString(): {toStringResult}");
        Console.WriteLine($"ToJsonString(): {toJsonStringResult}");
    }

    [Test]
    public void ApplyOperation_VerifyOutputCanBeDeserialized()
    {
        // This test verifies the full flow - apply operation, then verify the resulting
        // JSON can be correctly deserialized by downstream systems
        var json = """
        {
            "values": [
                {
                    "alias": "contentBlocks",
                    "culture": null,
                    "segment": null,
                    "value": {
                        "contentData": [
                            { "key": "block-1", "values": [{ "alias": "headline", "value": "Block 1" }] },
                            { "key": "block-2", "values": [{ "alias": "headline", "value": "Block 2" }] }
                        ],
                        "settingsData": [],
                        "expose": [],
                        "Layout": { "Umbraco.BlockList": [] }
                    }
                }
            ]
        }
        """;

        // Act - Apply operation
        var result = _evaluator.ApplyOperation(
            json,
            PatchOperationType.Replace,
            "$.values[?(@.alias == 'contentBlocks' && @.culture == null && @.segment == null)].value.contentData[?(@.key == 'block-2')].values[?(@.alias == 'headline')].value",
            "Updated Block 2");

        // Verify the result is valid JSON
        var doc = JsonDocument.Parse(result);

        // Extract the value property (which would be deserialized as JsonObject)
        var valueElement = doc.RootElement.GetProperty("values").EnumerateArray().First().GetProperty("value");
        var valueJson = valueElement.GetRawText();

        // Parse as JsonObject (simulates JsonObjectConverter behavior)
        var valueAsJsonObject = JsonNode.Parse(valueJson) as JsonObject;
        Assert.That(valueAsJsonObject, Is.Not.Null);

        // Verify ToString() produces valid JSON that can be parsed by BlockEditorValues
        var valueAsString = valueAsJsonObject!.ToString();
        Console.WriteLine($"Value as string: {valueAsString}");

        // Verify the modified value is in the JSON string
        Assert.That(valueAsString, Does.Contain("Updated Block 2"));

        // Parse and verify the structure is maintained
        var reparsed = JsonDocument.Parse(valueAsString);
        var contentData = reparsed.RootElement.GetProperty("contentData").EnumerateArray().ToList();
        Assert.That(contentData, Has.Count.EqualTo(2));

        var block2 = contentData.First(b => b.GetProperty("key").GetString() == "block-2");
        var headline = block2.GetProperty("values").EnumerateArray().First().GetProperty("value").GetString();
        Assert.That(headline, Is.EqualTo("Updated Block 2"));
    }
}
