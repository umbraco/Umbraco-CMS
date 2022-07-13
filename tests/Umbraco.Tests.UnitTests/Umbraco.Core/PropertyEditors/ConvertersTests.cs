// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.PublishedCache.Internal;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class ConvertersTests
{
    [Test]
    public void SimpleConverter3Test()
    {
        var register = new ServiceCollection();

        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        composition.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>()
            .Append<SimpleConverter3A>()
            .Append<SimpleConverter3B>();

        IPublishedModelFactory factory = new PublishedModelFactory(
            new[]
            {
                typeof(PublishedSnapshotTestObjects.TestElementModel1),
                typeof(PublishedSnapshotTestObjects.TestElementModel2),
                typeof(PublishedSnapshotTestObjects.TestContentModel1),
                typeof(PublishedSnapshotTestObjects.TestContentModel2),
            },
            Mock.Of<IPublishedValueFallback>());
        register.AddTransient(f => factory);

        var cacheMock = new Mock<IPublishedContentCache>();
        var cacheContent = new Dictionary<int, IPublishedContent>();
        cacheMock.Setup(x => x.GetById(It.IsAny<int>())).Returns<int>(id =>
            cacheContent.TryGetValue(id, out var content) ? content : null);
        var publishedSnapshotMock = new Mock<IPublishedSnapshot>();
        publishedSnapshotMock.Setup(x => x.Content).Returns(cacheMock.Object);
        var publishedSnapshotAccessorMock = new Mock<IPublishedSnapshotAccessor>();
        var localPublishedSnapshot = publishedSnapshotMock.Object;
        publishedSnapshotAccessorMock.Setup(x => x.TryGetPublishedSnapshot(out localPublishedSnapshot)).Returns(true);
        register.AddTransient(f => publishedSnapshotAccessorMock.Object);

        var registerFactory = composition.CreateServiceProvider();
        var converters =
            registerFactory.GetRequiredService<PropertyValueConverterCollection>();

        var serializer = new ConfigurationEditorJsonSerializer();
        var dataTypeServiceMock = new Mock<IDataTypeService>();
        var dataType1 = new DataType(
            new VoidEditor(
                Mock.Of<IDataValueEditorFactory>()),
            serializer)
        { Id = 1 };
        var dataType2 = new DataType(
            new VoidEditor(
                "2",
                Mock.Of<IDataValueEditorFactory>()),
            serializer)
        { Id = 2 };

        dataTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { dataType1, dataType2 });

        var contentTypeFactory = new PublishedContentTypeFactory(factory, converters, dataTypeServiceMock.Object);

        IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType, int i)
        {
            yield return contentTypeFactory.CreatePropertyType(contentType, "prop" + i, i);
        }

        var elementType1 =
            contentTypeFactory.CreateContentType(Guid.NewGuid(), 1000, "element1", t => CreatePropertyTypes(t, 1));
        var elementType2 =
            contentTypeFactory.CreateContentType(Guid.NewGuid(), 1001, "element2", t => CreatePropertyTypes(t, 2));
        var contentType1 =
            contentTypeFactory.CreateContentType(Guid.NewGuid(), 1002, "content1", t => CreatePropertyTypes(t, 1));
        var contentType2 =
            contentTypeFactory.CreateContentType(Guid.NewGuid(), 1003, "content2", t => CreatePropertyTypes(t, 2));

        var element1 = new PublishedElement(
            elementType1,
            Guid.NewGuid(),
            new Dictionary<string, object> { { "prop1", "val1" } },
            false);
        var element2 = new PublishedElement(
            elementType2,
            Guid.NewGuid(),
            new Dictionary<string, object> { { "prop2", "1003" } },
            false);
        var cnt1 = new InternalPublishedContent(contentType1)
        {
            Id = 1003,
            Properties = new[]
            {
                new InternalPublishedProperty { Alias = "prop1", SolidHasValue = true, SolidValue = "val1" },
            },
        };
        var cnt2 = new InternalPublishedContent(contentType1)
        {
            Id = 1004,
            Properties = new[]
            {
                new InternalPublishedProperty { Alias = "prop2", SolidHasValue = true, SolidValue = "1003" },
            },
        };

        var publishedModelFactory = registerFactory.GetRequiredService<IPublishedModelFactory>();
        cacheContent[cnt1.Id] = cnt1.CreateModel(publishedModelFactory);
        cacheContent[cnt2.Id] = cnt2.CreateModel(publishedModelFactory);

        // can get the actual property Clr type
        // ie ModelType gets properly mapped by IPublishedContentModelFactory
        // must test ModelClrType with special equals 'cos they are not ref-equals
        Assert.IsTrue(ModelType.Equals(
            typeof(IEnumerable<>).MakeGenericType(ModelType.For("content1")),
            contentType2.GetPropertyType("prop2").ModelClrType));
        Assert.AreEqual(
            typeof(IEnumerable<PublishedSnapshotTestObjects.TestContentModel1>),
            contentType2.GetPropertyType("prop2").ClrType);

        // can create a model for an element
        var model1 = factory.CreateModel(element1);
        Assert.IsInstanceOf<PublishedSnapshotTestObjects.TestElementModel1>(model1);
        Assert.AreEqual("val1", ((PublishedSnapshotTestObjects.TestElementModel1)model1).Prop1);

        // can create a model for a published content
        var model2 = factory.CreateModel(element2);
        Assert.IsInstanceOf<PublishedSnapshotTestObjects.TestElementModel2>(model2);
        var mmodel2 = (PublishedSnapshotTestObjects.TestElementModel2)model2;

        // and get direct property
        Assert.IsInstanceOf<PublishedSnapshotTestObjects.TestContentModel1[]>(
            model2.Value(Mock.Of<IPublishedValueFallback>(), "prop2"));
        Assert.AreEqual(
            1,
            ((PublishedSnapshotTestObjects.TestContentModel1[])model2.Value(Mock.Of<IPublishedValueFallback>(), "prop2")).Length);

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

        public SimpleConverter3B(IPublishedSnapshotAccessor publishedSnapshotAccessor) =>
            _publishedSnapshotAccessor = publishedSnapshotAccessor;

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias == "Umbraco.Void.2";

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(IEnumerable<>).MakeGenericType(ModelType.For("content1"));

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Elements;

        public override object ConvertSourceToIntermediate(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            object source,
            bool preview)
        {
            var s = source as string;
            return s?.Split(',').Select(int.Parse).ToArray() ?? Array.Empty<int>();
        }

        public override object ConvertIntermediateToObject(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object inter,
            bool preview)
        {
            var publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
            return ((int[])inter).Select(x =>
                (PublishedSnapshotTestObjects.TestContentModel1)publishedSnapshot.Content
                    .GetById(x)).ToArray();
        }
    }
}
