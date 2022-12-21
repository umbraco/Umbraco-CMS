using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

[TestFixture]
public class MultiNodeTreePickerValueConverterTests : PropertyValueConverterTests
{
    [Test]
    public void MultiNodeTreePickerValueConverter_InSingleMode_ConvertsValueToLink()
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new MultiNodePickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = new MultiNodeTreePickerValueConverter(PublishedSnapshotAccessor, Mock.Of<IUmbracoContextAccessor>(), Mock.Of<IMemberService>(), PublishedUrlProvider, new ContentNameProvider());

        Assert.AreEqual(typeof(ApiLink), valueConverter.GetContentApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as ApiLink;
        Assert.NotNull(result);
        Assert.AreEqual(PublishedContent.Name, result.Title);
        Assert.AreEqual(PublishedContent.Key, result.Key);
        Assert.AreEqual("the-page-url", result.Url);
        Assert.AreEqual("TheContentType", result.DestinationType);
        Assert.AreEqual(LinkType.Content, result.LinkType);
        Assert.AreEqual(null, result.Target);
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InMultiMode_ConvertsValueToLinks()
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new MultiNodePickerConfiguration { MaxNumber = 10 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = new MultiNodeTreePickerValueConverter(PublishedSnapshotAccessor, Mock.Of<IUmbracoContextAccessor>(), Mock.Of<IMemberService>(), PublishedUrlProvider, new ContentNameProvider());

        Assert.AreEqual(typeof(IEnumerable<ApiLink>), valueConverter.GetContentApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key), new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key) };
        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<ApiLink>;
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

    [TestCase(123)]
    [TestCase("123")]
    [TestCase(null)]
    public void MultiNodeTreePickerValueConverter_InSingleMode_ConvertsInvalidValueToNull(object? inter)
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new MultiNodePickerConfiguration { MaxNumber = 1 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = new MultiNodeTreePickerValueConverter(PublishedSnapshotAccessor, Mock.Of<IUmbracoContextAccessor>(), Mock.Of<IMemberService>(), PublishedUrlProvider, new ContentNameProvider());

        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as ApiLink;
        Assert.Null(result);
    }

    [TestCase(123)]
    [TestCase("123")]
    [TestCase(null)]
    public void MultiNodeTreePickerValueConverter_InMultiMode_ConvertsInvalidValueToEmptyArray(object? inter)
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new MultiNodePickerConfiguration { MaxNumber = 10 }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = new MultiNodeTreePickerValueConverter(PublishedSnapshotAccessor, Mock.Of<IUmbracoContextAccessor>(), Mock.Of<IMemberService>(), PublishedUrlProvider, new ContentNameProvider());

        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<ApiLink>;
        Assert.NotNull(result);
        Assert.IsEmpty(result);
    }
}
