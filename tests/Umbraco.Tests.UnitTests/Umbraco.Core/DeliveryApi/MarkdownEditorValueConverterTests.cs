using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Templates;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class MarkdownEditorValueConverterTests : PropertyValueConverterTests
{
    [TestCase("hello world", "hello world")]
    [TestCase("hello *world*", "hello *world*")]
    [TestCase("", "")]
    [TestCase(null, "")]
    [TestCase(123, "")]
    public void MarkdownEditorValueConverter_ConvertsValueToMarkdownString(object inter, string expected)
    {
        var linkParser = new HtmlLocalLinkParser(Mock.Of<IPublishedUrlProvider>());
        var urlParser = new HtmlUrlParser(Mock.Of<IOptionsMonitor<ContentSettings>>(), Mock.Of<ILogger<HtmlUrlParser>>(), Mock.Of<IProfilingLogger>(), Mock.Of<IIOHelper>());
        var valueConverter = new MarkdownEditorValueConverter(linkParser, urlParser);

        Assert.AreEqual(typeof(string), valueConverter.GetDeliveryApiPropertyValueType(Mock.Of<IPublishedPropertyType>()));
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), Mock.Of<IPublishedPropertyType>(), PropertyCacheLevel.Element, inter, false, false);
        Assert.AreEqual(expected, result);
    }
}
