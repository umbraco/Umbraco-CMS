using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Infrastructure.DeliveryApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class RichTextParserTests
{
    private readonly Guid _contentKey = Guid.NewGuid();
    private readonly Guid _contentRootKey = Guid.NewGuid();
    private readonly Guid _mediaKey = Guid.NewGuid();

    [Test]
    public void ParseElement_DocumentElementIsCalledRoot()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p>Hello</p>");
        Assert.IsNotNull(element);
        Assert.AreEqual("#root", element.Tag);
    }

    [Test]
    public void ParseElement_SimpleParagraphHasSingleTextElement()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p>Some text paragraph</p>") as RichTextGenericElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(1, element.Elements.Count());
        var paragraph = element.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("p", paragraph.Tag);
        var textElement = paragraph.Elements.First() as RichTextTextElement;
        Assert.IsNotNull(textElement);
        Assert.AreEqual("Some text paragraph", textElement.Text);
    }

    [Test]
    public void ParseElement_ParagraphWithLineBreaksWrapsTextInElements()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p>Some text<br/>More text<br/>Even more text</p>") as RichTextGenericElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(1, element.Elements.Count());
        var paragraph = element.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("p", paragraph.Tag);
        var paragraphElements = paragraph.Elements.ToArray();
        Assert.AreEqual(5, paragraphElements.Length);
        for (var i = 0; i < paragraphElements.Length; i++)
        {
            var paragraphElement = paragraphElements[i] as RichTextGenericElement;
            var textElement = paragraphElements[i] as RichTextTextElement;
            switch (i)
            {
                case 0:
                    Assert.IsNull(paragraphElement);
                    Assert.IsNotNull(textElement);
                    Assert.AreEqual("#text", textElement.Tag);
                    Assert.AreEqual("Some text", textElement.Text);
                    break;
                case 2:
                    Assert.IsNull(paragraphElement);
                    Assert.IsNotNull(textElement);
                    Assert.AreEqual("#text", textElement.Tag);
                    Assert.AreEqual("More text", textElement.Text);
                    break;
                case 4:
                    Assert.IsNull(paragraphElement);
                    Assert.IsNotNull(textElement);
                    Assert.AreEqual("#text", textElement.Tag);
                    Assert.AreEqual("Even more text", textElement.Text);
                    break;
                case 1:
                case 3:
                    Assert.IsNull(textElement);
                    Assert.IsNotNull(paragraphElement);
                    Assert.IsEmpty(paragraphElement.Elements);
                    Assert.AreEqual("br", paragraphElement.Tag);
                    break;
            }
        }
    }

    [Test]
    public void ParseElement_DataAttributesAreSanitized()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p><span data-something=\"the data-something value\">Text in a data-something SPAN</span></p>") as RichTextGenericElement;
        Assert.IsNotNull(element);
        var span = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.Tag);
        Assert.AreEqual(1, span.Attributes.Count);
        Assert.AreEqual("something", span.Attributes.First().Key);
        Assert.AreEqual("the data-something value", span.Attributes.First().Value);
        var textElement = span.Elements.Single() as RichTextTextElement;
        Assert.IsNotNull(textElement);
        Assert.AreEqual("Text in a data-something SPAN", textElement.Text);
    }

    [Test]
    public void ParseElement_DataAttributesDoNotOverwriteExistingAttributes()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p><span something=\"the original something\" data-something=\"the data something\">Text in a data-something SPAN</span></p>") as RichTextGenericElement;
        Assert.IsNotNull(element);
        var span = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.Tag);
        Assert.AreEqual(1, span.Attributes.Count);
        Assert.AreEqual("something", span.Attributes.First().Key);
        Assert.AreEqual("the original something", span.Attributes.First().Value);
    }

    [Test]
    public void ParseElement_CanParseContentLink()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"/{{localLink:umb://document/{_contentKey:N}}}\"></a></p>") as RichTextGenericElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("a", link.Tag);
        Assert.AreEqual(1, link.Attributes.Count);
        Assert.AreEqual("route", link.Attributes.First().Key);
        var route = link.Attributes.First().Value as IApiContentRoute;
        Assert.IsNotNull(route);
        Assert.AreEqual("/some-content-path", route.Path);
        Assert.AreEqual(_contentRootKey, route.StartItem.Id);
        Assert.AreEqual("the-root-path", route.StartItem.Path);
    }

    [Test]
    public void ParseElement_CanParseMediaLink()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"/{{localLink:umb://media/{_mediaKey:N}}}\"></a></p>") as RichTextGenericElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("a", link.Tag);
        Assert.AreEqual(1, link.Attributes.Count);
        Assert.AreEqual("href", link.Attributes.First().Key);
        Assert.AreEqual("/some-media-url", link.Attributes.First().Value);
    }

    [Test]
    public void ParseElement_CanHandleNonLocalLink()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"https://some.where/else/\"></a></p>") as RichTextGenericElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("a", link.Tag);
        Assert.AreEqual(1, link.Attributes.Count);
        Assert.AreEqual("href", link.Attributes.First().Key);
        Assert.AreEqual("https://some.where/else/", link.Attributes.First().Value);
    }

    [Test]
    public void ParseElement_LinkTextIsWrappedInTextElement()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"https://some.where/else/\">This is the link text</a></p>") as RichTextGenericElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("a", link.Tag);
        var textElement = link.Elements.Single() as RichTextTextElement;
        Assert.IsNotNull(textElement);
        Assert.AreEqual("This is the link text", textElement.Text);
    }

    [TestCase("{localLink:umb://document/fe5bf80d37db4373adb9b206896b4a3b}")]
    [TestCase("{localLink:umb://media/03b9a8721c4749a9a7026033ec78d860}")]
    public void ParseElement_InvalidLocalLinkYieldsEmptyLink(string href)
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"/{href}\"></a></p>") as RichTextGenericElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("a", link.Tag);
        Assert.IsEmpty(link.Attributes);
    }

    [Test]
    public void ParseElement_CanParseMediaImage()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><img src=\"/media/whatever/something.png?rmode=max&amp;width=500\" data-udi=\"umb://media/{_mediaKey:N}\"></p>") as RichTextGenericElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("img", link.Tag);
        Assert.AreEqual(1, link.Attributes.Count);
        Assert.AreEqual("src", link.Attributes.First().Key);
        Assert.AreEqual("/some-media-url", link.Attributes.First().Value);
    }

    [Test]
    public void ParseElement_CanHandleNonLocalImage()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><img src=\"https://some.where/something.png?rmode=max&amp;width=500\"></p>") as RichTextGenericElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("img", link.Tag);
        Assert.AreEqual(1, link.Attributes.Count);
        Assert.AreEqual("src", link.Attributes.First().Key);
        Assert.AreEqual("https://some.where/something.png?rmode=max&amp;width=500", link.Attributes.First().Value);
    }

    [Test]
    public void ParseMarkup_CanParseContentLink()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{{localLink:umb://document/{_contentKey:N}}}\"></a></p>");
        Assert.IsTrue(result.Contains("href=\"/some-content-path\""));
        Assert.IsTrue(result.Contains("data-start-item-path=\"the-root-path\""));
        Assert.IsTrue(result.Contains($"data-start-item-id=\"{_contentRootKey:D}\""));
    }

    [Test]
    public void ParseMarkup_CanParseMediaLink()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{{localLink:umb://media/{_mediaKey:N}}}\"></a></p>");
        Assert.IsTrue(result.Contains("href=\"/some-media-url\""));
    }

    [TestCase("{localLink:umb://document/fe5bf80d37db4373adb9b206896b4a3b}")]
    [TestCase("{localLink:umb://media/03b9a8721c4749a9a7026033ec78d860}")]
    public void ParseMarkup_InvalidLocalLinkYieldsEmptyLink(string href)
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{href}\"></a></p>");
        Assert.AreEqual($"<p><a></a></p>", result);
    }

    [TestCase("<p><a href=\"https://some.where/else/\"></a></p>")]
    [TestCase("<p><img src=\"https://some.where/something.png?rmode=max&amp;width=500\"></p>")]
    public void ParseMarkup_CanHandleNonLocalReferences(string html)
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse(html);
        Assert.AreEqual(html, result);
    }

    [Test]
    public void ParseMarkup_CanParseMediaImage()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><img src=\"/media/whatever/something.png?rmode=max&amp;width=500\" data-udi=\"umb://media/{_mediaKey:N}\"></p>");
        Assert.IsTrue(result.Contains("src=\"/some-media-url?rmode=max&amp;width=500\""));
        Assert.IsFalse(result.Contains("data-udi"));
    }

    [Test]
    public void ParseMarkup_RemovesMediaDataCaption()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><img src=\"/media/whatever/something.png?rmode=max&amp;width=500\" data-udi=\"umb://media/{_mediaKey:N}\"></p>");
        Assert.IsTrue(result.Contains("src=\"/some-media-url?rmode=max&amp;width=500\""));
        Assert.IsFalse(result.Contains("data-udi"));
    }

    [Test]
    public void ParseMarkup_DataAttributesAreRetained()
    {
        var parser = CreateRichTextMarkupParser();

        const string html = "<p><span data-something=\"the data-something value\">Text in a data-something SPAN</span></p>";
        var result = parser.Parse(html);
        Assert.AreEqual(html, result);
    }

    private ApiRichTextElementParser CreateRichTextElementParser()
    {
        SetupTestContent(out var routeBuilder, out var snapshotAccessor, out var urlProvider);

        return new ApiRichTextElementParser(
            routeBuilder,
            urlProvider,
            snapshotAccessor,
            Mock.Of<ILogger<ApiRichTextElementParser>>());
    }

    private ApiRichTextMarkupParser CreateRichTextMarkupParser()
    {
        SetupTestContent(out var routeBuilder, out var snapshotAccessor, out var urlProvider);

        return new ApiRichTextMarkupParser(
            routeBuilder,
            urlProvider,
            snapshotAccessor,
            Mock.Of<ILogger<ApiRichTextMarkupParser>>());
    }

    private void SetupTestContent(out IApiContentRouteBuilder routeBuilder, out IPublishedSnapshotAccessor snapshotAccessor, out IPublishedUrlProvider urlProvider)
    {
        var contentMock = new Mock<IPublishedContent>();
        contentMock.SetupGet(m => m.Key).Returns(_contentKey);
        contentMock.SetupGet(m => m.ItemType).Returns(PublishedItemType.Content);

        var mediaMock = new Mock<IPublishedContent>();
        mediaMock.SetupGet(m => m.Key).Returns(_mediaKey);
        mediaMock.SetupGet(m => m.ItemType).Returns(PublishedItemType.Media);

        var contentCacheMock = new Mock<IPublishedContentCache>();
        contentCacheMock.Setup(m => m.GetById(new GuidUdi(Constants.UdiEntityType.Document, _contentKey))).Returns(contentMock.Object);
        var mediaCacheMock = new Mock<IPublishedMediaCache>();
        mediaCacheMock.Setup(m => m.GetById(new GuidUdi(Constants.UdiEntityType.Media, _mediaKey))).Returns(mediaMock.Object);

        var snapshotMock = new Mock<IPublishedSnapshot>();
        snapshotMock.SetupGet(m => m.Content).Returns(contentCacheMock.Object);
        snapshotMock.SetupGet(m => m.Media).Returns(mediaCacheMock.Object);

        var snapshot = snapshotMock.Object;
        var snapshotAccessorMock = new Mock<IPublishedSnapshotAccessor>();
        snapshotAccessorMock.Setup(m => m.TryGetPublishedSnapshot(out snapshot)).Returns(true);

        var routeBuilderMock = new Mock<IApiContentRouteBuilder>();
        routeBuilderMock
            .Setup(m => m.Build(contentMock.Object, null))
            .Returns(new ApiContentRoute("/some-content-path", new ApiContentStartItem(_contentRootKey, "the-root-path")));

        var urlProviderMock = new Mock<IPublishedUrlProvider>();
        urlProviderMock
            .Setup(m => m.GetMediaUrl(mediaMock.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<Uri?>()))
            .Returns("/some-media-url");

        routeBuilder = routeBuilderMock.Object;
        snapshotAccessor = snapshotAccessorMock.Object;
        urlProvider = urlProviderMock.Object;
    }
}
