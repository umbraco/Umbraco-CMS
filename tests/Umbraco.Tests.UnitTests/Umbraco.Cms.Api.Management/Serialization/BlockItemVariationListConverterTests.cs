using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Serialization;

[TestFixture]
public class BlockItemVariationListConverterTests
{
    private JsonSerializerOptions _jsonSerializerOptions;

    [SetUp]
    public void SetUp()
        => _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new BlockItemVariationListConverter() },
        };

    [Test]
    public void Write_Orders_By_Culture_Then_Segment_Then_ContentKey()
    {
        var contentKey1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var contentKey2 = Guid.Parse("00000000-0000-0000-0000-000000000002");

        // Two entries share (culture, segment) - content key is the deciding tiebreaker between them.
        // BlockItemVariation has no property alias, so the canonical (culture, segment, alias) key
        // falls back to content key in its place.
        IList<BlockItemVariation> model =
        [
            new BlockItemVariation(contentKey2, "en-us", "segment-a"),
            new BlockItemVariation(contentKey1, "en-us", "segment-a"),
            new BlockItemVariation(contentKey1, "da-dk", null),
            new BlockItemVariation(contentKey2, null, null),
        ];

        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);

        using JsonDocument document = JsonDocument.Parse(json);
        var actualOrder = document.RootElement.EnumerateArray()
            .Select(value => (
                Culture: value.TryGetProperty("culture", out JsonElement culture) && culture.ValueKind != JsonValueKind.Null ? culture.GetString() : null,
                Segment: value.TryGetProperty("segment", out JsonElement segment) && segment.ValueKind != JsonValueKind.Null ? segment.GetString() : null,
                ContentKey: value.GetProperty("contentKey").GetGuid()))
            .ToArray();

        var expectedOrder = new (string Culture, string Segment, Guid ContentKey)[]
        {
            (null, null, contentKey2),
            ("da-dk", null, contentKey1),
            ("en-us", "segment-a", contentKey1),
            ("en-us", "segment-a", contentKey2),
        };

        CollectionAssert.AreEqual(expectedOrder, actualOrder);
    }

    [Test]
    public void Read_Delegates_To_Default_Deserialization()
    {
        IList<BlockItemVariation> model =
        [
            new BlockItemVariation(Guid.NewGuid(), "en-us", null),
            new BlockItemVariation(Guid.NewGuid(), "da-dk", null),
        ];

        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);

        IList<BlockItemVariation> deserialized = JsonSerializer.Deserialize<IList<BlockItemVariation>>(json, _jsonSerializerOptions);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.Select(v => v.ContentKey), Is.EquivalentTo(model.Select(v => v.ContentKey)));
            Assert.That(deserialized.Select(v => v.Culture), Is.EquivalentTo(model.Select(v => v.Culture)));
        });
    }

    [Test]
    public void Write_Throws_When_Converter_Instance_Is_Reused_With_Different_Options()
    {
        // The two options must be configurably distinct: System.Text.Json shares a single cached JsonTypeInfo
        // across JsonSerializerOptions instances with equivalent settings, so options that are "equal enough"
        // would never actually surface as different instances to the converter.
        var converter = new BlockItemVariationListConverter();
        var firstOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { converter } };
        var secondOptions = new JsonSerializerOptions { PropertyNamingPolicy = null, Converters = { converter } };

        IList<BlockItemVariation> model = [new BlockItemVariation(Guid.NewGuid(), null, null)];

        JsonSerializer.Serialize(model, firstOptions);

        Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize(model, secondOptions));
    }
}
