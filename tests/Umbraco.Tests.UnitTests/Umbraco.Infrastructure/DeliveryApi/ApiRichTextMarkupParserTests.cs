using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.DeliveryApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.DeliveryApi;

/// <summary>
/// Provides unit tests for the <see cref="ApiRichTextMarkupParser"/> class, verifying its functionality and behavior.
/// </summary>
[TestFixture]
public class ApiRichTextMarkupParserTests
{
    private Mock<IApiContentRouteBuilder> _apiContentRouteBuilder;
    private Mock<IApiMediaUrlProvider> _apiMediaUrlProvider;

    /// <summary>
    /// Tests that the parser can correctly parse legacy local link markup in rich text HTML.
    /// </summary>
    [Test]
    public void Can_Parse_Legacy_LocalLinks()
    {
        var key1 = Guid.Parse("a1c5d649977f4ea59b1cb26055f3eed3");
        var data1 = new MockData()
            .WithKey(key1)
            .WithContentTypeAlias("someAlias")
            .WithRoutePath("/inline/")
            .WithRouteStartPath("inline");

        var mockData = new Dictionary<Guid, MockData>
        {
            { key1, data1 },
        };

        var parser = BuildDefaultSut(mockData);

        var legacyHtml =
            "<p><a href=\"/{localLink:umb://document/a1c5d649977f4ea59b1cb26055f3eed3}\" title=\"Inline\">link </a>to another page</p>";

        var expectedOutput =
            $"<p><a href=\"/inline/\" title=\"Inline\" data-destination-id=\"{key1:D}\" data-destination-type=\"someAlias\" data-start-item-path=\"inline\" data-start-item-id=\"a1c5d649-977f-4ea5-9b1c-b26055f3eed3\" data-link-type=\"{LinkType.Content}\">link </a>to another page</p>";

        var parsedHtml = parser.Parse(legacyHtml);

        Assert.AreEqual(expectedOutput, parsedHtml);
    }

    /// <summary>
    /// Verifies that the rich text markup parser correctly identifies and replaces local link placeholders
    /// (e.g., <c>/{localLink:guid}</c>) in HTML with the appropriate resolved URLs and metadata attributes.
    /// Ensures that links to internal content are parsed and rendered as expected.
    /// </summary>
    [Test]
    public void Can_Parse_LocalLinks()
    {
        var key1 = Guid.Parse("eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f");
        var data1 = new MockData()
            .WithKey(key1)
            .WithContentTypeAlias("someAlias")
            .WithRoutePath("/self/")
            .WithRouteStartPath("self");

        var key2 = Guid.Parse("cc143afe-4cbf-46e5-b399-c9f451384373");
        var data2 = new MockData()
            .WithKey(key2)
            .WithContentTypeAlias("someAliasTwo")
            .WithRoutePath("/other/")
            .WithRouteStartPath("other");

        var mockData = new Dictionary<Guid, MockData>
        {
            { key1, data1 },
            { key2, data2 },
        };

        var parser = BuildDefaultSut(mockData);

        var html =
            @"<p>Rich text outside of the blocks with a link to <a type=""document"" href=""/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}"" title=""itself"">itself</a><br><br></p>
<p>and to the <a type=""document"" href=""/{localLink:cc143afe-4cbf-46e5-b399-c9f451384373}"" title=""other page"">other page</a></p>";

        var expectedOutput =
            $@"<p>Rich text outside of the blocks with a link to <a href=""/self/"" title=""itself"" data-destination-id=""{key1:D}"" data-destination-type=""someAlias"" data-start-item-path=""self"" data-start-item-id=""eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f"" data-link-type=""{LinkType.Content}"">itself</a><br><br></p>
<p>and to the <a href=""/other/"" title=""other page"" data-destination-id=""{key2:D}"" data-destination-type=""someAliasTwo"" data-start-item-path=""other"" data-start-item-id=""cc143afe-4cbf-46e5-b399-c9f451384373"" data-link-type=""{LinkType.Content}"">other page</a></p>";

        var parsedHtml = parser.Parse(html);

        Assert.AreEqual(expectedOutput, parsedHtml);
    }

