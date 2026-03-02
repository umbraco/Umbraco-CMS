using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors.Patching;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Patching;

[TestFixture]
public class PatchEngineTests
{
    [Test]
    public void Replace_SimpleProperty_UpdatesValue()
    {
        var json = """
        {
            "name": "Original Name",
            "title": "Original Title"
        }
        """;

        var result = PatchEngine.ApplyOperation(json, PatchOperationType.Replace, "/name", "Updated Name");

        var doc = JsonDocument.Parse(result);
        Assert.That(doc.RootElement.GetProperty("name").GetString(), Is.EqualTo("Updated Name"));
        Assert.That(doc.RootElement.GetProperty("title").GetString(), Is.EqualTo("Original Title"));
    }

    [Test]
    public void Replace_ArrayElementProperty_UpdatesValue()
    {
        var json = """
        {
            "values": [
                { "alias": "title", "value": "Original Value" },
                { "alias": "description", "value": "Description Value" }
            ]
        }
        """;

        var result = PatchEngine.ApplyOperation(
            json, PatchOperationType.Replace,
            "/values[alias=title]/value",
            "Updated Value");

        var doc = JsonDocument.Parse(result);
        var values = doc.RootElement.GetProperty("values").EnumerateArray().ToList();
        Assert.That(values[0].GetProperty("value").GetString(), Is.EqualTo("Updated Value"));
        Assert.That(values[1].GetProperty("value").GetString(), Is.EqualTo("Description Value"));
    }

