using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class RichTextEditorPastedImagesTests : UmbracoIntegrationTest
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        // the integration tests do not really play nice with published content, so we need to mock a fair bit in order to generate media URLs
        var publishedMediaTypeMock = new Mock<IPublishedContentType>();
        publishedMediaTypeMock.SetupGet(c => c.ItemType).Returns(PublishedItemType.Media);

        var publishedMediaMock = new Mock<IPublishedContent>();
        publishedMediaMock.SetupGet(m => m.ContentType).Returns(publishedMediaTypeMock.Object);

        var publishedMediaCacheMock = new Mock<IPublishedMediaCache>();
        publishedMediaCacheMock.Setup(mc => mc.GetById(It.IsAny<Guid>())).Returns(publishedMediaMock.Object);

        var umbracoContextMock = new Mock<IUmbracoContext>();
        umbracoContextMock.SetupGet(c => c.Media).Returns(publishedMediaCacheMock.Object);
        var umbracoContext = umbracoContextMock.Object;

        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        umbracoContextAccessor.Setup(ca => ca.TryGetUmbracoContext(out umbracoContext)).Returns(true);

        services.AddUnique<IUmbracoContextAccessor>(umbracoContextAccessor.Object);

        var publishedUrlProviderMock = new Mock<IPublishedUrlProvider>();
        publishedUrlProviderMock
            .Setup(pu => pu.GetMediaUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<Uri?>()))
            .Returns("the-media-url");

        services.AddUnique<IPublishedUrlProvider>(publishedUrlProviderMock.Object);
    }

    [SetUp]
    public void Setup()
    {
        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "https",
                Host = new HostString("localhost"),
                Path = "/",
                QueryString = new QueryString(string.Empty)
            }
        };
        Services.GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();
    }

    [TestCase(@"<p><img src=""data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7""></p>", Constants.Conventions.MediaTypes.Image)]
    [TestCase(@"<p><img src=""data:image/svg+xml;utf8,<svg viewBox=""0 0 70 74"" fill=""none"" xmlns=""http://www.w3.org/2000/svg""><rect width=""100%"" height=""100%"" fill=""black""/></svg>""></p>", Constants.Conventions.MediaTypes.VectorGraphicsAlias)]
    [TestCase(@"<p><img src=""data:image/jpg;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7""></p>", Constants.Conventions.MediaTypes.Image)]
    public async Task Can_Handle_Valid_Embedded_Images(string html, string expectedMediaTypeAlias)
    {
        var subject = Services.GetRequiredService<RichTextEditorPastedImages>();
        var result = await subject.FindAndPersistEmbeddedImagesAsync(html, Guid.Empty, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Contains("data:image"));

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(result);
        var imageNode = htmlDoc.DocumentNode.SelectNodes("//img").FirstOrDefault();
        Assert.IsNotNull(imageNode);

        Assert.IsTrue(imageNode.Attributes.Contains("src"));
        Assert.AreEqual("the-media-url", imageNode.Attributes["src"].Value);

        Assert.IsTrue(imageNode.Attributes.Contains("data-udi"));
        Assert.IsTrue(UdiParser.TryParse(imageNode.Attributes["data-udi"].Value, out GuidUdi udi));
        Assert.AreEqual(Constants.UdiEntityType.Media, udi.EntityType);

        var media = Services.GetRequiredService<IMediaService>().GetById(udi.Guid);
        Assert.IsNotNull(media);
        Assert.AreEqual(expectedMediaTypeAlias, media.ContentType.Alias);
    }

    [TestCase(@"<p><img src=""/some/random/image.jpg""></p>")]
    [TestCase(@"<p><img src=""/some/random/image.jpg""></p><p><img src=""/some/other/image.jpg""></p>")]
    public async Task Ignores_Non_Embedded_Images(string html)
    {
        var subject = Services.GetRequiredService<RichTextEditorPastedImages>();
        var result = await subject.FindAndPersistEmbeddedImagesAsync(html, Guid.Empty, Constants.Security.SuperUserKey);
        Assert.AreEqual(html, result);
    }

    [TestCase(@"<p><img src=""data:image/notallowedextension;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7""></p>")]
    [TestCase(@"<p><img src=""data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7""></p><p><img src=""data:image/notallowedextension;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7""></p>")]
    public async Task Ignores_Disallowed_Embedded_Images(string html)
    {
        var subject = Services.GetRequiredService<RichTextEditorPastedImages>();
        var result = await subject.FindAndPersistEmbeddedImagesAsync(html, Guid.Empty, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Contains("data:image/notallowedextension"));
        Assert.IsFalse(result.Contains("data:image/gif"));
    }

    [Test]
    public async Task Can_Handle_Multiple_Embedded_Images()
    {
        const string html = @"<p><img src=""data:image/jpg;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7""></p><p><img src=""data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7""></p>";
        var subject = Services.GetRequiredService<RichTextEditorPastedImages>();
        var result = await subject.FindAndPersistEmbeddedImagesAsync(html, Guid.Empty, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Contains("data:image"));

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(result);
        var imageNodes = htmlDoc.DocumentNode.SelectNodes("//img");
        Assert.AreEqual(2, imageNodes.Count);

        var udis = imageNodes.Select(imageNode => UdiParser.Parse(imageNode.Attributes["data-udi"].Value))
            .OfType<GuidUdi>().ToArray();
        Assert.AreEqual(2, udis.Length);

        Assert.AreNotEqual(udis.First().Guid, udis.Last().Guid);

        var mediaService = Services.GetRequiredService<IMediaService>();
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(mediaService.GetById(udis.First().Guid));
            Assert.IsNotNull(mediaService.GetById(udis.Last().Guid));
        });
    }
}
