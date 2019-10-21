﻿using System;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Services.Changes;
using Umbraco.Web.Cache;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class RefreshersTests
    {
        [Test]
        public void MediaCacheRefresherCanDeserializeJsonPayload()
        {
            var source = new[] { new MediaCacheRefresher.JsonPayload(1234, Guid.NewGuid(), TreeChangeTypes.None) };
            var json = JsonConvert.SerializeObject(source);
            var payload = JsonConvert.DeserializeObject<MediaCacheRefresher.JsonPayload[]>(json);
            Assert.AreEqual(source[0].Id, payload[0].Id);
            Assert.AreEqual(source[0].Key, payload[0].Key);
            Assert.AreEqual(source[0].ChangeTypes, payload[0].ChangeTypes);
        }

        [Test]
        public void ContentCacheRefresherCanDeserializeJsonPayload()
        {
            var source = new[] { new ContentCacheRefresher.JsonPayload(1234, Guid.NewGuid(), TreeChangeTypes.None) };
            var json = JsonConvert.SerializeObject(source);
            var payload = JsonConvert.DeserializeObject<ContentCacheRefresher.JsonPayload[]>(json);
            Assert.AreEqual(source[0].Id, payload[0].Id);
            Assert.AreEqual(source[0].Key, payload[0].Key);
            Assert.AreEqual(source[0].ChangeTypes, payload[0].ChangeTypes);
        }

        [Test]
        public void ContentTypeCacheRefresherCanDeserializeJsonPayload()
        {
            var source = new[] { new ContentTypeCacheRefresher.JsonPayload("xxx", 1234, ContentTypeChangeTypes.None) };
            var json = JsonConvert.SerializeObject(source);
            var payload = JsonConvert.DeserializeObject<ContentTypeCacheRefresher.JsonPayload[]>(json);
            Assert.AreEqual(source[0].ItemType, payload[0].ItemType);
            Assert.AreEqual(source[0].Id, payload[0].Id);
            Assert.AreEqual(source[0].ChangeTypes, payload[0].ChangeTypes);
        }

        [Test]
        public void DataTypeCacheRefresherCanDeserializeJsonPayload()
        {
            var source = new[] { new DataTypeCacheRefresher.JsonPayload(1234, Guid.NewGuid(), true) };
            var json = JsonConvert.SerializeObject(source);
            var payload = JsonConvert.DeserializeObject<DataTypeCacheRefresher.JsonPayload[]>(json);
            Assert.AreEqual(source[0].Id, payload[0].Id);
            Assert.AreEqual(source[0].Key, payload[0].Key);
            Assert.AreEqual(source[0].Removed, payload[0].Removed);
        }

        [Test]
        public void DomainCacheRefresherCanDeserializeJsonPayload()
        {
            var source = new[] { new DomainCacheRefresher.JsonPayload(1234, DomainChangeTypes.None) };
            var json = JsonConvert.SerializeObject(source);
            var payload = JsonConvert.DeserializeObject<DomainCacheRefresher.JsonPayload[]>(json);
            Assert.AreEqual(source[0].Id, payload[0].Id);
            Assert.AreEqual(source[0].ChangeType, payload[0].ChangeType);
        }
    }
}
