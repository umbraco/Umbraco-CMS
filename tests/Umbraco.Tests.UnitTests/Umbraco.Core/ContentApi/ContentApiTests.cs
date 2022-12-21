using System;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ContentApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

public class ContentApiTests
{
    protected Mock<IContentApiPropertyValueConverter> ContentApiPropertyValueConverter { get; private set; }
    protected IPublishedPropertyType ContentApiPropertyType { get; private set; }
    protected IPublishedPropertyType DefaultPropertyType { get; private set; }

    [SetUp]
    public virtual void Setup()
    {
        ContentApiPropertyValueConverter = new Mock<IContentApiPropertyValueConverter>();
        ContentApiPropertyValueConverter.Setup(p => p.ConvertIntermediateToContentApiObject(
            It.IsAny<IPublishedElement>(),
            It.IsAny<IPublishedPropertyType>(),
            It.IsAny<PropertyCacheLevel>(),
            It.IsAny<object?>(),
            It.IsAny<bool>())
        ).Returns("Content API value");
        ContentApiPropertyValueConverter.Setup(p => p.ConvertIntermediateToObject(
            It.IsAny<IPublishedElement>(),
            It.IsAny<IPublishedPropertyType>(),
            It.IsAny<PropertyCacheLevel>(),
            It.IsAny<object?>(),
            It.IsAny<bool>())
        ).Returns("Default value");
        ContentApiPropertyValueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        ContentApiPropertyValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);

        ContentApiPropertyType = SetupPublishedPropertyType(ContentApiPropertyValueConverter.Object, "contentApi");

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
