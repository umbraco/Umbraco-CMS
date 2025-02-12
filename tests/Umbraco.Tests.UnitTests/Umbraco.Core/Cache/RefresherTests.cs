// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class RefresherTests
{
    [Test]
    public void MediaCacheRefresherCanDeserializeJsonPayload()
    {
        MediaCacheRefresher.JsonPayload[] source =
        {
            new MediaCacheRefresher.JsonPayload(1234, Guid.NewGuid(), TreeChangeTypes.None),
        };

        var json = JsonSerializer.Serialize(source);
        var payload = JsonSerializer.Deserialize<MediaCacheRefresher.JsonPayload[]>(json);

        Assert.AreEqual(source[0].Id, payload[0].Id);
        Assert.AreEqual(source[0].Key, payload[0].Key);
        Assert.AreEqual(source[0].ChangeTypes, payload[0].ChangeTypes);
    }

    [TestCase(TreeChangeTypes.None, false)]
    [TestCase(TreeChangeTypes.RefreshAll, true)]
    [TestCase(TreeChangeTypes.RefreshBranch, false)]
    [TestCase(TreeChangeTypes.Remove, true)]
    [TestCase(TreeChangeTypes.RefreshNode, false)]
    public void ContentCacheRefresherCanDeserializeJsonPayload(TreeChangeTypes changeTypes, bool blueprint)
    {
        var key = Guid.NewGuid();
        ContentCacheRefresher.JsonPayload[] source =
        {
            new ContentCacheRefresher.JsonPayload()
            {
                Id = 1234,
                Key = key,
                ChangeTypes = changeTypes,
                Blueprint = blueprint
            }
        };

        var json = JsonSerializer.Serialize(source);
        var payload = JsonSerializer.Deserialize<ContentCacheRefresher.JsonPayload[]>(json);

        Assert.AreEqual(1234, payload[0].Id);
        Assert.AreEqual(key, payload[0].Key);
        Assert.AreEqual(changeTypes, payload[0].ChangeTypes);
        Assert.AreEqual(blueprint, payload[0].Blueprint);
        Assert.IsNull(payload[0].PublishedCultures);
        Assert.IsNull(payload[0].UnpublishedCultures);
    }

    [Test]
    public void ContentCacheRefresherCanDeserializeJsonPayloadWithCultures()
    {
        var key = Guid.NewGuid();
        ContentCacheRefresher.JsonPayload[] source =
        {
            new ContentCacheRefresher.JsonPayload()
            {
                Id = 1234,
                Key = key,
                PublishedCultures = ["en-US", "da-DK"],
                UnpublishedCultures = ["de-DE"]
            }
        };

        var json = JsonSerializer.Serialize(source);
        var payload = JsonSerializer.Deserialize<ContentCacheRefresher.JsonPayload[]>(json);

        Assert.IsNotNull(payload[0].PublishedCultures);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, payload[0].PublishedCultures.Length);
            Assert.AreEqual("en-US", payload[0].PublishedCultures.First());
            Assert.AreEqual("da-DK", payload[0].PublishedCultures.Last());
        });

        Assert.IsNotNull(payload[0].UnpublishedCultures);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, payload[0].UnpublishedCultures.Length);
            Assert.AreEqual("de-DE", payload[0].UnpublishedCultures.First());
        });
    }

    [Test]
    public void ContentTypeCacheRefresherCanDeserializeJsonPayload()
    {
        ContentTypeCacheRefresher.JsonPayload[] source =
        {
            new ContentTypeCacheRefresher.JsonPayload("xxx", 1234, ContentTypeChangeTypes.None),
        };

        var json = JsonSerializer.Serialize(source);
        var payload = JsonSerializer.Deserialize<ContentTypeCacheRefresher.JsonPayload[]>(json);

        Assert.AreEqual(source[0].ItemType, payload[0].ItemType);
        Assert.AreEqual(source[0].Id, payload[0].Id);
        Assert.AreEqual(source[0].ChangeTypes, payload[0].ChangeTypes);
    }

    [Test]
    public void DataTypeCacheRefresherCanDeserializeJsonPayload()
    {
        DataTypeCacheRefresher.JsonPayload[] source =
        {
            new DataTypeCacheRefresher.JsonPayload(1234, Guid.NewGuid(), true),
        };

        var json = JsonSerializer.Serialize(source);
        var payload = JsonSerializer.Deserialize<DataTypeCacheRefresher.JsonPayload[]>(json);

        Assert.AreEqual(source[0].Id, payload[0].Id);
        Assert.AreEqual(source[0].Key, payload[0].Key);
        Assert.AreEqual(source[0].Removed, payload[0].Removed);
    }

    [Test]
    public void DomainCacheRefresherCanDeserializeJsonPayload()
    {
        DomainCacheRefresher.JsonPayload[] source =
        {
            new DomainCacheRefresher.JsonPayload(1234, DomainChangeTypes.None)
        };

        var json = JsonSerializer.Serialize(source);
        var payload = JsonSerializer.Deserialize<DomainCacheRefresher.JsonPayload[]>(json);

        Assert.AreEqual(source[0].Id, payload[0].Id);
        Assert.AreEqual(source[0].ChangeType, payload[0].ChangeType);
    }
}
