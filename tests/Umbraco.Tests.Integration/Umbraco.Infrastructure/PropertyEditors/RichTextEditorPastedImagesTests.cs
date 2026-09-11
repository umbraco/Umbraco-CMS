using System.Text;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.TemporaryFile;
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
internal sealed class RichTextEditorPastedImagesTests : UmbracoIntegrationTest
{
    private static readonly Guid GifFileKey = Guid.Parse("E625C7FA-6CA7-4A01-92CD-FB5C6F89973D");

    private static readonly Guid SvgFileKey = Guid.Parse("0E3A7DFE-DF09-4C3B-881C-E1B815A4502F");

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        // mock out the temporary file service so we don't have to read/write files from/to disk
        var temporaryFileServiceMock = new Mock<ITemporaryFileService>();
        temporaryFileServiceMock
            .Setup(t => t.GetAsync(GifFileKey))
            .Returns(Task.FromResult(new TemporaryFileModel
            {
                AvailableUntil = DateTime.UtcNow.AddDays(1),
                FileName = "the-pixel.gif",
                Key = GifFileKey,
                OpenReadStream = () => new MemoryStream(Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"))
            }));
        temporaryFileServiceMock
            .Setup(t => t.GetAsync(SvgFileKey))
            .Returns(Task.FromResult(new TemporaryFileModel
            {
                AvailableUntil = DateTime.UtcNow.AddDays(1),
                FileName = "the-vector.svg",
                Key = SvgFileKey,
                OpenReadStream = () => new MemoryStream(Encoding.UTF8.GetBytes(@"<svg viewBox=""0 0 70 74"" fill=""none"" xmlns=""http://www.w3.org/2000/svg""><rect width=""100%"" height=""100%"" fill=""black""/></svg>"))
            }));

        services.AddUnique<ITemporaryFileService>(temporaryFileServiceMock.Object);

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

    [Test]
    public async Task Can_Handle_Temp_Gif_Image()
    {
        var html = $"<p><img data-tmpimg=\"{GifFileKey:D}\"></p>";
        var subject = Services.GetRequiredService<RichTextEditorPastedImages>();

        var result = await subject.FindAndPersistPastedTempImagesAsync(html, Guid.Empty, Constants.Security.SuperUserKey);
        AssertContainsMedia(result, Constants.Conventions.MediaTypes.Image);
    }

    [Test]
    public async Task Can_Handle_Temp_Svg_Image()
    {
        var html = $"<p><img data-tmpimg=\"{SvgFileKey:D}\"></p>";
        var subject = Services.GetRequiredService<RichTextEditorPastedImages>();

        var result = await subject.FindAndPersistPastedTempImagesAsync(html, Guid.Empty, Constants.Security.SuperUserKey);
        AssertContainsMedia(result, Constants.Conventions.MediaTypes.VectorGraphicsAlias);
    }

    [Test]
    public async Task Ignores_Non_Existing_Temp_Image()
    {
        var key = Guid.NewGuid();
        var html = $"<p><img data-tmpimg=\"{key:D}\"></p>";
        var subject = Services.GetRequiredService<RichTextEditorPastedImages>();

        var result = await subject.FindAndPersistPastedTempImagesAsync(html, Guid.Empty, Constants.Security.SuperUserKey);
        Assert.That(result, Is.EqualTo(html));
    }

    [Test]
    public async Task Can_Handle_Multiple_Temp_Images()
    {
        var html = $"<p><img data-tmpimg=\"{SvgFileKey:D}\"></p><p><img data-tmpimg=\"{GifFileKey:D}\"></p>";
        var subject = Services.GetRequiredService<RichTextEditorPastedImages>();

        var result = await subject.FindAndPersistPastedTempImagesAsync(html, Guid.Empty, Constants.Security.SuperUserKey);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(result);
        var imageNodes = htmlDoc.DocumentNode.SelectNodes("//img");
        Assert.That(imageNodes, Has.Count.EqualTo(2));

        var udis = imageNodes.Select(imageNode => UdiParser.Parse(imageNode.Attributes["data-udi"].Value)).OfType<GuidUdi>().ToArray();
        Assert.That(udis, Has.Length.EqualTo(2));
        Assert.That(udis.Last().Guid, Is.Not.EqualTo(udis.First().Guid));

        var mediaService = Services.GetRequiredService<IMediaService>();
        Assert.Multiple(() =>
        {
            Assert.That(mediaService.GetById(udis.First().Guid), Is.Not.Null);
            Assert.That(mediaService.GetById(udis.Last().Guid), Is.Not.Null);
        });
    }

    [Test]
    public async Task Does_Not_Create_Duplicates_Of_The_Same_Temp_Image()
    {
        var html = $"<p><img data-tmpimg=\"{GifFileKey:D}\"></p><p><img data-tmpimg=\"{GifFileKey:D}\"></p>";
        var subject = Services.GetRequiredService<RichTextEditorPastedImages>();

        var result = await subject.FindAndPersistPastedTempImagesAsync(html, Guid.Empty, Constants.Security.SuperUserKey);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(result);
        var imageNodes = htmlDoc.DocumentNode.SelectNodes("//img");
        Assert.That(imageNodes, Has.Count.EqualTo(2));

        var udis = imageNodes.Select(imageNode => UdiParser.Parse(imageNode.Attributes["data-udi"].Value)).OfType<GuidUdi>().ToArray();
        Assert.That(udis, Has.Length.EqualTo(2));
        Assert.That(udis.Last().Guid, Is.EqualTo(udis.First().Guid));

        var mediaService = Services.GetRequiredService<IMediaService>();
        Assert.That(mediaService.GetById(udis.First().Guid), Is.Not.Null);
    }

    private void AssertContainsMedia(string result, string expectedMediaTypeAlias)
    {
        Assert.That(result, Does.Not.Contain("data-tmpimg"));

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(result);
        var imageNode = htmlDoc.DocumentNode.SelectNodes("//img").FirstOrDefault();
        Assert.That(imageNode, Is.Not.Null);

        Assert.That(imageNode.Attributes.Contains("src"), Is.True);
        Assert.That(imageNode.Attributes["src"].Value, Is.EqualTo("the-media-url"));

        Assert.That(imageNode.Attributes.Contains("data-udi"), Is.True);
        Assert.That(UdiParser.TryParse(imageNode.Attributes["data-udi"].Value, out GuidUdi udi), Is.True);
        Assert.That(udi.EntityType, Is.EqualTo(Constants.UdiEntityType.Media));

        var media = Services.GetRequiredService<IMediaService>().GetById(udi.Guid);
        Assert.That(media, Is.Not.Null);
        Assert.That(media.ContentType.Alias, Is.EqualTo(expectedMediaTypeAlias));
    }
}
