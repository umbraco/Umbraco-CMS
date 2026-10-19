using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class MultipleDocumentPickerValueConverterTests : PropertyValueConverterTests
{
    [Test]
    public void MultipleDocumentPickerValueConverter_YieldsACollectionOfDocuments()
    {
        var valueConverter = CreateValueConverter();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(typeof(IEnumerable<IPublishedContent>), valueConverter.GetPropertyValueType(PropertyType()));
            Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetDeliveryApiPropertyValueType(PropertyType()));
        });
    }

    [Test]
    public void MultipleDocumentPickerValueConverter_ConvertsTheKeysToTheirDocuments()
    {
        var valueConverter = CreateValueConverter();

        var result = valueConverter.ConvertIntermediateToObject(
            Mock.Of<IPublishedElement>(),
            PropertyType(),
            PropertyCacheLevel.Element,
            Serialize(PublishedContent.Key),
            false) as IEnumerable<IPublishedContent>;

        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(PublishedContent.Key, result.First().Key);
    }

    [Test]
    public void MultipleDocumentPickerValueConverter_SkipsKeysThatDoNotResolve()
    {
        var valueConverter = CreateValueConverter();

        var result = valueConverter.ConvertIntermediateToObject(
            Mock.Of<IPublishedElement>(),
            PropertyType(),
            PropertyCacheLevel.Element,
            Serialize(PublishedContent.Key, Guid.NewGuid()),
            false) as IEnumerable<IPublishedContent>;

        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(PublishedContent.Key, result.First().Key);
    }

    [Test]
    public void MultipleDocumentPickerValueConverter_DoesNotYieldPickedMedia()
    {
        var valueConverter = CreateValueConverter();

        var result = valueConverter.ConvertIntermediateToObject(
            Mock.Of<IPublishedElement>(),
            PropertyType(),
            PropertyCacheLevel.Element,
            Serialize(PublishedMedia.Key),
            false) as IEnumerable<IPublishedContent>;

        Assert.NotNull(result);
        Assert.IsEmpty(result);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("[]")]
    public void MultipleDocumentPickerValueConverter_ConvertsAnEmptyValueToAnEmptyCollection(string? inter)
    {
        var valueConverter = CreateValueConverter();

        var result = valueConverter.ConvertIntermediateToObject(
            Mock.Of<IPublishedElement>(),
            PropertyType(),
            PropertyCacheLevel.Element,
            inter,
            false) as IEnumerable<IPublishedContent>;

        Assert.NotNull(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void MultipleDocumentPickerValueConverter_BuildsDeliveryApiOutput()
    {
        var valueConverter = CreateValueConverter();

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedElement>(),
            PropertyType(),
            PropertyCacheLevel.Element,
            Serialize(PublishedContent.Key),
            false,
            false) as IEnumerable<IApiContent>;

        Assert.NotNull(result);
        var content = result.Single();
        Assert.AreEqual("The page", content.Name);
        Assert.AreEqual(PublishedContent.Key, content.Id);
        Assert.AreEqual("TheContentType", content.ContentType);
    }

    private MultipleDocumentPickerValueConverter CreateValueConverter()
        => new(
            Serializer(),
            PublishedContentCacheMock.Object,
            new ApiContentBuilder(
                new ApiContentNameProvider(),
                CreateContentRouteBuilder(ApiContentPathProvider, CreateGlobalSettings()),
                CreateOutputExpansionStrategyAccessor(),
                CreateVariationContextAccessor()));

    private static IPublishedPropertyType PropertyType()
    {
        var propertyType = new Mock<IPublishedPropertyType>();
        propertyType.SetupGet(p => p.Alias).Returns("test");
        return propertyType.Object;
    }

    private static IJsonSerializer Serializer() => new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    private static string Serialize(params Guid[] keys) => Serializer().Serialize(keys);
}
