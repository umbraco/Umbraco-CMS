using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.Components;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Published
{
    [TestFixture]
    public class ConvertersTests
    {
        #region SimpleConverter1

        [Test]
        public void SimpleConverter1Test()
        {
            var converters = new PropertyValueConverterCollection(new IPropertyValueConverter[]
            {
                new SimpleConverter1(),
            });

            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new VoidEditor(Mock.Of<ILogger>())) { Id = 1 });

            var contentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, dataTypeService);

            var elementType1 = contentTypeFactory.CreateContentType(1000, "element1", new[]
            {
                contentTypeFactory.CreatePropertyType("prop1", 1),
            });

            var element1 = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false);

            Assert.AreEqual(1234, element1.Value("prop1"));

            // 'null' would be considered a 'missing' value by the default, magic logic
            var e = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", null } }, false);
            Assert.IsFalse(e.HasValue("prop1"));

            // '0' would not - it's a valid integer - but the converter knows better
            e = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "0" } }, false);
            Assert.IsFalse(e.HasValue("prop1"));
        }

        private class SimpleConverter1 : IPropertyValueConverter
        {
            public bool? IsValue(object value, PropertyValueLevel level)
            {
                switch (level)
                {
                    case PropertyValueLevel.Source:
                        return null;
                    case PropertyValueLevel.Inter:
                        return value is int ivalue && ivalue != 0;
                    default:
                        throw new NotSupportedException($"Invalid level: {level}.");
                }
            }

            public bool IsConverter(PublishedPropertyType propertyType)
                => propertyType.EditorAlias.InvariantEquals("Umbraco.Void");

            public Type GetPropertyValueType(PublishedPropertyType propertyType)
                => typeof (int);

            public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
                => PropertyCacheLevel.Element;

            public object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
                => int.TryParse(source as string, out int i) ? i : 0;

            public object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
                => (int) inter;

            public object ConvertIntermediateToXPath(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
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
            var publishedSnapshotMock = new Mock<IPublishedSnapshot>();
            publishedSnapshotMock.Setup(x => x.Content).Returns(cacheMock.Object);
            var publishedSnapshotAccessorMock = new Mock<IPublishedSnapshotAccessor>();
            publishedSnapshotAccessorMock.Setup(x => x.PublishedSnapshot).Returns(publishedSnapshotMock.Object);
            var publishedSnapshotAccessor = publishedSnapshotAccessorMock.Object;

            var converters = new PropertyValueConverterCollection(new IPropertyValueConverter[]
            {
                new SimpleConverter2(publishedSnapshotAccessor),
            });

            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new VoidEditor(Mock.Of<ILogger>())) { Id = 1 });

            var contentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, dataTypeService);

            var elementType1 = contentTypeFactory.CreateContentType(1000, "element1", new[]
            {
                contentTypeFactory.CreatePropertyType("prop1", 1),
            });

            var element1 = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false);

            var cntType1 = contentTypeFactory.CreateContentType(1001, "cnt1", Array.Empty<PublishedPropertyType>());
            var cnt1 = new TestPublishedContent(cntType1, 1234, Guid.NewGuid(), new Dictionary<string, object>(), false);
            cacheContent[cnt1.Id] = cnt1;

            Assert.AreSame(cnt1, element1.Value("prop1"));
        }

        private class SimpleConverter2 : IPropertyValueConverter
        {
            private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
            private readonly PropertyCacheLevel _cacheLevel;

            public SimpleConverter2(IPublishedSnapshotAccessor publishedSnapshotAccessor, PropertyCacheLevel cacheLevel = PropertyCacheLevel.None)
            {
                _publishedSnapshotAccessor = publishedSnapshotAccessor;
                _cacheLevel = cacheLevel;
            }

            public bool? IsValue(object value, PropertyValueLevel level)
                => value != null && (!(value is string) || string.IsNullOrWhiteSpace((string) value) == false);

            public bool IsConverter(PublishedPropertyType propertyType)
                => propertyType.EditorAlias.InvariantEquals("Umbraco.Void");

            public Type GetPropertyValueType(PublishedPropertyType propertyType)
                // the first version would be the "generic" version, but say we want to be more precise
                // and return: whatever Clr type is generated for content type with alias "cnt1" -- which
                // we cannot really typeof() at the moment because it has not been generated, hence ModelType.
                // => typeof (IPublishedContent);
                => ModelType.For("cnt1");

            public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
                => _cacheLevel;

            public object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
                => int.TryParse(source as string, out int i) ? i : -1;

            public object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
                => _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById((int) inter);

            public object ConvertIntermediateToXPath(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
                => ((int) inter).ToString();
        }

        #endregion

        #region SimpleConverter3

        [Test]
        public void SimpleConverter3Test()
        {
            Current.Reset();
            var register = RegisterFactory.Create();

            var composition = new Composition(register, new TypeLoader(), Mock.Of<IProfilingLogger>(), ComponentTests.MockRuntimeState(RuntimeLevel.Run));

            composition.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>()
                .Append<SimpleConverter3A>()
                .Append<SimpleConverter3B>();

            IPublishedModelFactory factory = new PublishedModelFactory(new[]
            {
                typeof (PublishedSnapshotTestObjects.TestElementModel1), typeof (PublishedSnapshotTestObjects.TestElementModel2),
                typeof (PublishedSnapshotTestObjects.TestContentModel1), typeof (PublishedSnapshotTestObjects.TestContentModel2),
            });
            register.Register(f => factory);

            Current.Factory = composition.CreateFactory();

            var cacheMock = new Mock<IPublishedContentCache>();
            var cacheContent = new Dictionary<int, IPublishedContent>();
            cacheMock.Setup(x => x.GetById(It.IsAny<int>())).Returns<int>(id => cacheContent.TryGetValue(id, out IPublishedContent content) ? content : null);
            var publishedSnapshotMock = new Mock<IPublishedSnapshot>();
            publishedSnapshotMock.Setup(x => x.Content).Returns(cacheMock.Object);
            var publishedSnapshotAccessorMock = new Mock<IPublishedSnapshotAccessor>();
            publishedSnapshotAccessorMock.Setup(x => x.PublishedSnapshot).Returns(publishedSnapshotMock.Object);
            register.Register(f => publishedSnapshotAccessorMock.Object);

            var converters = Current.Factory.GetInstance<PropertyValueConverterCollection>();

            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new VoidEditor(Mock.Of<ILogger>())) { Id = 1 },
                new DataType(new VoidEditor("2", Mock.Of<ILogger>())) { Id = 2 });

            var contentTypeFactory = new PublishedContentTypeFactory(factory, converters, dataTypeService);

            var elementType1 = contentTypeFactory.CreateContentType(1000, "element1", new[]
            {
                contentTypeFactory.CreatePropertyType("prop1", 1),
            });

            var elementType2 = contentTypeFactory.CreateContentType(1001, "element2", new[]
            {
                contentTypeFactory.CreatePropertyType("prop2", 2),
            });

            var contentType1 = contentTypeFactory.CreateContentType(1002, "content1", new[]
            {
                contentTypeFactory.CreatePropertyType("prop1", 1),
            });

            var contentType2 = contentTypeFactory.CreateContentType(1003, "content2", new[]
            {
                contentTypeFactory.CreatePropertyType("prop2", 2),
            });

            var element1 = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "val1" } }, false);
            var element2 = new PublishedElement(elementType2, Guid.NewGuid(), new Dictionary<string, object> { { "prop2", "1003" } }, false);
            var cnt1 = new TestPublishedContent(contentType1, 1003, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "val1" } }, false);
            var cnt2 = new TestPublishedContent(contentType2, 1004, Guid.NewGuid(), new Dictionary<string, object> { { "prop2", "1003" } }, false);

            cacheContent[cnt1.Id] = cnt1.CreateModel();
            cacheContent[cnt2.Id] = cnt2.CreateModel();

            // can get the actual property Clr type
            // ie ModelType gets properly mapped by IPublishedContentModelFactory
            // must test ModelClrType with special equals 'cos they are not ref-equals
            Assert.IsTrue(ModelType.Equals(typeof (IEnumerable<>).MakeGenericType(ModelType.For("content1")), contentType2.GetPropertyType("prop2").ModelClrType));
            Assert.AreEqual(typeof (IEnumerable<PublishedSnapshotTestObjects.TestContentModel1>), contentType2.GetPropertyType("prop2").ClrType);

            // can create a model for an element
            var model1 = factory.CreateModel(element1);
            Assert.IsInstanceOf<PublishedSnapshotTestObjects.TestElementModel1>(model1);
            Assert.AreEqual("val1", ((PublishedSnapshotTestObjects.TestElementModel1) model1).Prop1);

            // can create a model for a published content
            var model2 = factory.CreateModel(element2);
            Assert.IsInstanceOf<PublishedSnapshotTestObjects.TestElementModel2>(model2);
            var mmodel2 = (PublishedSnapshotTestObjects.TestElementModel2) model2;

            // and get direct property
            Assert.IsInstanceOf<PublishedSnapshotTestObjects.TestContentModel1[]>(model2.Value("prop2"));
            Assert.AreEqual(1, ((PublishedSnapshotTestObjects.TestContentModel1[]) model2.Value("prop2")).Length);

            // and get model property
            Assert.IsInstanceOf<IEnumerable<PublishedSnapshotTestObjects.TestContentModel1>>(mmodel2.Prop2);
            Assert.IsInstanceOf<PublishedSnapshotTestObjects.TestContentModel1[]>(mmodel2.Prop2);
            var mmodel1 = mmodel2.Prop2.First();

            // and we get what we want
            Assert.AreSame(cacheContent[mmodel1.Id], mmodel1);
        }

        public class SimpleConverter3A : PropertyValueConverterBase
        {
            public override bool IsConverter(PublishedPropertyType propertyType)
                => propertyType.EditorAlias == "Umbraco.Void";

            public override Type GetPropertyValueType(PublishedPropertyType propertyType)
                => typeof (string);

            public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
                => PropertyCacheLevel.Element;
        }

        public class SimpleConverter3B : PropertyValueConverterBase
        {
            private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

            public SimpleConverter3B(IPublishedSnapshotAccessor publishedSnapshotAccessor)
            {
                _publishedSnapshotAccessor = publishedSnapshotAccessor;
            }

            public override bool IsConverter(PublishedPropertyType propertyType)
                => propertyType.EditorAlias == "Umbraco.Void.2";

            public override Type GetPropertyValueType(PublishedPropertyType propertyType)
                => typeof (IEnumerable<>).MakeGenericType(ModelType.For("content1"));

            public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
                => PropertyCacheLevel.Elements;

            public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
            {
                var s = source as string;
                return s?.Split(',').Select(int.Parse).ToArray() ?? Array.Empty<int>();
            }

            public override object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            {
                return ((int[]) inter).Select(x => (PublishedSnapshotTestObjects.TestContentModel1) _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(x)).ToArray();
            }
        }

        #endregion
    }
}
