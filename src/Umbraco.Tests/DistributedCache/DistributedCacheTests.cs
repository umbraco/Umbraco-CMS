using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Profiling;
using Umbraco.Core.Sync;
using umbraco.interfaces;

namespace Umbraco.Tests.DistributedCache
{
    /// <summary>
    /// Ensures that calls to DistributedCache methods carry through to the IServerMessenger correctly
    /// </summary>
    [TestFixture]
    public class DistributedCacheTests
    {
        [SetUp]
        public void Setup()
        {
            ServerRegistrarResolver.Current = new ServerRegistrarResolver(
                new TestServerRegistrar());
            ServerMessengerResolver.Current = new ServerMessengerResolver(
                new TestServerMessenger());
            CacheRefreshersResolver.Current = new CacheRefreshersResolver(
                new ActivatorServiceProvider(), Mock.Of<ILogger>(), () => new[] { typeof(TestCacheRefresher) });
            Resolution.Freeze();
        }

        [TearDown]
        public void Teardown()
        {
            ServerRegistrarResolver.Reset();
            ServerMessengerResolver.Reset();
            CacheRefreshersResolver.Reset();
        }

        [Test]
        public void RefreshIntId()
        {
            for (var i = 1; i < 11; i++)
            {
                Web.Cache.DistributedCache.Instance.Refresh(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), i);
            }
            Assert.AreEqual(10, ((TestServerMessenger)ServerMessengerResolver.Current.Messenger).IntIdsRefreshed.Count);
        }

        [Test]
        public void RefreshIntIdFromObject()
        {
            for (var i = 0; i < 10; i++)
            {
                Web.Cache.DistributedCache.Instance.Refresh(
                    Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"),
                    x => x.Id,
                    new TestObjectWithId{Id = i});
            }
            Assert.AreEqual(10, ((TestServerMessenger)ServerMessengerResolver.Current.Messenger).IntIdsRefreshed.Count);
        }

        [Test]
        public void RefreshGuidId()
        {
            for (var i = 0; i < 11; i++)
            {
                Web.Cache.DistributedCache.Instance.Refresh(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), Guid.NewGuid());
            }
            Assert.AreEqual(11, ((TestServerMessenger)ServerMessengerResolver.Current.Messenger).GuidIdsRefreshed.Count);
        }

        [Test]
        public void RemoveIds()
        {
            for (var i = 1; i < 13; i++)
            {
                Web.Cache.DistributedCache.Instance.Remove(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), i);
            }
            Assert.AreEqual(12, ((TestServerMessenger)ServerMessengerResolver.Current.Messenger).IntIdsRemoved.Count);
        }

        [Test]
        public void FullRefreshes()
        {
            for (var i = 0; i < 13; i++)
            {
                Web.Cache.DistributedCache.Instance.RefreshAll(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"));
            }
            Assert.AreEqual(13, ((TestServerMessenger)ServerMessengerResolver.Current.Messenger).CountOfFullRefreshes);
        }

        #region internal test classes

        internal class TestObjectWithId
        {
            public int Id { get; set; }
        }

        internal class TestCacheRefresher : ICacheRefresher
        {
            public Guid UniqueIdentifier
            {
                get { return Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"); }
            }
            public string Name
            {
                get { return "Test"; }
            }
            public void RefreshAll()
            {
                
            }

            public void Refresh(int id)
            {
                
            }

            public void Remove(int id)
            {
                
            }

            public void Refresh(Guid id)
            {
               
            }
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


            public void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, string jsonPayload)
            {
                PayloadsRefreshed.Add(jsonPayload);
            }

            public void PerformRefresh<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
            {
                IntIdsRefreshed.AddRange(instances.Select(getNumericId));
            }

            public void PerformRefresh<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances)
            {
                GuidIdsRefreshed.AddRange(instances.Select(getGuidId));
            }

            public void PerformRemove(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, string jsonPayload)
            {
                PayloadsRemoved.Add(jsonPayload);
            }

            public void PerformRemove<T>(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
            {
                IntIdsRemoved.AddRange(instances.Select(getNumericId));
            }

            public void PerformRemove(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds)
            {
                IntIdsRemoved.AddRange(numericIds);
            }

            public void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params int[] numericIds)
            {
                IntIdsRefreshed.AddRange(numericIds);
            }

            public void PerformRefresh(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, params Guid[] guidIds)
            {
                GuidIdsRefreshed.AddRange(guidIds);
            }

            public void PerformRefreshAll(IEnumerable<IServerAddress> servers, ICacheRefresher refresher)
            {
                CountOfFullRefreshes++;
            }
        }

        internal class TestServerRegistrar : IServerRegistrar
        {
            public IEnumerable<IServerAddress> Registrations
            {
                get
                {
                    return new List<IServerAddress>()
                        {
                            new TestServerAddress("localhost")
                        };
                }
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