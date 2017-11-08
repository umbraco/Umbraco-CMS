using System;
using System.Collections;
using Moq;
using NUnit.Framework;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Tests.Cache.DistributedCache;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Cache;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    public class ScopedXmlTests : BaseDatabaseFactoryTest
    {
        private CacheRefresherEventHandler _cacheHandler;

        protected override void FreezeResolution()
        {
            ServerRegistrarResolver.Current = new ServerRegistrarResolver(
                new DistributedCacheTests.TestServerRegistrar());
            ServerMessengerResolver.Current = new ServerMessengerResolver(
                new DatabaseServerMessenger(ApplicationContext, false, new DatabaseServerMessengerOptions()));
            CacheRefreshersResolver.Current = new CacheRefreshersResolver(
                new ActivatorServiceProvider(), Mock.Of<ILogger>(), () => new[]
                {
                    typeof(PageCacheRefresher),
                    typeof(UnpublishedPageCacheRefresher),

                    typeof(DomainCacheRefresher),
                    typeof(MacroCacheRefresher)
                });

            base.FreezeResolution();
        }

        [TearDown]
        public void Teardown()
        {
            if (_cacheHandler != null)
                _cacheHandler.Unbind();
            _cacheHandler = null;

            _onPublishedAssertAction = null;
            content.HttpContextItemsGetter = null;
            ContentService.Published -= OnPublishedAssert;
            SafeXmlReaderWriter.Cloning = null;

            ServerRegistrarResolver.Reset();
            ServerMessengerResolver.Reset();
            CacheRefreshersResolver.Reset();
        }

        [Test]
        public void TestNoScope()
        {
            content.TestingUpdateSitemapProvider = false;

            var settings = SettingsForTests.GenerateMockSettings();
            var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.XmlCacheEnabled).Returns(false);
            SettingsForTests.ConfigureSettings(settings);

            var contentType = new ContentType(-1) { Alias = "contenttype", Name = "test"};
            ApplicationContext.Services.ContentTypeService.Save(contentType);

            _cacheHandler = new CacheRefresherEventHandler(true);
            _cacheHandler.OnApplicationStarted(null, ApplicationContext);

            var xml = content.Instance.XmlContent;
            Assert.IsNotNull(xml);
            var beforeXml = xml.OuterXml;
            var item = new Content("name", -1, contentType);
            ApplicationContext.Services.ContentService.SaveAndPublishWithStatus(item);

            Console.WriteLine("Xml Before:");
            Console.WriteLine(xml.OuterXml);
            Assert.AreEqual(beforeXml, xml.OuterXml);

            xml = content.Instance.XmlContent;
            Assert.IsNotNull(xml);
            Console.WriteLine("Xml After:");
            Console.WriteLine(xml.OuterXml);
            var node = xml.GetElementById(item.Id.ToString());
            Assert.IsNotNull(node);
        }

        private void OnPublishedAssert(IPublishingStrategy sender, PublishEventArgs<IContent> args)
        {
            if (_onPublishedAssertAction != null)
                _onPublishedAssertAction();
        }

        private Action _onPublishedAssertAction;

        [TestCase(true)]
        [TestCase(false)]
        public void TestScope(bool complete)
        {
            content.TestingUpdateSitemapProvider = false;

            var httpContextItems = new Hashtable();
            content.HttpContextItemsGetter = () => httpContextItems;

            var settings = SettingsForTests.GenerateMockSettings();
            var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.XmlCacheEnabled).Returns(false);
            SettingsForTests.ConfigureSettings(settings);

            var contentType = new ContentType(-1) { Alias = "contenttype", Name = "test"};
            ApplicationContext.Services.ContentTypeService.Save(contentType);

            _cacheHandler = new CacheRefresherEventHandler(true);
            _cacheHandler.OnApplicationStarted(null, ApplicationContext);

            var xml = content.Instance.XmlContent;
            Assert.IsNotNull(xml);
            Assert.AreSame(xml, httpContextItems[content.XmlContextContentItemKey]);
            var beforeXml = xml;
            var beforeOuterXml = beforeXml.OuterXml;
            var item = new Content("name", -1, contentType);

            Console.WriteLine("Xml Before:");
            Console.WriteLine(xml.OuterXml);

            var evented = 0;
            _onPublishedAssertAction = () =>
            {
                evented++;
                xml = content.Instance.XmlContent;
                Assert.IsNotNull(xml);
                Assert.AreNotSame(beforeXml, xml);
                Console.WriteLine("Xml Event:");
                Console.WriteLine(xml.OuterXml);
                var node = xml.GetElementById(item.Id.ToString());
                Assert.IsNotNull(node);
            };

            ContentService.Published += OnPublishedAssert;

            using (var scope = ApplicationContext.Current.ScopeProvider.CreateScope())
            {
                ApplicationContext.Services.ContentService.SaveAndPublishWithStatus(item); // should create an xml clone
                item.Name = "changed";
                ApplicationContext.Services.ContentService.SaveAndPublishWithStatus(item); // should re-use the xml clone

                // this should never change
                Assert.AreEqual(beforeOuterXml, beforeXml.OuterXml);

                // this does not change, other thread don't see the changes
                xml = content.Instance.XmlContentInternal;
                Assert.IsNotNull(xml);
                Assert.AreSame(beforeXml, xml);
                Console.WriteLine("XmlInternal During:");
                Console.WriteLine(xml.OuterXml);
                Assert.AreEqual(beforeOuterXml, xml.OuterXml);

                // this does not change during the scope (only in events)
                // because it is the events that trigger the changes
                xml = content.Instance.XmlContent;
                Assert.IsNotNull(xml);
                Assert.AreSame(beforeXml, xml);

                // note
                // this means that, as long as ppl don't create scopes, they'll see their
                // changes right after SaveAndPublishWithStatus, but if they create scopes,
                // they will have to wail until the scope is completed, ie wait until the
                // events trigger, to use eg GetUrl etc

                if (complete)
                    scope.Complete();
            }

            //The reason why there is only 1 event occuring is because we are publishing twice for the same event for the same
            //object and the scope deduplicates the events (uses the latest)
            Assert.AreEqual(complete ? 1 : 0, evented);

            // this should never change
            Assert.AreEqual(beforeOuterXml, beforeXml.OuterXml);

            xml = content.Instance.XmlContent;
            Assert.IsNotNull(xml);
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
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestScopeMany(bool complete)
        {
            content.TestingUpdateSitemapProvider = false;

            var settings = SettingsForTests.GenerateMockSettings();
            var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.XmlCacheEnabled).Returns(false);
            SettingsForTests.ConfigureSettings(settings);

            var contentType = new ContentType(-1) { Alias = "contenttype", Name = "test" };
            ApplicationContext.Services.ContentTypeService.Save(contentType);

            _cacheHandler = new CacheRefresherEventHandler(true);
            _cacheHandler.OnApplicationStarted(null, ApplicationContext);

            var xml = content.Instance.XmlContent;
            Assert.IsNotNull(xml);
            var beforeXml = xml;
            var beforeOuterXml = beforeXml.OuterXml;
            var item = new Content("name", -1, contentType);
            const int count = 10;
            var ids = new int[count];
            var clones = 0;

            SafeXmlReaderWriter.Cloning = () => { clones++; };

            Console.WriteLine("Xml Before:");
            Console.WriteLine(xml.OuterXml);

            using (var scope = ApplicationContext.Current.ScopeProvider.CreateScope())
            {
                ApplicationContext.Services.ContentService.SaveAndPublishWithStatus(item);

                for (var i = 0; i < count; i++)
                {
                    var temp = new Content("content_" + i, -1, contentType);
                    ApplicationContext.Services.ContentService.SaveAndPublishWithStatus(temp);
                    ids[i] = temp.Id;
                }

                // this should never change
                Assert.AreEqual(beforeOuterXml, beforeXml.OuterXml);

                xml = content.Instance.XmlContent;
                Assert.IsNotNull(xml);
                Console.WriteLine("Xml InScope (before complete):");
                Console.WriteLine(xml.OuterXml);
                Assert.AreEqual(beforeOuterXml, xml.OuterXml);

                if (complete)
                    scope.Complete();

                xml = content.Instance.XmlContent;
                Assert.IsNotNull(xml);
                Console.WriteLine("Xml InScope (after complete):");
                Console.WriteLine(xml.OuterXml);
                Assert.AreEqual(beforeOuterXml, xml.OuterXml);
            }

            var scopeProvider = ApplicationContext.Current.ScopeProvider as IScopeProviderInternal;
            Assert.IsNotNull(scopeProvider);
            // ambient scope may be null, or maybe not, depending on whether the code that
            // was called did proper scoped work, or some direct (NoScope) use of the database
            Assert.IsNull(scopeProvider.AmbientContext);

            // limited number of clones!
            Assert.AreEqual(complete ? 1 : 0, clones);

            // this should never change
            Assert.AreEqual(beforeOuterXml, beforeXml.OuterXml);

            xml = content.Instance.XmlContent;
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
    }
}
