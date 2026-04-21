using System.Xml;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class OEmbedProviderXxeSecurityTests
{
    private TestOEmbedProvider Provider { get; } = new(Mock.Of<IJsonSerializer>());

    /// <summary>
    /// Tests that XML containing a DTD is rejected, preventing XXE attacks.
    /// </summary>
    [TestCase(
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE oembed [
          <!ENTITY xxe SYSTEM "file:///etc/hostname">
        ]>
        <oembed>
          <type>photo</type>
          <url>&xxe;</url>
          <width>100</width>
          <height>100</height>
          <title>XXE Test</title>
        </oembed>
        """,
        TestName = "ExternalEntityReference")]
    [TestCase(
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE oembed [
          <!ENTITY company "Umbraco">
        ]>
        <oembed>
          <type>photo</type>
          <title>&company; Photo</title>
          <url>https://example.com/photo.jpg</url>
          <width>800</width>
          <height>600</height>
        </oembed>
        """,
        TestName = "InlineEntityDeclaration")]
    [TestCase(
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE oembed SYSTEM "http://attacker.com/evil.dtd">
        <oembed>
          <type>photo</type>
          <url>https://example.com/photo.jpg</url>
          <width>800</width>
          <height>600</height>
        </oembed>
        """,
        TestName = "ExternalDtdReference")]
    public void GetXmlResponseAsync_WithDtd_ThrowsXmlException(string maliciousXml)
    {
        Provider.SetMockResponse(maliciousXml);

        var exception = Assert.ThrowsAsync<XmlException>(
            async () => await Provider.GetXmlResponseAsync("http://test.local/oembed", CancellationToken.None));

        Assert.That(exception!.Message, Does.Contain("DTD").IgnoreCase);
    }

    [Test]
    public async Task GetXmlResponseAsync_WithLegitimateXml_SuccessfullyParses()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <oembed>
              <type>photo</type>
              <url>https://example.com/photo.jpg</url>
              <width>1024</width>
              <height>768</height>
              <title>Legitimate Photo</title>
              <author_name>Test Author</author_name>
            </oembed>
            """;

        Provider.SetMockResponse(xml);

        // Act
        var doc = await Provider.GetXmlResponseAsync("http://test.local/oembed", CancellationToken.None);

        // Assert
        Assert.That(doc, Is.Not.Null);
        Assert.That(doc.DocumentElement?.Name, Is.EqualTo("oembed"));
        Assert.That(doc.SelectSingleNode("//type")?.InnerText, Is.EqualTo("photo"));
        Assert.That(doc.SelectSingleNode("//url")?.InnerText, Is.EqualTo("https://example.com/photo.jpg"));
        Assert.That(doc.SelectSingleNode("//width")?.InnerText, Is.EqualTo("1024"));
        Assert.That(doc.SelectSingleNode("//height")?.InnerText, Is.EqualTo("768"));
        Assert.That(doc.SelectSingleNode("//title")?.InnerText, Is.EqualTo("Legitimate Photo"));
    }

    [Test]
    public async Task GetXmlResponseAsync_WithCDataSection_SuccessfullyParses()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <oembed>
              <type>rich</type>
              <html><![CDATA[<iframe src="https://example.com/embed" width="640" height="480"></iframe>]]></html>
              <width>640</width>
              <height>480</height>
            </oembed>
            """;

        Provider.SetMockResponse(xml);

        // Act
        var doc = await Provider.GetXmlResponseAsync("http://test.local/oembed", CancellationToken.None);

        // Assert
        Assert.That(doc, Is.Not.Null);
        Assert.That(
            doc.SelectSingleNode("//html")?.InnerText,
            Does.Contain("<iframe src=\"https://example.com/embed\""));
    }

    [Test]
    public async Task GetXmlResponseAsync_WithSpecialCharacters_SuccessfullyParses()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <oembed>
              <type>photo</type>
              <title>Photo with &quot;quotes&quot; &amp; &lt;special&gt; chars</title>
              <url>https://example.com/photo.jpg</url>
              <width>800</width>
              <height>600</height>
            </oembed>
            """;

        Provider.SetMockResponse(xml);

        // Act
        var doc = await Provider.GetXmlResponseAsync("http://test.local/oembed", CancellationToken.None);

        // Assert
        Assert.That(
            doc.SelectSingleNode("//title")?.InnerText,
            Is.EqualTo("Photo with \"quotes\" & <special> chars"));
    }

    private class TestOEmbedProvider : OEmbedProviderBase
    {
        private string _mockResponse = string.Empty;

        public TestOEmbedProvider(IJsonSerializer jsonSerializer)
            : base(jsonSerializer)
        {
        }

        public override string ApiEndpoint => "http://test.local/oembed";

        public override string[] UrlSchemeRegex => [@"^https?://test\.local/"];

        public override Dictionary<string, string> RequestParams => new();

        public override Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public void SetMockResponse(string response) => _mockResponse = response;

        public override Task<string> DownloadResponseAsync(string url, CancellationToken cancellationToken)
            => Task.FromResult(_mockResponse);
    }
}
