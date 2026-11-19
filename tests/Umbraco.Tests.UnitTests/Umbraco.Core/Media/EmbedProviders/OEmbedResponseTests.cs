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
}
