// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class WebhookLogFactoryTests
{
    [Test]
    public async Task Creates_Log_For_Request_Without_Content()
    {
        var webhook = new Webhook("https://example.com/webhook") { Key = Guid.NewGuid() };
        using var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url);
        var exception = new InvalidOperationException("Failed before sending.");

        WebhookLog log = await new WebhookLogFactory().CreateAsync("event", request, null, 0, exception, webhook, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(log.RequestBody, Is.Empty);
            Assert.That(log.ExceptionOccured, Is.True);
            Assert.That(log.ResponseBody, Is.EqualTo(exception.Message));
            Assert.That(log.WebhookKey, Is.EqualTo(webhook.Key));
        });
    }

    [Test]
    public async Task Decodes_Bodies_With_Unknown_Charset_As_Utf8()
    {
        var webhook = new Webhook("https://example.com/webhook") { Key = Guid.NewGuid() };
        using var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url) { Content = CreateContent("{\"name\":\"æøå\"}") };
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = CreateContent("received æøå") };

        WebhookLog log = await new WebhookLogFactory().CreateAsync("event", request, response, 0, null, webhook, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(log.RequestBody, Is.EqualTo("{\"name\":\"æøå\"}"));
            Assert.That(log.ResponseBody, Is.EqualTo("received æøå"));
        });
    }

    private static ByteArrayContent CreateContent(string body)
    {
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(body));
        content.Headers.TryAddWithoutValidation("Content-Type", "application/json; charset=not-a-real-encoding");
        return content;
    }
}
