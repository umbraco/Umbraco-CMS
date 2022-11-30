using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Headless;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Headless;

[TestFixture]
public class MultiUrlPickerValueConverterTests : PropertyValueConverterTests
{
    [Test]
    public void MultiUrlPickerValueConverter_InSingleMode_ConvertsContentToLink()
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var serializer = new JsonNetSerializer();
        var valueConverter = new MultiUrlPickerValueConverter(PublishedSnapshotAccessor, Mock.Of<IProfilingLogger>(), serializer, Mock.Of<IUmbracoContextAccessor>(), PublishedUrlProvider, new HeadlessContentNameProvider());
        Assert.AreEqual(typeof(HeadlessLink), valueConverter.GetHeadlessPropertyValueType(publishedPropertyType.Object));

        var inter = serializer.Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key)
            }
        });
        var result = valueConverter.ConvertIntermediateToHeadlessObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as HeadlessLink;
        Assert.NotNull(result);
        Assert.AreEqual(PublishedContent.Name, result.Title);
        Assert.AreEqual(PublishedContent.Key, result.Key);
        Assert.AreEqual("the-page-url", result.Url);
        Assert.AreEqual("TheContentType", result.DestinationType);
        Assert.AreEqual(LinkType.Content, result.LinkType);
        Assert.AreEqual(null, result.Target);
    }

    [Test]
    public void MultiUrlPickerValueConverter_InMultiMode_ConvertsContentToLinks()
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 10 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var serializer = new JsonNetSerializer();
        var valueConverter = new MultiUrlPickerValueConverter(PublishedSnapshotAccessor, Mock.Of<IProfilingLogger>(), serializer, Mock.Of<IUmbracoContextAccessor>(), PublishedUrlProvider, new HeadlessContentNameProvider());
        Assert.AreEqual(typeof(IEnumerable<HeadlessLink>), valueConverter.GetHeadlessPropertyValueType(publishedPropertyType.Object));

        var inter = serializer.Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key)
            },
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key)
            }
        });
        var result = valueConverter.ConvertIntermediateToHeadlessObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<HeadlessLink>;
        Assert.NotNull(result);
        Assert.AreEqual(2, result.Count());

        Assert.AreEqual(PublishedContent.Name, result.First().Title);
        Assert.AreEqual(PublishedContent.Key, result.First().Key);
        Assert.AreEqual("the-page-url", result.First().Url);
        Assert.AreEqual("TheContentType", result.First().DestinationType);
        Assert.AreEqual(LinkType.Content, result.First().LinkType);
        Assert.AreEqual(null, result.First().Target);

        Assert.AreEqual(PublishedMedia.Name, result.Last().Title);
        Assert.AreEqual(PublishedMedia.Key, result.Last().Key);
        Assert.AreEqual("the-media-url", result.Last().Url);
        Assert.AreEqual("TheMediaType", result.Last().DestinationType);
        Assert.AreEqual(LinkType.Media, result.Last().LinkType);
        Assert.AreEqual(null, result.Last().Target);
    }

    [Test]
    public void MultiUrlPickerValueConverter_ConvertsExternalUrlToLink()
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var serializer = new JsonNetSerializer();
        var valueConverter = new MultiUrlPickerValueConverter(PublishedSnapshotAccessor, Mock.Of<IProfilingLogger>(), serializer, Mock.Of<IUmbracoContextAccessor>(), PublishedUrlProvider, new HeadlessContentNameProvider());

        var inter = serializer.Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Name = "The link",
                QueryString = "?something=true",
                Target = "_blank",
                Url = "https://umbraco.com/"
            }
        });
        var result = valueConverter.ConvertIntermediateToHeadlessObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as HeadlessLink;
        Assert.NotNull(result);
        Assert.AreEqual("The link", result.Title);
        Assert.AreEqual(null, result.Key);
        Assert.AreEqual("https://umbraco.com/?something=true", result.Url);
        Assert.AreEqual(LinkType.External, result.LinkType);
        Assert.AreEqual("_blank", result.Target);
    }

    [Test]
    public void MultiUrlPickerValueConverter_AppliesExplicitConfigurationToContentLink()
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var serializer = new JsonNetSerializer();
        var valueConverter = new MultiUrlPickerValueConverter(PublishedSnapshotAccessor, Mock.Of<IProfilingLogger>(), serializer, Mock.Of<IUmbracoContextAccessor>(), PublishedUrlProvider, new HeadlessContentNameProvider());

        var inter = serializer.Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
                Name = "Custom link name",
                QueryString = "?something=true",
                Target = "_blank"
            }
        });
        var result = valueConverter.ConvertIntermediateToHeadlessObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as HeadlessLink;
        Assert.NotNull(result);
        Assert.AreEqual("Custom link name", result.Title);
        Assert.AreEqual(PublishedContent.Key, result.Key);
        Assert.AreEqual("the-page-url?something=true", result.Url);
        Assert.AreEqual(LinkType.Content, result.LinkType);
        Assert.AreEqual("_blank", result.Target);
    }

    [Test]
    public void MultiUrlPickerValueConverter_PrioritizesContentUrlOverConfiguredUrl()
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var serializer = new JsonNetSerializer();
        var valueConverter = new MultiUrlPickerValueConverter(PublishedSnapshotAccessor, Mock.Of<IProfilingLogger>(), serializer, Mock.Of<IUmbracoContextAccessor>(), PublishedUrlProvider, new HeadlessContentNameProvider());

        var inter = serializer.Serialize(new[]
        {
            new MultiUrlPickerValueEditor.LinkDto
            {
                Udi = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
                Url = "https://umbraco.com/",
                QueryString = "?something=true"
            }
        });
        var result = valueConverter.ConvertIntermediateToHeadlessObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as HeadlessLink;
        Assert.NotNull(result);
        Assert.AreEqual(PublishedContent.Name, result.Title);
        Assert.AreEqual(PublishedContent.Key, result.Key);
        Assert.AreEqual("the-page-url?something=true", result.Url);
        Assert.AreEqual(LinkType.Content, result.LinkType);
        Assert.AreEqual(null, result.Target);
    }

    [TestCase(123)]
    [TestCase("123")]
    [TestCase(null)]
    public void MultiUrlPickerValueConverter_InSingleMode_ConvertsInvalidValueToNull(object? inter)
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var serializer = new JsonNetSerializer();
        var valueConverter = new MultiUrlPickerValueConverter(PublishedSnapshotAccessor, Mock.Of<IProfilingLogger>(), serializer, Mock.Of<IUmbracoContextAccessor>(), PublishedUrlProvider, new HeadlessContentNameProvider());

        var result = valueConverter.ConvertIntermediateToHeadlessObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as HeadlessLink;
        Assert.Null(result);
    }

    [TestCase(123)]
    [TestCase("123")]
    [TestCase(null)]
    public void MultiUrlPickerValueConverter_InMultiMode_ConvertsInvalidValueToEmptyArray(object? inter)
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new MultiUrlPickerConfiguration { MaxNumber = 10 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var serializer = new JsonNetSerializer();
        var valueConverter = new MultiUrlPickerValueConverter(PublishedSnapshotAccessor, Mock.Of<IProfilingLogger>(), serializer, Mock.Of<IUmbracoContextAccessor>(), PublishedUrlProvider, new HeadlessContentNameProvider());

        var result = valueConverter.ConvertIntermediateToHeadlessObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<HeadlessLink>;
        Assert.NotNull(result);
        Assert.IsEmpty(result);
    }
}