    [Test]
    public void Replace_NestedProperty_UpdatesValue()
    {
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

        var result = PatchEngine.ApplyOperation(
            json,
            PatchOperationType.Replace,
            "/values[alias=contentBlocks]/value/contentData[key=block-2]/values[alias=headline]/value",
            "Updated Block 2 Headline");

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
    public void Replace_NullFilters_UpdatesValue()
    {
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

        var result = PatchEngine.ApplyOperation(
            json,
            PatchOperationType.Replace,
            "/values[alias=contentBlocks,culture=null,segment=null]/value/contentData[key=block-2]/values[alias=headline]/value",
            "Updated Block 2 Headline");

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
    public void Replace_NoMatches_ThrowsInvalidOperationException()
    {
        var json = """
        {
            "values": []
        }
        """;

        Assert.Throws<InvalidOperationException>(() =>
            PatchEngine.ApplyOperation(json, PatchOperationType.Replace, "/values[alias=nonexistent]/value", "New Value"));
    }

    [Test]
    public void Replace_RealBlockListStructure_UpdatesValue()
    {
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

        var path = $"/values[alias=contentBlocks,culture=null,segment=null]/value/contentData[key={blockKey}]/values[alias=headline]/value";

        var result = PatchEngine.ApplyOperation(json, PatchOperationType.Replace, path, "Updated Block 2 Headline");

        var doc = JsonDocument.Parse(result);
        var values = doc.RootElement.GetProperty("values").EnumerateArray().First();
        var contentData = values.GetProperty("value").GetProperty("contentData").EnumerateArray().ToList();

        var block2 = contentData.First(b => b.GetProperty("key").GetString() == blockKey);
        var headline = block2.GetProperty("values").EnumerateArray().First(v => v.GetProperty("alias").GetString() == "headline");
        Assert.That(headline.GetProperty("value").GetString(), Is.EqualTo("Updated Block 2 Headline"));

        // Verify block 1 wasn't changed
        var block1 = contentData.First(b => b.GetProperty("key").GetString() == "629e4129-3128-4367-9dac-d90edbfa68df");
        var block1Headline = block1.GetProperty("values").EnumerateArray().First(v => v.GetProperty("alias").GetString() == "headline");
        Assert.That(block1Headline.GetProperty("value").GetString(), Is.EqualTo("Block 1 Headline"));
    }

    [Test]
    public void Add_AppendToArray_AddsElementAtEnd()
    {
        var json = """
        {
            "contentData": [
                { "key": "block-1", "value": "first" }
            ]
        }
        """;

        var newBlock = new { key = "block-2", value = "second" };

        var result = PatchEngine.ApplyOperation(json, PatchOperationType.Add, "/contentData/-", newBlock);

        var doc = JsonDocument.Parse(result);
        var contentData = doc.RootElement.GetProperty("contentData").EnumerateArray().ToList();
        Assert.That(contentData, Has.Count.EqualTo(2));
        Assert.That(contentData[0].GetProperty("key").GetString(), Is.EqualTo("block-1"));
        Assert.That(contentData[1].GetProperty("key").GetString(), Is.EqualTo("block-2"));
    }

    [Test]
    public void Add_InsertAtIndex_InsertsElement()
    {
        var json = """
        {
            "items": ["a", "b", "c"]
        }
        """;

        var result = PatchEngine.ApplyOperation(json, PatchOperationType.Add, "/items/1", "inserted");

        var doc = JsonDocument.Parse(result);
        var items = doc.RootElement.GetProperty("items").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.That(items, Is.EqualTo(new[] { "a", "inserted", "b", "c" }));
    }

    [Test]
    public void Add_NewObjectProperty_AddsProperty()
    {
        var json = """
        {
            "name": "Test"
        }
        """;

        var result = PatchEngine.ApplyOperation(json, PatchOperationType.Add, "/description", "New Description");

        var doc = JsonDocument.Parse(result);
        Assert.That(doc.RootElement.GetProperty("name").GetString(), Is.EqualTo("Test"));
        Assert.That(doc.RootElement.GetProperty("description").GetString(), Is.EqualTo("New Description"));
    }

    [Test]
    public void Add_AppendToNestedArray_AddsElement()
    {
        var json = """
        {
            "values": [
                {
                    "alias": "contentBlocks",
                    "culture": null,
                    "segment": null,
                    "value": {
                        "contentData": [
                            { "key": "block-1", "values": [] }
                        ]
                    }
                }
            ]
        }
        """;

        var newBlock = new { key = "block-2", values = Array.Empty<object>() };

        var result = PatchEngine.ApplyOperation(
            json,
            PatchOperationType.Add,
            "/values[alias=contentBlocks,culture=null,segment=null]/value/contentData/-",
            newBlock);

        var doc = JsonDocument.Parse(result);
        var contentData = doc.RootElement
            .GetProperty("values").EnumerateArray().First()
            .GetProperty("value")
            .GetProperty("contentData").EnumerateArray().ToList();
        Assert.That(contentData, Has.Count.EqualTo(2));
        Assert.That(contentData[1].GetProperty("key").GetString(), Is.EqualTo("block-2"));
    }

    [Test]
    public void Remove_ArrayElementByFilter_RemovesElement()
    {
        var json = """
        {
            "values": [
                { "alias": "title", "value": "Title Value" },
                { "alias": "description", "value": "Description Value" }
            ]
        }
        """;

        var result = PatchEngine.ApplyOperation(json, PatchOperationType.Remove, "/values[alias=title]", null);

        var doc = JsonDocument.Parse(result);
        var values = doc.RootElement.GetProperty("values").EnumerateArray().ToList();
        Assert.That(values, Has.Count.EqualTo(1));
        Assert.That(values[0].GetProperty("alias").GetString(), Is.EqualTo("description"));
    }

    [Test]
    public void Remove_ArrayElementByIndex_RemovesElement()
    {
        var json = """
        {
            "items": ["a", "b", "c"]
        }
        """;

        var result = PatchEngine.ApplyOperation(json, PatchOperationType.Remove, "/items/1", null);

        var doc = JsonDocument.Parse(result);
        var items = doc.RootElement.GetProperty("items").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.That(items, Is.EqualTo(new[] { "a", "c" }));
    }

    [Test]
    public void Remove_ObjectProperty_RemovesProperty()
    {
        var json = """
        {
            "name": "Test",
            "description": "To Remove"
        }
        """;

        var result = PatchEngine.ApplyOperation(json, PatchOperationType.Remove, "/description", null);

        var doc = JsonDocument.Parse(result);
        Assert.That(doc.RootElement.GetProperty("name").GetString(), Is.EqualTo("Test"));
        Assert.That(doc.RootElement.TryGetProperty("description", out _), Is.False);
    }

    [Test]
    public void Replace_WithAppendSegment_ThrowsInvalidOperationException()
    {
        var json = """
        {
            "items": ["a", "b"]
        }
        """;

        Assert.Throws<InvalidOperationException>(() =>
            PatchEngine.ApplyOperation(json, PatchOperationType.Replace, "/items/-", "new"));
    }

    [Test]
    public void Remove_WithAppendSegment_ThrowsInvalidOperationException()
    {
        var json = """
        {
            "items": ["a", "b"]
        }
        """;

        Assert.Throws<InvalidOperationException>(() =>
            PatchEngine.ApplyOperation(json, PatchOperationType.Remove, "/items/-", null));
    }

    [Test]
    public void ApplyOperation_InvalidPath_ThrowsFormatException()
    {
        var json = """{ "name": "test" }""";

        Assert.Throws<FormatException>(() =>
            PatchEngine.ApplyOperation(json, PatchOperationType.Replace, "name", "new"));
    }

    [Test]
    public void ApplyOperation_NullJson_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            PatchEngine.ApplyOperation(null!, PatchOperationType.Replace, "/name", "new"));
    }

