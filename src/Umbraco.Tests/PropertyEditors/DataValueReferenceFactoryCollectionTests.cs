using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors;
using static Umbraco.Core.Models.Property;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class DataValueReferenceFactoryCollectionTests
    {
        [Test]
        public void GetAllReferences_All_Variants_With_IDataValueReferenceFactory()
        {
            var collection = new DataValueReferenceFactoryCollection(new TestDataValueReferenceFactory().Yield());

            // label does not implement IDataValueReference
            var labelEditor = new LabelPropertyEditor(Mock.Of<ILogger>());
            var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(labelEditor.Yield()));
            var trackedUdi1 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi2 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi3 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi4 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var property = new Property(new PropertyType(new DataType(labelEditor))
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
            var mediaPicker = new MediaPickerPropertyEditor(Mock.Of<ILogger>());
            var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(mediaPicker.Yield()));
            var trackedUdi1 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi2 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi3 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi4 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var property = new Property(new PropertyType(new DataType(mediaPicker))
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
            var mediaPicker = new MediaPickerPropertyEditor(Mock.Of<ILogger>());
            var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(mediaPicker.Yield()));
            var trackedUdi1 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi2 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi3 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var trackedUdi4 = Udi.Create(Constants.UdiEntityType.Media, Guid.NewGuid()).ToString();
            var property = new Property(new PropertyType(new DataType(mediaPicker))
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

                    if (Udi.TryParse(asString, out var udi))
                        yield return new UmbracoEntityReference(udi);
                }
            }
        }
    }
}