    /// <summary>
    /// Tests that local links with various postfixes are correctly parsed and replaced with the expected HTML output.
    /// </summary>
    /// <param name="postfix">The postfix string to append to the local link.</param>
    [TestCase("#some-anchor")]
    [TestCase("?something=true")]
    [TestCase("#!some-hashbang")]
    [TestCase("?something=true#some-anchor")]
    public void Can_Parse_LocalLinks_With_Postfix(string postfix)
    {
        var key1 = Guid.Parse("eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f");
        var data1 = new MockData()
            .WithKey(key1)
            .WithContentTypeAlias("someAlias")
            .WithRoutePath($"/self/{postfix}")
            .WithRouteStartPath("self");

        var key2 = Guid.Parse("cc143afe-4cbf-46e5-b399-c9f451384373");
        var data2 = new MockData()
            .WithKey(key2)
            .WithContentTypeAlias("someAliasTwo")
            .WithRoutePath($"/other/{postfix}")
            .WithRouteStartPath("other");

        var mockData = new Dictionary<Guid, MockData>
        {
            { key1, data1 },
            { key2, data2 },
        };

        var parser = BuildDefaultSut(mockData);

        var html =
            $@"<p>Rich text outside of the blocks with a link to <a type=""document"" href=""/{{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}}{postfix}"" title=""itself"">itself</a><br><br></p>
<p>and to the <a type=""document"" href=""/{{localLink:cc143afe-4cbf-46e5-b399-c9f451384373}}{postfix}"" title=""other page"">other page</a></p>";

        var expectedOutput =
            $@"<p>Rich text outside of the blocks with a link to <a href=""/self/{postfix}"" title=""itself"" data-destination-id=""{key1:D}"" data-destination-type=""someAlias"" data-start-item-path=""self"" data-start-item-id=""eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f"" data-link-type=""{LinkType.Content}"">itself</a><br><br></p>
<p>and to the <a href=""/other/{postfix}"" title=""other page"" data-destination-id=""{key2:D}"" data-destination-type=""someAliasTwo"" data-start-item-path=""other"" data-start-item-id=""cc143afe-4cbf-46e5-b399-c9f451384373"" data-link-type=""{LinkType.Content}"">other page</a></p>";

        var parsedHtml = parser.Parse(html);

        Assert.AreEqual(expectedOutput, parsedHtml);
    }

    /// <summary>
    /// Tests that inline local images are correctly parsed and their URLs replaced with the expected media URLs.
    /// </summary>
    [Test]
    public void Can_Parse_Inline_LocalImages()
    {
        var key1 = Guid.Parse("395bdc0e8f4d4ad4af7f3a3f6265651e");
        var data1 = new MockData()
            .WithKey(key1)
            .WithMediaUrl("https://localhost:44331/media/bdofwokn/77gtp8fbrxmgkefatp10aw.webp");

        var mockData = new Dictionary<Guid, MockData>
        {
            { key1, data1 },
        };
        var parser = BuildDefaultSut(mockData);

        var legacyHtml =
            @"<p>An image</p>\n<p><img src=""/media/bdofwokn/77gtp8fbrxmgkefatp10aw.webp?rmode=max&amp;width=500&amp;height=500"" alt="""" width=""500"" height=""500"" data-udi=""umb://media/395bdc0e8f4d4ad4af7f3a3f6265651e""></p>";

        var expectedOutput =
            @"<p>An image</p>\n<p><img src=""https://localhost:44331/media/bdofwokn/77gtp8fbrxmgkefatp10aw.webp?rmode=max&amp;width=500&amp;height=500"" alt="""" width=""500"" height=""500""></p>";

        var parsedHtml = parser.Parse(legacyHtml);

        Assert.AreEqual(expectedOutput, parsedHtml);
    }

