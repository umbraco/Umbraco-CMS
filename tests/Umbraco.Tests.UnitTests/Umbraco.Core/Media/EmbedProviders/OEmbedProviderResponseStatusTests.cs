// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class OEmbedProviderResponseStatusTests
{
    private const string Url = "https://test.local/media/123";

    private static readonly IJsonSerializer _jsonSerializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.NotImplemented)]
    public async Task Json_Based_Markup_Is_Null_When_Provider_Reports_Resource_Is_Not_Embeddable(HttpStatusCode statusCode)
    {
        using var httpClient = CreateHttpClient(statusCode, "Not Found");
        var provider = new TestOEmbedProvider(httpClient);

        var markup = await provider.GetJsonBasedMarkupAsync(Url, null, null, CancellationToken.None);

        Assert.That(markup, Is.Null);
    }

    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.NotImplemented)]
    public async Task Xml_Based_Markup_Is_Null_When_Provider_Reports_Resource_Is_Not_Embeddable(HttpStatusCode statusCode)
    {
        using var httpClient = CreateHttpClient(statusCode, "Not Found");
        var provider = new TestOEmbedProvider(httpClient);

        var markup = await provider.GetXmlBasedMarkupAsync(Url, null, null, CancellationToken.None);

        Assert.That(markup, Is.Null);
    }

    [TestCase(HttpStatusCode.InternalServerError)]
    [TestCase(HttpStatusCode.ServiceUnavailable)]
    [TestCase(HttpStatusCode.TooManyRequests)]
    public void Json_Based_Markup_Throws_HttpRequestException_For_Other_Non_Success_Status(HttpStatusCode statusCode)
    {
        using var httpClient = CreateHttpClient(statusCode, "Error");
        var provider = new TestOEmbedProvider(httpClient);

        HttpRequestException? exception = Assert.ThrowsAsync<HttpRequestException>(
            () => provider.GetJsonBasedMarkupAsync(Url, null, null, CancellationToken.None));

        Assert.That(exception!.StatusCode, Is.EqualTo(statusCode));
    }

    [TestCase(HttpStatusCode.InternalServerError)]
    [TestCase(HttpStatusCode.ServiceUnavailable)]
    [TestCase(HttpStatusCode.TooManyRequests)]
    public void Xml_Based_Markup_Throws_HttpRequestException_For_Other_Non_Success_Status(HttpStatusCode statusCode)
    {
        using var httpClient = CreateHttpClient(statusCode, "Error");
        var provider = new TestOEmbedProvider(httpClient);

        HttpRequestException? exception = Assert.ThrowsAsync<HttpRequestException>(
            () => provider.GetXmlBasedMarkupAsync(Url, null, null, CancellationToken.None));

        Assert.That(exception!.StatusCode, Is.EqualTo(statusCode));
    }

    [Test]
    public void Xml_Response_Throws_HttpRequestException_When_Provider_Reports_Resource_Is_Not_Embeddable()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.NotFound, "Not Found");
        var provider = new TestOEmbedProvider(httpClient);

        HttpRequestException? exception = Assert.ThrowsAsync<HttpRequestException>(
            () => provider.GetXmlResponseAsync(Url, CancellationToken.None));

        Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Json_Based_Markup_Is_Returned_For_Success_Status()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, """{"type":"video","html":"<iframe></iframe>"}""");
        var provider = new TestOEmbedProvider(httpClient);

        var markup = await provider.GetJsonBasedMarkupAsync(Url, null, null, CancellationToken.None);

        Assert.That(markup, Is.EqualTo("<iframe></iframe>"));
    }

    [Test]
    public async Task Xml_Based_Markup_Is_Returned_For_Success_Status()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, "<oembed><type>video</type><html>&lt;iframe&gt;&lt;/iframe&gt;</html></oembed>");
        var provider = new TestOEmbedProvider(httpClient);

        var markup = await provider.GetXmlBasedMarkupAsync(Url, null, null, CancellationToken.None);

        Assert.That(markup, Is.EqualTo("<iframe></iframe>"));
    }

    [Test]
    public async Task YouTube_Markup_Is_Null_For_Removed_Video()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.NotFound, "Not Found");
        var provider = new TestYouTube(httpClient);

        var markup = await provider.GetMarkupAsync("https://www.youtube.com/watch?v=abc123", null, null, CancellationToken.None);

        Assert.That(markup, Is.Null);
    }

    [Test]
    public async Task Flickr_Markup_Is_Null_When_Provider_Reports_Resource_Is_Not_Embeddable()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.NotFound, "Not Found");
        var provider = new TestFlickr(httpClient);

        var markup = await provider.GetMarkupAsync("https://www.flickr.com/photos/abc/123", null, null, CancellationToken.None);

        Assert.That(markup, Is.Null);
    }

    private static HttpClient CreateHttpClient(HttpStatusCode statusCode, string body)
        => new(new StubHttpMessageHandler(statusCode, body));

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;

        public StubHttpMessageHandler(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "text/plain"),
                RequestMessage = request,
            });
    }

    private sealed class TestOEmbedProvider : OEmbedProviderBase
    {
        private readonly HttpClient _httpClient;

        public TestOEmbedProvider(HttpClient httpClient)
            : base(_jsonSerializer) => _httpClient = httpClient;

        public override string ApiEndpoint => "https://test.local/oembed";

        public override string[] UrlSchemeRegex => [@"^https?://test\.local/"];

        public override Dictionary<string, string> RequestParams => [];

        public override Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
            => GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);

        protected override HttpClient GetHttpClient() => _httpClient;
    }

    private sealed class TestYouTube : YouTube
    {
        private readonly HttpClient _httpClient;

        public TestYouTube(HttpClient httpClient)
            : base(_jsonSerializer) => _httpClient = httpClient;

        protected override HttpClient GetHttpClient() => _httpClient;
    }

    private sealed class TestFlickr : Flickr
    {
        private readonly HttpClient _httpClient;

        public TestFlickr(HttpClient httpClient)
            : base(_jsonSerializer) => _httpClient = httpClient;

        protected override HttpClient GetHttpClient() => _httpClient;
    }
}
