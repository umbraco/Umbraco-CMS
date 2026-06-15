// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class WebhookBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int id = 1;
        var key = Guid.NewGuid();
        const string url = "https://www.test.com";
        const bool enabled = true;
        Guid[] entityKeys = new Guid[] { Guid.NewGuid() };
        string[] events = new string[] { "ContentPublished" };
        var headers = new Dictionary<string, string>() { { "Content-Type", "application/json" } };

        var builder = new WebhookBuilder();

        // Act
        var webhook = builder
            .WithId(id)
            .WithKey(key)
            .WithUrl(url)
            .WithEnabled(enabled)
            .WithEntityKeys(entityKeys)
            .WithEvents(events)
            .WithHeaders(headers)
            .Build();

        // Assert
        Assert.That(webhook.Id, Is.EqualTo(id));
        Assert.That(webhook.Key, Is.EqualTo(key));
        Assert.That(webhook.Url, Is.EqualTo(url));
        Assert.That(webhook.Enabled, Is.EqualTo(enabled));
        Assert.That(webhook.ContentTypeKeys.Length, Is.EqualTo(entityKeys.Length));
        Assert.That(webhook.ContentTypeKeys[0], Is.EqualTo(entityKeys[0]));
        Assert.That(webhook.Events.Length, Is.EqualTo(events.Length));
        Assert.That(webhook.Events[0], Is.EqualTo(events[0]));
        Assert.That(webhook.Events.Length, Is.EqualTo(events.Length));
        Assert.That(webhook.Headers["Content-Type"], Is.EqualTo("application/json"));
    }
}
