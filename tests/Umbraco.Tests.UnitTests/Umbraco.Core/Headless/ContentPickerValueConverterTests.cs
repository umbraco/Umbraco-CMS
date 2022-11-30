using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Headless;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Headless;

[TestFixture]
public class ContentPickerValueConverterTests : PropertyValueConverterTests
{
    [Test]
    public void ContentPickerValueConverter_BuildsHeadlessOutput()
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var valueConverter = new ContentPickerValueConverter(PublishedSnapshotAccessor, PublishedUrlProvider, new HeadlessContentNameProvider());
        Assert.AreEqual(typeof(HeadlessLink), valueConverter.GetHeadlessPropertyValueType(publishedPropertyType.Object));
        var result = valueConverter.ConvertIntermediateToHeadlessObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
            false) as HeadlessLink;

        Assert.NotNull(result);
        Assert.AreEqual("The page", result.Title);
        Assert.AreEqual(PublishedContent.Key, result.Key);
        Assert.AreEqual("the-page-url", result.Url);
        Assert.AreEqual("TheContentType", result.DestinationType);
        Assert.AreEqual(LinkType.Content, result.LinkType);
        Assert.Null(result.Target);
    }

    [Test]
    public void ContentPickerValueConverter_CanCustomizeContentNameInHeadlessOutput()
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var customNameProvider = new Mock<IHeadlessContentNameProvider>();
        customNameProvider.Setup(n => n.GetName(PublishedContent)).Returns($"Custom name for: {PublishedContent.Name}");

        var valueConverter = new ContentPickerValueConverter(PublishedSnapshotAccessor, PublishedUrlProvider, customNameProvider.Object);
        var result = valueConverter.ConvertIntermediateToHeadlessObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
            false) as HeadlessLink;

        Assert.NotNull(result);
        Assert.AreEqual("Custom name for: The page", result.Title);
    }
}
