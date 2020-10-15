using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Published
{
    [TestFixture]
    public class ConvertersTests
    {
        #region SimpleConverter3

        [Test]
        public void SimpleConverter3Test()
        {
           // Current.Reset();
            var register = TestHelper.GetRegister();

            var composition = new Composition(register, TestHelper.GetMockedTypeLoader(), Mock.Of<IProfilingLogger>(), Mock.Of<IRuntimeState>(), Mock.Of<IIOHelper>(), AppCaches.NoCache);

            composition.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>()
                .Append<SimpleConverter3A>()
                .Append<SimpleConverter3B>();

            IPublishedModelFactory factory = new PublishedModelFactory(new[]
            {
                typeof (PublishedSnapshotTestObjects.TestElementModel1), typeof (PublishedSnapshotTestObjects.TestElementModel2),
                typeof (PublishedSnapshotTestObjects.TestContentModel1), typeof (PublishedSnapshotTestObjects.TestContentModel2),
            }, Mock.Of<IPublishedValueFallback>());
            register.Register(f => factory);

            var registerFactory = composition.CreateFactory();

            var cacheMock = new Mock<IPublishedContentCache>();
            var cacheContent = new Dictionary<int, IPublishedContent>();
            cacheMock.Setup(x => x.GetById(It.IsAny<int>())).Returns<int>(id => cacheContent.TryGetValue(id, out IPublishedContent content) ? content : null);
            var publishedSnapshotMock = new Mock<IPublishedSnapshot>();
            publishedSnapshotMock.Setup(x => x.Content).Returns(cacheMock.Object);
            var publishedSnapshotAccessorMock = new Mock<IPublishedSnapshotAccessor>();
            publishedSnapshotAccessorMock.Setup(x => x.PublishedSnapshot).Returns(publishedSnapshotMock.Object);
            register.Register(f => publishedSnapshotAccessorMock.Object);

            var converters = registerFactory.GetInstance<PropertyValueConverterCollection>();

            var dataTypeServiceMock = new Mock<IDataTypeService>();
            var dataType1 = new DataType(new VoidEditor(NullLoggerFactory.Instance, dataTypeServiceMock.Object,
                    Mock.Of<ILocalizationService>(), Mock.Of<ILocalizedTextService>(), Mock.Of<IShortStringHelper>()))
                { Id = 1 };
            var dataType2 = new DataType(new VoidEditor("2", NullLoggerFactory.Instance, Mock.Of<IDataTypeService>(),
                    Mock.Of<ILocalizationService>(), Mock.Of<ILocalizedTextService>(), Mock.Of<IShortStringHelper>()))
                { Id = 2 };

            dataTypeServiceMock.Setup(x => x.GetAll()).Returns(new []{dataType1, dataType2 });

            var contentTypeFactory = new PublishedContentTypeFactory(factory, converters, dataTypeServiceMock.Object);

            IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType, int i)
            {
                yield return contentTypeFactory.CreatePropertyType(contentType, "prop" + i, i);
            }

            var elementType1 = contentTypeFactory.CreateContentType(Guid.NewGuid(), 1000, "element1", t => CreatePropertyTypes(t, 1));
            var elementType2 = contentTypeFactory.CreateContentType(Guid.NewGuid(), 1001, "element2", t => CreatePropertyTypes(t, 2));
            var contentType1 = contentTypeFactory.CreateContentType(Guid.NewGuid(), 1002, "content1", t => CreatePropertyTypes(t, 1));
            var contentType2 = contentTypeFactory.CreateContentType(Guid.NewGuid(), 1003, "content2", t => CreatePropertyTypes(t, 2));

            var element1 = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "val1" } }, false);
            var element2 = new PublishedElement(elementType2, Guid.NewGuid(), new Dictionary<string, object> { { "prop2", "1003" } }, false);
            var cnt1 = new SolidPublishedContent(contentType1)
            {
                Id = 1003,
                Properties = new[] { new SolidPublishedProperty { Alias = "prop1", SolidHasValue = true, SolidValue = "val1" } }
            };
            var cnt2 = new SolidPublishedContent(contentType1)
            {
                Id = 1004,
                Properties = new[] { new SolidPublishedProperty { Alias = "prop2", SolidHasValue = true, SolidValue = "1003" } }
            };

            var publishedModelFactory = registerFactory.GetInstance<IPublishedModelFactory>();
            cacheContent[cnt1.Id] = cnt1.CreateModel(publishedModelFactory);
            cacheContent[cnt2.Id] = cnt2.CreateModel(publishedModelFactory);

            // can get the actual property Clr type
            // ie ModelType gets properly mapped by IPublishedContentModelFactory
            // must test ModelClrType with special equals 'cos they are not ref-equals
            Assert.IsTrue(ModelType.Equals(typeof(IEnumerable<>).MakeGenericType(ModelType.For("content1")), contentType2.GetPropertyType("prop2").ModelClrType));
            Assert.AreEqual(typeof(IEnumerable<PublishedSnapshotTestObjects.TestContentModel1>), contentType2.GetPropertyType("prop2").ClrType);

            // can create a model for an element
            var model1 = factory.CreateModel(element1);
            Assert.IsInstanceOf<PublishedSnapshotTestObjects.TestElementModel1>(model1);
            Assert.AreEqual("val1", ((PublishedSnapshotTestObjects.TestElementModel1)model1).Prop1);

            // can create a model for a published content
            var model2 = factory.CreateModel(element2);
            Assert.IsInstanceOf<PublishedSnapshotTestObjects.TestElementModel2>(model2);
            var mmodel2 = (PublishedSnapshotTestObjects.TestElementModel2)model2;

            // and get direct property
            Assert.IsInstanceOf<PublishedSnapshotTestObjects.TestContentModel1[]>(model2.Value(Mock.Of<IPublishedValueFallback>(), "prop2"));
            Assert.AreEqual(1, ((PublishedSnapshotTestObjects.TestContentModel1[])model2.Value(Mock.Of<IPublishedValueFallback>(), "prop2")).Length);

            // and get model property
            Assert.IsInstanceOf<IEnumerable<PublishedSnapshotTestObjects.TestContentModel1>>(mmodel2.Prop2);
            Assert.IsInstanceOf<PublishedSnapshotTestObjects.TestContentModel1[]>(mmodel2.Prop2);
            var mmodel1 = mmodel2.Prop2.First();

            // and we get what we want
            Assert.AreSame(cacheContent[mmodel1.Id], mmodel1);
        }

        public class SimpleConverter3A : PropertyValueConverterBase
        {
            public override bool IsConverter(IPublishedPropertyType propertyType)
                => propertyType.EditorAlias == "Umbraco.Void";

            public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
                => typeof(string);

            public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
                => PropertyCacheLevel.Element;
        }

        public class SimpleConverter3B : PropertyValueConverterBase
        {
            private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

            public SimpleConverter3B(IPublishedSnapshotAccessor publishedSnapshotAccessor)
            {
                _publishedSnapshotAccessor = publishedSnapshotAccessor;
            }

            public override bool IsConverter(IPublishedPropertyType propertyType)
                => propertyType.EditorAlias == "Umbraco.Void.2";

            public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
                => typeof(IEnumerable<>).MakeGenericType(ModelType.For("content1"));

            public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
                => PropertyCacheLevel.Elements;

            public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
            {
                var s = source as string;
                return s?.Split(',').Select(int.Parse).ToArray() ?? Array.Empty<int>();
            }

            public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            {
                return ((int[])inter).Select(x => (PublishedSnapshotTestObjects.TestContentModel1)_publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(x)).ToArray();
            }
        }

        #endregion
    }
}
