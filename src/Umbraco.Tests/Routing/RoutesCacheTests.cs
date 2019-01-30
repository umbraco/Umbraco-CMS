﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Tests.LegacyXmlPublishedCache;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class RoutesCacheTests : BaseWebTest
    {
        [Test]
        public void U4_7939()
        {
            //var routingContext = GetRoutingContext("/test", 1111);
            var umbracoContext = GetUmbracoContext("/test", 0);
            var cache = umbracoContext.PublishedSnapshot.Content as PublishedContentCache;
            if (cache == null) throw new Exception("Unsupported IPublishedContentCache, only the Xml one is supported.");

            // FIXME: not sure?
            //PublishedContentCache.UnitTesting = false; // else does not write to routes cache
            //Assert.IsFalse(PublishedContentCache.UnitTesting);

            var z = cache.GetByRoute(false, "/home/sub1");
            Assert.IsNotNull(z);
            Assert.AreEqual(1173, z.Id);

            var routes = cache.RoutesCache.GetCachedRoutes();
            Assert.AreEqual(1, routes.Count);

            // before the fix, the following assert would fail because the route would
            // have been stored as { 0, "/home/sub1" } - essentially meaning we were NOT
            // storing anything in the route cache!

            Assert.AreEqual(1173, routes.Keys.First());
            Assert.AreEqual("/home/sub1", routes.Values.First());
        }
    }
}
