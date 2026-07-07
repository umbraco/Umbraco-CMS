using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.DeliveryApi;
using Umbraco.Extensions;
using VariationContext = Umbraco.Cms.Core.Models.PublishedContent.VariationContext;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class MediaPickerWithCropsValueConverterTests
{
    [TestCase("en", null, null, "English alt text", "English alt text", TestName = "AltText returns invariant when no culture dictionary")]
    [TestCase("da", "da", "Dansk alt tekst", "English alt text", "Dansk alt tekst", TestName = "AltText returns culture-specific value when culture matches")]
    [TestCase("fr", "da", "Dansk alt tekst", "Fallback alt text", "Fallback alt text", TestName = "AltText falls back to invariant when culture not in dictionary")]
    [TestCase(null, "da", "Dansk alt tekst", "Invariant alt text", "Invariant alt text", TestName = "AltText returns invariant when no active culture")]
    public void AltText_Resolves_Expected_Value(
        string? culture,
        string? cultureKey,
        string? cultureValue,
        string invariantAltText,
        string expected)
    {
        var accessor = MockAccessor(new VariationContext(culture));

        Dictionary<string, string>? altTextByCulture =
            cultureKey is null ? null : new Dictionary<string, string> { [cultureKey] = cultureValue! };

        var dto = new MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto
        {
            AltText = invariantAltText,
            AltTextByCulture = altTextByCulture,
        };

        Assert.AreEqual(expected, ResolveAltText(accessor, dto));
    }

    [TestCase("A person standing by a window", "A person standing by a window", TestName = "ImageCrop alt text is exposed in the Delivery API")]
    [TestCase(null, null, TestName = "ImageCrop alt text is null when not set")]
    public void ImageCrop_AltText_Is_Exposed_In_Delivery_Api(string? altText, string? expected)
    {
        var sourceCrop = new ImageCropperValue.ImageCropperCrop
        {
            Alias = "portrait",
            Width = 800,
            Height = 600,
            AltText = altText,
        };

        var imageCropperValue = new ImageCropperValue { Crops = [sourceCrop] };
        var apiCrops = imageCropperValue.GetImageCrops()?.ToList();

        Assert.IsNotNull(apiCrops);
        Assert.AreEqual(1, apiCrops!.Count);
        Assert.AreEqual(expected, apiCrops[0].AltText);
    }

    [Test]
    public void ApplyConfiguration_Preserves_PerCrop_AltText()
    {
        // Arrange — a DTO with one crop that has both coordinates and alt text
        var dto = new MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto
        {
            Crops =
            [
                new ImageCropperValue.ImageCropperCrop
                {
                    Alias = "portrait",
                    Width = 800,
                    Height = 600,
                    Coordinates = new ImageCropperValue.ImageCropperCropCoordinates { X1 = 0.1m, Y1 = 0.1m, X2 = 0.9m, Y2 = 0.9m },
                    AltText = "A person standing by a window",
                },
            ],
        };

        var configuration = new MediaPicker3Configuration
        {
            Crops =
            [
                new MediaPicker3Configuration.CropConfiguration { Alias = "portrait", Width = 800, Height = 600 },
            ],
        };

        // Act
        dto.ApplyConfiguration(configuration);

        // Assert
        var crop = dto.Crops?.SingleOrDefault(c => c.Alias == "portrait");
        Assert.IsNotNull(crop);
        Assert.AreEqual("A person standing by a window", crop!.AltText);
    }

    [Test]
    public void ApplyConfiguration_Preserves_AltText_Even_Without_Coordinates()
    {
        // A crop with alt text but no custom coordinates (uses focal-point auto-crop)
        var dto = new MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto
        {
            Crops =
            [
                new ImageCropperValue.ImageCropperCrop
                {
                    Alias = "square",
                    Width = 400,
                    Height = 400,
                    Coordinates = null,
                    AltText = "A square crop alt text",
                },
            ],
        };

        var configuration = new MediaPicker3Configuration
        {
            Crops =
            [
                new MediaPicker3Configuration.CropConfiguration { Alias = "square", Width = 400, Height = 400 },
            ],
        };

        dto.ApplyConfiguration(configuration);

        var crop = dto.Crops?.SingleOrDefault(c => c.Alias == "square");
        Assert.IsNotNull(crop);
        Assert.AreEqual("A square crop alt text", crop!.AltText);
    }

    [Test]
    public void Prune_Keeps_Crop_With_AltText_And_No_Coordinates()
    {
        // A crop with alt text but no coordinates must survive Prune so the alt text is persisted.
        var dto = new MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto
        {
            Crops =
            [
                new ImageCropperValue.ImageCropperCrop
                {
                    Alias = "landscape",
                    Width = 1200,
                    Height = 630,
                    Coordinates = null,
                    AltText = "A scenic landscape",
                },
                new ImageCropperValue.ImageCropperCrop
                {
                    Alias = "noData",
                    Width = 200,
                    Height = 200,
                    Coordinates = null,
                    AltText = null,
                },
            ],
        };

        dto.Prune();

        // "landscape" survives because it has alt text; "noData" is pruned because it has neither
        Assert.AreEqual(1, dto.Crops?.Count());
        Assert.AreEqual("landscape", dto.Crops?.First().Alias);
    }

    [Test]
    public void Prune_Keeps_Crop_With_Coordinates_And_No_AltText()
    {
        var dto = new MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto
        {
            Crops =
            [
                new ImageCropperValue.ImageCropperCrop
                {
                    Alias = "hero",
                    Width = 1600,
                    Height = 900,
                    Coordinates = new ImageCropperValue.ImageCropperCropCoordinates { X1 = 0.1m, Y1 = 0.1m, X2 = 0.9m, Y2 = 0.9m },
                    AltText = null,
                },
            ],
        };

        dto.Prune();

        Assert.AreEqual(1, dto.Crops?.Count());
        Assert.AreEqual("hero", dto.Crops?.First().Alias);
    }

    [Test]
    public void GetPropertyCacheLevel_Returns_Elements_When_AltText_Disabled()
    {
        var converter = CreateConverter(MockAccessor(new VariationContext("en")));
        var propertyType = CreatePropertyType(new MediaPicker3Configuration());

        Assert.AreEqual(PropertyCacheLevel.Elements, converter.GetPropertyCacheLevel(propertyType));
    }

    [Test]
    public void GetPropertyCacheLevel_Returns_None_When_AltText_Enabled_On_Invariant_Property()
    {
        var converter = CreateConverter(MockAccessor(new VariationContext("en")));
        var propertyType = CreatePropertyType(new MediaPicker3Configuration { AltTextMode = "altText" });

        Assert.AreEqual(PropertyCacheLevel.None, converter.GetPropertyCacheLevel(propertyType));
    }

    [Test]
    public void GetPropertyCacheLevel_Returns_Elements_When_AltText_Enabled_But_Property_Varies_By_Culture()
    {
        var converter = CreateConverter(MockAccessor(new VariationContext("en")));
        var propertyType = CreatePropertyType(
            new MediaPicker3Configuration { AltTextMode = "altText" },
            ContentVariation.Culture);

        Assert.AreEqual(PropertyCacheLevel.Elements, converter.GetPropertyCacheLevel(propertyType));
    }

    [Test]
    public void GetPropertyCacheLevel_Returns_Elements_When_Decorative_On_Invariant_Property()
    {
        // Decorative alt text is always empty, so it never varies by culture and stays cacheable
        var converter = CreateConverter(MockAccessor(new VariationContext("en")));
        var propertyType = CreatePropertyType(new MediaPicker3Configuration { AltTextMode = "decorative" });

        Assert.AreEqual(PropertyCacheLevel.Elements, converter.GetPropertyCacheLevel(propertyType));
    }

    [Test]
    public void GetPropertyCacheLevel_Returns_None_When_PerCrop_AltText_Enabled_On_Invariant_Property()
    {
        var converter = CreateConverter(MockAccessor(new VariationContext("en")));
        var propertyType = CreatePropertyType(new MediaPicker3Configuration { EnableAltTextPerCrop = true });

        Assert.AreEqual(PropertyCacheLevel.None, converter.GetPropertyCacheLevel(propertyType));
    }

    private static IPublishedPropertyType CreatePropertyType(
        MediaPicker3Configuration configuration,
        ContentVariation variations = ContentVariation.Nothing)
    {
        var dataType = new PublishedDataType(1, Constants.PropertyEditors.Aliases.MediaPicker3, "test",
            new Lazy<object?>(() => configuration));
        return Mock.Of<IPublishedPropertyType>(pt => pt.DataType == dataType && pt.Variations == variations);
    }

    [Test]
    public void MediaPicker3Configuration_Default_AltTextMode_Is_Off()
    {
        var config = new MediaPicker3Configuration();
        Assert.AreEqual("off", config.AltTextMode);
    }

    // --- Helpers ---

    private static IVariationContextAccessor MockAccessor(VariationContext context)
    {
        var mock = new Mock<IVariationContextAccessor>();
        mock.SetupGet(a => a.VariationContext).Returns(context);
        return mock.Object;
    }

    private static string? ResolveAltText(
        IVariationContextAccessor accessor,
        MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto dto)
    {
        var culture = accessor.VariationContext?.Culture;
        if (!string.IsNullOrEmpty(culture) && dto.AltTextByCulture?.TryGetValue(culture, out var cultureAltText) == true)
        {
            return cultureAltText;
        }

        return dto.AltText;
    }

    private static MediaPickerWithCropsValueConverter CreateConverter(IVariationContextAccessor accessor)
        => new(
            Mock.Of<IPublishedMediaCache>(),
            Mock.Of<IPublishedUrlProvider>(),
            Mock.Of<IPublishedValueFallback>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IApiMediaWithCropsBuilder>(),
            accessor);
}
