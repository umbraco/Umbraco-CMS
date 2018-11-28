using System;
using System.Collections.Generic;
using System.Xml;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Sync;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web.Cache;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
    public class ScopedXmlTests : TestWithDatabaseBase
    {
        private CacheRefresherComponent _cacheRefresher;

        protected override void Compose()
        {
            base.Compose();

            // the cache refresher component needs to trigger to refresh caches
            // but then, it requires a lot of plumbing ;(
            // fixme - and we cannot inject a DistributedCache yet
            // so doing all this mess
            Composition.RegisterSingleton<IServerMessenger, LocalServerMessenger>();
            Composition.RegisterSingleton(f => Mock.Of<IServerRegistrar>());
            Composition.GetCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add(() => Composition.TypeLoader.GetCacheRefreshers());
        }

        [TearDown]
        public void Teardown()
        {
            _cacheRefresher?.Unbind();
            _cacheRefresher = null;

            _onPublishedAssertAction = null;
            ContentService.Published -= OnPublishedAssert;
            SafeXmlReaderWriter.Cloning = null;
        }

        private void OnPublishedAssert(IContentService sender, PublishEventArgs<IContent> args)
        {
            _onPublishedAssertAction?.Invoke();
        }

        private Action _onPublishedAssertAction;

        // in 7.6, content.Instance
        //  .XmlContent - comes from .XmlContentInternal and is cached in context items for current request
        //  .XmlContentInternal - the actual main xml document
        // become in 8
        //  xmlStore.Xml - the actual main xml document
        //  publishedContentCache.GetXml() - the captured xml

        private static XmlStore XmlStore => (Current.Factory.GetInstance<IPublishedSnapshotService>() as PublishedSnapshotService).XmlStore;
        private static XmlDocument XmlMaster => XmlStore.Xml;
        private static XmlDocument XmlInContext => ((PublishedContentCache) Umbraco.Web.Composing.Current.UmbracoContext.ContentCache).GetXml(false);

        [TestCase(true)]
        [TestCase(false)]
        public void TestScope(bool complete)
        {
            var umbracoContext = GetUmbracoContext("http://example.com/", setSingleton: true);

            // sanity checks
            Assert.AreSame(umbracoContext, Umbraco.Web.Composing.Current.UmbracoContext);
            Assert.AreSame(XmlStore, ((PublishedContentCache) umbracoContext.ContentCache).XmlStore);

            // settings
            var settings = SettingsForTests.GenerateMockUmbracoSettings();
            var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.XmlCacheEnabled).Returns(false);
            SettingsForTests.ConfigureSettings(settings);

            // create document type, document
            var contentType = new ContentType(-1) { Alias = "CustomDocument", Name = "Custom Document" };
            Current.Services.ContentTypeService.Save(contentType);
            var item = new Content("name", -1, contentType);

            // wire cache refresher
            _cacheRefresher = new CacheRefresherComponent(true);
            _cacheRefresher.Initialize(new DistributedCache());

            // check xml in context = "before"
            var xml = XmlInContext;
            var beforeXml = xml;
            var beforeOuterXml = beforeXml.OuterXml;
            Console.WriteLine("Xml Before:");
            Console.WriteLine(xml.OuterXml);

            // event handler
            var evented = 0;
            _onPublishedAssertAction = () =>
            {
                evented++;

                // should see the changes in context, not in master
                xml = XmlInContext;
                Assert.AreNotSame(beforeXml, xml);
                Console.WriteLine("Xml Event:");
                Console.WriteLine(xml.OuterXml);
                var node = xml.GetElementById(item.Id.ToString());
                Assert.IsNotNull(node);

                xml = XmlMaster;
                Assert.AreSame(beforeXml, xml);
                Assert.AreEqual(beforeOuterXml, xml.OuterXml);
            };

            ContentService.Published += OnPublishedAssert;

            using (var scope = ScopeProvider.CreateScope())
            {
                Current.Services.ContentService.SaveAndPublish(item); // should create an xml clone
                item.Name = "changed";
                Current.Services.ContentService.SaveAndPublish(item); // should re-use the xml clone

                // this should never change
                Assert.AreEqual(beforeOuterXml, beforeXml.OuterXml);

                // this does not change, other thread don't see the changes
                xml = XmlMaster;
                Assert.AreSame(beforeXml, xml);
                Console.WriteLine("XmlInternal During:");
                Console.WriteLine(xml.OuterXml);
                Assert.AreEqual(beforeOuterXml, xml.OuterXml);

                // this does not change during the scope (only in events)
                // because it is the events that trigger the changes
                xml = XmlInContext;
                Assert.IsNotNull(xml);
                Assert.AreSame(beforeXml, xml);
                Assert.AreEqual(beforeOuterXml, xml.OuterXml);

                // note
                // this means that, as long as ppl don't create scopes, they'll see their
                // changes right after SaveAndPublish, but if they create scopes,
                // they will have to wail until the scope is completed, ie wait until the
                // events trigger, to use eg GetUrl etc

                if (complete)
                    scope.Complete();
            }

            //The reason why there is only 1 event occuring is because we are publishing twice for the same event for the same
            //object and the scope deduplicates the events(uses the latest)
            Assert.AreEqual(complete? 1 : 0, evented);

            // this should never change
            Assert.AreEqual(beforeOuterXml, beforeXml.OuterXml);

            xml = XmlInContext;
            Console.WriteLine("Xml After:");
            Console.WriteLine(xml.OuterXml);
            if (complete)
            {
                var node = xml.GetElementById(item.Id.ToString());
                Assert.IsNotNull(node);
            }
            else
            {
                Assert.AreSame(beforeXml, xml);
                Assert.AreEqual(beforeOuterXml, xml.OuterXml); // nothing has changed!
            }

            xml = XmlMaster;
            if (complete)
            {
                var node = xml.GetElementById(item.Id.ToString());
                Assert.IsNotNull(node);
            }
            else
            {
                Assert.AreSame(beforeXml, xml);
                Assert.AreEqual(beforeOuterXml, xml.OuterXml); // nothing has changed!
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestScopeMany(bool complete)
        {
            var umbracoContext = GetUmbracoContext("http://example.com/", setSingleton: true);

            // sanity checks
            Assert.AreSame(umbracoContext, Umbraco.Web.Composing.Current.UmbracoContext);
            Assert.AreSame(XmlStore, ((PublishedContentCache)umbracoContext.ContentCache).XmlStore);

            // settings
            var settings = SettingsForTests.GenerateMockUmbracoSettings();
            var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.XmlCacheEnabled).Returns(false);
            SettingsForTests.ConfigureSettings(settings);

            // create document type
            var contentType = new ContentType(-1) { Alias = "CustomDocument", Name = "Custom Document" };
            Current.Services.ContentTypeService.Save(contentType);

            // wire cache refresher
            _cacheRefresher = new CacheRefresherComponent(true);
            _cacheRefresher.Initialize(new DistributedCache());

            // check xml in context = "before"
            var xml = XmlInContext;
            var beforeXml = xml;
            var beforeOuterXml = beforeXml.OuterXml;
            var item = new Content("name", -1, contentType);
            const int count = 10;
            var ids = new int[count];
            var clones = 0;

            SafeXmlReaderWriter.Cloning = () => { clones++; };

            Console.WriteLine("Xml Before:");
            Console.WriteLine(xml.OuterXml);

            using (var scope = ScopeProvider.CreateScope())
            {
                Current.Services.ContentService.SaveAndPublish(item);

                for (var i = 0; i < count; i++)
                {
                    var temp = new Content("content_" + i, -1, contentType);
                    Current.Services.ContentService.SaveAndPublish(temp);
                    ids[i] = temp.Id;
                }

                // this should never change
                Assert.AreEqual(beforeOuterXml, beforeXml.OuterXml);

                xml = XmlMaster;
                Assert.IsNotNull(xml);
                Console.WriteLine("Xml InScope (before complete):");
                Console.WriteLine(xml.OuterXml);
                Assert.AreEqual(beforeOuterXml, xml.OuterXml);

                if (complete)
                    scope.Complete();

                xml = XmlMaster;
                Assert.IsNotNull(xml);
                Console.WriteLine("Xml InScope (after complete):");
                Console.WriteLine(xml.OuterXml);
                Assert.AreEqual(beforeOuterXml, xml.OuterXml);
            }

            var scopeProvider = ScopeProvider;
            Assert.IsNotNull(scopeProvider);
            // ambient scope may be null, or maybe not, depending on whether the code that
            // was called did proper scoped work, or some direct (NoScope) use of the database
            Assert.IsNull(scopeProvider.AmbientContext);

            // limited number of clones!
            Assert.AreEqual(complete ? 1 : 0, clones);

            // this should never change
            Assert.AreEqual(beforeOuterXml, beforeXml.OuterXml);

            xml = XmlMaster;
            Assert.IsNotNull(xml);
            Console.WriteLine("Xml After:");
            Console.WriteLine(xml.OuterXml);
            if (complete)
            {
                var node = xml.GetElementById(item.Id.ToString());
                Assert.IsNotNull(node);

                for (var i = 0; i < 10; i++)
                {
                    node = xml.GetElementById(ids[i].ToString());
                    Assert.IsNotNull(node);
                }
            }
            else
            {
                Assert.AreEqual(beforeOuterXml, xml.OuterXml); // nothing has changed!
            }
        }

        public class LocalServerMessenger : ServerMessengerBase
        {
            public LocalServerMessenger()
                : base(false)
            { }

            protected override void DeliverRemote(ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
            {
                throw new NotImplementedException();
            }
        }
    }
}
