using System;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Headless;

public class HeadlessTests
{
    protected Mock<IHeadlessPropertyValueConverter> HeadlessPropertyValueConverter { get; private set; }
    protected IPublishedPropertyType HeadlessPropertyType { get; private set; }
    protected IPublishedPropertyType DefaultPropertyType { get; private set; }

    [SetUp]
    public virtual void Setup()
    {
        HeadlessPropertyValueConverter = new Mock<IHeadlessPropertyValueConverter>();
        HeadlessPropertyValueConverter.Setup(p => p.ConvertIntermediateToHeadlessObject(
            It.IsAny<IPublishedElement>(),
            It.IsAny<IPublishedPropertyType>(),
            It.IsAny<PropertyCacheLevel>(),
            It.IsAny<object?>(),
            It.IsAny<bool>())
        ).Returns("Headless value");
        HeadlessPropertyValueConverter.Setup(p => p.ConvertIntermediateToObject(
            It.IsAny<IPublishedElement>(),
            It.IsAny<IPublishedPropertyType>(),
            It.IsAny<PropertyCacheLevel>(),
            It.IsAny<object?>(),
            It.IsAny<bool>())
        ).Returns("Default value");
        HeadlessPropertyValueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        HeadlessPropertyValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);

        HeadlessPropertyType = SetupPublishedPropertyType(HeadlessPropertyValueConverter.Object, "headless");

        var defaultPropertyValueConverter = new Mock<IPropertyValueConverter>();
        defaultPropertyValueConverter.Setup(p => p.ConvertIntermediateToObject(
            It.IsAny<IPublishedElement>(),
            It.IsAny<IPublishedPropertyType>(),
            It.IsAny<PropertyCacheLevel>(),
            It.IsAny<object?>(),
            It.IsAny<bool>())
        ).Returns("Default value");
        defaultPropertyValueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        defaultPropertyValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);

        DefaultPropertyType = SetupPublishedPropertyType(defaultPropertyValueConverter.Object, "default");
    }

    private IPublishedPropertyType SetupPublishedPropertyType(IPropertyValueConverter valueConverter, string alias)
    {
        var mockPublishedContentTypeFactory = new Mock<IPublishedContentTypeFactory>();
        mockPublishedContentTypeFactory.Setup(x => x.GetDataType(It.IsAny<int>()))
            .Returns(new PublishedDataType(123, "test", new Lazy<object>()));

        var publishedPropType = new PublishedPropertyType(
            alias,
            123,
            true,
            ContentVariation.Nothing,
            new PropertyValueConverterCollection(() => new[] { valueConverter }),
            Mock.Of<IPublishedModelFactory>(),
            mockPublishedContentTypeFactory.Object);

        return publishedPropType;
    }
}
