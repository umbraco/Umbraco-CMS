using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Serialization;

[TestFixture]
public class BlockValueJsonTypeInfoModifiersTests
{
    private JsonSerializerOptions _jsonSerializerOptions;

    [SetUp]
    public void SetUp()
        => _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new ModifyingJsonTypeInfoResolver(new UmbracoJsonTypeInfoResolver(TestHelper.GetTypeFinder()), BlockValueJsonTypeInfoModifiers.Apply),
        };

    [Test]
    public void Write_Orders_BlockItemData_Values_By_Culture_Then_Segment_Then_Alias()
    {
        // Deliberately picks aliases/segments so an alias-first sort would produce a different (wrong)
        // order than the expected (culture, segment, alias) one.
        BlockItemData model = CreateBlockItemData(
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
    public void Write_Does_Not_Mutate_Unrelated_BlockItemData_Properties()
    {
        BlockItemData model = CreateBlockItemData([(null, "title", null)]);

        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);

        using JsonDocument document = JsonDocument.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(document.RootElement.GetProperty("key").GetGuid(), Is.EqualTo(model.Key));
            Assert.That(document.RootElement.GetProperty("contentTypeKey").GetGuid(), Is.EqualTo(model.ContentTypeKey));
        });
    }

    [Test]
    public void Write_Does_Not_Leave_BlockItemData_Values_Reordered_On_The_Original_Instance()
    {
        // Culture order here ("en-us" before "da-dk") is deliberately not the sorted order, so a leaked
        // mutation from the OnSerializing/OnSerialized pair would be observable as a changed Values order
        // on the original instance.
        BlockItemData model = CreateBlockItemData([("en-us", "title", null), ("da-dk", "title", null)]);
        var originalOrder = model.Values.Select(v => v.Culture).ToArray();

        JsonSerializer.Serialize(model, _jsonSerializerOptions);

        CollectionAssert.AreEqual(originalOrder, model.Values.Select(v => v.Culture).ToArray());
    }

    [Test]
    public void Read_Delegates_BlockItemData_To_Default_Deserialization()
    {
        BlockItemData model = CreateBlockItemData([("en-us", "title", null), ("da-dk", "title", null)]);

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
    public void Write_Orders_Expose_By_Culture_Then_Segment_Then_ContentKey()
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
    public void Write_Does_Not_Leave_Expose_Reordered_On_The_Original_Instance()
    {
        // Culture order here ("en-us" before "da-dk") is deliberately not the sorted order, so a leaked
        // mutation from the OnSerializing/OnSerialized pair would be observable as a changed order on the
        // original list instance.
        IList<BlockItemVariation> model =
        [
            new BlockItemVariation(Guid.NewGuid(), "en-us", null),
            new BlockItemVariation(Guid.NewGuid(), "da-dk", null),
        ];
        var originalOrder = model.Select(v => v.Culture).ToArray();

        JsonSerializer.Serialize(model, _jsonSerializerOptions);

        CollectionAssert.AreEqual(originalOrder, model.Select(v => v.Culture).ToArray());
    }

    [Test]
    public void Read_Delegates_Expose_To_Default_Deserialization()
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
    public void Apply_Is_Stateless_Across_Different_Options_Instances()
    {
        // Apply is a stateless static method shared across every JsonSerializerOptions that wires it in -
        // unlike the previous JsonConverter-based design, there is no per-instance cache keyed by options that
        // could confuse one options instance for another.
        var firstOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, TypeInfoResolver = new ModifyingJsonTypeInfoResolver(new UmbracoJsonTypeInfoResolver(TestHelper.GetTypeFinder()), BlockValueJsonTypeInfoModifiers.Apply) };
        var secondOptions = new JsonSerializerOptions { PropertyNamingPolicy = null, TypeInfoResolver = new ModifyingJsonTypeInfoResolver(new UmbracoJsonTypeInfoResolver(TestHelper.GetTypeFinder()), BlockValueJsonTypeInfoModifiers.Apply) };

        BlockItemData model = CreateBlockItemData([(null, "title", null)]);

        var firstJson = JsonSerializer.Serialize(model, firstOptions);
        var secondJson = JsonSerializer.Serialize(model, secondOptions);

        Assert.Multiple(() =>
        {
            Assert.That(firstJson, Does.Contain("\"values\""));
            Assert.That(secondJson, Does.Contain("\"Values\""));
        });
    }

    private static BlockItemData CreateBlockItemData(IEnumerable<(string? Culture, string Alias, string? Segment)> values)
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
