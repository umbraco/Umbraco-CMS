using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Facade
{
    [TestFixture]
    public class PropertyCacheLevelTests
    {
        [TestCase(PropertyCacheLevel.None, 2)]
        [TestCase(PropertyCacheLevel.Content, 1)]
        [TestCase(PropertyCacheLevel.Snapshot, 1)]
        [TestCase(PropertyCacheLevel.Facade, 1)]
        public void CacheLevelTest(PropertyCacheLevel cacheLevel, int interConverts)
        {
            var converter = new CacheConverter1(cacheLevel);

            var converters = new PropertyValueConverterCollection(new IPropertyValueConverter[]
            {
                converter,
            });

            var publishedContentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, Mock.Of<IDataTypeConfigurationSource>());
            var setType1 = publishedContentTypeFactory.CreateContentType(1000, "set1", new[]
            {
                publishedContentTypeFactory.CreatePropertyType("prop1", 0, "editor1"),
            });

            // PublishedElementPropertyBase.GetCacheLevels:
            //
            //   if property level is > reference level, or both are None
            //     use None for property & new reference
            //   else
            //     use Content for property, & keep reference
            //
            // PublishedElement creates properties with reference being None
            // if converter specifies None, keep using None
            // anything else is not > None, use Content
            //
            // for standalone elements, it's only None or Content

            var set1 = new PublishedElement(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false);

            Assert.AreEqual(1234, set1.Value("prop1"));
            Assert.AreEqual(1, converter.SourceConverts);
            Assert.AreEqual(1, converter.InterConverts);

            // source is always converted once and cached per content
            // inter conversion depends on the specified cache level

            Assert.AreEqual(1234, set1.Value("prop1"));
            Assert.AreEqual(1, converter.SourceConverts);
            Assert.AreEqual(interConverts, converter.InterConverts);
        }

        // property is not cached, converted cached at Content, exept
        //  /None = not cached at all
        [TestCase(PropertyCacheLevel.None, PropertyCacheLevel.None, 2, 0, 0, 0, 0)]
        [TestCase(PropertyCacheLevel.None, PropertyCacheLevel.Content, 1, 0, 0, 0, 0)]
        [TestCase(PropertyCacheLevel.None, PropertyCacheLevel.Snapshot, 1, 0, 0, 0, 0)]
        [TestCase(PropertyCacheLevel.None, PropertyCacheLevel.Facade, 1, 0, 0, 0, 0)]

        // property is cached at content level, converted cached at
        //  /None = not at all
        //  /Content = in content
        //  /Facade = in facade
        //  /Snapshot = in snapshot
        [TestCase(PropertyCacheLevel.Content, PropertyCacheLevel.None, 2, 0, 0, 0, 0)]
        [TestCase(PropertyCacheLevel.Content, PropertyCacheLevel.Content, 1, 0, 0, 0, 0)]
        [TestCase(PropertyCacheLevel.Content, PropertyCacheLevel.Snapshot, 1, 1, 0, 1, 0)]
        [TestCase(PropertyCacheLevel.Content, PropertyCacheLevel.Facade, 1, 0, 1, 0, 1)]

        // property is cached at snapshot level, converted cached at Content, exept
        //  /None = not cached at all
        //  /Facade = cached in facade
        [TestCase(PropertyCacheLevel.Snapshot, PropertyCacheLevel.None, 2, 0, 0, 0, 0)]
        [TestCase(PropertyCacheLevel.Snapshot, PropertyCacheLevel.Content, 1, 0, 0, 0, 0)]
        [TestCase(PropertyCacheLevel.Snapshot, PropertyCacheLevel.Snapshot, 1, 0, 0, 0, 0)]
        [TestCase(PropertyCacheLevel.Snapshot, PropertyCacheLevel.Facade, 1, 0, 1, 0, 1)]

        // property is cached at facade level, converted cached at Content, exept
        //  /None = not cached at all
        [TestCase(PropertyCacheLevel.Facade, PropertyCacheLevel.None, 2, 0, 0, 0, 0)]
        [TestCase(PropertyCacheLevel.Facade, PropertyCacheLevel.Content, 1, 0, 0, 0, 0)]
        [TestCase(PropertyCacheLevel.Facade, PropertyCacheLevel.Snapshot, 1, 0, 0, 0, 0)]
        [TestCase(PropertyCacheLevel.Facade, PropertyCacheLevel.Facade, 1, 0, 0, 0, 0)]

        public void CacheFacadeTest(PropertyCacheLevel referenceCacheLevel, PropertyCacheLevel converterCacheLevel, int interConverts,
            int snapshotCount1, int facadeCount1, int snapshotCount2, int facadeCount2)
        {
            var converter = new CacheConverter1(converterCacheLevel);

            var converters = new PropertyValueConverterCollection(new IPropertyValueConverter[]
            {
                converter,
            });

            var publishedContentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, Mock.Of<IDataTypeConfigurationSource>());
            var setType1 = publishedContentTypeFactory.CreateContentType(1000, "set1", new[]
            {
                publishedContentTypeFactory.CreatePropertyType("prop1", 0, "editor1"),
            });

            var snapshotCache = new DictionaryCacheProvider();
            var facadeCache = new DictionaryCacheProvider();

            var facade = new Mock<IFacade>();
            facade.Setup(x => x.FacadeCache).Returns(facadeCache);
            facade.Setup(x => x.SnapshotCache).Returns(snapshotCache);

            var facadeAccessor = new Mock<IFacadeAccessor>();
            facadeAccessor.Setup(x => x.Facade).Returns(facade.Object);

            // pretend we're creating this set as a value for a property
            // referenceCacheLevel is the cache level for this fictious property
            // converterCacheLevel is the cache level specified by the converter

            var set1 = new PublishedElement(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false, referenceCacheLevel, facadeAccessor.Object);

            Assert.AreEqual(1234, set1.Value("prop1"));
            Assert.AreEqual(1, converter.SourceConverts);
            Assert.AreEqual(1, converter.InterConverts);

            Assert.AreEqual(snapshotCount1, snapshotCache.Items.Count);
            Assert.AreEqual(facadeCount1, facadeCache.Items.Count);

            Assert.AreEqual(1234, set1.Value("prop1"));
            Assert.AreEqual(1, converter.SourceConverts);
            Assert.AreEqual(interConverts, converter.InterConverts);

            Assert.AreEqual(snapshotCount2, snapshotCache.Items.Count);
            Assert.AreEqual(facadeCount2, facadeCache.Items.Count);

            var oldFacadeCache = facadeCache;
            facadeCache.Items.Clear();

            Assert.AreEqual(1234, set1.Value("prop1"));
            Assert.AreEqual(1, converter.SourceConverts);

            Assert.AreEqual(snapshotCount2, snapshotCache.Items.Count);
            Assert.AreEqual(facadeCount2, facadeCache.Items.Count);
            Assert.AreEqual(facadeCount2, oldFacadeCache.Items.Count);

            Assert.AreEqual((interConverts == 1 ? 1 : 3) + facadeCache.Items.Count, converter.InterConverts);

            var oldSnapshotCache = snapshotCache;
            snapshotCache.Items.Clear();

            Assert.AreEqual(1234, set1.Value("prop1"));
            Assert.AreEqual(1, converter.SourceConverts);

            Assert.AreEqual(snapshotCount2, snapshotCache.Items.Count);
            Assert.AreEqual(snapshotCount2, oldSnapshotCache.Items.Count);
            Assert.AreEqual(facadeCount2, facadeCache.Items.Count);

            Assert.AreEqual((interConverts == 1 ? 1 : 4) + facadeCache.Items.Count + snapshotCache.Items.Count, converter.InterConverts);
        }

        [Test]
        public void CacheUnknownTest()
        {
            var converter = new CacheConverter1(PropertyCacheLevel.Unknown);

            var converters = new PropertyValueConverterCollection(new IPropertyValueConverter[]
            {
                converter,
            });

            var publishedContentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, Mock.Of<IDataTypeConfigurationSource>());
            var setType1 = publishedContentTypeFactory.CreateContentType(1000, "set1", new[]
            {
                publishedContentTypeFactory.CreatePropertyType("prop1", 0, "editor1"),
            });

            Assert.Throws<Exception>(() =>
            {
                var unused = new PublishedElement(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false);
            });
        }

        private class CacheConverter1 : IPropertyValueConverter
        {
            private readonly PropertyCacheLevel _cacheLevel;

            public CacheConverter1(PropertyCacheLevel cacheLevel)
            {
                _cacheLevel = cacheLevel;
            }

            public int SourceConverts { get; private set; }
            public int InterConverts { get; private set; }

            public bool IsConverter(PublishedPropertyType propertyType)
                => propertyType.PropertyEditorAlias.InvariantEquals("editor1");

            public Type GetPropertyValueType(PublishedPropertyType propertyType)
                => typeof(int);

            public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
                => _cacheLevel;

            public object ConvertSourceToInter(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
            {
                SourceConverts++;
                return int.TryParse(source as string, out int i) ? i : 0;
            }

            public object ConvertInterToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            {
                InterConverts++;
                return (int) inter;
            }

            public object ConvertInterToXPath(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
                => ((int) inter).ToString();
        }

        // fixme kill
        private class TestPublishedElementProperty : PublishedElementPropertyBase
        {
            public TestPublishedElementProperty(PublishedPropertyType propertyType, IPublishedElement element, bool previewing, PropertyCacheLevel referenceCacheLevel, object sourceValue,
                Func<Dictionary<string, object>> getSnapshotCache, Func<Dictionary<string, object>> getFacadeCache)
                : base(propertyType, element, previewing, referenceCacheLevel, sourceValue)
            { }
        }
    }
}