    [Test]
    public void Replace_OutputCanBeDeserialized()
    {
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

        var result = PatchEngine.ApplyOperation(
            json,
            PatchOperationType.Replace,
            "/values[alias=contentBlocks,culture=null,segment=null]/value/contentData[key=block-2]/values[alias=headline]/value",
            "Updated Block 2");

        // Verify the result is valid JSON
        var doc = JsonDocument.Parse(result);

        // Extract the value property (simulates what JsonObjectConverter returns)
        var valueElement = doc.RootElement.GetProperty("values").EnumerateArray().First().GetProperty("value");
        var valueJson = valueElement.GetRawText();

        // Verify the modified value is in the JSON string
        Assert.That(valueJson, Does.Contain("Updated Block 2"));

        // Parse and verify the structure is maintained
        var reparsed = JsonDocument.Parse(valueJson);
        var contentData = reparsed.RootElement.GetProperty("contentData").EnumerateArray().ToList();
        Assert.That(contentData, Has.Count.EqualTo(2));

        var block2 = contentData.First(b => b.GetProperty("key").GetString() == "block-2");
        var headline = block2.GetProperty("values").EnumerateArray().First().GetProperty("value").GetString();
        Assert.That(headline, Is.EqualTo("Updated Block 2"));
    }

    [Test]
    public void Replace_MultipleFilterConditions_MatchesCorrectElement()
    {
        var json = """
        {
            "values": [
                { "alias": "price", "culture": "en-US", "segment": "standard", "value": "100" },
                { "alias": "price", "culture": "en-US", "segment": "premium", "value": "150" },
                { "alias": "price", "culture": "da-DK", "segment": "premium", "value": "1000" }
            ]
        }
        """;

        var result = PatchEngine.ApplyOperation(
            json,
            PatchOperationType.Replace,
            "/values[alias=price,culture=en-US,segment=premium]/value",
            "200");

        var doc = JsonDocument.Parse(result);
        var values = doc.RootElement.GetProperty("values").EnumerateArray().ToList();
        Assert.That(values[0].GetProperty("value").GetString(), Is.EqualTo("100"));
        Assert.That(values[1].GetProperty("value").GetString(), Is.EqualTo("200"));
        Assert.That(values[2].GetProperty("value").GetString(), Is.EqualTo("1000"));
    }
}
