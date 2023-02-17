using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ContentApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

public class ContentApiTests
{
    protected IPublishedPropertyType ContentApiPropertyType { get; private set; }

    protected IPublishedPropertyType DefaultPropertyType { get; private set; }

    [SetUp]
    public virtual void Setup()
    {
        var contentApiPropertyValueConverter = new Mock<IContentApiPropertyValueConverter>();
        contentApiPropertyValueConverter.Setup(p => p.ConvertIntermediateToContentApiObject(
            It.IsAny<IPublishedElement>(),
            It.IsAny<IPublishedPropertyType>(),
            It.IsAny<PropertyCacheLevel>(),
            It.IsAny<object?>(),
            It.IsAny<bool>())
        ).Returns("Content API value");
        contentApiPropertyValueConverter.Setup(p => p.ConvertIntermediateToObject(
            It.IsAny<IPublishedElement>(),
            It.IsAny<IPublishedPropertyType>(),
            It.IsAny<PropertyCacheLevel>(),
            It.IsAny<object?>(),
            It.IsAny<bool>())
        ).Returns("Default value");
        contentApiPropertyValueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        contentApiPropertyValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);

        ContentApiPropertyType = SetupPublishedPropertyType(contentApiPropertyValueConverter.Object, "contentApi", "Content.Api.Editor");

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

        DefaultPropertyType = SetupPublishedPropertyType(defaultPropertyValueConverter.Object, "default", "Default.Editor");
    }

    protected IPublishedPropertyType SetupPublishedPropertyType(IPropertyValueConverter valueConverter, string propertyTypeAlias, string editorAlias)
    {
        var mockPublishedContentTypeFactory = new Mock<IPublishedContentTypeFactory>();
        mockPublishedContentTypeFactory.Setup(x => x.GetDataType(It.IsAny<int>()))
            .Returns(new PublishedDataType(123, editorAlias, new Lazy<object>()));

        var publishedPropType = new PublishedPropertyType(
            propertyTypeAlias,
            123,
            true,
            ContentVariation.Nothing,
            new PropertyValueConverterCollection(() => new[] { valueConverter }),
            Mock.Of<IPublishedModelFactory>(),
            mockPublishedContentTypeFactory.Object);

        return publishedPropType;
    }

    protected IOutputExpansionStrategyAccessor CreateOutputExpansionStrategyAccessor() => new DefaultOutputExpansionStrategyAccessor();
}
