using NUnit.Framework;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class OEmbedResponseTests
{
    [Test]
    public void Deserialize_PhotoResponse_PopulatesDimensionsInMarkup()
    {
        const string json = """
            {
                "type": "photo",
                "version": "1.0",
                "title": "A gif",
                "url": "https://media4.giphy.com/media/abc123/giphy.gif",
                "width": 480,
                "height": 342
            }
            """;

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

        OEmbedResponse? response = serializer.Deserialize<OEmbedResponse>(json);

        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response!.Width, Is.EqualTo(480));
            Assert.That(response.Height, Is.EqualTo(342));
            Assert.That(
                response.GetHtml(),
                Is.EqualTo("<img src=\"https://media4.giphy.com/media/abc123/giphy.gif\" width=\"480\" height=\"342\" alt=\"A gif\" />"));
        });
    }

    [Test]
    public void Deserialize_BindsAllOEmbedFieldsByTheirSpecNames()
    {
        const string json = """
            {
                "type": "video",
                "version": "1.0",
                "title": "A video",
                "author_name": "An author",
                "author_url": "https://example.com/author",
                "provider_name": "A provider",
                "provider_url": "https://example.com/",
                "thumbnail_url": "https://example.com/thumbnail.jpg",
                "thumbnail_width": 320,
                "thumbnail_height": 180,
                "html": "<iframe></iframe>",
                "url": "https://example.com/video",
                "width": 640,
                "height": 360
            }
            """;

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

        OEmbedResponse? response = serializer.Deserialize<OEmbedResponse>(json);

        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response!.Type, Is.EqualTo("video"));
            Assert.That(response.Version, Is.EqualTo("1.0"));
            Assert.That(response.Title, Is.EqualTo("A video"));
            Assert.That(response.AuthorName, Is.EqualTo("An author"));
            Assert.That(response.AuthorUrl, Is.EqualTo("https://example.com/author"));
            Assert.That(response.ProviderName, Is.EqualTo("A provider"));
            Assert.That(response.ProviderUrl, Is.EqualTo("https://example.com/"));
            Assert.That(response.ThumbnailUrl, Is.EqualTo("https://example.com/thumbnail.jpg"));
            Assert.That(response.ThumbnailWidth, Is.EqualTo(320));
            Assert.That(response.ThumbnailHeight, Is.EqualTo(180));
            Assert.That(response.Html, Is.EqualTo("<iframe></iframe>"));
            Assert.That(response.Url, Is.EqualTo("https://example.com/video"));
            Assert.That(response.Width, Is.EqualTo(640));
            Assert.That(response.Height, Is.EqualTo(360));
        });
    }

    [Test]
    public void Deserialize_WithStringDimensions_BindsDimensionsByTheirSpecNames()
    {
        const string json = """
            {
                "type": "rich",
                "thumbnail_width": "320",
                "thumbnail_height": "180",
                "width": "100%",
                "height": "360"
            }
            """;

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

        OEmbedResponseWithStringDimensions? response = serializer.Deserialize<OEmbedResponseWithStringDimensions>(json);

        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response!.ThumbnailWidth, Is.EqualTo("320"));
            Assert.That(response.ThumbnailHeight, Is.EqualTo("180"));
            Assert.That(response.Width, Is.EqualTo("100%"));
            Assert.That(response.Height, Is.EqualTo("360"));
        });
    }
}
