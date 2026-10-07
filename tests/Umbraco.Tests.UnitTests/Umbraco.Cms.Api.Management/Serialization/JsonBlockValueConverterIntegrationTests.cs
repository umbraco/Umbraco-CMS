using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Serialization;

/// <summary>
/// Verifies that <see cref="JsonBlockValueConverter"/> - which owns (de)serialization of the containing
/// <see cref="BlockValue"/> - actually routes its <see cref="BlockValue.ContentData"/>/<see cref="BlockValue.SettingsData"/>
/// and <see cref="BlockValue.Expose"/> through <see cref="BlockValueJsonTypeInfoModifiers"/>, rather than each
/// modifier only being exercised in isolation.
/// </summary>
[TestFixture]
public class JsonBlockValueConverterIntegrationTests
{
    [Test]
    public void Write_Sorts_ContentData_Values_When_Modifier_Is_Wired_In()
    {
        JsonSerializerOptions options = CreateOptions(includeBlockSortingModifier: true);

        var json = SerializeBlockListValueWithUnsortedContentDataValues(options);

        var order = GetContentDataValueCultures(json);
        CollectionAssert.AreEqual(new[] { "da-dk", "en-us" }, order);
    }

    [Test]
    public void Write_Does_Not_Sort_ContentData_Values_When_Modifier_Is_Not_Wired_In()
    {
        JsonSerializerOptions options = CreateOptions(includeBlockSortingModifier: false);

        var json = SerializeBlockListValueWithUnsortedContentDataValues(options);

        // Unsorted input order survives when the modifier isn't wired in - proving the previous test's
        // sorted output genuinely comes from it, not from JsonBlockValueConverter itself.
        var order = GetContentDataValueCultures(json);
        CollectionAssert.AreEqual(new[] { "en-us", "da-dk" }, order);
    }

    [Test]
    public void Write_Sorts_Expose_When_Modifier_Is_Wired_In()
    {
        JsonSerializerOptions options = CreateOptions(includeBlockSortingModifier: true);

        var json = SerializeBlockListValueWithUnsortedExpose(options);

        var order = GetExposeCultures(json);
        CollectionAssert.AreEqual(new[] { "da-dk", "en-us" }, order);
    }

    [Test]
    public void Write_Does_Not_Sort_Expose_When_Modifier_Is_Not_Wired_In()
    {
        JsonSerializerOptions options = CreateOptions(includeBlockSortingModifier: false);

        var json = SerializeBlockListValueWithUnsortedExpose(options);

        // Unsorted input order survives when the modifier isn't wired in - proving the previous test's
        // sorted output genuinely comes from it.
        var order = GetExposeCultures(json);
        CollectionAssert.AreEqual(new[] { "en-us", "da-dk" }, order);
    }

    [Test]
    public void Read_Round_Trips_BlockValue_When_Modifier_Is_Wired_In()
    {
        JsonSerializerOptions options = CreateOptions(includeBlockSortingModifier: true);

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

    [Test]
    public void Write_Sorts_Nested_Block_Expose_And_ContentData_Without_Throwing()
    {
        // Reproduces a reported crash from an earlier, JsonConverter-based design: a block whose own property
        // value is itself another block (e.g. Block List nested inside Block List) made a long-lived converter
        // singleton see two different JsonSerializerOptions instances within one serialize call, which was
        // misread as unsupported reuse and threw. A JsonTypeInfo.OnSerializing/OnSerialized modifier runs the
        // DEFAULT serialization unchanged - it only mutates the value first - so a nested occurrence is just
        // another instance it runs for, at any depth, with no options-cloning or reentrancy handling involved.
        JsonSerializerOptions options = CreateOptions(includeBlockSortingModifier: true);

        var innerContentKey = Guid.NewGuid();
        var innerBlockValue = new BlockListValue([new BlockListLayoutItem(innerContentKey)])
        {
            ContentData =
            [
                new BlockItemData(innerContentKey, Guid.NewGuid(), "innerElementType")
                {
                    Values =
                    [
                        new BlockPropertyValue { Culture = "en-us", Alias = "title", Value = "inner-en" },
                        new BlockPropertyValue { Culture = "da-dk", Alias = "title", Value = "inner-da" },
                    ],
                },
            ],
            Expose =
            [
                new BlockItemVariation(innerContentKey, "en-us", null),
                new BlockItemVariation(innerContentKey, "da-dk", null),
            ],
        };

        var outerContentKey = Guid.NewGuid();
        var outerBlockValue = new BlockListValue([new BlockListLayoutItem(outerContentKey)])
        {
            ContentData =
            [
                new BlockItemData(outerContentKey, Guid.NewGuid(), "outerElementType")
                {
                    Values = [new BlockPropertyValue { Culture = "en-us", Alias = "nestedBlocks", Value = innerBlockValue }],
                },
            ],
            Expose =
            [
                new BlockItemVariation(outerContentKey, "en-us", null),
                new BlockItemVariation(outerContentKey, "da-dk", null),
            ],
        };

        string json = null;
        Assert.DoesNotThrow(() => json = JsonSerializer.Serialize(outerBlockValue, options));

        using JsonDocument document = JsonDocument.Parse(json);
        var outerExposeCultures = document.RootElement.GetProperty("expose")
            .EnumerateArray().Select(v => v.GetProperty("culture").GetString()).ToArray();

        JsonElement nestedBlockValue = document.RootElement.GetProperty("contentData")[0].GetProperty("values")[0].GetProperty("value");
        var innerExposeCultures = nestedBlockValue.GetProperty("expose")
            .EnumerateArray().Select(v => v.GetProperty("culture").GetString()).ToArray();
        var innerContentDataValueCultures = nestedBlockValue.GetProperty("contentData")[0].GetProperty("values")
            .EnumerateArray().Select(v => v.GetProperty("culture").GetString()).ToArray();

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEqual(new[] { "da-dk", "en-us" }, outerExposeCultures);
            CollectionAssert.AreEqual(new[] { "da-dk", "en-us" }, innerExposeCultures);
            CollectionAssert.AreEqual(new[] { "da-dk", "en-us" }, innerContentDataValueCultures);
        });
    }

    private static JsonSerializerOptions CreateOptions(bool includeBlockSortingModifier)
    {
        IJsonTypeInfoResolver typeInfoResolver = new DefaultJsonTypeInfoResolver();
        if (includeBlockSortingModifier)
        {
            typeInfoResolver = new ModifyingJsonTypeInfoResolver(typeInfoResolver, BlockValueJsonTypeInfoModifiers.Apply);
        }

        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonBlockValueConverter() },
            TypeInfoResolver = typeInfoResolver,
        };
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
