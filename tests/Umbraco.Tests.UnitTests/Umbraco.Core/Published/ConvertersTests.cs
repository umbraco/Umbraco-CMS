// Copyright (c) Umbraco.
// See LICENSE for more details.

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
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Published;

/// <summary>
/// Contains unit tests for the published content converters in Umbraco.
/// </summary>
[TestFixture]
public class ConvertersTests
{
    /// <summary>
    /// Tests the <see cref="SimpleConverter1"/> property value converter, verifying that it correctly converts property values and determines value presence for various inputs.
    /// Specifically, checks conversion of string values to integers and the handling of null and zero values.
    /// </summary>
    [Test]
    public void SimpleConverter1Test()
    {
        var converters =
            new PropertyValueConverterCollection(() => new IPropertyValueConverter[] { new SimpleConverter1() });

        var serializer = new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());
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

        var element1 = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false, new VariationContext());

        Assert.AreEqual(1234, element1.Value(Mock.Of<IPublishedValueFallback>(), "prop1"));

        // 'null' would be considered a 'missing' value by the default, magic logic
        var e = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", null } }, false, new VariationContext());
        Assert.IsFalse(e.HasValue("prop1"));

        // '0' would not - it's a valid integer - but the converter knows better
        e = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "0" } }, false, new VariationContext());
        Assert.IsFalse(e.HasValue("prop1"));
    }

    private class SimpleConverter1 : IPropertyValueConverter
    {
    /// <summary>
    /// Determines whether the specified <paramref name="value"/> is considered valid for the given <paramref name="level"/> of property value.
    /// </summary>
    /// <param name="value">The value to evaluate for validity.</param>
    /// <param name="level">The <see cref="PropertyValueLevel"/> at which to check the value.</param>
    /// <returns>
    /// <c>true</c> if the value is valid at the specified level; <c>false</c> if it is invalid; <c>null</c> if the validity cannot be determined (e.g., for <see cref="PropertyValueLevel.Source"/>).
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if an unsupported <paramref name="level"/> is specified.</exception>
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

    /// <summary>
    /// Determines whether the specified property type can be converted by this converter.
    /// </summary>
    /// <param name="propertyType">The property type to check.</param>
    /// <returns><c>true</c> if this converter can convert the specified property type; otherwise, <c>false</c>.</returns>
        public bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals("Umbraco.Void");

    /// <summary>
    /// Gets the type of the property value for the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type (not used).</param>
    /// <returns>Always returns <see cref="int"/> type.</returns>
        public Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(int);

    /// <summary>
    /// Gets the property cache level for the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>The property cache level.</returns>
        public PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

    /// <summary>
    /// Converts the source object to an intermediate representation.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The type of the published property.</param>
    /// <param name="source">The source object to convert.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <returns>The intermediate representation of the source object.</returns>
        public object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
            => int.TryParse(source as string, out var i) ? i : 0;

    /// <summary>
    /// Converts the intermediate value to its final object representation for the property.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The type of the published property.</param>
    /// <param name="referenceCacheLevel">The cache level for the reference.</param>
    /// <param name="inter">The intermediate value to convert, expected to be an <see cref="int"/>.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <returns>The converted <see cref="int"/> value.</returns>
        public object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            => (int)inter;

    /// <summary>
    /// Converts the given intermediate value to its string representation suitable for use in XPath queries.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The type of the published property.</param>
    /// <param name="referenceCacheLevel">The cache level for property references.</param>
    /// <param name="inter">The intermediate value to convert, expected to be an <c>int</c>.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <returns>The string representation of the intermediate integer value for XPath compatibility.</returns>
        public object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            => ((int)inter).ToString();
    }

    /// <summary>
    /// Verifies that the <see cref="SimpleConverter2"/> correctly resolves and returns the expected <see cref="IPublishedContent"/> instance
    /// when used as a property value converter within a published element.
    /// </summary>
    [Test]
    public void SimpleConverter2Test()
    {
        var cacheMock = new Mock<IPublishedContentCache>();
        var cacheContent = new Dictionary<int, IPublishedContent>();
        cacheMock.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns<int>(id => cacheContent.TryGetValue(id, out var content) ? content : null);

        var converters = new PropertyValueConverterCollection(() =>
        [
            new SimpleConverter2(cacheMock.Object)
        ]);

        var serializer = new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());
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

        var element1 = new PublishedElement(elementType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false, new VariationContext());

        var cntType1 = contentTypeFactory.CreateContentType(Guid.NewGuid(), 1001, "cnt1", t => Enumerable.Empty<PublishedPropertyType>());
        var cnt1 = new InternalPublishedContent(cntType1) { Id = 1234 };
        cacheContent[cnt1.Id] = cnt1;

        Assert.AreSame(cnt1, element1.Value(Mock.Of<IPublishedValueFallback>(), "prop1"));
    }

    private class SimpleConverter2 : IPropertyValueConverter
    {
        private readonly IPublishedContentCache _contentCache;
        private readonly PropertyCacheLevel _cacheLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleConverter2"/> class.
    /// </summary>
    /// <param name="contentCache">The content cache to use.</param>
    /// <param name="cacheLevel">The cache level to apply, optional.</param>
        public SimpleConverter2(IPublishedContentCache contentCache, PropertyCacheLevel cacheLevel = PropertyCacheLevel.None)
        {
            _contentCache = contentCache;
            _cacheLevel = cacheLevel;
        }

    /// <summary>
    /// Determines whether the specified value is considered valid, meaning it is not null and, if a string, is not empty or whitespace.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <param name="level">The level of the property value (not used in this implementation).</param>
    /// <returns>
    /// <c>true</c> if the value is not null and, if a string, is not empty or whitespace; otherwise, <c>false</c>.
    /// </returns>
        public bool? IsValue(object value, PropertyValueLevel level)
            => value != null && (!(value is string) || string.IsNullOrWhiteSpace((string)value) == false);

    /// <summary>
    /// Determines whether the specified property type can be converted by this converter.
    /// </summary>
    /// <param name="propertyType">The property type to check.</param>
    /// <returns><c>true</c> if this converter can convert the specified property type; otherwise, <c>false</c>.</returns>
        public bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals("Umbraco.Void");

    /// <summary>
    /// Gets the CLR type representing the model generated for the content type with alias "cnt1".
    /// </summary>
    /// <param name="propertyType">The published property type (not used in this implementation).</param>
    /// <returns>The <see cref="Type"/> corresponding to the model for content type alias "cnt1".</returns>
        public Type GetPropertyValueType(IPublishedPropertyType propertyType)

            // The first version would be the "generic" version, but say we want to be more precise
            // and return: whatever Clr type is generated for content type with alias "cnt1" -- which
            // we cannot really typeof() at the moment because it has not been generated, hence ModelType.
            // => typeof(IPublishedContent);
            => ModelType.For("cnt1");

    /// <summary>
    /// Gets the property cache level for the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>The cache level of the property.</returns>
        public PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => _cacheLevel;

    /// <summary>
    /// Converts the source object to an intermediate representation.
    /// </summary>
    /// <param name="owner">The owning published element.</param>
    /// <param name="propertyType">The published property type.</param>
    /// <param name="source">The source object to convert.</param>
    /// <param name="preview">Indicates whether this is a preview conversion.</param>
    /// <returns>The intermediate object representation.</returns>
        public object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
            => int.TryParse(source as string, out var i) ? i : -1;

    /// <summary>
    /// Converts the specified intermediate object to its final object representation using the content cache.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The type of the published property.</param>
    /// <param name="referenceCacheLevel">The cache level for the reference.</param>
    /// <param name="inter">The intermediate object to convert, expected to be an integer identifier.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <returns>The published content object retrieved by the specified identifier.</returns>
        public object ConvertIntermediateToObject(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object inter,
            bool preview)
        {
            return _contentCache.GetById((int)inter)!;
        }

    /// <summary>
    /// Converts the intermediate value to an XPath-compatible string representation.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The type of the published property.</param>
    /// <param name="referenceCacheLevel">The cache level for the reference.</param>
    /// <param name="inter">The intermediate value to convert.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <returns>A string representation of the intermediate value suitable for XPath queries.</returns>
        public object ConvertIntermediateToXPath(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object inter,
            bool preview)
            => ((int)inter).ToString();
    }
}
