using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Serialization;

/// <summary>
/// Verifies that <see cref="JsonBlockValueConverter"/> - which owns (de)serialization of the containing
/// <see cref="BlockValue"/> - actually routes its <see cref="BlockValue.ContentData"/>/<see cref="BlockValue.SettingsData"/>
/// and <see cref="BlockValue.Expose"/> through <see cref="BlockItemDataConverter"/> and
/// <see cref="BlockItemVariationListConverter"/> respectively, rather than each converter only being exercised
/// in isolation.
/// </summary>
[TestFixture]
public class JsonBlockValueConverterIntegrationTests
{
    [Test]
    public void Write_Sorts_ContentData_Values_When_BlockItemDataConverter_Is_Registered()
    {
        JsonSerializerOptions options = CreateOptions(includeBlockItemDataConverter: true, includeBlockItemVariationListConverter: false);

        var json = SerializeBlockListValueWithUnsortedContentDataValues(options);

        var order = GetContentDataValueCultures(json);
        CollectionAssert.AreEqual(new[] { "da-dk", "en-us" }, order);
    }

    [Test]
    public void Write_Does_Not_Sort_ContentData_Values_When_BlockItemDataConverter_Is_Not_Registered()
    {
        JsonSerializerOptions options = CreateOptions(includeBlockItemDataConverter: false, includeBlockItemVariationListConverter: false);

        var json = SerializeBlockListValueWithUnsortedContentDataValues(options);

        // Unsorted input order survives when BlockItemDataConverter isn't wired in - proving the previous
        // test's sorted output genuinely comes from that converter, not from JsonBlockValueConverter itself.
        var order = GetContentDataValueCultures(json);
        CollectionAssert.AreEqual(new[] { "en-us", "da-dk" }, order);
    }

    [Test]
    public void Write_Sorts_Expose_When_BlockItemVariationListConverter_Is_Registered()
    {
        JsonSerializerOptions options = CreateOptions(includeBlockItemDataConverter: false, includeBlockItemVariationListConverter: true);

        var json = SerializeBlockListValueWithUnsortedExpose(options);

        var order = GetExposeCultures(json);
        CollectionAssert.AreEqual(new[] { "da-dk", "en-us" }, order);
    }

    [Test]
    public void Write_Does_Not_Sort_Expose_When_BlockItemVariationListConverter_Is_Not_Registered()
    {
        JsonSerializerOptions options = CreateOptions(includeBlockItemDataConverter: false, includeBlockItemVariationListConverter: false);

        var json = SerializeBlockListValueWithUnsortedExpose(options);

        // Unsorted input order survives when BlockItemVariationListConverter isn't wired in - proving the
        // previous test's sorted output genuinely comes from that converter.
        var order = GetExposeCultures(json);
        CollectionAssert.AreEqual(new[] { "en-us", "da-dk" }, order);
    }

    [Test]
    public void Read_Round_Trips_BlockValue_When_Both_Converters_Are_Registered()
    {
        JsonSerializerOptions options = CreateOptions(includeBlockItemDataConverter: true, includeBlockItemVariationListConverter: true);

        var contentKey = Guid.NewGuid();
        var elementTypeKey = Guid.NewGuid();

        var original = new BlockListValue([new BlockListLayoutItem(contentKey)])
        {
            ContentData =
            [
                new BlockItemData(contentKey, elementTypeKey, "myElementType")
                {
                    Values =
                    [
                        new BlockPropertyValue { Culture = "en-us", Alias = "title", Value = "value-en" },
                        new BlockPropertyValue { Culture = "da-dk", Alias = "title", Value = "value-da" },
                    ],
                },
            ],
            Expose =
            [
                new BlockItemVariation(contentKey, "en-us", null),
                new BlockItemVariation(contentKey, "da-dk", null),
            ],
        };

        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<BlockListValue>(json, options);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.ContentData, Has.Count.EqualTo(1));
            Assert.That(deserialized.ContentData[0].Values.Select(v => v.Culture), Is.EquivalentTo(new[] { "en-us", "da-dk" }));
            Assert.That(deserialized.Expose.Select(v => v.Culture), Is.EquivalentTo(new[] { "en-us", "da-dk" }));
        });
    }

    private static JsonSerializerOptions CreateOptions(bool includeBlockItemDataConverter, bool includeBlockItemVariationListConverter)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonBlockValueConverter() },
        };

        if (includeBlockItemDataConverter)
        {
            options.Converters.Add(new BlockItemDataConverter());
        }

        if (includeBlockItemVariationListConverter)
        {
            options.Converters.Add(new BlockItemVariationListConverter());
        }

        return options;
    }

    private static string SerializeBlockListValueWithUnsortedContentDataValues(JsonSerializerOptions options)
    {
        var contentKey = Guid.NewGuid();
        var elementTypeKey = Guid.NewGuid();

        var blockListValue = new BlockListValue([new BlockListLayoutItem(contentKey)])
        {
            ContentData =
            [
                new BlockItemData(contentKey, elementTypeKey, "myElementType")
                {
                    Values =
                    [
                        new BlockPropertyValue { Culture = "en-us", Alias = "title", Value = "value" },
                        new BlockPropertyValue { Culture = "da-dk", Alias = "title", Value = "value" },
                    ],
                },
            ],
        };

        return JsonSerializer.Serialize(blockListValue, options);
    }

    private static string SerializeBlockListValueWithUnsortedExpose(JsonSerializerOptions options)
    {
        var contentKey1 = Guid.NewGuid();
        var contentKey2 = Guid.NewGuid();

        var blockListValue = new BlockListValue(
        [
            new BlockListLayoutItem(contentKey1),
            new BlockListLayoutItem(contentKey2),
        ])
        {
            ContentData =
            [
                new BlockItemData(contentKey1, Guid.NewGuid(), "myElementType"),
                new BlockItemData(contentKey2, Guid.NewGuid(), "myElementType"),
            ],
            Expose =
            [
                new BlockItemVariation(contentKey1, "en-us", null),
                new BlockItemVariation(contentKey2, "da-dk", null),
            ],
        };

        return JsonSerializer.Serialize(blockListValue, options);
    }

    private static string[] GetContentDataValueCultures(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("contentData")[0].GetProperty("values")
            .EnumerateArray()
            .Select(v => v.GetProperty("culture").GetString())
            .ToArray();
    }

    private static string[] GetExposeCultures(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("expose")
            .EnumerateArray()
            .Select(v => v.GetProperty("culture").GetString())
            .ToArray();
    }
}
