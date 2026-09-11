using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Media;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Serialization;

[TestFixture]
public class ContentModelBaseConverterFactoryTests
{
    private JsonSerializerOptions _jsonSerializerOptions;

    [SetUp]
    public void SetUp()
        => _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new ContentModelBaseConverterFactory() },
        };

    [TestCase(typeof(DocumentResponseModel))]
    [TestCase(typeof(MediaResponseModel))]
    public void CanConvert_Returns_True_For_Types_Deriving_From_ContentModelBase_At_Any_Depth(Type type)
        => Assert.That(new ContentModelBaseConverterFactory().CanConvert(type), Is.True);

    [TestCase(typeof(string))]
    [TestCase(typeof(DocumentTypeReferenceResponseModel))]
    public void CanConvert_Returns_False_For_Unrelated_Types(Type type)
        => Assert.That(new ContentModelBaseConverterFactory().CanConvert(type), Is.False);

    [Test]
    public void CreateConverter_Throws_For_Unrelated_Type()
        => Assert.Throws<NotSupportedException>(() => new ContentModelBaseConverterFactory().CreateConverter(typeof(string), _jsonSerializerOptions));

    [Test]
    public void Write_Orders_Variants_By_Culture_Then_Segment_For_DocumentResponseModel()
    {
        var model = new DocumentResponseModel
        {
            Id = Guid.NewGuid(),
            DocumentType = new DocumentTypeReferenceResponseModel { Id = Guid.NewGuid() },
            Variants =
            [
                new DocumentVariantResponseModel { Culture = "en-us", Segment = "segment-b", Name = "B" },
                new DocumentVariantResponseModel { Culture = "en-us", Segment = "segment-a", Name = "A" },
                new DocumentVariantResponseModel { Culture = "da-dk", Segment = null, Name = "Danish" },
            ],
        };

        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);

        using JsonDocument document = JsonDocument.Parse(json);
        var actualOrder = document.RootElement.GetProperty("variants")
            .EnumerateArray()
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

    [Test]
    public void Write_Orders_Values_By_Culture_Then_Segment_Then_Alias_For_DocumentResponseModel()
    {
        var model = new DocumentResponseModel
        {
            Id = Guid.NewGuid(),
            DocumentType = new DocumentTypeReferenceResponseModel { Id = Guid.NewGuid() },
            Values =
            [
                new DocumentValueResponseModel { Culture = "en-us", Segment = "segment-a", Alias = "zzz-alias", Value = "value" },
                new DocumentValueResponseModel { Culture = "en-us", Segment = "segment-b", Alias = "aaa-alias", Value = "value" },
                new DocumentValueResponseModel { Culture = "en-us", Segment = null, Alias = "mmm-alias", Value = "value" },
                new DocumentValueResponseModel { Culture = "da-dk", Segment = null, Alias = "title", Value = "value" },
            ],
        };

        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);

        var actualOrder = GetValueOrder(json);

        var expectedOrder = new (string Culture, string Segment, string Alias)[]
        {
            ("da-dk", null, "title"),
            ("en-us", null, "mmm-alias"),
            ("en-us", "segment-a", "zzz-alias"),
            ("en-us", "segment-b", "aaa-alias"),
        };

        CollectionAssert.AreEqual(expectedOrder, actualOrder);
    }

    [Test]
    public void Write_Orders_Values_By_Culture_Then_Segment_Then_Alias_For_MediaResponseModel()
    {
        // MediaResponseModel has no converter of its own - this proves the factory generalizes
        // ordering to any ContentModelBase<,> implementation, not just DocumentResponseModel.
        var model = new MediaResponseModel
        {
            Id = Guid.NewGuid(),
            Values =
            [
                new MediaValueResponseModel { Culture = "en-us", Segment = "segment-a", Alias = "zzz-alias", Value = "value" },
                new MediaValueResponseModel { Culture = "en-us", Segment = "segment-b", Alias = "aaa-alias", Value = "value" },
                new MediaValueResponseModel { Culture = "en-us", Segment = null, Alias = "mmm-alias", Value = "value" },
                new MediaValueResponseModel { Culture = "da-dk", Segment = null, Alias = "title", Value = "value" },
            ],
        };

        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);

        var actualOrder = GetValueOrder(json);

        var expectedOrder = new (string Culture, string Segment, string Alias)[]
        {
            ("da-dk", null, "title"),
            ("en-us", null, "mmm-alias"),
            ("en-us", "segment-a", "zzz-alias"),
            ("en-us", "segment-b", "aaa-alias"),
        };

        CollectionAssert.AreEqual(expectedOrder, actualOrder);
    }

    [Test]
    public void Write_Does_Not_Leave_Variants_Or_Values_Reordered_On_The_Original_Instance()
    {
        // Culture order here is deliberately not the sorted order, so a leaked mutation from Write would
        // be observable as a changed Variants/Values order on the original instance.
        var model = new DocumentResponseModel
        {
            Id = Guid.NewGuid(),
            DocumentType = new DocumentTypeReferenceResponseModel { Id = Guid.NewGuid() },
            Variants =
            [
                new DocumentVariantResponseModel { Culture = "en-us", Name = "English" },
                new DocumentVariantResponseModel { Culture = "da-dk", Name = "Danish" },
            ],
            Values =
            [
                new DocumentValueResponseModel { Culture = "en-us", Alias = "title", Value = "value" },
                new DocumentValueResponseModel { Culture = "da-dk", Alias = "title", Value = "value" },
            ],
        };
        var originalVariantOrder = model.Variants.Select(v => v.Culture).ToArray();
        var originalValueOrder = model.Values.Select(v => v.Culture).ToArray();

        JsonSerializer.Serialize(model, _jsonSerializerOptions);

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEqual(originalVariantOrder, model.Variants.Select(v => v.Culture).ToArray());
            CollectionAssert.AreEqual(originalValueOrder, model.Values.Select(v => v.Culture).ToArray());
        });
    }

    [Test]
    public void Read_Delegates_To_Default_Deserialization()
    {
        var model = new DocumentResponseModel
        {
            Id = Guid.NewGuid(),
            DocumentType = new DocumentTypeReferenceResponseModel { Id = Guid.NewGuid() },
            Variants = [new DocumentVariantResponseModel { Culture = "en-us", Name = "English" }],
            Values = [new DocumentValueResponseModel { Culture = "en-us", Alias = "title", Value = "value" }],
        };

        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);

        var deserialized = JsonSerializer.Deserialize<DocumentResponseModel>(json, _jsonSerializerOptions);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.Id, Is.EqualTo(model.Id));
            Assert.That(deserialized.Variants.Select(v => v.Culture), Is.EquivalentTo(model.Variants.Select(v => v.Culture)));
            Assert.That(deserialized.Values.Select(v => v.Alias), Is.EquivalentTo(model.Values.Select(v => v.Alias)));
        });
    }

    private static (string Culture, string Segment, string Alias)[] GetValueOrder(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("values")
            .EnumerateArray()
            .Select(v => (Culture: GetNullableString(v, "culture"), Segment: GetNullableString(v, "segment"), Alias: v.GetProperty("alias").GetString()))
            .ToArray();
    }

    private static string GetNullableString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind != JsonValueKind.Null
            ? value.GetString()
            : null;
}
