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
        Assert.AreEqual(id, webhook.Id);
        Assert.AreEqual(key, webhook.Key);
        Assert.AreEqual(url, webhook.Url);
        Assert.AreEqual(enabled, webhook.Enabled);
        Assert.AreEqual(entityKeys.Length, webhook.ContentTypeKeys.Length);
        Assert.AreEqual(entityKeys[0], webhook.ContentTypeKeys[0]);
        Assert.AreEqual(events.Length, webhook.Events.Length);
        Assert.AreEqual(events[0], webhook.Events[0]);
        Assert.AreEqual(events.Length, webhook.Events.Length);
        Assert.AreEqual("application/json", webhook.Headers["Content-Type"]);
    }
}
