using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.PropertyEditors;
using static Umbraco.Core.Models.Property;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.PropertyEditors
{
    [TestFixture]
    public class DataValueReferenceFactoryCollectionTests
    {
        IDataTypeService DataTypeService { get; } = Mock.Of<IDataTypeService>();
        private IIOHelper IOHelper { get; } = Mock.Of<IIOHelper>();
        ILocalizedTextService LocalizedTextService { get; } = Mock.Of<ILocalizedTextService>();
        ILocalizationService LocalizationService { get; } = Mock.Of<ILocalizationService>();
        IShortStringHelper ShortStringHelper { get; } = Mock.Of<IShortStringHelper>();

        [Test]
        public void GetAllReferences_All_Variants_With_IDataValueReferenceFactory()
        {
            var collection = new DataValueReferenceFactoryCollection(new TestDataValueReferenceFactory().Yield());


            // label does not implement IDataValueReference
            var labelEditor = new LabelPropertyEditor(
                NullLoggerFactory.Instance,
                IOHelper,
                DataTypeService,
                LocalizedTextService,
                LocalizationService,
                ShortStringHelper
            );
            var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(labelEditor.Yield()));
            var trackedUdi1 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi2 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi3 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi4 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var serializer = new ConfigurationEditorJsonSerializer();
            var property = new Property(new PropertyType(ShortStringHelper, new DataType(labelEditor, serializer))
            {
                Variations = ContentVariation.CultureAndSegment
            })
            {
                Values = new List<PropertyValue>
                {
                    // Ignored (no culture)
                    new PropertyValue
                    {
                        EditedValue = trackedUdi1
                    },
                    new PropertyValue
                    {
                        Culture = "en-US",
                        EditedValue = trackedUdi2
                    },
                    new PropertyValue
                    {
                        Culture = "en-US",
                        Segment = "A",
                        EditedValue = trackedUdi3
                    },
                    // Ignored (no culture)
                    new PropertyValue
                    {
                        Segment = "A",
                        EditedValue = trackedUdi4
                    },
                    // duplicate
                    new PropertyValue
                    {
                        Culture = "en-US",
                        Segment = "B",
                        EditedValue = trackedUdi3
                    }
                }
            };
            var properties = new PropertyCollection
            {
                property
            };
            var result = collection.GetAllReferences(properties, propertyEditors);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(trackedUdi2, result.ElementAt(0).Udi.ToString());
            Assert.AreEqual(trackedUdi3, result.ElementAt(1).Udi.ToString());
        }

        [Test]
        public void GetAllReferences_All_Variants_With_IDataValueReference_Editor()
        {
            var collection = new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>());

            // mediaPicker does implement IDataValueReference
            var mediaPicker = new MediaPickerPropertyEditor(
                NullLoggerFactory.Instance,
                DataTypeService,
                LocalizationService,
                IOHelper,
                ShortStringHelper,
                LocalizedTextService
            );
            var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(mediaPicker.Yield()));
            var trackedUdi1 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi2 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi3 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi4 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var serializer = new ConfigurationEditorJsonSerializer();
            var property = new Property(new PropertyType(ShortStringHelper, new DataType(mediaPicker, serializer))
            {
                Variations = ContentVariation.CultureAndSegment
            })
            {
                Values = new List<PropertyValue>
                {
                    // Ignored (no culture)
                    new PropertyValue
                    {
                        EditedValue = trackedUdi1
                    },
                    new PropertyValue
                    {
                        Culture = "en-US",
                        EditedValue = trackedUdi2
                    },
                    new PropertyValue
                    {
                        Culture = "en-US",
                        Segment = "A",
                        EditedValue = trackedUdi3
                    },
                    // Ignored (no culture)
                    new PropertyValue
                    {
                        Segment = "A",
                        EditedValue = trackedUdi4
                    },
                    // duplicate
                    new PropertyValue
                    {
                        Culture = "en-US",
                        Segment = "B",
                        EditedValue = trackedUdi3
                    }
                }
            };
            var properties = new PropertyCollection
            {
                property
            };
            var result = collection.GetAllReferences(properties, propertyEditors);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(trackedUdi2, result.ElementAt(0).Udi.ToString());
            Assert.AreEqual(trackedUdi3, result.ElementAt(1).Udi.ToString());
        }

        [Test]
        public void GetAllReferences_Invariant_With_IDataValueReference_Editor()
        {
            var collection = new DataValueReferenceFactoryCollection(Enumerable.Empty<IDataValueReferenceFactory>());

            // mediaPicker does implement IDataValueReference
            var mediaPicker = new MediaPickerPropertyEditor(
                NullLoggerFactory.Instance,
                DataTypeService,
                LocalizationService,
                IOHelper,
                ShortStringHelper,
                LocalizedTextService
            );
            var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(mediaPicker.Yield()));
            var trackedUdi1 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi2 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi3 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi4 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var serializer = new ConfigurationEditorJsonSerializer();
            var property = new Property(new PropertyType(ShortStringHelper, new DataType(mediaPicker, serializer))
            {
                Variations = ContentVariation.Nothing | ContentVariation.Segment
            })
            {
                Values = new List<PropertyValue>
                {
                    new PropertyValue
                    {
                        EditedValue = trackedUdi1
                    },
                    // Ignored (has culture)
                    new PropertyValue
                    {
                        Culture = "en-US",
                        EditedValue = trackedUdi2
                    },
                    // Ignored (has culture)
                    new PropertyValue
                    {
                        Culture = "en-US",
                        Segment = "A",
                        EditedValue = trackedUdi3
                    },
                    new PropertyValue
                    {
                        Segment = "A",
                        EditedValue = trackedUdi4
                    },
                    // duplicate
                    new PropertyValue
                    {
                        Segment = "B",
                        EditedValue = trackedUdi4
                    }
                }
            };
            var properties = new PropertyCollection
            {
                property
            };
            var result = collection.GetAllReferences(properties, propertyEditors);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(trackedUdi1, result.ElementAt(0).Udi.ToString());
            Assert.AreEqual(trackedUdi4, result.ElementAt(1).Udi.ToString());
        }

        private class TestDataValueReferenceFactory : IDataValueReferenceFactory
        {
            public IDataValueReference GetDataValueReference() => new TestMediaDataValueReference();

            public bool IsForEditor(IDataEditor dataEditor) => dataEditor.Alias == Constants.PropertyEditors.Aliases.Label;

            private class TestMediaDataValueReference : IDataValueReference
            {
                public IEnumerable<UmbracoEntityReference> GetReferences(object value)
                {
                    // This is the same as the media picker, it will just try to parse the value directly as a UDI

                    var asString = value is string str ? str : value?.ToString();

                    if (string.IsNullOrEmpty(asString)) yield break;

                    if (UdiParser.TryParse(asString, out var udi))
                        yield return new UmbracoEntityReference(udi);
                }
            }
        }
    }
}
