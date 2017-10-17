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

namespace Umbraco.Tests.Facade
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
            var contentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, Mock.Of<IDataTypeConfigurationSource>());

            var elementType1 = contentTypeFactory.CreateContentType(1000, "element1", new[]
            {
                contentTypeFactory.CreatePropertyType("prop1", 0, "editor1"),
            });

            var element1 = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false);

            Assert.AreEqual(1234, element1.Value("prop1"));
        }

        private class SimpleConverter1 : IPropertyValueConverter
        {
            public bool IsConverter(PublishedPropertyType propertyType)
                => propertyType.PropertyEditorAlias.InvariantEquals("editor1");

            public Type GetPropertyValueType(PublishedPropertyType propertyType)
                => typeof (int);

            public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
                => PropertyCacheLevel.Content;

            public object ConvertSourceToInter(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
                => int.TryParse(source as string, out int i) ? i : 0;

            public object ConvertInterToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
                => (int) inter;

            public object ConvertInterToXPath(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
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
            var contentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, Mock.Of<IDataTypeConfigurationSource>());

            var elementType1 = contentTypeFactory.CreateContentType(1000, "element1", new[]
            {
                contentTypeFactory.CreatePropertyType("prop1", 0, "editor2"),
            });

            var element1 = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false);

            var cntType1 = contentTypeFactory.CreateContentType(1001, "cnt1", Array.Empty<PublishedPropertyType>());
            var cnt1 = new FacadeTestObjects.TestPublishedContent(cntType1, 1234, Guid.NewGuid(), new Dictionary<string, object>(), false);
            cacheContent[cnt1.Id] = cnt1;

            Assert.AreSame(cnt1, element1.Value("prop1"));
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

            public object ConvertSourceToInter(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
                => int.TryParse(source as string, out int i) ? i : -1;

            public object ConvertInterToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
                => _facadeAccessor.Facade.ContentCache.GetById((int) inter);

            public object ConvertInterToXPath(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
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

            IPublishedModelFactory factory = new PublishedModelFactory(new[]
            {
                typeof (FacadeTestObjects.TestElementModel1), typeof (FacadeTestObjects.TestElementModel2),
                typeof (FacadeTestObjects.TestContentModel1), typeof (FacadeTestObjects.TestContentModel2),
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

            var converters = Current.Container.GetInstance<PropertyValueConverterCollection>();
            var contentTypeFactory = new PublishedContentTypeFactory(factory, converters, Mock.Of<IDataTypeConfigurationSource>());

            var elementType1 = contentTypeFactory.CreateContentType(1000, "element1", new[]
            {
                contentTypeFactory.CreatePropertyType("prop1", 0, "editor1"),
            });

            var elementType2 = contentTypeFactory.CreateContentType(1001, "element2", new[]
            {
                contentTypeFactory.CreatePropertyType("prop2", 0, "editor2"),
            });

            var contentType1 = contentTypeFactory.CreateContentType(1002, "content1", new[]
            {
                contentTypeFactory.CreatePropertyType("prop1", 0, "editor1"),
            });

            var contentType2 = contentTypeFactory.CreateContentType(1003, "content2", new[]
            {
                contentTypeFactory.CreatePropertyType("prop2", 0, "editor2"),
            });

            var element1 = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "val1" } }, false);
            var element2 = new PublishedElement(elementType2, Guid.NewGuid(), new Dictionary<string, object> { { "prop2", "1003" } }, false);
            var cnt1 = new FacadeTestObjects.TestPublishedContent(contentType1, 1003, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "val1" } }, false);
            var cnt2 = new FacadeTestObjects.TestPublishedContent(contentType2, 1004, Guid.NewGuid(), new Dictionary<string, object> { { "prop2", "1003" } }, false);

            cacheContent[cnt1.Id] = cnt1.CreateModel();
            cacheContent[cnt2.Id] = cnt2.CreateModel();

            // can get the actual property Clr type
            // ie ModelType gets properly mapped by IPublishedContentModelFactory
            // must test ModelClrType with special equals 'cos they are not ref-equals
            Assert.IsTrue(ModelType.Equals(typeof (IEnumerable<>).MakeGenericType(ModelType.For("content1")), contentType2.GetPropertyType("prop2").ModelClrType));
            Assert.AreEqual(typeof (IEnumerable<FacadeTestObjects.TestContentModel1>), contentType2.GetPropertyType("prop2").ClrType);

            // can create a model for an element
            var model1 = factory.CreateModel(element1);
            Assert.IsInstanceOf<FacadeTestObjects.TestElementModel1>(model1);
            Assert.AreEqual("val1", ((FacadeTestObjects.TestElementModel1) model1).Prop1);

            // can create a model for a published content
            var model2 = factory.CreateModel(element2);
            Assert.IsInstanceOf<FacadeTestObjects.TestElementModel2>(model2);
            var mmodel2 = (FacadeTestObjects.TestElementModel2) model2;

            // and get direct property
            Assert.IsInstanceOf<FacadeTestObjects.TestContentModel1[]>(model2.Value("prop2"));
            Assert.AreEqual(1, ((FacadeTestObjects.TestContentModel1[]) model2.Value("prop2")).Length);

            // and get model property
            Assert.IsInstanceOf<IEnumerable<FacadeTestObjects.TestContentModel1>>(mmodel2.Prop2);
            Assert.IsInstanceOf<FacadeTestObjects.TestContentModel1[]>(mmodel2.Prop2);
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

            public override object ConvertSourceToInter(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
            {
                var s = source as string;
                return s?.Split(',').Select(int.Parse).ToArray() ?? Array.Empty<int>();
            }

            public override object ConvertInterToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            {
                return ((int[]) inter).Select(x => (FacadeTestObjects.TestContentModel1) _facadeAccessor.Facade.ContentCache.GetById(x)).ToArray();
            }
        }

        #endregion
    }
}
