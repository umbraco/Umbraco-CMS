using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class ElementPickerValueConverterTests : PropertyValueConverterTests
{
    private ElementPickerValueConverter CreateValueConverter(IVariationContextAccessor? variationContextAccessor = null) =>
        new(
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            PublishedElementCacheMock.Object,
            variationContextAccessor ?? CreateVariationContextAccessor(),
            new ApiElementBuilder(CreateOutputExpansionStrategyAccessor()));

    [Test]
    public void ElementPickerValueConverter_BuildsDeliveryApiOutput()
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var valueConverter = CreateValueConverter();
        Assert.AreEqual(typeof(IEnumerable<IApiElement>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            $"[\"{PublishedElement.Key}\"]",
            false,
            false) as IEnumerable<IApiElement>;

        Assert.NotNull(result);
        Assert.AreEqual(1, result!.Count());
        var element = result.First();
        Assert.AreEqual(PublishedElement.Key, element.Id);
        Assert.AreEqual(PublishedElement.Name, element.Name);
        Assert.AreEqual("TheElementType", element.ContentType);
        Assert.IsEmpty(element.Properties);
    }

    [Test]
    public void ElementPickerValueConverter_RendersContentProperties()
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("theElementType");
        contentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);

        var publishedElement = new Mock<IPublishedElement>();
        publishedElement.SetupGet(c => c.ContentType).Returns(contentType.Object);

        var propertyData = new PropertyData { Value = "n/a", Culture = "abc", Segment = string.Empty };

        var prop1 = new PublishedProperty(DeliveryApiPropertyType, publishedElement.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);
        var prop2 = new PublishedProperty(DefaultPropertyType, publishedElement.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);

        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var key = Guid.NewGuid();
        ConfigurePublishedElementMock(publishedElement, key, "The element", contentType.Object, [prop1, prop2]);

        PublishedElementCacheMock
            .Setup(ecc => ecc.GetByIds(false, It.Is<IReadOnlyCollection<Guid>>(keys => keys.SequenceEqual(new[] { key }))))
            .Returns([publishedElement.Object]);

        var valueConverter = CreateValueConverter();
        Assert.AreEqual(typeof(IEnumerable<IApiElement>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            $"[\"{key}\"]",
            false,
            false) as IEnumerable<IApiElement>;

        Assert.NotNull(result);
        Assert.AreEqual(1, result!.Count());
        var element = result.First();
        Assert.AreEqual(key, element.Id);
        Assert.AreEqual("The element", element.Name);
        Assert.AreEqual("theElementType", element.ContentType);
        Assert.AreEqual(2, element.Properties.Count);
        Assert.AreEqual("Delivery API value", element.Properties[DeliveryApiPropertyType.Alias]);
        Assert.AreEqual("Default value", element.Properties[DefaultPropertyType.Alias]);
    }

    [TestCase("NL-nl", false)]
    [TestCase("", true)]
    [TestCase(null, true)]
    public void Preview_Filters_By_Culture_Only_When_A_Culture_Is_Requested(string? culture, bool includeOtherCulture)
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(type => type.Variations).Returns(ContentVariation.Culture);
        IPublishedElement dutch = CreateVariant("nl-NL");
        IPublishedElement english = CreateVariant("en-US");
        Guid[] keys = [dutch.Key, english.Key];
        PublishedElementCacheMock
            .Setup(cache => cache.GetByIds(true, It.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(keys))))
            .Returns([dutch, english]);
        PublishedElementCacheMock
            .Setup(cache => cache.GetByIds(false, It.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(new[] { english.Key }))))
            .Returns([]);

        IVariationContextAccessor variationContextAccessor = CreateVariationContextAccessor();
        variationContextAccessor.VariationContext = culture is null ? null : new VariationContext(culture);
        var result = (IEnumerable<IPublishedElement>)CreateValueConverter(variationContextAccessor)
            .ConvertIntermediateToObject(
                Mock.Of<IPublishedElement>(),
                Mock.Of<IPublishedPropertyType>(),
                PropertyCacheLevel.Element,
                $"[\"{dutch.Key}\",\"{english.Key}\"]",
                preview: true)!;

        Guid[] expectedKeys = includeOtherCulture ? keys : [dutch.Key];
        Assert.That(result.Select(element => element.Key), Is.EqualTo(expectedKeys));
        PublishedElementCacheMock.Verify(
            cache => cache.GetByIds(false, It.IsAny<IReadOnlyCollection<Guid>>()),
            includeOtherCulture ? Times.Never() : Times.Once());

        IPublishedElement CreateVariant(string isoCode)
        {
            var element = new Mock<IPublishedElement>();
            element.SetupGet(item => item.Key).Returns(Guid.NewGuid());
            element.SetupGet(item => item.ContentType).Returns(contentType.Object);
            element.SetupGet(item => item.Cultures).Returns(new Dictionary<string, PublishedCultureInfo>(StringComparer.OrdinalIgnoreCase)
            {
                [isoCode] = new PublishedCultureInfo(isoCode, isoCode, null, DateTime.MinValue),
            });
            return element.Object;
        }
    }
}
