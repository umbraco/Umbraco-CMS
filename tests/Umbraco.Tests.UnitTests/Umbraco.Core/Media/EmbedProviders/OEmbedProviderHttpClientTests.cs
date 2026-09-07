// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class OEmbedProviderHttpClientTests
{
    [Test]
    public void Can_Get_Default_Shared_HttpClient_Instance()
    {
        var provider = new TestOEmbedProvider(Mock.Of<IJsonSerializer>());

        Assert.That(provider.ResolvedHttpClient, Is.SameAs(SharedHttpClient.Instance));
    }

    [Test]
    public void Can_Override_HttpClient_In_Provider()
    {
        using var httpClient = new HttpClient();
        var provider = new CustomHttpClientOEmbedProvider(Mock.Of<IJsonSerializer>(), httpClient);

        Assert.That(provider.ResolvedHttpClient, Is.SameAs(httpClient));
    }

    private class TestOEmbedProvider : OEmbedProviderBase
    {
        public TestOEmbedProvider(IJsonSerializer jsonSerializer)
            : base(jsonSerializer)
        {
        }

        public override string ApiEndpoint => "http://test.local/oembed";

        public override string[] UrlSchemeRegex => [@"^https?://test\.local/"];

        public override Dictionary<string, string> RequestParams => new();

        public HttpClient ResolvedHttpClient => GetHttpClient();

        public override Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }

    private class CustomHttpClientOEmbedProvider : TestOEmbedProvider
    {
        private readonly HttpClient _httpClient;

        public CustomHttpClientOEmbedProvider(IJsonSerializer jsonSerializer, HttpClient httpClient)
            : base(jsonSerializer) => _httpClient = httpClient;

        protected override HttpClient GetHttpClient() => _httpClient;
    }
}
