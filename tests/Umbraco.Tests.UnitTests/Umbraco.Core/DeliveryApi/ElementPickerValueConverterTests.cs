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

    [TestCase("NL-nl", "en-US", false)]
    [TestCase("NL-nl", "nl-NL", true)]
    [TestCase("", "en-US", true)]
    [TestCase(null, "en-US", true)]
    public void Preview_Filters_By_Culture_Only_When_A_Culture_Is_Requested(string? culture, string otherCulture, bool includeOtherElement)
    {
        IPublishedElement dutch = CreateVariant("nl-NL");
        IPublishedElement other = CreateVariant(otherCulture);
        Guid[] keys = [dutch.Key, other.Key];
        PublishedElementCacheMock
            .Setup(cache => cache.GetByIds(true, It.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(keys))))
            .Returns([dutch, other]);
        PublishedElementCacheMock
            .Setup(cache => cache.GetByIds(false, It.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(new[] { other.Key }))))
            .Returns([]);

        IVariationContextAccessor variationContextAccessor = CreateVariationContextAccessor();
        variationContextAccessor.VariationContext = culture is null ? null : new VariationContext(culture);
        var result = (IEnumerable<IPublishedElement>)CreateValueConverter(variationContextAccessor)
            .ConvertIntermediateToObject(
                Mock.Of<IPublishedElement>(),
                Mock.Of<IPublishedPropertyType>(),
                PropertyCacheLevel.Element,
                $"[\"{dutch.Key}\",\"{other.Key}\"]",
                preview: true)!;

        Guid[] expectedKeys = includeOtherElement ? keys : [dutch.Key];
        Assert.That(result.Select(element => element.Key), Is.EqualTo(expectedKeys));
        PublishedElementCacheMock.Verify(
            cache => cache.GetByIds(false, It.IsAny<IReadOnlyCollection<Guid>>()),
            includeOtherElement ? Times.Never() : Times.Once());
    }

    [Test]
    public void Preview_Batches_Published_Fallback_And_Preserves_Picked_Order()
    {
        IPublishedElement firstDraft = CreateVariant("en-US");
        IPublishedElement validDraft = CreateVariant("nl-NL");
        IPublishedElement lastDraft = CreateVariant("en-US");
        IPublishedElement firstPublished = CreateVariant("nl-NL", firstDraft.Key);
        IPublishedElement lastPublished = CreateVariant("nl-NL", lastDraft.Key);
        Guid[] keys = [firstDraft.Key, validDraft.Key, lastDraft.Key];
        PublishedElementCacheMock
            .Setup(cache => cache.GetByIds(true, It.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(keys))))
            .Returns([firstDraft, validDraft, lastDraft]);
        PublishedElementCacheMock
            .Setup(cache => cache.GetByIds(false, It.IsAny<IReadOnlyCollection<Guid>>()))
            .Returns([lastPublished, firstPublished]);

        IVariationContextAccessor variationContextAccessor = CreateVariationContextAccessor();
        variationContextAccessor.VariationContext = new VariationContext("nl-NL");
        var result = (IEnumerable<IPublishedElement>)CreateValueConverter(variationContextAccessor)
            .ConvertIntermediateToObject(
                Mock.Of<IPublishedElement>(),
                Mock.Of<IPublishedPropertyType>(),
                PropertyCacheLevel.Element,
                $"[\"{firstDraft.Key}\",\"{validDraft.Key}\",\"{lastDraft.Key}\"]",
                preview: true)!;

        Assert.That(result, Is.EqualTo(new[] { firstPublished, validDraft, lastPublished }));
        PublishedElementCacheMock.Verify(cache => cache.GetByIds(false, It.IsAny<IReadOnlyCollection<Guid>>()), Times.Once());
        PublishedElementCacheMock.Verify(
            cache => cache.GetByIds(false, It.Is<IReadOnlyCollection<Guid>>(ids =>
                ids.Count == 2 && ids.Contains(firstDraft.Key) && ids.Contains(lastDraft.Key))),
            Times.Once());
    }

    private static IPublishedElement CreateVariant(string isoCode, Guid? key = null)
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(type => type.Variations).Returns(ContentVariation.Culture);
        var element = new Mock<IPublishedElement>();
        element.SetupGet(item => item.Key).Returns(key ?? Guid.NewGuid());
        element.SetupGet(item => item.ContentType).Returns(contentType.Object);
        element.SetupGet(item => item.Cultures).Returns(new Dictionary<string, PublishedCultureInfo>(StringComparer.OrdinalIgnoreCase)
        {
            [isoCode] = new PublishedCultureInfo(isoCode, isoCode, null, DateTime.MinValue),
        });
        return element.Object;
    }
}
