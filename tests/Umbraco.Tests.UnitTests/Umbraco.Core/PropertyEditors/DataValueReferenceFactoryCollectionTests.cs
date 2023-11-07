// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Models.Property;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class DataValueReferenceFactoryCollectionTests
{
    private IDataValueEditorFactory DataValueEditorFactory { get; } = Mock.Of<IDataValueEditorFactory>(
        x => x.Create<MediaPickerPropertyEditor.MediaPickerPropertyValueEditor>(It.IsAny<DataEditorAttribute>())
             ==
             new MediaPickerPropertyEditor.MediaPickerPropertyValueEditor(
                 Mock.Of<ILocalizedTextService>(),
                 Mock.Of<IShortStringHelper>(),
                 Mock.Of<IJsonSerializer>(),
                 Mock.Of<IIOHelper>(),
                 new DataEditorAttribute("a", "a", "a")));

    private IIOHelper IOHelper { get; } = Mock.Of<IIOHelper>();

    private IShortStringHelper ShortStringHelper { get; } = Mock.Of<IShortStringHelper>();

    private IEditorConfigurationParser EditorConfigurationParser { get; } = Mock.Of<IEditorConfigurationParser>();

    [Test]
    public void GetAllReferences_All_Variants_With_IDataValueReferenceFactory()
    {
        var collection = new DataValueReferenceFactoryCollection(() => new TestDataValueReferenceFactory().Yield());

        // label does not implement IDataValueReference
        var labelEditor = new LabelPropertyEditor(
            DataValueEditorFactory,
            IOHelper,
            EditorConfigurationParser);
        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => labelEditor.Yield()));
        var trackedUdi1 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi2 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi3 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi4 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var serializer = new ConfigurationEditorJsonSerializer();
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
        var mediaPicker = new MediaPickerPropertyEditor(
            DataValueEditorFactory,
            IOHelper,
            EditorConfigurationParser);
        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => mediaPicker.Yield()));
        var trackedUdi1 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi2 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi3 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi4 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var serializer = new ConfigurationEditorJsonSerializer();
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
        var mediaPicker = new MediaPickerPropertyEditor(
            DataValueEditorFactory,
            IOHelper,
            EditorConfigurationParser);
        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => mediaPicker.Yield()));
        var trackedUdi1 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi2 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi3 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var trackedUdi4 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
        var serializer = new ConfigurationEditorJsonSerializer();
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
        var properties = new PropertyCollection();

        var resultA = collection.GetAutomaticRelationTypesAliases(propertyEditors).ToArray();
        var resultB = collection.GetAutomaticRelationTypesAliases(properties, propertyEditors).ToArray();

        Assert.Multiple(() =>
        {
            foreach (var alias in Constants.Conventions.RelationTypes.AutomaticRelationTypes)
            {
                Assert.Contains(alias, resultA, "Result A does not contain one of the default automatic relation types.");
                Assert.Contains(alias, resultB, "Result B does not contain one of the default automatic relation types.");
            }
        });

        // Ensure we don't have more than just the default
        int expectedCount = Constants.Conventions.RelationTypes.AutomaticRelationTypes.Length;
        Assert.AreEqual(expectedCount, resultA.Length, "Result A should only contain the default automatic relation types.");
        Assert.AreEqual(expectedCount, resultB.Length, "Result B should only contain the default automatic relation types.");
    }

    [Test]
    public void GetAutomaticRelationTypesAliases_ContainsCustom()
    {
        var collection = new DataValueReferenceFactoryCollection(() => new TestDataValueReferenceFactory().Yield());

        var labelPropertyEditor = new LabelPropertyEditor(DataValueEditorFactory, IOHelper, EditorConfigurationParser);
        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => labelPropertyEditor.Yield()));
        var serializer = new ConfigurationEditorJsonSerializer();
        var property = new Property(new PropertyType(ShortStringHelper, new DataType(labelPropertyEditor, serializer)));
        var properties = new PropertyCollection { property, property }; // Duplicate on purpose to test distinct aliases

        var resultA = collection.GetAutomaticRelationTypesAliases(propertyEditors).ToArray();
        var resultB = collection.GetAutomaticRelationTypesAliases(properties, propertyEditors).ToArray();

        Assert.Multiple(() =>
        {
            Assert.Contains("umbTest", resultA, "Result A does not contain the custom automatic relation type.");
            Assert.Contains("umbTest", resultB, "Result B does not contain the custom automatic relation type.");
        });

        // Ensure we don't have more than just the default and single custom
        int expectedCount = Constants.Conventions.RelationTypes.AutomaticRelationTypes.Length + 1;
        Assert.AreEqual(expectedCount, resultA.Length, "Result A should only contain the default and a single custom automatic relation type.");
        Assert.AreEqual(expectedCount, resultB.Length, "Result B should only contain the default and a single custom automatic relation type.");
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
