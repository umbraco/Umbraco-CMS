// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.PublishedCache.Internal;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Published;

[TestFixture]
public class ConvertersTests
{
    [Test]
    public void SimpleConverter1Test()
    {
        var converters =
            new PropertyValueConverterCollection(() => new IPropertyValueConverter[] { new SimpleConverter1() });

        var serializer = new ConfigurationEditorJsonSerializer();
        var dataTypeServiceMock = new Mock<IDataTypeService>();
        var dataType = new DataType(
            new VoidEditor(Mock.Of<IDataValueEditorFactory>()), serializer)
        { Id = 1 };
        dataTypeServiceMock.Setup(x => x.GetAll()).Returns(dataType.Yield);

        var contentTypeFactory =
            new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, dataTypeServiceMock.Object);

        IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
        {
            yield return contentTypeFactory.CreatePropertyType(contentType, "prop1", 1);
        }

        var elementType1 = contentTypeFactory.CreateContentType(Guid.NewGuid(), 1000, "element1", CreatePropertyTypes);

        var element1 = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false);

        Assert.AreEqual(1234, element1.Value(Mock.Of<IPublishedValueFallback>(), "prop1"));

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

        public bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals("Umbraco.Void");

        public Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(int);

        public PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
            => int.TryParse(source as string, out var i) ? i : 0;

        public object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            => (int)inter;

        public object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            => ((int)inter).ToString();
    }

    [Test]
    public void SimpleConverter2Test()
    {
        var cacheMock = new Mock<IPublishedContentCache>();
        var cacheContent = new Dictionary<int, IPublishedContent>();
        cacheMock.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns<int>(id => cacheContent.TryGetValue(id, out var content) ? content : null);
        var publishedSnapshotMock = new Mock<IPublishedSnapshot>();
        publishedSnapshotMock.Setup(x => x.Content).Returns(cacheMock.Object);
        var publishedSnapshotAccessorMock = new Mock<IPublishedSnapshotAccessor>();
        var localPublishedSnapshot = publishedSnapshotMock.Object;
        publishedSnapshotAccessorMock.Setup(x => x.TryGetPublishedSnapshot(out localPublishedSnapshot)).Returns(true);
        var publishedSnapshotAccessor = publishedSnapshotAccessorMock.Object;

        var converters = new PropertyValueConverterCollection(() => new IPropertyValueConverter[]
        {
            new SimpleConverter2(publishedSnapshotAccessor),
        });

        var serializer = new ConfigurationEditorJsonSerializer();
        var dataTypeServiceMock = new Mock<IDataTypeService>();
        var dataType = new DataType(
            new VoidEditor(Mock.Of<IDataValueEditorFactory>()), serializer)
        { Id = 1 };
        dataTypeServiceMock.Setup(x => x.GetAll()).Returns(dataType.Yield);

        var contentTypeFactory =
            new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, dataTypeServiceMock.Object);

        IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
        {
            yield return contentTypeFactory.CreatePropertyType(contentType, "prop1", 1);
        }

        var elementType1 = contentTypeFactory.CreateContentType(Guid.NewGuid(), 1000, "element1", CreatePropertyTypes);

        var element1 = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false);

        var cntType1 = contentTypeFactory.CreateContentType(Guid.NewGuid(), 1001, "cnt1", t => Enumerable.Empty<PublishedPropertyType>());
        var cnt1 = new InternalPublishedContent(cntType1) { Id = 1234 };
        cacheContent[cnt1.Id] = cnt1;

        Assert.AreSame(cnt1, element1.Value(Mock.Of<IPublishedValueFallback>(), "prop1"));
    }

    private class SimpleConverter2 : IPropertyValueConverter
    {
        private readonly PropertyCacheLevel _cacheLevel;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public SimpleConverter2(IPublishedSnapshotAccessor publishedSnapshotAccessor, PropertyCacheLevel cacheLevel = PropertyCacheLevel.None)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _cacheLevel = cacheLevel;
        }

        public bool? IsValue(object value, PropertyValueLevel level)
            => value != null && (!(value is string) || string.IsNullOrWhiteSpace((string)value) == false);

        public bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals("Umbraco.Void");

        public Type GetPropertyValueType(IPublishedPropertyType propertyType)

            // The first version would be the "generic" version, but say we want to be more precise
            // and return: whatever Clr type is generated for content type with alias "cnt1" -- which
            // we cannot really typeof() at the moment because it has not been generated, hence ModelType.
            // => typeof(IPublishedContent);
            => ModelType.For("cnt1");

        public PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => _cacheLevel;

        public object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
            => int.TryParse(source as string, out var i) ? i : -1;

        public object ConvertIntermediateToObject(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object inter,
            bool preview)
        {
            var publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
            return publishedSnapshot.Content.GetById((int)inter);
        }

        public object ConvertIntermediateToXPath(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object inter,
            bool preview)
            => ((int)inter).ToString();
    }
}
