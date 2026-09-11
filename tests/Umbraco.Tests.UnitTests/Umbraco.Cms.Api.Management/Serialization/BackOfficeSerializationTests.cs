using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Serialization;

[TestFixture]
public class BackOfficeSerializationTests
{
    private JsonOptions jsonOptions;

    [SetUp]
    public void SetupOptions()
    {
        var typeInfoResolver = new UmbracoJsonTypeInfoResolver(TestHelper.GetTypeFinder());
        var configurationOptions = new ConfigureUmbracoBackofficeJsonOptions(typeInfoResolver);
        var options = new JsonOptions();
        configurationOptions.Configure(global::Umbraco.Cms.Core.Constants.JsonOptionsNames.BackOffice, options);
        jsonOptions = options;
    }

    [Test]
    public void Will_Serialize_To_Camel_Case()
    {
        var objectToSerialize = new UnNestedJsonTestValue();

        var json = JsonSerializer.Serialize(objectToSerialize, jsonOptions.JsonSerializerOptions);

        Assert.AreEqual("{\"stringValue\":\"theValue\"}", json);
    }

    // the limit is 64, but it seems like the functional limit is that minus 1
    [TestCase(1, true, TestName = "Can_Serialize_At_Min_Depth(1)")]
    [TestCase(48, true, TestName = "Can_Serialize_At_High_Depth(33)")]
    [TestCase(63, true, TestName = "Can_Serialize_To_Max_Depth(63)")]
    [TestCase(64, false, TestName = "Can_NOT_Serialize_Beyond_Max_Depth(64)")]
    public void Can_Serialize_To_Max_Depth(int depth, bool shouldPass)
    {
        var objectToSerialize = CreateNestedObject(depth);

        if (shouldPass)
        {
            var json = JsonSerializer.Serialize(objectToSerialize, jsonOptions.JsonSerializerOptions);
            Assert.IsNotEmpty(json);
        }
        else
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Serialize(objectToSerialize, jsonOptions.JsonSerializerOptions));
        }
    }

    [Test]
    public void Will_Serialize_ValidationProblemDetails_To_Casing_Aligned_With_Mvc()
    {
        var objectToSerialize = new TestValueWithValidationProblemDetail();

        var json = JsonSerializer.Serialize(objectToSerialize, jsonOptions.JsonSerializerOptions);

        var expectedJson = "{\"problemDetails\":{\"type\":\"Test type\",\"title\":\"Test title\",\"status\":400,\"detail\":\"Test detail\",\"instance\":\"Test instance\",\"errors\":[{\"$.testError1\":[\"Test error 1a\",\"Test error 1b\"]},{\"$.testError2\":[\"Test error 2a\"]},{\"$.testError3.testError3a\":[\"Test error 3b\"]}],\"traceId\":\"traceValue\"}}";
        Assert.AreEqual(expectedJson, json);
    }

    [Test]
    public void Will_Order_Document_Variants_And_Values_When_Serialized()
    {
        var objectToSerialize = new DocumentResponseModel
        {
            Id = Guid.NewGuid(),
            DocumentType = new DocumentTypeReferenceResponseModel { Id = Guid.NewGuid() },
            Variants =
            [
                new DocumentVariantResponseModel { Culture = "nb-no", Name = "Norwegian" },
                new DocumentVariantResponseModel { Culture = "da-dk", Name = "Danish" },
            ],
            Values =
            [
                new DocumentValueResponseModel { Culture = "en-us", Alias = "title", Value = "value" },
                new DocumentValueResponseModel { Culture = "da-dk", Alias = "title", Value = "value" },
            ],
        };

        var json = JsonSerializer.Serialize(objectToSerialize, jsonOptions.JsonSerializerOptions);

        using JsonDocument document = JsonDocument.Parse(json);
        var variantCultures = document.RootElement.GetProperty("variants").EnumerateArray().Select(v => v.GetProperty("culture").GetString()).ToArray();
        var valueCultures = document.RootElement.GetProperty("values").EnumerateArray().Select(v => v.GetProperty("culture").GetString()).ToArray();

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEqual(new[] { "da-dk", "nb-no" }, variantCultures);
            CollectionAssert.AreEqual(new[] { "da-dk", "en-us" }, valueCultures);
        });
    }

    [Test]
    public void Will_Order_BlockItemData_Values_When_Serialized()
    {
        var objectToSerialize = new BlockItemData(Guid.NewGuid(), Guid.NewGuid(), "myElementType")
        {
            Values =
            [
                new BlockPropertyValue { Culture = "en-us", Alias = "title", Value = "value" },
                new BlockPropertyValue { Culture = "da-dk", Alias = "title", Value = "value" },
            ],
        };

        var json = JsonSerializer.Serialize(objectToSerialize, jsonOptions.JsonSerializerOptions);

        using JsonDocument document = JsonDocument.Parse(json);
        var valueCultures = document.RootElement.GetProperty("values").EnumerateArray().Select(v => v.GetProperty("culture").GetString()).ToArray();

        CollectionAssert.AreEqual(new[] { "da-dk", "en-us" }, valueCultures);
    }

    [Test]
    public void Will_Order_BlockItemVariation_Expose_List_When_Serialized()
    {
        var contentKey1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var contentKey2 = Guid.Parse("00000000-0000-0000-0000-000000000002");

        // BlockItemVariation has no property alias, so ordering falls back to (culture, segment,
        // content key) in its place.
        IList<BlockItemVariation> objectToSerialize =
        [
            new BlockItemVariation(contentKey1, "en-us", "segment-b"),
            new BlockItemVariation(contentKey2, "en-us", "segment-a"),
            new BlockItemVariation(contentKey1, "da-dk", null),
        ];

        var json = JsonSerializer.Serialize(objectToSerialize, jsonOptions.JsonSerializerOptions);

        using JsonDocument document = JsonDocument.Parse(json);
        var actualOrder = document.RootElement.EnumerateArray()
            .Select(v => (Culture: v.GetProperty("culture").GetString(), Segment: GetNullableString(v, "segment")))
            .ToArray();

        var expectedOrder = new (string Culture, string Segment)[]
        {
            ("da-dk", null),
            ("en-us", "segment-a"),
            ("en-us", "segment-b"),
        };

        CollectionAssert.AreEqual(expectedOrder, actualOrder);
    }

    private static string GetNullableString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind != JsonValueKind.Null
            ? value.GetString()
            : null;

    private static NestedJsonTestValue CreateNestedObject(int levels)
    {
        var root = new NestedJsonTestValue { Level = 1 };
        var outer = root;
        for (var i = 2; i <= levels; i++)
        {
            var inner = new NestedJsonTestValue { Level = i };
            outer.Inner = inner;
            outer = inner;
        }

        return root;
    }

    public class UnNestedJsonTestValue
    {
        public string StringValue { get; set; } = "theValue";
    }

    public class NestedJsonTestValue
    {
        public int Level { get; set; }

        public NestedJsonTestValue? Inner { get; set; }
    }

    private class TestValueWithValidationProblemDetail
    {
        public ValidationProblemDetails ProblemDetails { get; set; } = new()
        {
            Title = "Test title",
            Detail = "Test detail",
            Status = 400,
            Type = "Test type",
            Instance = "Test instance",
            Extensions =
            {
                ["traceId"] = "traceValue",
                ["someOtherExtension"] = "someOtherExtensionValue",
            },
            Errors =
            {
                ["TestError1"] = ["Test error 1a", "Test error 1b"],
                ["TestError2"] = ["Test error 2a"],
                ["TestError3.TestError3a"] = ["Test error 3b"],
            }
        };
    }
}
