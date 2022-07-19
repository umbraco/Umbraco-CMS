// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
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
        <a href=""{locallink:umb://document/C093961595094900AAF9170DDE6AD442}"">hello</a>
    </div>
</p><p><img src='/media/234234.jpg' data-udi=""umb://media-type/B726D735E4C446D58F703F3FBCFC97A5"" />
<a href=""{locallink:umb://document-type/2D692FCB070B4CDA92FB6883FDBFD6E2}"">hello</a>
</p>";

        var umbracoContextAccessor = new TestUmbracoContextAccessor();
        var parser = new HtmlLocalLinkParser(umbracoContextAccessor, Mock.Of<IPublishedUrlProvider>());

        var result = parser.FindUdisFromLocalLinks(input).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(UdiParser.Parse("umb://document/C093961595094900AAF9170DDE6AD442"), result[0]);
        Assert.AreEqual(UdiParser.Parse("umb://document-type/2D692FCB070B4CDA92FB6883FDBFD6E2"), result[1]);
    }

    [TestCase("", "")]
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
        "hello href=\"{localLink:umb://media/9931BDE0AAC34BABB838909A7B47570E}\" world ",
        "hello href=\"/media/1001/my-image.jpg\" world ")]

    // This one has an invalid char so won't match.
    [TestCase(
        "hello href=\"{localLink:umb^://document/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ",
        "hello href=\"{localLink:umb^://document/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ")]
    [TestCase(
        "hello href=\"{localLink:umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ",
        "hello href=\"#\" world ")]
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
        var publishedUrlProvider = new UrlProvider(
            umbracoContextAccessor,
            Options.Create(webRoutingSettings),
            new UrlProviderCollection(() => new[] { contentUrlProvider.Object }),
            new MediaUrlProviderCollection(() => new[] { mediaUrlProvider.Object }),
            Mock.Of<IVariationContextAccessor>());
        using (var reference = umbracoContextFactory.EnsureUmbracoContext())
        {
            var contentCache = Mock.Get(reference.UmbracoContext.Content);
            contentCache.Setup(x => x.GetById(It.IsAny<int>())).Returns(publishedContent.Object);
            contentCache.Setup(x => x.GetById(It.IsAny<Guid>())).Returns(publishedContent.Object);

            var mediaCache = Mock.Get(reference.UmbracoContext.Media);
            mediaCache.Setup(x => x.GetById(It.IsAny<int>())).Returns(media.Object);
            mediaCache.Setup(x => x.GetById(It.IsAny<Guid>())).Returns(media.Object);

            var linkParser = new HtmlLocalLinkParser(umbracoContextAccessor, publishedUrlProvider);

            var output = linkParser.EnsureInternalLinks(input);

            Assert.AreEqual(result, output);
        }
    }
}
