using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

[TestFixture]
public class ContentPickerValueConverterTests : PropertyValueConverterTests
{
    private ContentPickerValueConverter CreateValueConverter(bool shouldExpand, IPublishedContentNameProvider? nameProvider = null)
    {
        var expansionStrategy = new Mock<IOutputExpansionStrategy>();
        expansionStrategy.Setup(e => e.ShouldExpand(It.IsAny<IPublishedPropertyType>())).Returns(shouldExpand);
        return new ContentPickerValueConverter(PublishedSnapshotAccessor, expansionStrategy.Object, new ApiContentBuilder(new PropertyMapper(), nameProvider ?? new PublishedContentNameProvider(), PublishedUrlProvider));
    }

    [Test]
    public void ContentPickerValueConverter_BuildsContentApiOutput()
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var valueConverter = CreateValueConverter(false);
        Assert.AreEqual(typeof(IApiContent), valueConverter.GetContentApiPropertyValueType(publishedPropertyType.Object));
        var result = valueConverter.ConvertIntermediateToContentApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
            false) as IApiContent;

        Assert.NotNull(result);
        Assert.AreEqual("The page", result.Name);
        Assert.AreEqual(PublishedContent.Key, result.Id);
        Assert.AreEqual("the-page-url", result.Url);
        Assert.AreEqual("TheContentType", result.ContentType);
        Assert.IsEmpty(result.Properties);
    }

    [Test]
    public void ContentPickerValueConverter_CanCustomizeContentNameInContentApiOutput()
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var customNameProvider = new Mock<IPublishedContentNameProvider>();
        customNameProvider.Setup(n => n.GetName(PublishedContent)).Returns($"Custom name for: {PublishedContent.Name}");

        var valueConverter = CreateValueConverter(false, customNameProvider.Object);
        var result = valueConverter.ConvertIntermediateToContentApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
            false) as IApiContent;

        Assert.NotNull(result);
        Assert.AreEqual("Custom name for: The page", result.Name);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ContentPickerValueConverter_HandlesOutputExpansionStrategy(bool shouldExpand)
    {
        var content = new Mock<IPublishedContent>();

        var prop1 = new PublishedElementPropertyBase(ContentApiPropertyType, content.Object, false, PropertyCacheLevel.None);
        var prop2 = new PublishedElementPropertyBase(DefaultPropertyType, content.Object, false, PropertyCacheLevel.None);

        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var key = Guid.NewGuid();
        content.SetupGet(c => c.Properties).Returns(new[] { prop1, prop2 });
        content.SetupGet(c => c.UrlSegment).Returns("page-url-segment");
        content.SetupGet(c => c.Name).Returns("The page");
        content.SetupGet(c => c.Key).Returns(key);
        content.SetupGet(c => c.ContentType).Returns(PublishedContentType);
        content.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);

        PublishedUrlProviderMock
            .Setup(p => p.GetUrl(content.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(content.Object.UrlSegment);
        PublishedContentCacheMock
            .Setup(pcc => pcc.GetById(key))
            .Returns(content.Object);

        var valueConverter = CreateValueConverter(shouldExpand);
        Assert.AreEqual(typeof(IApiContent), valueConverter.GetContentApiPropertyValueType(publishedPropertyType.Object));
        var result = valueConverter.ConvertIntermediateToContentApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, key),
            false) as IApiContent;

        Assert.NotNull(result);
        Assert.AreEqual("The page", result.Name);
        Assert.AreEqual(content.Object.Key, result.Id);
        Assert.AreEqual("page-url-segment", result.Url);
        Assert.AreEqual("TheContentType", result.ContentType);
        if (shouldExpand)
        {
            Assert.AreEqual(2, result.Properties.Count);
        }
        else
        {
            Assert.IsEmpty(result.Properties);
        }
    }
}
