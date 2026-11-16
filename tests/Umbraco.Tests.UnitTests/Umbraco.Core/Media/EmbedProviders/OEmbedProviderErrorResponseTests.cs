// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Tests how OEmbed providers handle an unsuccessful response from the OEmbed endpoint, e.g. YouTube's
/// plain-text "Not Found" for a removed or private video.
/// </summary>
[TestFixture]
public class OEmbedProviderErrorResponseTests
{
    [Test]
    public async Task GetJsonBasedMarkupAsync_With_Not_Found_Response_Returns_Null()
    {
        using var httpClient = new HttpClient(new StaticResponseHandler(HttpStatusCode.NotFound, "Not Found"));
        var provider = new TestOEmbedProvider(new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()), httpClient);

        var markup = await provider.GetJsonBasedMarkupAsync("http://test.local/video", null, null, CancellationToken.None);

        Assert.That(markup, Is.Null);
    }

    [Test]
    public async Task GetXmlBasedMarkupAsync_With_Not_Found_Response_Returns_Empty()
    {
        using var httpClient = new HttpClient(new StaticResponseHandler(HttpStatusCode.NotFound, "Not Found"));
        var provider = new TestOEmbedProvider(new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()), httpClient);

        var markup = await provider.GetXmlBasedMarkupAsync("http://test.local/video", null, null, CancellationToken.None);

        Assert.That(markup, Is.Empty);
    }

    [Test]
    public async Task GetJsonBasedMarkupAsync_With_Successful_Response_Returns_Html()
    {
        using var httpClient = new HttpClient(new StaticResponseHandler(HttpStatusCode.OK, """{"type":"video","html":"<iframe></iframe>"}"""));
        var provider = new TestOEmbedProvider(new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()), httpClient);

        var markup = await provider.GetJsonBasedMarkupAsync("http://test.local/video", null, null, CancellationToken.None);

        Assert.That(markup, Is.EqualTo("<iframe></iframe>"));
    }

    private class TestOEmbedProvider : OEmbedProviderBase
    {
        private readonly HttpClient _httpClient;

        public TestOEmbedProvider(IJsonSerializer jsonSerializer, HttpClient httpClient)
            : base(jsonSerializer) => _httpClient = httpClient;

        public override string ApiEndpoint => "http://test.local/oembed";

        public override string[] UrlSchemeRegex => [@"^https?://test\.local/"];

        public override Dictionary<string, string> RequestParams => new();

        public override Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        protected override HttpClient GetHttpClient() => _httpClient;
    }

    private class StaticResponseHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public StaticResponseHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content, Encoding.UTF8, "text/plain"),
            });
    }
}
