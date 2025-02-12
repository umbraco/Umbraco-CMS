// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.UnitTests.TestHelpers.Objects;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Templates;

[TestFixture]
public class HtmlLocalLinkParserTests
{
    [Test]
    public void Returns_Udis_From_LocalLinks()
    {
        var input = @"<p>
    <div>
        <img src='/media/12312.jpg' data-udi='umb://media/D4B18427A1544721B09AC7692F35C264' />
        <a type=""document"" href=""/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}"" title=""other page"">other page</a>
    </div>
</p><p><img src='/media/234234.jpg' data-udi=""umb://media-type/B726D735E4C446D58F703F3FBCFC97A5"" />
<a type=""media"" href=""/{localLink:7e21a725-b905-4c5f-86dc-8c41ec116e39}"" title=""media"">media</a>
</p>";

        var parser = new HtmlLocalLinkParser(Mock.Of<IPublishedUrlProvider>());

        var result = parser.FindUdisFromLocalLinks(input).ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, result.Count);
            Assert.Contains(UdiParser.Parse("umb://document/eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f"), result);
            Assert.Contains(UdiParser.Parse("umb://media/7e21a725-b905-4c5f-86dc-8c41ec116e39"), result);
        });
    }

    // todo remove at some point and the implementation.
    [Test]
    public void Returns_Udis_From_Legacy_LocalLinks()
    {
        var input = @"<p>
    <div>
        <img src='/media/12312.jpg' data-udi='umb://media/D4B18427A1544721B09AC7692F35C264' />
        <a href=""{locallink:umb://document/C093961595094900AAF9170DDE6AD442}"">hello</a>
    </div>
</p><p><img src='/media/234234.jpg' data-udi=""umb://media-type/B726D735E4C446D58F703F3FBCFC97A5"" />
<a href=""{locallink:umb://document-type/2D692FCB070B4CDA92FB6883FDBFD6E2}"">hello</a>
</p>";

        var parser = new HtmlLocalLinkParser(Mock.Of<IPublishedUrlProvider>());

        var result = parser.FindUdisFromLocalLinks(input).ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, result.Count);
            Assert.Contains(UdiParser.Parse("umb://document/C093961595094900AAF9170DDE6AD442"), result);
            Assert.Contains(UdiParser.Parse("umb://document-type/2D692FCB070B4CDA92FB6883FDBFD6E2"), result);
        });
    }

    // todo remove at some point and the implementation.
    [Test]
    public void Returns_Udis_From_Legacy_And_Current_LocalLinks()
    {
        var input = @"<p>
    <div>
        <img src='/media/12312.jpg' data-udi='umb://media/D4B18427A1544721B09AC7692F35C264' />
        <a href=""{locallink:umb://document/C093961595094900AAF9170DDE6AD442}"">hello</a>
    </div>
</p><p><img src='/media/234234.jpg' data-udi=""umb://media-type/B726D735E4C446D58F703F3FBCFC97A5"" />
<a href=""{locallink:umb://document-type/2D692FCB070B4CDA92FB6883FDBFD6E2}"">hello</a>
</p>
<p>
    <div>
        <img src='/media/12312.jpg' data-udi='umb://media/D4B18427A1544721B09AC7692F35C264' />
        <a type=""document"" href=""/{localLink:eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f}"" title=""other page"">other page</a>
    </div>
</p><p><img src='/media/234234.jpg' data-udi=""umb://media-type/B726D735E4C446D58F703F3FBCFC97A5"" />
<a type=""media"" href=""/{localLink:7e21a725-b905-4c5f-86dc-8c41ec116e39}"" title=""media"">media</a>
</p>";

        var parser = new HtmlLocalLinkParser(Mock.Of<IPublishedUrlProvider>());

        var result = parser.FindUdisFromLocalLinks(input).ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(4, result.Count);
            Assert.Contains(UdiParser.Parse("umb://document/eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f"), result);
            Assert.Contains(UdiParser.Parse("umb://media/7e21a725-b905-4c5f-86dc-8c41ec116e39"), result);
            Assert.Contains(UdiParser.Parse("umb://document/C093961595094900AAF9170DDE6AD442"), result);
            Assert.Contains(UdiParser.Parse("umb://document-type/2D692FCB070B4CDA92FB6883FDBFD6E2"), result);
        });
    }

    [TestCase("", "")]
    // current
    [TestCase(
        "<a type=\"document\" href=\"/{localLink:9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" title=\"world\">world</a>",
        "<a href=\"/my-test-url\" title=\"world\">world</a>")]
    [TestCase(
        "<a type=\"media\" href=\"/{localLink:9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" title=\"world\">world</a>",
        "<a href=\"/media/1001/my-image.jpg\" title=\"world\">world</a>")]
    [TestCase(
        "<a href=\"/{localLink:9931BDE0-AAC3-4BAB-B838-909A7B47570E}\"type=\"document\" title=\"world\">world</a>",
        "<a href=\"/my-test-url\" title=\"world\">world</a>")]
    [TestCase(
        "<a href=\"/{localLink:9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" title=\"world\"type=\"media\">world</a>",
        "<a href=\"/media/1001/my-image.jpg\" title=\"world\">world</a>")]
    [TestCase(
        "<p><a type=\"document\" href=\"/{localLink:9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" title=\"world\">world</a></p><p><a href=\"/{localLink:7e21a725-b905-4c5f-86dc-8c41ec116e39}\" title=\"world\" type=\"media\">world</a></p>",
        "<p><a href=\"/my-test-url\" title=\"world\">world</a></p><p><a href=\"/media/1001/my-image.jpg\" title=\"world\">world</a></p>")]

    // attributes order should not matter
    [TestCase(
        "<a rel=\"noopener\" title=\"world\" type=\"document\" href=\"/{localLink:9931BDE0-AAC3-4BAB-B838-909A7B47570E}\">world</a>",
        "<a rel=\"noopener\" title=\"world\" href=\"/my-test-url\">world</a>")]
    [TestCase(
        "<a rel=\"noopener\" title=\"world\" href=\"/{localLink:9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" type=\"document\">world</a>",
        "<a rel=\"noopener\" title=\"world\" href=\"/my-test-url\">world</a>")]
    [TestCase(
        "<a rel=\"noopener\" title=\"world\" href=\"/{localLink:9931BDE0-AAC3-4BAB-B838-909A7B47570E}#anchor\" type=\"document\">world</a>",
        "<a rel=\"noopener\" title=\"world\" href=\"/my-test-url#anchor\">world</a>")]

    // anchors and query strings
    [TestCase(
        "<a type=\"document\" href=\"/{localLink:9931BDE0-AAC3-4BAB-B838-909A7B47570E}#anchor\" title=\"world\">world</a>",
        "<a href=\"/my-test-url#anchor\" title=\"world\">world</a>")]
    [TestCase(
        "<a type=\"document\" href=\"/{localLink:9931BDE0-AAC3-4BAB-B838-909A7B47570E}?v=1\" title=\"world\">world</a>",
        "<a href=\"/my-test-url?v=1\" title=\"world\">world</a>")]

    // custom type ignored
    [TestCase(
        "<a type=\"custom\" href=\"/{localLink:9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" title=\"world\">world</a>",
        "<a type=\"custom\" href=\"/{localLink:9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" title=\"world\">world</a>")]

    // legacy
    [TestCase(
        "hello href=\"{localLink:1234}\" world ",
        "hello href=\"/my-test-url\" world ")]
    [TestCase(
        "hello href=\"{localLink:umb://document/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ",
        "hello href=\"/my-test-url\" world ")]
    [TestCase(
        "hello href=\"{localLink:umb://document/9931BDE0AAC34BABB838909A7B47570E}\" world ",
        "hello href=\"/my-test-url\" world ")]
    [TestCase(
        "hello href=\"{localLink:umb://document/9931BDE0AAC34BABB838909A7B47570E}#anchor\" world ",
        "hello href=\"/my-test-url#anchor\" world ")]
    [TestCase(
        "hello href=\"{localLink:umb://media/9931BDE0AAC34BABB838909A7B47570E}\" world ",
        "hello href=\"/media/1001/my-image.jpg\" world ")]
    [TestCase(
        "hello href='{localLink:umb://media/9931BDE0AAC34BABB838909A7B47570E}' world ",
        "hello href='/media/1001/my-image.jpg' world ")]

    // This one has an invalid char so won't match.
    [TestCase(
        "hello href=\"{localLink:umb^://document/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ",
        "hello href=\"{localLink:umb^://document/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ")]
    [TestCase(
        "hello href=\"{localLink:umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ",
        "hello href=\"\" world ")]
    public void ParseLocalLinks(string input, string result)
    {
        // setup a mock URL provider which we'll use for testing
        var contentUrlProvider = new Mock<IUrlProvider>();
        contentUrlProvider
            .Setup(x => x.GetUrl(
                It.IsAny<IPublishedContent>(),
                It.IsAny<UrlMode>(),
                It.IsAny<string>(),
                It.IsAny<Uri>()))
            .Returns(UrlInfo.Url("/my-test-url"));
        var contentType = new PublishedContentType(
            Guid.NewGuid(),
            666,
            "alias",
            PublishedItemType.Content,
            Enumerable.Empty<string>(),
            Enumerable.Empty<PublishedPropertyType>(),
            ContentVariation.Nothing);
        var publishedContent = new Mock<IPublishedContent>();
        publishedContent.Setup(x => x.Id).Returns(1234);
        publishedContent.Setup(x => x.ContentType).Returns(contentType);

        var mediaType = new PublishedContentType(
            Guid.NewGuid(),
            777,
            "image",
            PublishedItemType.Media,
            Enumerable.Empty<string>(),
            Enumerable.Empty<PublishedPropertyType>(),
            ContentVariation.Nothing);
        var media = new Mock<IPublishedContent>();
        media.Setup(x => x.ContentType).Returns(mediaType);
        var mediaUrlProvider = new Mock<IMediaUrlProvider>();
        mediaUrlProvider.Setup(x => x.GetMediaUrl(
                It.IsAny<IPublishedContent>(),
                It.IsAny<string>(),
                It.IsAny<UrlMode>(),
                It.IsAny<string>(),
                It.IsAny<Uri>()))
            .Returns(UrlInfo.Url("/media/1001/my-image.jpg"));

        var umbracoContextAccessor = new TestUmbracoContextAccessor();

        var umbracoContextFactory = TestUmbracoContextFactory.Create(
            umbracoContextAccessor: umbracoContextAccessor);

        var webRoutingSettings = new WebRoutingSettings();

        var navigationQueryService = new Mock<IDocumentNavigationQueryService>();
        Guid? parentKey = null;
        navigationQueryService.Setup(x => x.TryGetParentKey(It.IsAny<Guid>(), out parentKey)).Returns(true);

        using (var reference = umbracoContextFactory.EnsureUmbracoContext())
        {
            var contentCache = Mock.Get(reference.UmbracoContext.Content);
            contentCache.Setup(x => x.GetById(It.IsAny<int>())).Returns(publishedContent.Object);
            contentCache.Setup(x => x.GetById(It.IsAny<Guid>())).Returns(publishedContent.Object);

            var mediaCache = Mock.Get(reference.UmbracoContext.Media);
            mediaCache.Setup(x => x.GetById(It.IsAny<int>())).Returns(media.Object);
            mediaCache.Setup(x => x.GetById(It.IsAny<Guid>())).Returns(media.Object);

            var publishStatusQueryService = new Mock<IPublishStatusQueryService>();
            publishStatusQueryService
                .Setup(x => x.IsDocumentPublished(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(true);

            var publishedUrlProvider = new UrlProvider(
                umbracoContextAccessor,
                Options.Create(webRoutingSettings),
                new UrlProviderCollection(() => new[] { contentUrlProvider.Object }),
                new MediaUrlProviderCollection(() => new[] { mediaUrlProvider.Object }),
                Mock.Of<IVariationContextAccessor>(),
                contentCache.Object,
                navigationQueryService.Object,
                publishStatusQueryService.Object);

            var linkParser = new HtmlLocalLinkParser(publishedUrlProvider);

            var output = linkParser.EnsureInternalLinks(input);

            Assert.AreEqual(result, output);
        }
    }
}
