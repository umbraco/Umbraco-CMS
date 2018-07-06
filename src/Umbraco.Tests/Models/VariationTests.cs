using System;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class VariationTests
    {
        [SetUp]
        public void SetUp()
        {
            // annoying, but content type wants short string helper ;(
            SettingsForTests.Reset();

            // well, this is also annoying, but...
            // validating a value is performed by its data editor,
            // based upon the configuration in the data type, so we
            // need to be able to retrieve them all...

            Current.Reset();
            var container = Mock.Of<IServiceContainer>();
            Current.Container = new LightInjectContainer(container);

            var dataEditors = new DataEditorCollection(new IDataEditor[]
            {
                new DataEditor(Mock.Of<ILogger>()) { Alias = "editor", ExplicitValueEditor = new DataValueEditor("view") }
            });
            var propertyEditors = new PropertyEditorCollection(dataEditors);

            var dataType = Mock.Of<IDataType>();
            Mock.Get(dataType)
                .Setup(x => x.Configuration)
                .Returns(null);

            var dataTypeService = Mock.Of<IDataTypeService>();
            Mock.Get(dataTypeService)
                .Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns<int>(x => dataType);

            var serviceContext = new ServiceContext(dataTypeService: dataTypeService);

            Mock.Get(container)
                .Setup(x => x.GetInstance(It.IsAny<Type>()))
                .Returns<Type>(x =>
                {
                    if (x == typeof(PropertyEditorCollection)) return propertyEditors;
                    if (x == typeof(ServiceContext)) return serviceContext;
                    throw new Exception("oops");
                });
        }

        [Test]
        public void PropertyTests()
        {
            var propertyType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop" };
            var prop = new Property(propertyType);

            const string langFr = "fr-FR";

            // can set value
            // and get edited and published value
            // because non-publishing
            prop.SetValue("a");
            Assert.AreEqual("a", prop.GetValue());
            Assert.AreEqual("a", prop.GetValue(published: true));

            // illegal, 'cos non-publishing
            Assert.Throws<NotSupportedException>(() => prop.PublishValue());

            // change
            propertyType.IsPublishing = true;

            // can get value
            // and now published value is null
            Assert.AreEqual("a", prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));

            // cannot set non-supported variation value
            Assert.Throws<NotSupportedException>(() => prop.SetValue("x", langFr));
            Assert.IsNull(prop.GetValue(langFr));

            // can publish value
            // and get edited and published values
            prop.PublishValue();
            Assert.AreEqual("a", prop.GetValue());
            Assert.AreEqual("a", prop.GetValue(published: true));

            // can set value
            // and get edited and published values
            prop.SetValue("b");
            Assert.AreEqual("b", prop.GetValue());
            Assert.AreEqual("a", prop.GetValue(published: true));

            // can clear value
            prop.ClearPublishedValue();
            Assert.AreEqual("b", prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));

            // change
            propertyType.Variations |= ContentVariation.CultureNeutral;

            // can set value
            // and get values
            prop.SetValue("c", langFr);
            Assert.AreEqual("b", prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));
            Assert.AreEqual("c", prop.GetValue(langFr));
            Assert.IsNull(prop.GetValue(langFr, published: true));

            // can publish value
            // and get edited and published values
            prop.PublishValue(langFr);
            Assert.AreEqual("b", prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));
            Assert.AreEqual("c", prop.GetValue(langFr));
            Assert.AreEqual("c", prop.GetValue(langFr, published: true));

            // can clear all
            prop.ClearPublishedAllValues();
            Assert.AreEqual("b", prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));
            Assert.AreEqual("c", prop.GetValue(langFr));
            Assert.IsNull(prop.GetValue(langFr, published: true));

            // can publish all
            prop.PublishAllValues();
            Assert.AreEqual("b", prop.GetValue());
            Assert.AreEqual("b", prop.GetValue(published: true));
            Assert.AreEqual("c", prop.GetValue(langFr));
            Assert.AreEqual("c", prop.GetValue(langFr, published: true));

            // same for culture
            prop.ClearPublishedCultureValues(langFr);
            Assert.AreEqual("c", prop.GetValue(langFr));
            Assert.IsNull(prop.GetValue(langFr, published: true));
            prop.PublishCultureValues(langFr);
            Assert.AreEqual("c", prop.GetValue(langFr));
            Assert.AreEqual("c", prop.GetValue(langFr, published: true));

            prop.ClearPublishedCultureValues();
            Assert.AreEqual("b", prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));
            prop.PublishCultureValues();
            Assert.AreEqual("b", prop.GetValue());
            Assert.AreEqual("b", prop.GetValue(published: true));
        }

        [Test]
        public void ContentNames()
        {
            var contentType = new ContentType(-1) { Alias = "contentType" };
            var content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            const string langFr = "fr-FR";
            const string langUk = "en-UK";

            // throws if the content type does not support the variation
            Assert.Throws<NotSupportedException>(() => content.SetName("name-fr", langFr));

            // now it will work
            contentType.Variations = ContentVariation.CultureNeutral;

            // invariant name works
            content.Name = "name";
            Assert.AreEqual("name", content.GetName(null));
            content.SetName("name2", null);
            Assert.AreEqual("name2", content.Name);
            Assert.AreEqual("name2", content.GetName(null));

            // variant names work
            content.SetName("name-fr", langFr);
            content.SetName("name-uk", langUk);
            Assert.AreEqual("name-fr", content.GetName(langFr));
            Assert.AreEqual("name-uk", content.GetName(langUk));

            // variant dictionary of names work
            Assert.AreEqual(2, content.CultureNames.Count);
            Assert.IsTrue(content.CultureNames.ContainsKey(langFr));
            Assert.AreEqual("name-fr", content.CultureNames[langFr]);
            Assert.IsTrue(content.CultureNames.ContainsKey(langUk));
            Assert.AreEqual("name-uk", content.CultureNames[langUk]);
        }

        [Test]
        public void ContentPublishValues()
        {
            const string langFr = "fr-FR";

            var propertyType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop" };
            var contentType = new ContentType(-1) { Alias = "contentType" };
            contentType.AddPropertyType(propertyType);

            var content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            // can set value
            // and get edited value, published is null
            // because publishing
            content.SetValue("prop", "a");
            Assert.AreEqual("a", content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));

            // cannot set non-supported variation value
            Assert.Throws<NotSupportedException>(() => content.SetValue("prop", "x", langFr));
            Assert.IsNull(content.GetValue("prop", langFr));

            // can publish value
            // and get edited and published values
            Assert.IsTrue(content.TryPublishValues());
            Assert.AreEqual("a", content.GetValue("prop"));
            Assert.AreEqual("a", content.GetValue("prop", published: true));

            // can set value
            // and get edited and published values
            content.SetValue("prop", "b");
            Assert.AreEqual("b", content.GetValue("prop"));
            Assert.AreEqual("a", content.GetValue("prop", published: true));

            // can clear value
            content.ClearPublishedValues();
            Assert.AreEqual("b", content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));

            // change
            contentType.Variations |= ContentVariation.CultureNeutral;
            propertyType.Variations |= ContentVariation.CultureNeutral;

            // can set value
            // and get values
            content.SetValue("prop", "c", langFr);
            Assert.AreEqual("b", content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.IsNull(content.GetValue("prop", langFr, published: true));

            // can publish value
            // and get edited and published values
            Assert.IsFalse(content.TryPublishValues(langFr)); // no name
            content.SetName("name-fr", langFr);
            Assert.IsTrue(content.TryPublishValues(langFr));
            Assert.AreEqual("b", content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));

            // can clear all
            content.ClearAllPublishedValues();
            Assert.AreEqual("b", content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.IsNull(content.GetValue("prop", langFr, published: true));

            // can publish all
            Assert.IsTrue(content.TryPublishAllValues());
            Assert.AreEqual("b", content.GetValue("prop"));
            Assert.AreEqual("b", content.GetValue("prop", published: true));
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));

            // same for culture
            content.ClearCulturePublishedValues(langFr);
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.IsNull(content.GetValue("prop", langFr, published: true));
            content.PublishCultureValues(langFr);
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));

            content.ClearCulturePublishedValues();
            Assert.AreEqual("b", content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));
            content.PublishCultureValues();
            Assert.AreEqual("b", content.GetValue("prop"));
            Assert.AreEqual("b", content.GetValue("prop", published: true));

            var other = new Content("other", -1, contentType) { Id = 2, VersionId = 1 };
            other.SetValue("prop", "o");
            other.SetValue("prop", "o1", langFr);

            // can copy other's edited value
            content.CopyAllValues(other);
            Assert.AreEqual("o", content.GetValue("prop"));
            Assert.AreEqual("b", content.GetValue("prop", published: true));
            Assert.AreEqual("o1", content.GetValue("prop", langFr));
            Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));

            // can copy self's published value
            content.CopyAllValues(content);
            Assert.AreEqual("b", content.GetValue("prop"));
            Assert.AreEqual("b", content.GetValue("prop", published: true));
            Assert.AreEqual("c", content.GetValue("prop", langFr));
            Assert.AreEqual("c", content.GetValue("prop", langFr, published: true));
        }

        [Test]
        public void ContentPublishValuesWithMixedPropertyTypeVariations()
        {
            const string langFr = "fr-FR";

            var contentType = new ContentType(-1) { Alias = "contentType" };
            contentType.Variations |= ContentVariation.CultureNeutral; //supports both variant/invariant

            //In real life, a property cannot be both invariant + variant and be mandatory. If this happens validation will always fail when doing TryPublishValues since the invariant value will always be empty.
            //so here we are only setting properties to one or the other, not both
            var variantPropType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop1", Variations = ContentVariation.CultureNeutral, Mandatory = true };
            var invariantPropType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop2", Variations = ContentVariation.InvariantNeutral, Mandatory = true};
            
            contentType.AddPropertyType(variantPropType);
            contentType.AddPropertyType(invariantPropType);

            var content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            content.SetName("hello", langFr);

            Assert.IsFalse(content.TryPublishValues(langFr)); //will fail because prop1 is mandatory
            content.SetValue("prop1", "a", langFr);
            Assert.IsTrue(content.TryPublishValues(langFr));
            Assert.AreEqual("a", content.GetValue("prop1", langFr, published: true));
            //this will be null because we tried to publish values for a specific culture but this property is invariant
            Assert.IsNull(content.GetValue("prop2", published: true));

            Assert.IsFalse(content.TryPublishValues()); //will fail because prop2 is mandatory
            content.SetValue("prop2", "b");
            Assert.IsTrue(content.TryPublishValues());
            Assert.AreEqual("b", content.GetValue("prop2", published: true));
        }

        [Test]
        public void ContentPublishVariations()
        {
            const string langFr = "fr-FR";
            const string langUk = "en-UK";
            const string langEs = "es-ES";

            var propertyType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop" };
            var contentType = new ContentType(-1) { Alias = "contentType" };
            contentType.AddPropertyType(propertyType);

            var content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            contentType.Variations |= ContentVariation.CultureNeutral;
            propertyType.Variations |= ContentVariation.CultureNeutral;

            content.SetValue("prop", "a");
            content.SetValue("prop", "a-fr", langFr);
            content.SetValue("prop", "a-uk", langUk);
            content.SetValue("prop", "a-es", langEs);

            // cannot publish without a name
            Assert.IsFalse(content.TryPublishValues(langFr));

            // works with a name
            // and then FR is available, and published
            content.SetName("name-fr", langFr);
            Assert.IsTrue(content.TryPublishValues(langFr));

            // now UK is available too
            content.SetName("name-uk", langUk);

            // test available, published
            Assert.IsTrue(content.IsCultureAvailable(langFr));
            Assert.IsTrue(content.IsCulturePublished(langFr));
            Assert.AreEqual("name-fr", content.GetPublishName(langFr));
            Assert.AreNotEqual(DateTime.MinValue, content.GetCulturePublishDate(langFr));
            Assert.IsFalse(content.IsCultureEdited(langFr)); // once published, edited is *wrong* until saved

            Assert.IsTrue(content.IsCultureAvailable(langUk));
            Assert.IsFalse(content.IsCulturePublished(langUk));
            Assert.IsNull(content.GetPublishName(langUk));
            Assert.Throws<InvalidOperationException>(() => content.GetCulturePublishDate(langUk)); // not published!
            Assert.IsTrue(content.IsCultureEdited(langEs)); // not published, so... edited

            Assert.IsFalse(content.IsCultureAvailable(langEs));
            Assert.IsFalse(content.IsCulturePublished(langEs));
            Assert.IsNull(content.GetPublishName(langEs));
            Assert.Throws<InvalidOperationException>(() => content.GetCulturePublishDate(langEs)); // not published!
            Assert.IsTrue(content.IsCultureEdited(langEs)); // not published, so... edited

            // cannot test IsCultureEdited here - as that requires the content service and repository
            // see: ContentServiceTests.Can_SaveRead_Variations
        }

        [Test]
        public void IsDirtyTests()
        {
            var propertyType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop" };
            var prop = new Property(propertyType);
            var contentType = new ContentType(-1) { Alias = "contentType" };
            contentType.AddPropertyType(propertyType);

            var content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            prop.SetValue("a");
            Assert.AreEqual("a", prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));

            Assert.IsTrue(prop.IsDirty());

            content.SetValue("prop", "a");
            Assert.AreEqual("a", content.GetValue("prop"));
            Assert.IsNull(content.GetValue("prop", published: true));

            Assert.IsTrue(content.IsDirty());
            Assert.IsTrue(content.IsAnyUserPropertyDirty());
            // how can we tell which variation was dirty?
        }

        [Test]
        public void ValidationTests()
        {
            var propertyType = new PropertyType("editor", ValueStorageType.Nvarchar) { Alias = "prop", IsPublishing = true };
            var prop = new Property(propertyType);

            prop.SetValue("a");
            Assert.AreEqual("a", prop.GetValue());
            Assert.IsNull(prop.GetValue(published: true));

            Assert.IsTrue(prop.IsValid());

            propertyType.Mandatory = true;
            Assert.IsTrue(prop.IsValid());

            prop.SetValue(null);
            Assert.IsFalse(prop.IsValid());

            // can publish, even though invalid
            prop.PublishValue();
        }
    }
}
