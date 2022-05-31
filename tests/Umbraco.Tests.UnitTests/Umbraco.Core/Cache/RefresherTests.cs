// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Newtonsoft.Json;
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
        var json = JsonConvert.SerializeObject(source);
        var payload = JsonConvert.DeserializeObject<MediaCacheRefresher.JsonPayload[]>(json);
        Assert.AreEqual(source[0].Id, payload[0].Id);
        Assert.AreEqual(source[0].Key, payload[0].Key);
        Assert.AreEqual(source[0].ChangeTypes, payload[0].ChangeTypes);
    }

    [Test]
    public void ContentCacheRefresherCanDeserializeJsonPayload()
    {
        ContentCacheRefresher.JsonPayload[] source =
        {
            new ContentCacheRefresher.JsonPayload(1234, Guid.NewGuid(), TreeChangeTypes.None),
        };
        var json = JsonConvert.SerializeObject(source);
        var payload = JsonConvert.DeserializeObject<ContentCacheRefresher.JsonPayload[]>(json);
        Assert.AreEqual(source[0].Id, payload[0].Id);
        Assert.AreEqual(source[0].Key, payload[0].Key);
        Assert.AreEqual(source[0].ChangeTypes, payload[0].ChangeTypes);
    }

    [Test]
    public void ContentTypeCacheRefresherCanDeserializeJsonPayload()
    {
        ContentTypeCacheRefresher.JsonPayload[] source =
        {
            new ContentTypeCacheRefresher.JsonPayload("xxx", 1234, ContentTypeChangeTypes.None),
        };
        var json = JsonConvert.SerializeObject(source);
        var payload = JsonConvert.DeserializeObject<ContentTypeCacheRefresher.JsonPayload[]>(json);
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
        var json = JsonConvert.SerializeObject(source);
        var payload = JsonConvert.DeserializeObject<DataTypeCacheRefresher.JsonPayload[]>(json);
        Assert.AreEqual(source[0].Id, payload[0].Id);
        Assert.AreEqual(source[0].Key, payload[0].Key);
        Assert.AreEqual(source[0].Removed, payload[0].Removed);
    }

    [Test]
    public void DomainCacheRefresherCanDeserializeJsonPayload()
    {
        DomainCacheRefresher.JsonPayload[]
            source = { new DomainCacheRefresher.JsonPayload(1234, DomainChangeTypes.None) };
        var json = JsonConvert.SerializeObject(source);
        var payload = JsonConvert.DeserializeObject<DomainCacheRefresher.JsonPayload[]>(json);
        Assert.AreEqual(source[0].Id, payload[0].Id);
        Assert.AreEqual(source[0].ChangeType, payload[0].ChangeType);
    }
}
