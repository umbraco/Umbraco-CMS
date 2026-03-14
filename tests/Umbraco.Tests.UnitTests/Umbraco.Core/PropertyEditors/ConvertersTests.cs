// Copyright (c) Umbraco.
// See LICENSE for more details.

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
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Contains unit tests for verifying the functionality of property editor converters in the Umbraco Core.
/// </summary>
[TestFixture]
public class ConvertersTests
{
    /// <summary>
    /// Unit test that verifies the registration and behavior of multiple property value converters (SimpleConverter3A and SimpleConverter3B),
    /// the creation of published content and element models, and the correct mapping of property types and values in the Umbraco model factory.
    /// </summary>
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
        register.AddSingleton(f => cacheMock.Object);

        var registerFactory = composition.CreateServiceProvider();
        var converters =
            registerFactory.GetRequiredService<PropertyValueConverterCollection>();

        var serializer = new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());
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
            false,
            new VariationContext());
        var element2 = new PublishedElement(
            elementType2,
            Guid.NewGuid(),
            new Dictionary<string, object> { { "prop2", "1003" } },
            false,
            new VariationContext());
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

    /// <summary>
    /// A simple converter implementation for testing purposes.
    /// </summary>
    public class SimpleConverter3A : PropertyValueConverterBase
    {
    /// <summary>
    /// Determines whether the specified property type is handled by this converter.
    /// </summary>
    /// <param name="propertyType">The property type to check.</param>
    /// <returns><c>true</c> if this converter can handle the specified property type; otherwise, <c>false</c>.</returns>
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias == "Umbraco.Void";

    /// <summary>
    /// Gets the type of the property value.
    /// </summary>
    /// <param name="propertyType">The published property type.</param>
    /// <returns>The type of the property value.</returns>
        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(string);

    /// <summary>
    /// Gets the property cache level for the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>The cache level of the property.</returns>
        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;
    }

    /// <summary>
    /// Unit test class for testing the behavior of the SimpleConverter3B property value converter in Umbraco.
    /// </summary>
    public class SimpleConverter3B : PropertyValueConverterBase
    {
        private readonly IPublishedContentCache _publishedContentCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleConverter3B"/> class.
    /// </summary>
    /// <param name="publishedContentCache">The published content cache.</param>
        public SimpleConverter3B(IPublishedContentCache publishedContentCache)
        {
            _publishedContentCache = publishedContentCache;
        }

    /// <summary>
    /// Determines whether the specified property type can be converted by this converter.
    /// </summary>
    /// <param name="propertyType">The property type to check.</param>
    /// <returns><c>true</c> if this converter can convert the specified property type; otherwise, <c>false</c>.</returns>
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias == "Umbraco.Void.2";

    /// <summary>
    /// Gets the property value type for the specified published property type.
    /// Returns an <see cref="IEnumerable{T}"/> where <c>T</c> is the model type for "content1".
    /// </summary>
    /// <param name="propertyType">The published property type.</param>
    /// <returns>The <see cref="Type"/> representing an <see cref="IEnumerable{T}"/> for the model type.</returns>
        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(IEnumerable<>).MakeGenericType(ModelType.For("content1"));

    /// <summary>
    /// Gets the property cache level for the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>The property cache level.</returns>
        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Elements;

    /// <summary>
    /// Converts a comma-separated string from the source object into an array of integers as the intermediate representation.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The type of the published property.</param>
    /// <param name="source">The source value to convert, expected to be a comma-separated string of integers.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <returns>An array of integers parsed from the source string, or an empty array if the source is not a string.</returns>
        public override object? ConvertSourceToIntermediate(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            object source,
            bool preview)
        {
            var s = source as string;
            return s?.Split(',').Select(int.Parse).ToArray() ?? Array.Empty<int>();
        }

    /// <summary>
    /// Converts an intermediate array of integer IDs to an array of <see cref="PublishedSnapshotTestObjects.TestContentModel1"/> objects.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The property type metadata.</param>
    /// <param name="referenceCacheLevel">The cache level for the reference.</param>
    /// <param name="inter">The intermediate object to convert, expected to be an <c>int[]</c> of content IDs.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <returns>An array of <see cref="PublishedSnapshotTestObjects.TestContentModel1"/> objects corresponding to the provided IDs.</returns>
        public override object ConvertIntermediateToObject(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object inter,
            bool preview)
        {
            return ((int[])inter).Select(x =>
                (PublishedSnapshotTestObjects.TestContentModel1)_publishedContentCache
                    .GetById(x)).ToArray();
        }
    }
}