    private ApiRichTextMarkupParser BuildDefaultSut(Dictionary<Guid, MockData> mockData)
    {
        var contentCacheMock = new Mock<IPublishedContentCache>();

        contentCacheMock.Setup(cc => cc.GetById(It.IsAny<bool>(), It.IsAny<Guid>()))
            .Returns<bool, Guid>((preview, key) => mockData[key].PublishedContent);
        contentCacheMock.Setup(cc => cc.GetById(It.IsAny<Guid>()))
            .Returns<Guid>(key => mockData[key].PublishedContent);

        var mediaCacheMock = new Mock<IPublishedMediaCache>();
        mediaCacheMock.Setup(cc => cc.GetById(It.IsAny<bool>(), It.IsAny<Guid>()))
            .Returns<bool, Guid>((preview, key) => mockData[key].PublishedContent);
        mediaCacheMock.Setup(cc => cc.GetById(It.IsAny<Guid>()))
            .Returns<Guid>(key => mockData[key].PublishedContent);

        _apiMediaUrlProvider = new Mock<IApiMediaUrlProvider>();
        _apiMediaUrlProvider.Setup(mup => mup.GetUrl(It.IsAny<IPublishedContent>()))
            .Returns<IPublishedContent>(ipc => mockData[ipc.Key].MediaUrl);

        _apiContentRouteBuilder = new Mock<IApiContentRouteBuilder>();
        _apiContentRouteBuilder.Setup(acrb => acrb.Build(It.IsAny<IPublishedContent>(), It.IsAny<string>()))
            .Returns<IPublishedContent, string>((content, culture) => mockData[content.Key].ApiContentRoute);

        return new ApiRichTextMarkupParser(
            _apiContentRouteBuilder.Object,
            _apiMediaUrlProvider.Object,
            contentCacheMock.Object,
            mediaCacheMock.Object,
            Mock.Of<ILogger<ApiRichTextMarkupParser>>());
    }

    private class MockData
    {
        private Mock<IPublishedContent> _publishedContentMock = new Mock<IPublishedContent>();
        private Mock<IApiContentRoute> _apiContentRouteMock = new Mock<IApiContentRoute>();
        private Mock<IApiContentStartItem> _apiContentStartItem = new Mock<IApiContentStartItem>();

    /// <summary>
    /// Gets the mocked published content.
    /// </summary>
        public IPublishedContent PublishedContent => _publishedContentMock.Object;

    /// <summary>
    /// Gets the mocked API content route.
    /// </summary>
        public IApiContentRoute ApiContentRoute => _apiContentRouteMock.Object;

    /// <summary>
    /// Gets or sets the media URL.
    /// </summary>
        public string MediaUrl { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockData"/> class.
    /// </summary>
        public MockData()
        {
            _apiContentRouteMock.SetupGet(r => r.StartItem).Returns(_apiContentStartItem.Object);
        }

    /// <summary>
    /// Sets the key for the mock data.
    /// </summary>
    /// <param name="key">The key to set.</param>
    /// <returns>The current instance of <see cref="MockData"/> with the key set.</returns>
        public MockData WithKey(Guid key)
        {
            _publishedContentMock.SetupGet(i => i.Key).Returns(key);
            _apiContentStartItem.SetupGet(rsi => rsi.Id).Returns(key);
            return this;
        }

    /// <summary>
    /// Sets the content type alias for the mock data.
    /// </summary>
    /// <param name="alias">The content type alias to set.</param>
    /// <returns>The current instance of <see cref="MockData"/> for chaining.</returns>
        public MockData WithContentTypeAlias(string alias)
        {
            _publishedContentMock.SetupGet(x => x.ContentType.Alias).Returns(alias);
            return this;
        }

    /// <summary>
    /// Sets the route path for the mock data.
    /// </summary>
    /// <param name="path">The route path to set.</param>
    /// <returns>The updated MockData instance.</returns>
        public MockData WithRoutePath(string path)
        {
            _apiContentRouteMock.SetupGet(r => r.Path).Returns(path);
            return this;
        }

    /// <summary>
    /// Sets the route start path for the mock data.
    /// </summary>
    /// <param name="path">The route start path to set.</param>
    /// <returns>The current instance of <see cref="MockData"/> for chaining.</returns>
        public MockData WithRouteStartPath(string path)
        {
            _apiContentStartItem.SetupGet(rsi => rsi.Path).Returns(path);
            return this;
        }

    /// <summary>
    /// Sets the media URL and returns the current MockData instance.
    /// </summary>
    /// <param name="url">The media URL to set.</param>
    /// <returns>The current MockData instance with the updated media URL.</returns>
        public MockData WithMediaUrl(string url)
        {
            MediaUrl = url;
            return this;
        }
    }
}
