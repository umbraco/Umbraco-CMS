using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;

namespace Umbraco.Tests.Cache.DistributedCache
{
    /// <summary>
    /// Ensures that calls to DistributedCache methods carry through to the IServerMessenger correctly
    /// </summary>
    [TestFixture]
    public class DistributedCacheTests
    {
        private Umbraco.Web.Cache.DistributedCache _distributedCache;

        [SetUp]
        public void Setup()
        {
            var register = RegisterFactory.Create();

            var composition = new Composition(register, new TypeLoader(), Mock.Of<IProfilingLogger>(), RuntimeLevel.Run);

            register.Register<IServerRegistrar>(_ => new TestServerRegistrar());
            register.RegisterSingleton<IServerMessenger>(_ => new TestServerMessenger());

            composition.GetCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add<TestCacheRefresher>();

            Current.Factory = register.CreateFactory();

            _distributedCache = new Umbraco.Web.Cache.DistributedCache();
        }

        [TearDown]
        public void Teardown()
        {
            Current.Reset();
        }

        [Test]
        public void RefreshIntId()
        {
            for (var i = 1; i < 11; i++)
            {
                _distributedCache.Refresh(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), i);
            }
            Assert.AreEqual(10, ((TestServerMessenger)Current.ServerMessenger).IntIdsRefreshed.Count);
        }

        [Test]
        public void RefreshIntIdFromObject()
        {
            for (var i = 0; i < 10; i++)
            {
                _distributedCache.Refresh(
                    Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"),
                    x => x.Id,
                    new TestObjectWithId{Id = i});
            }
            Assert.AreEqual(10, ((TestServerMessenger)Current.ServerMessenger).IntIdsRefreshed.Count);
        }

        [Test]
        public void RefreshGuidId()
        {
            for (var i = 0; i < 11; i++)
            {
                _distributedCache.Refresh(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), Guid.NewGuid());
            }
            Assert.AreEqual(11, ((TestServerMessenger)Current.ServerMessenger).GuidIdsRefreshed.Count);
        }

        [Test]
        public void RemoveIds()
        {
            for (var i = 1; i < 13; i++)
            {
                _distributedCache.Remove(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), i);
            }
            Assert.AreEqual(12, ((TestServerMessenger)Current.ServerMessenger).IntIdsRemoved.Count);
        }

        [Test]
        public void FullRefreshes()
        {
            for (var i = 0; i < 13; i++)
            {
                _distributedCache.RefreshAll(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"));
            }
            Assert.AreEqual(13, ((TestServerMessenger)Current.ServerMessenger).CountOfFullRefreshes);
        }

        #region internal test classes

        internal class TestObjectWithId
        {
            public int Id { get; set; }
        }

        internal class TestCacheRefresher : ICacheRefresher
        {
            public static readonly Guid UniqueId = Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73");

            public Guid RefresherUniqueId => UniqueId;

            public string Name => "Test Cache Refresher";

            public void RefreshAll()
            { }

            public void Refresh(int id)
            { }

            public void Remove(int id)
            { }

            public void Refresh(Guid id)
            { }
        }

        internal class TestServerMessenger : IServerMessenger
        {
            //used for tests
            public List<int> IntIdsRefreshed = new List<int>();
            public List<Guid> GuidIdsRefreshed = new List<Guid>();
            public List<int> IntIdsRemoved = new List<int>();
            public List<string> PayloadsRemoved = new List<string>();
            public List<string> PayloadsRefreshed = new List<string>();
            public int CountOfFullRefreshes = 0;

            public void PerformRefresh<TPayload>(ICacheRefresher refresher, TPayload[] payload)
            {
                // doing nothing
            }

            public void PerformRefresh(ICacheRefresher refresher, string jsonPayload)
            {
                PayloadsRefreshed.Add(jsonPayload);
            }

            public void PerformRefresh<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
            {
                IntIdsRefreshed.AddRange(instances.Select(getNumericId));
            }

            public void PerformRefresh<T>(ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances)
            {
                GuidIdsRefreshed.AddRange(instances.Select(getGuidId));
            }

            public void PerformRemove(ICacheRefresher refresher, string jsonPayload)
            {
                PayloadsRemoved.Add(jsonPayload);
            }

            public void PerformRemove<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
            {
                IntIdsRemoved.AddRange(instances.Select(getNumericId));
            }

            public void PerformRemove(ICacheRefresher refresher, params int[] numericIds)
            {
                IntIdsRemoved.AddRange(numericIds);
            }

            public void PerformRefresh(ICacheRefresher refresher, params int[] numericIds)
            {
                IntIdsRefreshed.AddRange(numericIds);
            }

            public void PerformRefresh(ICacheRefresher refresher, params Guid[] guidIds)
            {
                GuidIdsRefreshed.AddRange(guidIds);
            }

            public void PerformRefreshAll(ICacheRefresher refresher)
            {
                CountOfFullRefreshes++;
            }
        }

        internal class TestServerRegistrar : IServerRegistrar
        {
            public IEnumerable<IServerAddress> Registrations => new List<IServerAddress>
            {
                new TestServerAddress("localhost")
            };

            public ServerRole GetCurrentServerRole()
            {
                throw new NotImplementedException();
            }

            public string GetCurrentServerUmbracoApplicationUrl()
            {
                throw new NotImplementedException();
            }
        }

        public class TestServerAddress : IServerAddress
        {
            public TestServerAddress(string address)
            {
                ServerAddress = address;
            }
            public string ServerAddress { get; private set; }
        }

        #endregion
    }
}
