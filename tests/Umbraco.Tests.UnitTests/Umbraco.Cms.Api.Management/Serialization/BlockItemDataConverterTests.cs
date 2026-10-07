using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Serialization;

[TestFixture]
public class BlockItemDataConverterTests
{
    private JsonSerializerOptions _jsonSerializerOptions;

    [SetUp]
    public void SetUp()
        => _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new BlockItemDataConverter() },
        };

    [Test]
    public void Write_Orders_Values_By_Culture_Then_Segment_Then_Alias()
    {
        // Deliberately picks aliases/segments so an alias-first sort would produce a different (wrong)
        // order than the expected (culture, segment, alias) one.
        BlockItemData model = CreateModel(
        [
            ("en-us", "zzz-alias", "segment-a"),
            ("en-us", "aaa-alias", "segment-b"),
            ("en-us", "mmm-alias", null),
            ("da-dk", "title", null),
            (null, "title", null),
        ]);

        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);

        using JsonDocument document = JsonDocument.Parse(json);
        var actualOrder = document.RootElement.GetProperty("values")
            .EnumerateArray()
            .Select(value => (
                Culture: value.TryGetProperty("culture", out JsonElement culture) && culture.ValueKind != JsonValueKind.Null ? culture.GetString() : null,
                Segment: value.TryGetProperty("segment", out JsonElement segment) && segment.ValueKind != JsonValueKind.Null ? segment.GetString() : null,
                Alias: value.GetProperty("alias").GetString()))
            .ToArray();

        var expectedOrder = new (string Culture, string Segment, string Alias)[]
        {
            (null, null, "title"),
            ("da-dk", null, "title"),
            ("en-us", null, "mmm-alias"),
            ("en-us", "segment-a", "zzz-alias"),
            ("en-us", "segment-b", "aaa-alias"),
        };

        CollectionAssert.AreEqual(expectedOrder, actualOrder);
    }

    [Test]
    public void Write_Does_Not_Mutate_Unrelated_Properties()
    {
        BlockItemData model = CreateModel([(null, "title", null)]);

        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);

        using JsonDocument document = JsonDocument.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(document.RootElement.GetProperty("key").GetGuid(), Is.EqualTo(model.Key));
            Assert.That(document.RootElement.GetProperty("contentTypeKey").GetGuid(), Is.EqualTo(model.ContentTypeKey));
        });
    }

    [Test]
    public void Write_Does_Not_Leave_Values_Reordered_On_The_Original_Instance()
    {
        // Culture order here ("en-us" before "da-dk") is deliberately not the sorted order, so a leaked
        // mutation from Write would be observable as a changed Values order on the original instance.
        BlockItemData model = CreateModel([("en-us", "title", null), ("da-dk", "title", null)]);
        var originalOrder = model.Values.Select(v => v.Culture).ToArray();

        JsonSerializer.Serialize(model, _jsonSerializerOptions);

        CollectionAssert.AreEqual(originalOrder, model.Values.Select(v => v.Culture).ToArray());
    }

    [Test]
    public void Read_Delegates_To_Default_Deserialization()
    {
        BlockItemData model = CreateModel([("en-us", "title", null), ("da-dk", "title", null)]);

        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);

        BlockItemData deserialized = JsonSerializer.Deserialize<BlockItemData>(json, _jsonSerializerOptions);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.Key, Is.EqualTo(model.Key));
            Assert.That(deserialized.ContentTypeKey, Is.EqualTo(model.ContentTypeKey));
            Assert.That(deserialized.Values.Select(v => v.Alias), Is.EquivalentTo(model.Values.Select(v => v.Alias)));
            Assert.That(deserialized.Values.Select(v => v.Culture), Is.EquivalentTo(model.Values.Select(v => v.Culture)));
        });
    }

    [Test]
    public void Write_Reused_With_Different_Options_Produces_Independently_Correct_Output()
    {
        // A converter instance is a long-lived singleton in production (registered once at startup), so the
        // same instance can end up handling more than one JsonSerializerOptions instance - not just when
        // literally registered twice, but also when a block nests another block of the same property type
        // (see JsonBlockValueConverterIntegrationTests.Write_Sorts_Nested_Block_Expose_And_ContentData_Without_Throwing).
        // Each options instance must get its own correctly-derived clone rather than the first one seen.
        var converter = new BlockItemDataConverter();
        var firstOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { converter } };
        var secondOptions = new JsonSerializerOptions { PropertyNamingPolicy = null, Converters = { converter } };

        BlockItemData model = CreateModel([(null, "title", null)]);

        var firstJson = JsonSerializer.Serialize(model, firstOptions);
        var secondJson = JsonSerializer.Serialize(model, secondOptions);

        Assert.Multiple(() =>
        {
            Assert.That(firstJson, Does.Contain("\"values\""));
            Assert.That(secondJson, Does.Contain("\"Values\""));
        });
    }

    private static BlockItemData CreateModel(IEnumerable<(string? Culture, string Alias, string? Segment)> values)
        => new(Guid.NewGuid(), Guid.NewGuid(), "myElementType")
        {
            Values = values.Select(value => new BlockPropertyValue
            {
                Culture = value.Culture,
                Alias = value.Alias,
                Segment = value.Segment,
                Value = "some value",
            }).ToList(),
        };
}
