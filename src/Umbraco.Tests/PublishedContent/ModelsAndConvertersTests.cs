using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class ModelsAndConvertersTests
    {
        #region ModelType

        [Test]
        public void ModelTypeEqualityTests()
        {
            Assert.AreNotEqual(ModelType.For("alias1"), ModelType.For("alias1"));

            Assert.IsTrue(ModelType.Equals(ModelType.For("alias1"), ModelType.For("alias1")));
            Assert.IsFalse(ModelType.Equals(ModelType.For("alias1"), ModelType.For("alias2")));

            Assert.IsTrue(ModelType.Equals(typeof (IEnumerable<>).MakeGenericType(ModelType.For("alias1")), typeof (IEnumerable<>).MakeGenericType(ModelType.For("alias1"))));
            Assert.IsFalse(ModelType.Equals(typeof (IEnumerable<>).MakeGenericType(ModelType.For("alias1")), typeof (IEnumerable<>).MakeGenericType(ModelType.For("alias2"))));

            Assert.IsTrue(ModelType.Equals(ModelType.For("alias1").MakeArrayType(), ModelType.For("alias1").MakeArrayType()));
            Assert.IsFalse(ModelType.Equals(ModelType.For("alias1").MakeArrayType(), ModelType.For("alias2").MakeArrayType()));
        }

        [Test]
        public void ModelTypeToStringTests()
        {
            Assert.AreEqual("{alias1}", ModelType.For("alias1").ToString());

            // there's an "*" there because the arrays are not true SZArray - but that changes when we map
            Assert.AreEqual("{alias1}[*]", ModelType.For("alias1").MakeArrayType().ToString());
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[{alias1}[*]]", typeof (IEnumerable<>).MakeGenericType(ModelType.For("alias1").MakeArrayType()).ToString());
        }

        [Test]
        public void ModelTypeMapTests()
        {
            var map = new Dictionary<string, Type>
            {
                { "alias1", typeof (TestSetModel1) },
                { "alias2", typeof (TestSetModel2) },
            };

            Assert.AreEqual("Umbraco.Tests.PublishedContent.ModelsAndConvertersTests+TestSetModel1",
                ModelType.Map(ModelType.For("alias1"), map).ToString());
            Assert.AreEqual("Umbraco.Tests.PublishedContent.ModelsAndConvertersTests+TestSetModel1[]",
                ModelType.Map(ModelType.For("alias1").MakeArrayType(), map).ToString());
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[Umbraco.Tests.PublishedContent.ModelsAndConvertersTests+TestSetModel1]",
                ModelType.Map(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1")), map).ToString());
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[Umbraco.Tests.PublishedContent.ModelsAndConvertersTests+TestSetModel1[]]",
                ModelType.Map(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1").MakeArrayType()), map).ToString());
        }

        #endregion

        #region SimpleConverter1

        [Test]
        public void SimpleConverter1Test()
        {
            var converters = new PropertyValueConverterCollection(new IPropertyValueConverter[]
            {
                new SimpleConverter1(),
            });

            var setType1 = new PublishedContentType(1000, "set1", new[]
            {
                new PublishedPropertyType("prop1", "editor1", converters),
            });

            var set1 = new PropertySet(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false);

            Assert.AreEqual(1234, set1.Value("prop1"));
        }

        private class SimpleConverter1 : IPropertyValueConverter
        {
            public bool IsConverter(PublishedPropertyType propertyType)
                => propertyType.PropertyEditorAlias.InvariantEquals("editor1");

            public Type GetPropertyValueType(PublishedPropertyType propertyType)
                => typeof (int);

            public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
                => PropertyCacheLevel.Content;

            public object ConvertSourceToInter(IPropertySet owner, PublishedPropertyType propertyType, object source, bool preview)
                => int.TryParse(source as string, out int i) ? i : 0;

            public object ConvertInterToObject(IPropertySet owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
                => (int) inter;

            public object ConvertInterToXPath(IPropertySet owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
                => ((int) inter).ToString();
        }

        #endregion

        #region SimpleConverter2

        [Test]
        public void SimpleConverter2Test()
        {
            var cacheMock = new Mock<IPublishedContentCache>();
            var cacheContent = new Dictionary<int, IPublishedContent>();
            cacheMock.Setup(x => x.GetById(It.IsAny<int>())).Returns<int>(id => cacheContent.TryGetValue(id, out IPublishedContent content) ? content : null);
            var facadeMock = new Mock<IFacade>();
            facadeMock.Setup(x => x.ContentCache).Returns(cacheMock.Object);
            var facadeAccessorMock = new Mock<IFacadeAccessor>();
            facadeAccessorMock.Setup(x => x.Facade).Returns(facadeMock.Object);
            var facadeAccessor = facadeAccessorMock.Object;

            var converters = new PropertyValueConverterCollection(new IPropertyValueConverter[]
            {
                new SimpleConverter2(facadeAccessor),
            });

            var setType1 = new PublishedContentType(1000, "set1", new[]
            {
                new PublishedPropertyType("prop1", "editor2", converters),
            });

            var set1 = new PropertySet(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false);

            var cntType1 = new PublishedContentType(1001, "cnt1", Array.Empty<PublishedPropertyType>());
            var cnt1 = new TestPublishedContent(cntType1, 1234, Guid.NewGuid(), new Dictionary<string, object>(), false);
            cacheContent[cnt1.Id] = cnt1;

            Assert.AreSame(cnt1, set1.Value("prop1"));
        }

        private class SimpleConverter2 : IPropertyValueConverter
        {
            private readonly IFacadeAccessor _facadeAccessor;
            private readonly PropertyCacheLevel _cacheLevel;

            public SimpleConverter2(IFacadeAccessor facadeAccessor, PropertyCacheLevel cacheLevel = PropertyCacheLevel.None)
            {
                _facadeAccessor = facadeAccessor;
                _cacheLevel = cacheLevel;
            }

            public bool IsConverter(PublishedPropertyType propertyType)
                => propertyType.PropertyEditorAlias.InvariantEquals("editor2");

            public Type GetPropertyValueType(PublishedPropertyType propertyType)
                // the first version would be the "generic" version, but say we want to be more precise
                // and return: whatever Clr type is generated for content type with alias "cnt1" -- which
                // we cannot really typeof() at the moment because it has not been generated, hence ModelType.
                // => typeof (IPublishedContent);
                => ModelType.For("cnt1");

            public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
                => _cacheLevel;

            public object ConvertSourceToInter(IPropertySet owner, PublishedPropertyType propertyType, object source, bool preview)
                => int.TryParse(source as string, out int i) ? i : -1;

            public object ConvertInterToObject(IPropertySet owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
                => _facadeAccessor.Facade.ContentCache.GetById((int) inter);

            public object ConvertInterToXPath(IPropertySet owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
                => ((int) inter).ToString();
        }

        #endregion

        #region SimpleConverter3

        [Test]
        public void SimpleConverter3Test()
        {
            Current.Reset();
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore();

            Current.Container.RegisterCollectionBuilder<PropertyValueConverterCollectionBuilder>()
                .Append<SimpleConverter3A>()
                .Append<SimpleConverter3B>();

            IPublishedContentModelFactory factory = new PublishedContentModelFactory(new[]
            {
                typeof(TestSetModel1), typeof(TestSetModel2),
                typeof(TestContentModel1), typeof(TestContentModel2),
            });
            Current.Container.Register(f => factory);

            var cacheMock = new Mock<IPublishedContentCache>();
            var cacheContent = new Dictionary<int, IPublishedContent>();
            cacheMock.Setup(x => x.GetById(It.IsAny<int>())).Returns<int>(id => cacheContent.TryGetValue(id, out IPublishedContent content) ? content : null);
            var facadeMock = new Mock<IFacade>();
            facadeMock.Setup(x => x.ContentCache).Returns(cacheMock.Object);
            var facadeAccessorMock = new Mock<IFacadeAccessor>();
            facadeAccessorMock.Setup(x => x.Facade).Returns(facadeMock.Object);
            Current.Container.Register(f => facadeAccessorMock.Object);

            var setType1 = new PublishedContentType(1000, "set1", new[]
            {
                new PublishedPropertyType("prop1", "editor1"),
            });

            var setType2 = new PublishedContentType(1001, "set2", new[]
            {
                new PublishedPropertyType("prop2", "editor2"),
            });

            var contentType1 = new PublishedContentType(1002, "content1", new[]
            {
                new PublishedPropertyType("prop1", "editor1"),
            });

            var contentType2 = new PublishedContentType(1003, "content2", new[]
            {
                new PublishedPropertyType("prop2", "editor2"),
            });

            var set1 = new PropertySet(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "val1" } }, false);
            var set2 = new PropertySet(setType2, Guid.NewGuid(), new Dictionary<string, object> { { "prop2", "1003" } }, false);
            var cnt1 = new TestPublishedContent(contentType1, 1003, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "val1" } }, false);
            var cnt2 = new TestPublishedContent(contentType2, 1004, Guid.NewGuid(), new Dictionary<string, object> { { "prop2", "1003" } }, false);

            cacheContent[cnt1.Id] = cnt1.CreateModel();
            cacheContent[cnt2.Id] = cnt2.CreateModel();

            // can get the actual property Clr type
            // ie ModelType gets properly mapped by IPublishedContentModelFactory
            // must test ModelClrType with special equals 'cos they are not ref-equals
            Assert.IsTrue(ModelType.Equals(typeof(IEnumerable<>).MakeGenericType(ModelType.For("content1")), contentType2.GetPropertyType("prop2").ModelClrType));
            Assert.AreEqual(typeof(IEnumerable<TestContentModel1>), contentType2.GetPropertyType("prop2").ClrType);

            // can create a model for a property set
            var model1 = factory.CreateModel(set1);
            Assert.IsInstanceOf<TestSetModel1>(model1);
            Assert.AreEqual("val1", ((TestSetModel1)model1).Prop1);

            // can create a model for a published content
            var model2 = factory.CreateModel(set2);
            Assert.IsInstanceOf<TestSetModel2>(model2);
            var mmodel2 = (TestSetModel2)model2;

            // and get direct property
            Assert.IsInstanceOf<TestContentModel1[]>(model2.Value("prop2"));
            Assert.AreEqual(1, ((TestContentModel1[])model2.Value("prop2")).Length);

            // and get model property
            Assert.IsInstanceOf<IEnumerable<TestContentModel1>>(mmodel2.Prop2);
            Assert.IsInstanceOf<TestContentModel1[]>(mmodel2.Prop2);
            var mmodel1 = mmodel2.Prop2.First();

            // and we get what we want
            Assert.AreSame(cacheContent[mmodel1.Id], mmodel1);
        }

        public class SimpleConverter3A : PropertyValueConverterBase
        {
            public override bool IsConverter(PublishedPropertyType propertyType)
                => propertyType.PropertyEditorAlias == "editor1";

            public override Type GetPropertyValueType(PublishedPropertyType propertyType)
                => typeof (string);

            public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
                => PropertyCacheLevel.Content;
        }

        public class SimpleConverter3B : PropertyValueConverterBase
        {
            private readonly IFacadeAccessor _facadeAccessor;

            public SimpleConverter3B(IFacadeAccessor facadeAccessor)
            {
                _facadeAccessor = facadeAccessor;
            }

            public override bool IsConverter(PublishedPropertyType propertyType)
                => propertyType.PropertyEditorAlias == "editor2";

            public override Type GetPropertyValueType(PublishedPropertyType propertyType)
                => typeof (IEnumerable<>).MakeGenericType(ModelType.For("content1"));

            public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
                => PropertyCacheLevel.Snapshot;

            public override object ConvertSourceToInter(IPropertySet owner, PublishedPropertyType propertyType, object source, bool preview)
            {
                var s = source as string;
                return s?.Split(',').Select(int.Parse).ToArray() ?? Array.Empty<int>();
            }

            public override object ConvertInterToObject(IPropertySet owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            {
                return ((int[]) inter).Select(x => (TestContentModel1) _facadeAccessor.Facade.ContentCache.GetById(x)).ToArray();
            }
        }

        #endregion

        #region ConversionCache

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

            var setType1 = new PublishedContentType(1000, "set1", new[]
            {
                new PublishedPropertyType("prop1", "editor1", converters),
            });

            // PropertySetPropertyBase.GetCacheLevels:
            //
            //   if property level is > reference level, or both are None
            //     use None for property & new reference
            //   else
            //     use Content for property, & keep reference
            //
            // PropertySet creates properties with reference being None
            // if converter specifies None, keep using None
            // anything else is not > None, use Content
            //
            // for standalone property sets, it's only None or Content

            var set1 = new PropertySet(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false);

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

            var setType1 = new PublishedContentType(1000, "set1", new[]
            {
                new PublishedPropertyType("prop1", "editor1", converters),
            });

            var snapshotCache = new Dictionary<string, object>();
            var facadeCache = new Dictionary<string, object>();

            var facadeServiceMock = new Mock<IFacadeService>();
            facadeServiceMock
                .Setup(x => x.CreateSetProperty(It.IsAny<PublishedPropertyType>(), It.IsAny<IPropertySet>(), It.IsAny<bool>(), It.IsAny<PropertyCacheLevel>(), It.IsAny<object>()))
                .Returns<PublishedPropertyType, IPropertySet, bool, PropertyCacheLevel, object>((propertyType, set, previewing, refCacheLevel, value) =>
                {
                    // ReSharper disable AccessToModifiedClosure
                    return new TestPropertySetProperty(propertyType, set, previewing, refCacheLevel, value, () => snapshotCache, () => facadeCache);
                    // ReSharper restore AccessToModifiedClosure
                });
            var facadeService = facadeServiceMock.Object;

            // pretend we're creating this set as a value for a property
            // referenceCacheLevel is the cache level for this fictious property
            // converterCacheLevel is the cache level specified by the converter

            var set1 = new PropertySet(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false, facadeService, referenceCacheLevel);

            Assert.AreEqual(1234, set1.Value("prop1"));
            Assert.AreEqual(1, converter.SourceConverts);
            Assert.AreEqual(1, converter.InterConverts);

            Assert.AreEqual(snapshotCount1, snapshotCache.Count);
            Assert.AreEqual(facadeCount1, facadeCache.Count);

            Assert.AreEqual(1234, set1.Value("prop1"));
            Assert.AreEqual(1, converter.SourceConverts);
            Assert.AreEqual(interConverts, converter.InterConverts);

            Assert.AreEqual(snapshotCount2, snapshotCache.Count);
            Assert.AreEqual(facadeCount2, facadeCache.Count);

            var oldFacadeCache = facadeCache;
            facadeCache = new Dictionary<string, object>();

            Assert.AreEqual(1234, set1.Value("prop1"));
            Assert.AreEqual(1, converter.SourceConverts);

            Assert.AreEqual(snapshotCount2, snapshotCache.Count);
            Assert.AreEqual(facadeCount2, facadeCache.Count);
            Assert.AreEqual(facadeCount2, oldFacadeCache.Count);

            Assert.AreEqual((interConverts == 1 ? 1 : 3) + facadeCache.Count, converter.InterConverts);

            var oldSnapshotCache = snapshotCache;
            snapshotCache = new Dictionary<string, object>();

            Assert.AreEqual(1234, set1.Value("prop1"));
            Assert.AreEqual(1, converter.SourceConverts);

            Assert.AreEqual(snapshotCount2, snapshotCache.Count);
            Assert.AreEqual(snapshotCount2, oldSnapshotCache.Count);
            Assert.AreEqual(facadeCount2, facadeCache.Count);

            Assert.AreEqual((interConverts == 1 ? 1 : 4) + facadeCache.Count + snapshotCache.Count, converter.InterConverts);
        }

        [Test]
        public void CacheUnknownTest()
        {
            var converter = new CacheConverter1(PropertyCacheLevel.Unknown);

            var converters = new PropertyValueConverterCollection(new IPropertyValueConverter[]
            {
                converter,
            });

            var setType1 = new PublishedContentType(1000, "set1", new[]
            {
                new PublishedPropertyType("prop1", "editor1", converters),
            });

            Assert.Throws<Exception>(() =>
            {
                var unused = new PropertySet(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false);
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

            public object ConvertSourceToInter(IPropertySet owner, PublishedPropertyType propertyType, object source, bool preview)
            {
                SourceConverts++;
                return int.TryParse(source as string, out int i) ? i : 0;
            }

            public object ConvertInterToObject(IPropertySet owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            {
                InterConverts++;
                return (int) inter;
            }

            public object ConvertInterToXPath(IPropertySet owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
                => ((int) inter).ToString();
        }

        private class TestPropertySetProperty : PropertySetPropertyBase
        {
            private readonly Func<Dictionary<string, object>> _getSnapshotCache;
            private readonly Func<Dictionary<string, object>> _getFacadeCache;
            private string _valuesCacheKey;

            public TestPropertySetProperty(PublishedPropertyType propertyType, IPropertySet set, bool previewing, PropertyCacheLevel referenceCacheLevel, object sourceValue,
                Func<Dictionary<string, object>> getSnapshotCache, Func<Dictionary<string, object>> getFacadeCache)
                : base(propertyType, set, previewing, referenceCacheLevel, sourceValue)
            {
                _getSnapshotCache = getSnapshotCache;
                _getFacadeCache = getFacadeCache;
            }

            private string ValuesCacheKey => _valuesCacheKey ?? (_valuesCacheKey = $"CacheValues[{(IsPreviewing ? "D" : "P")}{Set.Key}:{PropertyType.PropertyTypeAlias}");

            protected override CacheValues GetSnapshotCacheValues()
            {
                var snapshotCache = _getSnapshotCache();
                if (snapshotCache.TryGetValue(ValuesCacheKey, out object cacheValues))
                    return (CacheValues) cacheValues;
                snapshotCache[ValuesCacheKey] = cacheValues = new CacheValues();
                return (CacheValues) cacheValues;
            }

            protected override CacheValues GetFacadeCacheValues()
            {
                var facadeCache = _getFacadeCache();
                if (facadeCache.TryGetValue(ValuesCacheKey, out object cacheValues))
                    return (CacheValues)cacheValues;
                facadeCache[ValuesCacheKey] = cacheValues = new CacheValues();
                return (CacheValues)cacheValues;
            }
        }

        #endregion

        #region Model classes

        [PublishedContentModel("set1")]
        public class TestSetModel1 : PropertySetModel
        {
            public TestSetModel1(IPropertySet content)
                : base(content)
            { }

            public string Prop1 => this.Value<string>("prop1");
        }

        [PublishedContentModel("set2")]
        public class TestSetModel2 : PropertySetModel
        {
            public TestSetModel2(IPropertySet content)
                : base(content)
            { }

            public IEnumerable<TestContentModel1> Prop2 => this.Value<IEnumerable<TestContentModel1>>("prop2");
        }

        [PublishedContentModel("content1")]
        public class TestContentModel1 : PublishedContentModel
        {
            public TestContentModel1(IPublishedContent content)
                : base(content)
            { }

            public string Prop1 => this.Value<string>("prop1");
        }

        [PublishedContentModel("content2")]
        public class TestContentModel2 : PublishedContentModel
        {
            public TestContentModel2(IPublishedContent content)
                : base(content)
            { }

            public IEnumerable<TestContentModel1> Prop2 => this.Value<IEnumerable<TestContentModel1>>("prop2");
        }

        #endregion

        #region Support classes

        internal class TestPublishedContent : PropertySet, IPublishedContent
        {
            public TestPublishedContent(PublishedContentType contentType, int id, Guid key, Dictionary<string, object> values, bool previewing)
                : base(contentType, key, values, previewing)
            {
                Id = id;
            }

            public int Id { get; }
            public int TemplateId { get; set; }
            public int SortOrder { get; set; }
            public string Name { get; set; }
            public string UrlName { get; set; }
            public string DocumentTypeAlias => ContentType.Alias;
            public int DocumentTypeId { get; set; }
            public string WriterName { get; set; }
            public string CreatorName { get; set; }
            public int WriterId { get; set; }
            public int CreatorId { get; set; }
            public string Path { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime UpdateDate { get; set; }
            public Guid Version { get; set; }
            public int Level { get; set; }
            public string Url { get; set; }
            public PublishedItemType ItemType => ContentType.ItemType;
            public bool IsDraft { get; set; }
            public IPublishedContent Parent { get; set; }
            public IEnumerable<IPublishedContent> Children { get; set; }

            // copied from PublishedContentBase
            public IPublishedProperty GetProperty(string alias, bool recurse)
            {
                var property = GetProperty(alias);
                if (recurse == false) return property;

                IPublishedContent content = this;
                var firstNonNullProperty = property;
                while (content != null && (property == null || property.HasValue == false))
                {
                    content = content.Parent;
                    property = content?.GetProperty(alias);
                    if (firstNonNullProperty == null && property != null) firstNonNullProperty = property;
                }

                // if we find a content with the property with a value, return that property
                // if we find no content with the property, return null
                // if we find a content with the property without a value, return that property
                //   have to save that first property while we look further up, hence firstNonNullProperty

                return property != null && property.HasValue ? property : firstNonNullProperty;
            }
        }

        #endregion
    }
}
