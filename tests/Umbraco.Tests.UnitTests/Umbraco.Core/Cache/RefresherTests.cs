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

        Assert.That(payload[0].Id, Is.EqualTo(source[0].Id));
        Assert.That(payload[0].Key, Is.EqualTo(source[0].Key));
        Assert.That(payload[0].ChangeTypes, Is.EqualTo(source[0].ChangeTypes));
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

        Assert.That(payload[0].Id, Is.EqualTo(1234));
        Assert.That(payload[0].Key, Is.EqualTo(key));
        Assert.That(payload[0].ChangeTypes, Is.EqualTo(changeTypes));
        Assert.That(payload[0].Blueprint, Is.EqualTo(blueprint));
        Assert.That(payload[0].PublishedCultures, Is.Null);
        Assert.That(payload[0].UnpublishedCultures, Is.Null);
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

        Assert.That(payload[0].PublishedCultures, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(payload[0].PublishedCultures.Length, Is.EqualTo(2));
            Assert.That(payload[0].PublishedCultures.First(), Is.EqualTo("en-US"));
            Assert.That(payload[0].PublishedCultures.Last(), Is.EqualTo("da-DK"));
        });

        Assert.That(payload[0].UnpublishedCultures, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(payload[0].UnpublishedCultures.Length, Is.EqualTo(1));
            Assert.That(payload[0].UnpublishedCultures.First(), Is.EqualTo("de-DE"));
        });
    }

    [TestCase(TreeChangeTypes.None)]
    [TestCase(TreeChangeTypes.RefreshAll)]
    [TestCase(TreeChangeTypes.RefreshBranch)]
    [TestCase(TreeChangeTypes.Remove)]
    [TestCase(TreeChangeTypes.RefreshNode)]
    public void ElementCacheRefresherCanDeserializeJsonPayload(TreeChangeTypes changeTypes)
    {
        var key = Guid.NewGuid();
        ElementCacheRefresher.JsonPayload[] source =
        {
            new(1234, key, changeTypes)
        };

        var json = JsonSerializer.Serialize(source);
        var payload = JsonSerializer.Deserialize<ElementCacheRefresher.JsonPayload[]>(json);

        Assert.That(payload[0].Id, Is.EqualTo(1234));
        Assert.That(payload[0].Key, Is.EqualTo(key));
        Assert.That(payload[0].ChangeTypes, Is.EqualTo(changeTypes));
        Assert.That(payload[0].PublishedCultures, Is.Null);
        Assert.That(payload[0].UnpublishedCultures, Is.Null);
    }

    [Test]
    public void ElementCacheRefresherCanDeserializeJsonPayloadWithCultures()
    {
        var key = Guid.NewGuid();
        ElementCacheRefresher.JsonPayload[] source =
        {
            new(1234, key, TreeChangeTypes.RefreshNode)
            {
                PublishedCultures = ["en-US", "da-DK"],
                UnpublishedCultures = ["de-DE"]
            }
        };

        var json = JsonSerializer.Serialize(source);
        var payload = JsonSerializer.Deserialize<ElementCacheRefresher.JsonPayload[]>(json);

        Assert.That(payload[0].PublishedCultures, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(payload[0].PublishedCultures.Length, Is.EqualTo(2));
            Assert.That(payload[0].PublishedCultures.First(), Is.EqualTo("en-US"));
            Assert.That(payload[0].PublishedCultures.Last(), Is.EqualTo("da-DK"));
        });

        Assert.That(payload[0].UnpublishedCultures, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(payload[0].UnpublishedCultures.Length, Is.EqualTo(1));
            Assert.That(payload[0].UnpublishedCultures.First(), Is.EqualTo("de-DE"));
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

        Assert.That(payload[0].ItemType, Is.EqualTo(source[0].ItemType));
        Assert.That(payload[0].Id, Is.EqualTo(source[0].Id));
        Assert.That(payload[0].ChangeTypes, Is.EqualTo(source[0].ChangeTypes));
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

        Assert.That(payload[0].Id, Is.EqualTo(source[0].Id));
        Assert.That(payload[0].Key, Is.EqualTo(source[0].Key));
        Assert.That(payload[0].Removed, Is.EqualTo(source[0].Removed));
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

        Assert.That(payload[0].Id, Is.EqualTo(source[0].Id));
        Assert.That(payload[0].ChangeType, Is.EqualTo(source[0].ChangeType));
    }
}
