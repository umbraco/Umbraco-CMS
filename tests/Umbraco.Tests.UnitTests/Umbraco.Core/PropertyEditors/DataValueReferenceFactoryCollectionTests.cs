// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Models.Property;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class DataValueReferenceFactoryCollectionTests
{
    private IDataValueEditorFactory DataValueEditorFactory { get; } = Mock.Of<IDataValueEditorFactory>(
        x => x.Create<MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor>(It.IsAny<DataEditorAttribute>())
             ==
             new MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor(
                 Mock.Of<IShortStringHelper>(),
                 Mock.Of<IJsonSerializer>(),
                 Mock.Of<IIOHelper>(),
                 new DataEditorAttribute("a"),
                 Mock.Of<IMediaImportService>(),
                 Mock.Of<IMediaService>(),
                 Mock.Of<ITemporaryFileService>(),
                 Mock.Of<IScopeProvider>(),
                 Mock.Of<IBackOfficeSecurityAccessor>(),
                 Mock.Of<IDataTypeConfigurationCache>(),
                 Mock.Of<ILocalizedTextService>(),
                 Mock.Of<IMediaTypeService>(),
                 Mock.Of<IMediaNavigationQueryService>()));

    private IIOHelper IOHelper { get; } = Mock.Of<IIOHelper>();

    private IShortStringHelper ShortStringHelper { get; } = Mock.Of<IShortStringHelper>();

    [Test]
    public void GetAllReferences_All_Variants_With_IDataValueReferenceFactory()
    {
        var collection = new DataValueReferenceFactoryCollection(() => new TestDataValueReferenceFactory().Yield());

        // label does not implement IDataValueReference
        var labelEditor = new LabelPropertyEditor(
            DataValueEditorFactory,
            IOHelper);
        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => labelEditor.Yield()));
        var trackedUdi1 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi2 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi3 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi4 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var serializer = new SystemTextConfigurationEditorJsonSerializer();
        var property =
            new Property(
                new PropertyType(ShortStringHelper, new DataType(labelEditor, serializer))
                {
                    Variations = ContentVariation.CultureAndSegment,
                })
            {
                Values = new List<PropertyValue>
                {
                    // Ignored (no culture)
                    new() { EditedValue = trackedUdi1 },
                    new() { Culture = "en-US", EditedValue = trackedUdi2 },
                    new() { Culture = "en-US", Segment = "A", EditedValue = trackedUdi3 },

                    // Ignored (no culture)
                    new() { Segment = "A", EditedValue = trackedUdi4 },

                    // Duplicate
                    new() { Culture = "en-US", Segment = "B", EditedValue = trackedUdi3 },
                },
            };
        var properties = new PropertyCollection { property };
        var result = collection.GetAllReferences(properties, propertyEditors).ToArray();

        Assert.AreEqual(2, result.Count());
        Assert.AreEqual(trackedUdi2, result.ElementAt(0).Udi.ToString());
        Assert.AreEqual(trackedUdi3, result.ElementAt(1).Udi.ToString());
    }

    [Test]
    public void GetAllReferences_All_Variants_With_IDataValueReference_Editor()
    {
        var collection = new DataValueReferenceFactoryCollection(() => Enumerable.Empty<IDataValueReferenceFactory>());

        // mediaPicker does implement IDataValueReference
        var mediaPicker = new MediaPicker3PropertyEditor(
            DataValueEditorFactory,
            IOHelper);
        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => mediaPicker.Yield()));
        var trackedUdi1 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi2 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi3 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi4 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var serializer = new SystemTextConfigurationEditorJsonSerializer();
        var property =
            new Property(
                new PropertyType(ShortStringHelper, new DataType(mediaPicker, serializer))
                {
                    Variations = ContentVariation.CultureAndSegment,
                })
            {
                Values = new List<PropertyValue>
                {
                    // Ignored (no culture)
                    new() { EditedValue = trackedUdi1 },
                    new() { Culture = "en-US", EditedValue = trackedUdi2 },
                    new() { Culture = "en-US", Segment = "A", EditedValue = trackedUdi3 },

                    // Ignored (no culture)
                    new() { Segment = "A", EditedValue = trackedUdi4 },

                    // Duplicate
                    new() { Culture = "en-US", Segment = "B", EditedValue = trackedUdi3 },
                },
            };
        var properties = new PropertyCollection { property };
        var result = collection.GetAllReferences(properties, propertyEditors).ToArray();

        Assert.AreEqual(2, result.Count());
        Assert.AreEqual(trackedUdi2, result.ElementAt(0).Udi.ToString());
        Assert.AreEqual(trackedUdi3, result.ElementAt(1).Udi.ToString());
    }

    [Test]
    public void GetAllReferences_Invariant_With_IDataValueReference_Editor()
    {
        var collection = new DataValueReferenceFactoryCollection(() => Enumerable.Empty<IDataValueReferenceFactory>());

        // mediaPicker does implement IDataValueReference
        var mediaPicker = new MediaPicker3PropertyEditor(
            DataValueEditorFactory,
            IOHelper);
        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => mediaPicker.Yield()));
        var trackedUdi1 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi2 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi3 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi4 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var serializer = new SystemTextConfigurationEditorJsonSerializer();
        var property =
            new Property(new PropertyType(ShortStringHelper, new DataType(mediaPicker, serializer))
            {
                Variations = ContentVariation.Nothing | ContentVariation.Segment,
            })
            {
                Values = new List<PropertyValue>
                {
                    new() { EditedValue = trackedUdi1 },

                    // Ignored (has culture)
                    new() { Culture = "en-US", EditedValue = trackedUdi2 },

                    // Ignored (has culture)
                    new() { Culture = "en-US", Segment = "A", EditedValue = trackedUdi3 },
                    new() { Segment = "A", EditedValue = trackedUdi4 },

                    // Duplicate
                    new() { Segment = "B", EditedValue = trackedUdi4 },
                },
            };
        var properties = new PropertyCollection { property };
        var result = collection.GetAllReferences(properties, propertyEditors).ToArray();

        Assert.AreEqual(2, result.Count());
        Assert.AreEqual(trackedUdi1, result.ElementAt(0).Udi.ToString());
        Assert.AreEqual(trackedUdi4, result.ElementAt(1).Udi.ToString());
    }

    [Test]
    public void GetAutomaticRelationTypesAliases_ContainsDefault()
    {
        var collection = new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>);
        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(Enumerable.Empty<IDataEditor>));

        var result = collection.GetAllAutomaticRelationTypesAliases(propertyEditors).ToArray();

        var expected = Constants.Conventions.RelationTypes.AutomaticRelationTypes;
        CollectionAssert.AreEquivalent(expected, result, "Result does not contain the expected relation type aliases.");
    }

    [Test]
    public void GetAutomaticRelationTypesAliases_ContainsCustom()
    {
        var collection = new DataValueReferenceFactoryCollection(() => new TestDataValueReferenceFactory().Yield());

        var labelPropertyEditor = new LabelPropertyEditor(DataValueEditorFactory, IOHelper);
        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => labelPropertyEditor.Yield()));
        var serializer = new SystemTextConfigurationEditorJsonSerializer();

        var result = collection.GetAllAutomaticRelationTypesAliases(propertyEditors).ToArray();

        var expected = Constants.Conventions.RelationTypes.AutomaticRelationTypes.Append("umbTest");
        CollectionAssert.AreEquivalent(expected, result, "Result does not contain the expected relation type aliases.");
    }

    private class TestDataValueReferenceFactory : IDataValueReferenceFactory
    {
        public IDataValueReference GetDataValueReference() => new TestMediaDataValueReference();

        public bool IsForEditor(IDataEditor dataEditor) => dataEditor.Alias == Constants.PropertyEditors.Aliases.Label;

        private class TestMediaDataValueReference : IDataValueReference
        {
            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                // This is the same as the media picker, it will just try to parse the value directly as a UDI.
                var asString = value is string str ? str : value?.ToString();

                if (string.IsNullOrEmpty(asString))
                {
                    yield break;
                }

                if (UdiParser.TryParse(asString, out var udi))
                {
                    yield return new UmbracoEntityReference(udi);
                }
            }

            public IEnumerable<string> GetAutomaticRelationTypesAliases() => new[]
            {
                "umbTest",
                "umbTest", // Duplicate on purpose to test distinct aliases
            };
        }
    }
}
