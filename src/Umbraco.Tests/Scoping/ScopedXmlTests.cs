using System;
using Moq;
using NUnit.Framework;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
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
                _cacheHandler.Destroy();
            _cacheHandler = null;

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

            var contentType = new ContentType(-1) { Alias = "contenttype" };
            ApplicationContext.Services.ContentTypeService.Save(contentType);

            _cacheHandler = new CacheRefresherEventHandler();
            _cacheHandler.OnApplicationStarted(null, ApplicationContext);

            var xml = content.Instance.XmlContent;
            Assert.IsNotNull(xml);
            var beforeXml = xml.OuterXml;
            var item = new Content("name", -1, contentType);
            ApplicationContext.Services.ContentService.SaveAndPublishWithStatus(item);

            Console.WriteLine("BEFORE");
            Console.WriteLine(xml.OuterXml);
            Assert.AreEqual(beforeXml, xml.OuterXml);

            xml = content.Instance.XmlContent;
            Assert.IsNotNull(xml);
            Console.WriteLine("AFTER");
            Console.WriteLine(xml.OuterXml);
            var node = xml.GetElementById(item.Id.ToString());
            Assert.IsNotNull(node);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestScope(bool complete)
        {
            content.TestingUpdateSitemapProvider = false;

            var settings = SettingsForTests.GenerateMockSettings();
            var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.XmlCacheEnabled).Returns(false);
            SettingsForTests.ConfigureSettings(settings);

            var contentType = new ContentType(-1) { Alias = "contenttype" };
            ApplicationContext.Services.ContentTypeService.Save(contentType);

            _cacheHandler = new CacheRefresherEventHandler();
            _cacheHandler.OnApplicationStarted(null, ApplicationContext);

            var xml = content.Instance.XmlContent;
            Assert.IsNotNull(xml);
            var beforeXml = xml;
            var beforeOuterXml = beforeXml.OuterXml;
            var item = new Content("name", -1, contentType);

            Console.WriteLine("BEFORE");
            Console.WriteLine(xml.OuterXml);

            using (var scope = ApplicationContext.Current.ScopeProvider.CreateScope())
            {
                ApplicationContext.Services.ContentService.SaveAndPublishWithStatus(item); // should create an xml clone
                item.Name = "changed";
                ApplicationContext.Services.ContentService.SaveAndPublishWithStatus(item); // should re-use the xml clone

                // this should never change
                Assert.AreEqual(beforeOuterXml, beforeXml.OuterXml);

                xml = content.Instance.XmlContent;
                Assert.IsNotNull(xml);
                Console.WriteLine("AFTER");
                Console.WriteLine(xml.OuterXml);
                Assert.AreEqual(beforeOuterXml, xml.OuterXml); // fixme - should it change?

                if (complete)
                    scope.Complete();
            }

            // fixme - refactor when we have events dispatching,
            // the xml clone should prob be only created when the scope completes

            // this should never change
            Assert.AreEqual(beforeOuterXml, beforeXml.OuterXml);

            xml = content.Instance.XmlContent;
            Assert.IsNotNull(xml);
            Console.WriteLine("AFTER");
            Console.WriteLine(xml.OuterXml);
            if (complete)
            {
                var node = xml.GetElementById(item.Id.ToString());
                Assert.IsNotNull(node);
            }
            else
            {
                Assert.AreEqual(beforeOuterXml, xml.OuterXml); // nothing has changed!
            }
        }
    }
}
