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
    public void Write_Throws_When_Converter_Instance_Is_Reused_With_Different_Options()
    {
        // The two options must be configurably distinct: System.Text.Json shares a single cached JsonTypeInfo
        // across JsonSerializerOptions instances with equivalent settings, so options that are "equal enough"
        // would never actually surface as different instances to the converter.
        var converter = new BlockItemDataConverter();
        var firstOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { converter } };
        var secondOptions = new JsonSerializerOptions { PropertyNamingPolicy = null, Converters = { converter } };

        BlockItemData model = CreateModel([(null, "title", null)]);

        JsonSerializer.Serialize(model, firstOptions);

        Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize(model, secondOptions));
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
